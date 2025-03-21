using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml.Linq;

namespace LiveryConverter2024
{
    internal class LC24(MainWindow mainWindowRef, string path)
    {
        public MainWindow mainWindowRef = mainWindowRef;
        public string path = path;

        public class TextureType
        {
            public static string ALBD = "MTL_BITMAP_DECAL0";
            public static string COMP = "MTL_BITMAP_METAL_ROUGH_AO";
            public static string NORM = "MTL_BITMAP_NORMAL";
            // DECAL skipped, it follows ALBD
        };

        /// <summary>
        /// Method <c>ConsoleWriteLine</c> Writes a string to debug console in 
        /// MainWindow followed by a new line
        /// </summary>
        /// <param name="line">the line to write</param>
        private void ConsoleWriteLine(string line)
        {
            mainWindowRef.Dispatcher.Invoke(() =>
            {
                mainWindowRef.DebugConsole = line + "\n";
            });
        }

        /// <summary>
        /// Method <c>ClearPath</c> deletes all project files...
        /// </summary>
        /// <param name="directory">Project Directory</param>
        public void ClearPath(System.IO.DirectoryInfo directory)
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

        /// <summary>
        /// Method <c>PrepareDirs</c> Creates the main project structure, and extracts embedded resources.
        /// </summary>
        public void PrepareDirs()
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

        /// <summary>
        /// Method <c>ProcessHandler</c> runs all external processes and manages project files
        /// for the conversion.
        /// </summary>
        /// <param name="isJsonAvail">Did upload find json files? (default = false)</param>
        public async void ProcessHandler(bool isJsonAvail = false)
        {
            bool error = false;
            mainWindowRef.Dispatcher.Invoke(() => {
                mainWindowRef.Progress.Visibility = Visibility.Visible;
                mainWindowRef.Progress.IsIndeterminate = true;
            });

            ExeClass exeClass = new ExeClass(mainWindowRef);
            await exeClass.SpawnProc(path + "texconv.exe", "-r:keep " + path + "DDSINPUT\\*.png.dds -o " + path + "TEMP -f:rgba -ft:png");
            await exeClass.SpawnProc(path + "texconv.exe", "-r:keep " + path + "DDSINPUT\\*.tif.dds -o " + path + "TEMP -f:rgba -ft:tif");
            if (isJsonAvail)
            {
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
                mainWindowRef.GeneralError = true;
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
                        string target = mainWindowRef.texpath24 + "\\" + f.Name;
                        if (File.Exists(target))
                        {
                            ConsoleWriteLine(target + " Already exists! skipping...");
                        }
                        else
                        {
                            File.Copy(f.FullName, target);
                        }
                    }
                    if (File.Exists(mainWindowRef.layout24))
                    {
                        ConsoleWriteLine("layout.json found! running MSFSLayoutGenerator.exe...");
                        if (File.Exists(Properties.Settings.Default.layoutGenPath + "\\MSFSLayoutGenerator.exe"))
                        {
                            await exeClass.SpawnProc(Properties.Settings.Default.layoutGenPath + "\\MSFSLayoutGenerator.exe", "\"" + mainWindowRef.layout24 + "\"", true);
                        }
                        else
                        {
                            ConsoleWriteLine("Error: MSFSLayoutGenerator.exe not found! Layout not updated...");
                            ConsoleWriteLine("Converted textures can be found here: " + mainWindowRef.texpath24);
                            mainWindowRef.GeneralError = true;
                        }
                    }
                    else
                    {
                        ConsoleWriteLine("Unable to locate layout.json in Project Path");
                        ConsoleWriteLine("Converted textures can be found here: " + mainWindowRef.texpath24);
                        mainWindowRef.GeneralError = true;
                    }
                    if (mainWindowRef.GeneralError)
                    {
                        ConsoleWriteLine("Conversion Complete with errors!");
                    }
                    else
                    {
                        ConsoleWriteLine("Conversion Complete!");
                    }
                }
                else
                {
                    mainWindowRef.GeneralError = true;
                    ConsoleWriteLine("Unspecified Error: fspackagetool.exe Failed...");
                    
                    string logPath = "";
                    if (Properties.Settings.Default.store == "Steam")
                    {
                        logPath = "\\BuilderLogError.txt"; // TODO Default Steam path.
                    }
                    else
                    {
                        logPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Packages\\Microsoft.Limitless_8wekyb3d8bbwe\\LocalCache\\BuilderLogError.txt";
                    }
                    ConsoleWriteLine(logPath);
                    if (File.Exists(logPath))
                    {
                        ConsoleWriteLine("Attempt to read fspackagetool log...\n");
                        ConsoleWriteLine("#####################################\n");
                        // TODO check GetLastWriteTime() against current session
                        foreach (string line in File.ReadLines(logPath))
                        {
                            ConsoleWriteLine(line);
                        }
                        try
                        {
                            File.Delete(logPath); // Remove the log.
                        }
                        catch (Exception e)
                        {
                            ConsoleWriteLine("Unable to remove fspackagetool.exe's log, this may cause larger logs in the future.");
                            ConsoleWriteLine(e.Message);
                        }
                    }
                    else
                    {
                        ConsoleWriteLine("Unable to locate BuilderLogError.txt");
                    }
                }
            }
            else
            {
                ConsoleWriteLine("Conversion failed due to critical error!");
            }
            mainWindowRef.Dispatcher.Invoke(() =>
            {
                mainWindowRef.Progress.Visibility = Visibility.Hidden;
                mainWindowRef.Progress.IsIndeterminate = false;
            });
            string t = DateTime.Now.ToString(@"MM-dd-yy-HH-mm");
            string log = path + "LC24-" + t + ".log";
            File.WriteAllText(log, mainWindowRef.DebugConsole);
            if (mainWindowRef.GeneralError)
            {
                Uri uri = new Uri(log);
                string converted = uri.AbsoluteUri;
                if (MessageBox.Show("Do you want to view the log file?",
                        "An error occurred... Open log?",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    ProcessStartInfo p = new ProcessStartInfo(converted)
                    {
                        UseShellExecute = true,
                        Verb = "open"
                    };
                    Process.Start(p);
                }
            }
        }

