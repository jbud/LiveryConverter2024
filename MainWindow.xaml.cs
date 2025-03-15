using Microsoft.Win32;
using Newtonsoft.Json;
using System.IO;
using System.IO.Packaging;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

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

        class TextureType
        {
            public static string ALBD = "MTL_BITMAP_DECAL0";
            public static string COMP = "MTL_BITMAP_METAL_ROUGH_AO";
            public static string NORM = "MTL_BITMAP_NORMAL";
            // DECAL skipped, it follows ALBD
        };

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
                if (!file.Name.ToString().Contains("texconv.exe") && !file.Name.ToString().Contains(".log"))
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

        private void GenerateXMLs(bool isJsonAvail = false)
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
                bool jsonFailure = false;

                File.Move(file, pkgSourceDir + shortNewName);
                
                if (isJsonAvail) 
                {
                    string type = UnpackJson(path + "DDSINPUT\\" + shortNewName + ".DDS.json"); // Infer original name due to timing happening after conversion to PNG.
                    switch (type)
                    {
                        case "NORM":
                            GenerateSingleXML(pkgSourceDir, shortNewName + ".xml", TextureType.NORM);
                            break;
                        case "COMP":
                            GenerateSingleXML(pkgSourceDir, shortNewName + ".xml", TextureType.COMP);
                            break;
                        case "ALBD":
                        case "DECAL":
                            GenerateSingleXML(pkgSourceDir, shortNewName + ".xml", TextureType.ALBD);// Use ALBD settings for DECAL
                            break;
                        case "FAIL":
                        default:
                            jsonFailure = true;
                            break;
                    }
                }

                if (!isJsonAvail || jsonFailure) // fallback to filename check if json fails...
                {
                    if (file.Contains("ALBD") || file.Contains("DECAL"))
                    {
                        GenerateSingleXML(pkgSourceDir, shortNewName + ".xml", TextureType.ALBD);// Use ALBD settings for DECAL
                    }
                    else
                    if (file.Contains("COMP"))
                    {
                        GenerateSingleXML(pkgSourceDir, shortNewName + ".xml", TextureType.COMP);
                    }
                    else
                    if (file.Contains("NORM"))
                    {
                        GenerateSingleXML(pkgSourceDir, shortNewName + ".xml", TextureType.NORM);
                    }
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

        private void GenerateSingleXML(string dir, string file, string tex)
        {
            XDocument xmldoc = new XDocument(
                new XElement("BitmapConfiguration",
                    new XElement("BitmapSlot", "MTL_BITMAP_DECAL0"),
                    new XElement("UserFlags", new XAttribute("type", "_DEFAULT"), "QUALITYHIGH")
                )
            );
            if (tex == TextureType.COMP)
            {
                xmldoc.Descendants("BitmapConfiguration").First().Add(new XElement("ForceNoAlpha", true));
            }
            xmldoc.Save(dir + file);
        }

        private string UnpackJson(string filename)
        {
            try
            {
                ConsoleWriteLine("Scanning included DDS.json file...");
                string fileContents = File.ReadAllText(filename);
                dynamic stuff = JsonConvert.DeserializeObject(fileContents);
                string[] flags = stuff.Flags.ToObject<string[]>();
                foreach (string f in flags)
                {
                    ConsoleWriteLine("Found flag: " + f + "!");
                    switch (f)
                    {
                        case "FL_BITMAP_METAL_ROUGH_AO_DATA":
                            ConsoleWriteLine("Inferring Type COMP!");
                            return TextureType.COMP;
                        case "FL_BITMAP_TANGENT_DXT5N":
                            ConsoleWriteLine("Inferring Type NORM!");
                            return TextureType.NORM;
                    }
                }
                ConsoleWriteLine("Inferring Type ALBD!");
                return TextureType.ALBD;
            }
            catch (Exception err)
            {
                ConsoleWriteLine(err.Message);
                return "FAIL";
            }
        }

        private async void ProcessHandler(bool isJsonAvail = false)
        {
            bool error = false;
            Dispatcher.Invoke(() => {
                Progress.Visibility = Visibility.Visible;
                Progress.IsIndeterminate = true;
            });

            ExeClass exeClass = new ExeClass(this);
            await exeClass.SpawnProc(path + "texconv.exe", "-r:keep " + path + "DDSINPUT\\*.png.dds -o " + path + "TEMP -f:rgba -ft:png");
            await exeClass.SpawnProc(path + "texconv.exe", "-r:keep " + path + "DDSINPUT\\*.tif.dds -o " + path + "TEMP -f:rgba -ft:tif");
            if (isJsonAvail)
            {
                // TODO special handling
                GenerateXMLs(true);
            }
            else
            {
                GenerateXMLs();
            }
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
                if (File.Exists(path + "Packages\\livery-converter-2024\\manifest.json"))
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
                    ConsoleWriteLine("Unspecified Error: fspackagetool.exe Failed...");
                    ConsoleWriteLine("Logs from fspackagetool can be found in the same directory as FlightSimulator2024.exe under the name: BuilderLogError.txt");
                    //string logpath = "";
                    //if (Properties.Settings.Default.store == "Steam")
                    //{

                    //}
                    //else
                    //{
                    //    logpath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Packages\\Microsoft.Limitless_8wekyb3d8bbwe\\LocalCache\\BuilderLogError.txt";
                    //}
                }
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
            string t = DateTime.Now.ToString(@"MM-dd-yyyy-h-mm-tt");
            File.WriteAllText(path + "LC24-" + t + ".log", DebugConsole);

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
                bool isJsonAvail = false;
                foreach (string f in fileDialog.FileNames)
                {
                    ConsoleWriteLine("Uploading textures to project...");
                    File.Copy(f, path + "DDSINPUT\\" + System.IO.Path.GetFileName(f));
                    // Grab json files for typing if exists
                    if (File.Exists(f + ".json")) 
                    {
                        File.Copy(f+".json", path + "DDSINPUT\\" + System.IO.Path.GetFileName(f) + ".json");
                        isJsonAvail = true;
                    }
                }
                ProcessHandler(isJsonAvail);
            }
        }

        private void uploadButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {

        }
    }
}