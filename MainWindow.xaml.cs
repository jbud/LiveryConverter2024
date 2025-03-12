using Microsoft.Win32;
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
            prepare_Dirs();
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
        private readonly string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "com.budzique.livery-converter.app\\");
        private Process p1;
        private Process p2;
        private void clearPath(System.IO.DirectoryInfo directory)
        {
            foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
            foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }

        private void prepare_Dirs()
        {
            debug.AppendText("Creating Directories\n");
            debug.ScrollToEnd();
            System.IO.Directory.CreateDirectory(path);
            clearPath(new DirectoryInfo(path));
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(path, "ALBD\\"));
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(path, "COMP\\"));
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(path, "DECAL\\"));
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(path, "NORM\\"));
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(path, "DDSINPUT\\"));
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(path, "TEMP\\"));
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
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(path, "PackageSources\\SimObjects\\Airplanes\\png-2-ktx2\\common\\texture\\"));
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(path, "PackageDefinitions\\"));
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
                            new XElement("AssetDir", path + "PackageSources\\SimObjects\\Airplanes\\png-2-ktx2\\common\\texture\\")
                            new XElement("outputDir", "TODO")
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
                    new XElement("OutputDirectory", "TODO"),
                    new XElement("TemporaryOutputDirectory", "_PackageInt"),
                    new XElement("Packages", 
                        new XElement("Package", path+ "PackageDefinitions\\livery-converter-2024.xml")
                    )
                )
            );
            xProject.Save(path + "livery-converter-2024.xml");
            // spawn proc:
            //Properties.Settings.Default.sdkPath + "\tools\bin\fspackagetool.exe" -nopause -forcesteam -outputtoseparateconsole path + "livery-converter-2024.xml"
            /**
             * TODO:
             *  finish package folder generation and move textures. (_PackageInt, PackagesMetadata, Packages) ?
             *  spawn fspackagetool.exe from sdk and monitor for completion (mon proc FlightSimulator2024.exe?)
             *  move new textures to desired location: Properties.Settings.Default.texturePath
             *  run MSFSLayoutGenerator.exe
             * 
             * **/
        }
        private void button4_Click(object sender, RoutedEventArgs e)
        {
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
                using (p2 = new Process())
                {
                    p2.StartInfo.FileName = path + "texconv.exe";
                    p2.StartInfo.Arguments = "-r:keep " + path + "DDSINPUT\\*.tif.dds -o " + path + "TEMP -f:rgba -ft:tif";
                    p2.StartInfo.RedirectStandardOutput = true;
                    p2.StartInfo.CreateNoWindow = true;
                    p2.Start();
                    string output = p2.StandardOutput.ReadToEnd();
                    p2.WaitForExit();
                    debug.AppendText(output);
                    debug.ScrollToEnd();
                }
                using (p1 = new Process())
                {
                    p1.StartInfo.FileName = path + "texconv.exe"; 
                    p1.StartInfo.Arguments = "-r:keep " + path + "DDSINPUT\\*.png.dds -o " + path + "TEMP -f:rgba -ft:png"; //
                    p1.StartInfo.RedirectStandardOutput = true;
                    p1.EnableRaisingEvents = true;
                    p1.Exited += new EventHandler(p1_Exited);
                    p1.Start();
                    string output = p1.StandardOutput.ReadToEnd();
                    p1.WaitForExit();
                    debug.AppendText(output);
                    debug.ScrollToEnd();
                }
                DirectoryInfo d = new DirectoryInfo(path + "TEMP");
                FileInfo[] i = d.GetFiles();
                debug.AppendText("Sorting textures\n");
                debug.ScrollToEnd();
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
                        xmldoc.Save(path + "ALBD\\" + shortNewName + ".xml");
                        File.Move(file, path + "ALBD\\" + shortNewName);
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
                        xmldoc.Save(path + "COMP\\" + shortNewName + ".xml");
                        File.Move(file, path + "COMP\\" + shortNewName);
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
                        xmldoc.Save(path + "DECAL\\" + shortNewName + ".xml");
                        File.Move(file, path + "DECAL\\" + shortNewName);
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
                        xmldoc.Save(path + "NORM\\" + shortNewName + ".xml");
                        File.Move(file, path + "NORM\\" + shortNewName);
                    }
                }
            }

        }
        private void p1_Exited(object sender, System.EventArgs e)
        {
            
        }
    }
}