using Microsoft.Win32;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
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
            PrepareDirs();
            projectFolder.Text = Properties.Settings.Default.projectPath;
            textureFolder.Text = Properties.Settings.Default.texturePath;
            sdkPath.Text = Properties.Settings.Default.sdkPath;
            layoutGenPath.Text = Properties.Settings.Default.layoutGenPath;
            string? t_cb = Properties.Settings.Default.store;
            if (Properties.Settings.Default.texturePath == "")
            {
                Dispatcher.Invoke(() =>
                {
                    uploadButton.IsEnabled = false;
                    uploadButton.ToolTip = "Texture Path is required for conversion. Please add a texture path.";
                });
            }
            comboBox.SelectedIndex = t_cb switch
            {
                "MS Store" => 1,
                _ => 0,
            };
        }

        public string DebugConsole
        {
            get { return debug.Text; }
            set { 
                debug.AppendText(value);
                debug.ScrollToEnd();
            }
        }

        public void ConsoleWriteLine(string line)
        {
            Dispatcher.Invoke(() =>
            {
                DebugConsole = line + "\n";
            });
        }

        private readonly string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "com.budzique.livery-converter.app\\");

        private void ClearPath(System.IO.DirectoryInfo directory)
        {
            foreach (System.IO.FileInfo file in directory.GetFiles()) 
            {
                if (!file.Name.ToString().Contains("texconv.exe"))
                { 
                    file.Delete();    
                }
            }
            foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(path, "PackageSources\\SimObjects\\Airplanes\\livery-converter-2024\\common\\texture\\"));
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(path, "PackageDefinitions\\"));
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(path, "DDSINPUT\\"));
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(path, "TEMP\\"));
        }

        private void PrepareDirs()
        {
            System.IO.Directory.CreateDirectory(path);

            ConsoleWriteLine("App directory created at");
            ConsoleWriteLine(path);

            ClearPath(new DirectoryInfo(path));

            ConsoleWriteLine("Creating project structure...");
            
            Assembly asm = Assembly.GetExecutingAssembly();
            string[] resources = asm.GetManifestResourceNames();

            foreach (string r in resources)
            {
                if (r.Contains("texconv.exe"))
                {
                    ConsoleWriteLine("Extracting texconv.exe from resources!");
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
                Dispatcher.Invoke(() =>
                {
                    uploadButton.IsEnabled = true;
                    uploadButton.ToolTip = "Ready to upload textures!";
                });
            }
        }

        private void textureFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.store = ((ComboBoxItem)comboBox.SelectedItem).Content.ToString(); 
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

        private void GenerateXMLs()
        {
            DirectoryInfo d = new DirectoryInfo(path + "TEMP");
            FileInfo[] i = d.GetFiles();

            ConsoleWriteLine("Sorting Textures... Generating XMLs...");
            
            string pkgSourceDir = path + "PackageSources\\SimObjects\\Airplanes\\livery-converter-2024\\common\\texture\\";

            foreach (FileInfo f in i)
            {
                string shortFileName = f.Name;
                string shortNewName = shortFileName.Substring(0, shortFileName.Length - 4);
                string file = f.FullName;
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
            bool error = false;
            Dispatcher.Invoke(() => {
                Progress.Visibility = Visibility.Visible;
                Progress.IsIndeterminate = true;
            });

            ExeClass exeClass = new ExeClass(this);
            await exeClass.SpawnProc(path + "texconv.exe", "-r:keep " + path + "DDSINPUT\\*.png.dds -o " + path + "TEMP -f:rgba -ft:png");
            await exeClass.SpawnProc(path + "texconv.exe", "-r:keep " + path + "DDSINPUT\\*.tif.dds -o " + path + "TEMP -f:rgba -ft:tif");
            
            GenerateXMLs();

            string stearing = " ";
            if (Properties.Settings.Default.store == "Steam")
            {
                stearing = " -forceSteam ";
            }

            ConsoleWriteLine("Spawning MSFS2024 Package Manager, please wait...");
            if (File.Exists(Properties.Settings.Default.sdkPath + "\\tools\\bin\\fspackagetool.exe"))
            {
                await exeClass.SpawnProc(Properties.Settings.Default.sdkPath + "\\tools\\bin\\fspackagetool.exe", "-nopause -outputtoseparateconsole " + stearing + path + "livery-converter-2024.xml");

                await exeClass.ProcMon("FlightSimulator2024");
            }
            else
            {
                string message = "fspackagetool.exe not found! check MSFS SDK 2024 Path! is it installed?";
                ConsoleWriteLine(message);
                MessageBox.Show(message, "Critical Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                error = true;
            }
            if (!error)
            {
                DirectoryInfo d = new DirectoryInfo(path + "Packages\\livery-converter-2024\\SimObjects\\Airplanes\\livery-converter-2024\\common\\texture");
                FileInfo[] i = d.GetFiles();

                ConsoleWriteLine("Moving textures to project path...");

                string pkgSourceDir = path + "PackageSources\\SimObjects\\Airplanes\\livery-converter-2024\\common\\texture\\";

                foreach (FileInfo f in i)
                {
                    string target = Properties.Settings.Default.texturePath + "\\" + f.Name;
                    if (File.Exists(target))
                    {
                        ConsoleWriteLine(target + " Already exists! skipping...");
                    }
                    else
                    {
                        File.Copy(f.FullName, target);
                    }
                }
                if (File.Exists(Properties.Settings.Default.projectPath + "\\layout.json"))
                {
                    ConsoleWriteLine("layout.json found! running MSFSLayoutGenerator.exe...");
                    if (File.Exists(Properties.Settings.Default.layoutGenPath + "\\MSFSLayoutGenerator.exe"))
                    {
                        await exeClass.SpawnProc(Properties.Settings.Default.layoutGenPath + "\\MSFSLayoutGenerator.exe", Properties.Settings.Default.projectPath + "\\layout.json");
                    }
                    else
                    {
                        ConsoleWriteLine("Error: MSFSLayoutGenerator.exe not found! Layout not updated...");
                        ConsoleWriteLine("Converted textures can be found here: " + Properties.Settings.Default.texturePath);
                    }
                }
                else
                {
                    ConsoleWriteLine("Unable to locate layout.json in Project Path");
                    ConsoleWriteLine("Converted textures can be found here: " + Properties.Settings.Default.texturePath);
                }
                ConsoleWriteLine("Conversion Complete!");
            } 
            else
            {
                ConsoleWriteLine("Conversion failed due to critical error!");
            }
            Dispatcher.Invoke(() =>
            {
                Progress.Visibility = Visibility.Hidden;
                Progress.IsIndeterminate = false;
            });
            
        }


        private void button4_Click(object sender, RoutedEventArgs e)
        {
            ClearPath(new DirectoryInfo(path)); // cleanup work directory here again (in-case app isn't closed before next use).
            
            var fileDialog = new OpenFileDialog
            {
                Filter = "Direct Draw Surface files (*.DDS)|*.DDS",
                Multiselect = true
            };
            if (fileDialog.ShowDialog() == true)
            {
                foreach (string f in fileDialog.FileNames)
                {
                    ConsoleWriteLine("Uploading textures to project...");
                    File.Copy(f, path + "DDSINPUT\\" + System.IO.Path.GetFileName(f));
                }
                ProcessHandler();
            }
        }

        private void uploadButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {

        }
    }
}