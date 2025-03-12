﻿using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Wpf.Ui.Controls;

namespace LiveryConverter2024
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Prepare_Dirs();
            projectFolder.Text = Properties.Settings.Default.projectPath;
            textureFolder.Text = Properties.Settings.Default.texturePath;
            sdkPath.Text = Properties.Settings.Default.sdkPath;
            layoutGenPath.Text = Properties.Settings.Default.layoutGenPath;
            var t_cb = Properties.Settings.Default.store;
            switch (t_cb)
            {
                case "MS Store":
                    comboBox.SelectedIndex = 1;
                    break;
                default:
                    comboBox.SelectedIndex = 0;
                    break;
            }
        }

        public string DebugConsole
        {
            get { return debug.Text; }
            set { 
                debug.AppendText(value);
                debug.ScrollToEnd();
            }
        }
        private readonly string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "com.budzique.livery-converter.app\\");
        private Process p1;
        private Process p2;
        private Process p3;
        private void clearPath(System.IO.DirectoryInfo directory)
        {
            foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
            foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(path, "PackageSources\\SimObjects\\Airplanes\\livery-converter-2024\\common\\texture\\"));
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(path, "PackageDefinitions\\"));
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(path, "DDSINPUT\\"));
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(path, "TEMP\\"));
        }

        private void Prepare_Dirs()
        {
            debug.AppendText("Creating Directories\n");
            debug.ScrollToEnd();
            System.IO.Directory.CreateDirectory(path);
            clearPath(new DirectoryInfo(path));
            Assembly asm = Assembly.GetExecutingAssembly();
            string[] resources = asm.GetManifestResourceNames();

            foreach (string r in resources)
            {
                if (r.Contains("texconv.exe"))
                {
                    debug.AppendText("extracting texconv.exe!\n");
                    debug.ScrollToEnd();
                    string filename = r.Replace("LiveryConverter2024.includes.", "");
                    Stream? asmStream = GetType().Assembly.GetManifestResourceStream(r);
                    if (asmStream != null)
                    {
                        Stream stream = asmStream;
                        byte[] bytes = new byte[(int)stream.Length];
                        stream.Read(bytes, 0, bytes.Length);
                        File.WriteAllBytes(path + filename, bytes);
                    }
                }
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new OpenFolderDialog
            {
                // Set options here
            };

            if (folderDialog.ShowDialog() == true)
            {
                var folderName = folderDialog.FolderName;
                projectFolder.Text = folderName;
                Properties.Settings.Default.projectPath = folderName;
                Properties.Settings.Default.Save();
            }
        }

        private void projectFolder_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new OpenFolderDialog
            {
                // Set options here
            };

            if (folderDialog.ShowDialog() == true)
            {
                var folderName = folderDialog.FolderName;
                textureFolder.Text = folderName;
                Properties.Settings.Default.texturePath = folderName;
                Properties.Settings.Default.Save();
            }
        }

        private void textureFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.store = comboBox.SelectedItem.ToString();
            Properties.Settings.Default.Save();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new OpenFolderDialog
            {
                // Set options here
            };

            if (folderDialog.ShowDialog() == true)
            {
                var folderName = folderDialog.FolderName;
                sdkPath.Text = folderName;
                Properties.Settings.Default.sdkPath = folderName;
                Properties.Settings.Default.Save();
            }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new OpenFolderDialog
            {
                // Set options here
            };

            if (folderDialog.ShowDialog() == true)
            {
                var folderName = folderDialog.FolderName;
                layoutGenPath.Text = folderName;
                Properties.Settings.Default.layoutGenPath = folderName;
                Properties.Settings.Default.Save();
            }
        }
        private void generateKTX2()
        {
            

            string stearing = " ";
            if (Properties.Settings.Default.store == "Steam")
            {
                stearing = " -forceSteam ";
            }
            using (p3 = new Process()) 
            {
                p3.StartInfo.FileName = Properties.Settings.Default.sdkPath + "\\tools\\bin\\fspackagetool.exe";
                p3.StartInfo.Arguments = "-nopause"+stearing+ path + "livery-converter-2024.xml";
                p3.StartInfo.RedirectStandardOutput = true;
                p3.Start();
                string output = p3.StandardOutput.ReadToEnd();
                p3.WaitForExit();
                debug.AppendText(output);
                debug.ScrollToEnd();
            }
            
            /**
             * TODO:
             *  move new textures to desired location: Properties.Settings.Default.texturePath
             *  run MSFSLayoutGenerator.exe
             * 
             * **/
        }

        private void generateXMLs()
        {
            DirectoryInfo d = new DirectoryInfo(path + "TEMP");
            FileInfo[] i = d.GetFiles();
            Dispatcher.Invoke(() =>
            {
                DebugConsole = "Sorting Textures... Generating XMLs...\n";
            });
            string pkgSourceDir = path + "PackageSources\\SimObjects\\Airplanes\\livery-converter-2024\\common\\texture\\";

            foreach (FileInfo f in i)
            {
                string shortFileName = f.Name;
                string shortNewName = shortFileName.Substring(0, shortFileName.Length - 4);
                string ext = f.Extension;
                string file = f.FullName;
                //File.Move(file, newFile);
                if (file.Contains("ALBD"))
                {
                    XDocument xmldoc = new XDocument(
                        new XElement("BitmapConfiguration",
                            new XElement("BitmapSlot", "MTL_BITMAP_DECAL0"),
                            new XElement("UserFlags", new XAttribute("type", "_DEFAULT"), "QUALITYHIGH")
                        )
                    );
                    xmldoc.Save(pkgSourceDir + shortNewName + ".xml");
                    File.Move(file, pkgSourceDir + shortNewName);
                }
                else
                if (file.Contains("COMP"))
                {
                    XDocument xmldoc = new XDocument(
                        new XElement("BitmapConfiguration",
                            new XElement("BitmapSlot", "MTL_BITMAP_METAL_ROUGH_AO"),
                            new XElement("UserFlags", new XAttribute("type", "_DEFAULT"), "QUALITYHIGH"),
                            new XElement("ForceNoAlpha", true)
                        )
                    );
                    xmldoc.Save(pkgSourceDir + shortNewName + ".xml");
                    File.Move(file, pkgSourceDir + shortNewName);
                }
                else
                if (file.Contains("DECAL"))
                {
                    XDocument xmldoc = new XDocument(
                        new XElement("BitmapConfiguration",
                            new XElement("BitmapSlot", "MTL_BITMAP_DECAL0"),
                            new XElement("UserFlags", new XAttribute("type", "_DEFAULT"), "QUALITYHIGH")
                        )
                    );
                    xmldoc.Save(pkgSourceDir + shortNewName + ".xml");
                    File.Move(file, pkgSourceDir + shortNewName);
                }
                else
                if (file.Contains("NORM"))
                {
                    XDocument xmldoc = new XDocument(
                        new XElement("BitmapConfiguration",
                            new XElement("BitmapSlot", "MTL_BITMAP_NORMAL"),
                            new XElement("UserFlags", new XAttribute("type", "_DEFAULT"), "QUALITYHIGH")
                        )
                    );
                    xmldoc.Save(pkgSourceDir + shortNewName + ".xml");
                    File.Move(file, pkgSourceDir + shortNewName);
                }
            }
            XDocument xPkgDef = new XDocument(
                new XElement("AssetPackage", new XAttribute("Version", "0.1.0"),
                    new XElement("ItemSettings",
                        new XElement("ContentType", "AIRCRAFT"),
                        new XElement("Title", "LiveryConverter2024"),
                        new XElement("Manufacturer", "Budzique|FlakNine"),
                        new XElement("Creator", "Budzique|FlakNine")
                    ),
                    new XElement("Flags",
                        new XElement("VisibleInStore", true),
                        new XElement("CanBeReferenced", true)
                    ),
                    new XElement("AssetGroups",
                        new XElement("AssetGroup", new XAttribute("Name", "LiveryConverter2024"),
                            new XElement("Type", "ModularSimObject"),
                            new XElement("Flags",
                                new XElement("FSXCompatibility", false)
                            ),
                            new XElement("AssetDir", "PackageSources\\SimObjects\\Airplanes\\livery-converter-2024\\"),
                            new XElement("outputDir", "SimObjects\\Airplanes\\livery-converter-2024\\")
                            )
                        )
                    )
                );
            xPkgDef.Save(path + "PackageDefinitions\\livery-converter-2024.xml");
            XAttribute[] projAttrs =
            {
                new XAttribute("Version", 2),
                new XAttribute("Name", "LiveryConverter2024"),
                new XAttribute("FolderName", "Packages"),
                new XAttribute("MetadataFolderName", "PackagesMetadata")
            };
            XDocument xProject = new XDocument(
                new XElement("Project", projAttrs,
                    new XElement("OutputDirectory", "."),
                    new XElement("TemporaryOutputDirectory", "_PackageInt"),
                    new XElement("Packages",
                        new XElement("Package", "PackageDefinitions\\livery-converter-2024.xml")
                    )
                )
            );
            xProject.Save(path + "livery-converter-2024.xml");
        }

        private async void ProcessHandler()
        {
            this.Dispatcher.Invoke((Action)(() => {
                Progress.Visibility = Visibility.Visible;
                Progress.IsIndeterminate = true;
            }));
            ExeClass exeClass = new();
            await exeClass.SpawnProc(path + "texconv.exe", "-r:keep " + path + "DDSINPUT\\*.png.dds -o " + path + "TEMP -f:rgba -ft:png", this);
            await exeClass.SpawnProc(path + "texconv.exe", "-r:keep " + path + "DDSINPUT\\*.tif.dds -o " + path + "TEMP -f:rgba -ft:tif", this);
            generateXMLs();
            string stearing = " ";
            if (Properties.Settings.Default.store == "Steam")
            {
                stearing = " -forceSteam ";
            }
            Dispatcher.Invoke(() =>
            {
                DebugConsole = "Spawning MSFS2024 Package Manager, Please Wait...\n";
            });

            await exeClass.SpawnProc(Properties.Settings.Default.sdkPath + "\\tools\\bin\\fspackagetool.exe", "-nopause -outputtoseparateconsole " + stearing + path + "livery-converter-2024.xml", this);
            await exeClass.ProcMon("FlightSimulator2024", this);
            this.Dispatcher.Invoke((Action)(() => {
                Progress.Visibility = Visibility.Hidden;
            }));
            Dispatcher.Invoke(() =>
            {
                DebugConsole = "Conversion Complete, tune in next week for the rest of the functionality!\n";
            });
        }


        private void button4_Click(object sender, RoutedEventArgs e)
        {
            //Prepare_Dirs();
            var fileDialog = new OpenFileDialog
            {
                Filter = "Direct Draw Surface files (*.DDS)|*.DDS|" + "All files (*.*)|*.*",
                Multiselect = true
            };
            if (fileDialog.ShowDialog() == true)
            {
                
                
                foreach (string f in fileDialog.FileNames)
                {
                    debug.AppendText(f + "\n");
                    debug.ScrollToEnd();
                    File.Copy(f, path + "DDSINPUT\\" + System.IO.Path.GetFileName(f));

                }
                ProcessHandler();
            }

        }
        private void p1_Exited(object sender, System.EventArgs e)
        {
            
        }
    }
}