        /// <summary>
        /// Method <c>UnpackJson</c> parses JSON file found associated with DDS file
        /// for texture type and returns it as a simple string.
        /// </summary>
        /// <param name="filename">path/to/file.dds.json</param>
        /// <returns>String representing texture type or "FAIL"</returns>
        private string UnpackJson(string filename)
        {
            try
            {
                ConsoleWriteLine("Scanning included DDS.json file: ");
                ConsoleWriteLine(filename + " ...");
                string fileContents = File.ReadAllText(filename);
#pragma warning disable CS8602
#pragma warning disable CS8600
                dynamic stuff = JsonConvert.DeserializeObject(fileContents);
                string[] flags = stuff.Flags.ToObject<string[]>();
#pragma warning restore
                foreach (string f in flags)
                {
                    ConsoleWriteLine("Found flag: " + f + "!");
                    switch (f)
                    {
                        case "FL_BITMAP_METAL_ROUGH_AO_DATA":
                            ConsoleWriteLine("Inferring Type COMP!");
                            return "COMP";
                        case "FL_BITMAP_TANGENT_DXT5N":
                            ConsoleWriteLine("Inferring Type NORM!");
                            return "NORM";
                    }
                }
                ConsoleWriteLine("Inferring Type ALBD!");
                return "ALBD";
            }
            catch (Exception err)
            {
                ConsoleWriteLine(err.Message);
                return "FAIL";
            }
        }

        /// <summary>
        /// Method <c>GenerateSingleXML</c> generates the project XML file for the given texture
        /// </summary>
        /// <param name="dir">Path to save the XML</param>
        /// <param name="file">Name of the XML file</param>
        /// <param name="tex">Texture Format for BitmapSlot</param>
        private void GenerateSingleXML(string dir, string file, string tex)
        {
            XDocument xmldoc = new XDocument(
                new XElement("BitmapConfiguration",
                    new XElement("BitmapSlot", tex),
                    new XElement("UserFlags", new XAttribute("type", "_DEFAULT"), "QUALITYHIGH")
                )
            );
            if (tex == TextureType.COMP)
            {
                xmldoc.Descendants("BitmapConfiguration").First().Add(new XElement("ForceNoAlpha", true));
            }
            xmldoc.Save(dir + file);
        }

        /// <summary>
        /// Method <c>GenerateXMLs</c> handles moving and renaming textures as-well as 
        /// generating project XML files.
        /// </summary>
        /// <param name="isJsonAvail">Did upload find json files? (default = false)</param>
        private void GenerateXMLs(bool isJsonAvail = false)
        {
            DirectoryInfo d = new DirectoryInfo(path + "TEMP");
            FileInfo[] i = d.GetFiles();

            ConsoleWriteLine("Sorting Textures... Generating XMLs...");

            string pkgSourceDir = path + "PackageSources\\SimObjects\\Airplanes\\livery-converter-2024\\common\\texture\\";

            foreach (FileInfo f in i)
            {
                string shortFileName = f.Name;
                string shortNewName = shortFileName[..^4];
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
    }
}
