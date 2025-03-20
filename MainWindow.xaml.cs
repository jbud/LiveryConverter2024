using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace LiveryConverter2024
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly string version = "0.1.99";
        public MainWindow()
        {
            InitializeComponent();

            ConsoleWriteLine("LiveryConverter2024 Version " + version);
            ConsoleWriteLine("By Budzique");
            lc24 = new LC24(this, path);
            lc24.PrepareDirs();
            SDKPath.Text = Properties.Settings.Default.sdkPath;
            ValidateSDK(Properties.Settings.Default.sdkPath);
            LGPath.Text = Properties.Settings.Default.layoutGenPath;
            ValidateLG(Properties.Settings.Default.layoutGenPath);
            string? t_cb = Properties.Settings.Default.store;
            comboBox.SelectedIndex = t_cb switch
            {
                "MS Store" => 1,
                _ => 0,
            };

        }

        private readonly string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "com.budzique.livery-converter.app\\");

        private readonly LC24 lc24;

        public string DebugConsole
        {
            get { return debug.Text; }
            set
            {
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

        public string? cwd20;
        public string? layout24;
        public string? texpath24;
        private bool sdkValid = true;
        private bool lgValid = true;
        public bool GeneralError = false;

        private void CheckEnableConvertButton()
        {
            if(cwd20 != null && layout24 != null && texpath24 != null && sdkValid && lgValid)
            {
                button1.IsEnabled = true;
            }
            else
            {
                string tt = "Convert Disabled: Unknown Error..aliens..";
                if (cwd20 == null)
                {
                    tt = "Convert Disabled: Please check 2020 Livery Path";
                } else
                if (layout24 == null)
                {
                    tt = "Convert Disabled: Please check 2024 layout.json";
                } else 
                if (texpath24 == null)
                {
                    tt = "Convert Disabled: Please check 2024 Texture Out Path";
                } else
                if (!sdkValid)
                {
                    tt = "Convert Disabled: Please check SDK path";
                } else
                if (!lgValid)
                {
                    tt = "Convert Disabled: Please check Layout Generator Path";
                }
                button1.ToolTip = tt;
                button1.IsEnabled = false;
            }
        }

        private void LabelValidation(Label labelname, string text = "*", bool error = false, bool hide = false)
        {
            Brush color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C51FFF00"));
            if (error)
            {
                color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C5FF1F00"));
            }
            Dispatcher.Invoke(() => 
            {
                if (text != "*")
                {
                    labelname.Content = text;
                }
                if (hide)
                {
                    labelname.Visibility = Visibility.Hidden;
                }
                else
                {
                    labelname.Visibility = Visibility.Visible;
                }
                labelname.Foreground = color;
            });
        }

        private async Task LiveryValidate(string folderName) 
        {
            await Task.Run(() =>
            {
                try
                {
                    IEnumerable<string>? ddsFiles = Directory.EnumerateFiles(folderName, "*.DDS", SearchOption.AllDirectories);
                    string fullpath = ddsFiles.First().ToString();
                    string dirname = System.IO.Path.GetDirectoryName(fullpath)!;
                    if (dirname != null)
                    {
                        cwd20 = dirname;
                        Dispatcher.Invoke(() =>
                        {
                            LabelValidation(labelValidation1, "Found Texture files successfully!");
                            ConsoleWriteLine("Found Texture files successfully!");
                            ConsoleWriteLine("Using: "+cwd20);
                        });
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        ConsoleWriteLine("Unable to find textures!");
                        ConsoleWriteLine(ex.Message);
                        LabelValidation(labelValidation1, "Unable to find textures!", true);
                    });
                }
            });
        }

        private void LiveryPathButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog folderDialog = new OpenFolderDialog();
            if (folderDialog.ShowDialog() == true)
            {
                string? folderName = folderDialog.FolderName;
                LiveryPath.Text = folderName;
                _ = LiveryValidate(folderName);
                CheckEnableConvertButton();
            }
        }

        private void SimVersion_Changed(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.store = ((ComboBoxItem)comboBox.SelectedItem).Content.ToString();
            Properties.Settings.Default.Save();
            if (((ComboBoxItem)comboBox.SelectedItem).Content.ToString() == Properties.Settings.Default.store)
            {
                LabelValidation(labelValidation6);
            }
            else
            {
                LabelValidation(labelValidation6, "*", false, true);
            }
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("LiveryConverter2024 Version "+version, "About", System.Windows.MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DebugOpenButton_Click(object sender, RoutedEventArgs e)
        {
            string t = DateTime.Now.ToString(@"MM-dd-yy-HH-mm");
            string log = path + "LC24-" + t + ".log";
            File.WriteAllText(log, DebugConsole);
            Uri uri = new Uri(log);
            string converted = uri.AbsoluteUri;
            ProcessStartInfo p = new ProcessStartInfo(converted)
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(p);
        }

        private void LayoutPathButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog() 
            { 
                Filter = "JSON files (*.json)|*.json"
            };
            if (fileDialog.ShowDialog() == true)
            {
                LayoutPath.Text = fileDialog.FileName;
                if (fileDialog.FileName.Contains("layout.json"))
                {
                    LabelValidation(labelValidation2, "Found layout.json!");
                    ConsoleWriteLine("Found layout.json!");
                    layout24 = fileDialog.FileName;
                }
                else
                {
                    LabelValidation(labelValidation2, "Invalid json file please try again!", true);
                    ConsoleWriteLine("Invalid json file please try again!");
                }
                CheckEnableConvertButton();
            }
        }

        private void TexturePathButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog folderDialog = new OpenFolderDialog();
            if (folderDialog.ShowDialog() == true)
            {
                string? folderName = folderDialog.FolderName;
                TexturePath.Text = folderName;
                texpath24 = folderName;
                if (layout24 != null)
                {
                    if (folderName.Contains(System.IO.Path.GetDirectoryName(layout24)!))
                    {
                        LabelValidation(labelValidation3, "Texture directory inside layout!");
                        ConsoleWriteLine("Texture directory inside layout!");
                    }
                    else
                    {
                        LabelValidation(labelValidation3, "Texture directory outside layout!", true);
                        ConsoleWriteLine("WARN: Texture directory inside layout! ok to continue, layoutgenerator will have no effect.");
                    }
                }
                CheckEnableConvertButton();
            }
            
        }

        private void DebugCopyButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(debug.Text);

            DebugCopyButton.Icon = new SymbolIcon(SymbolRegular.Checkmark20, 30, true);
            Task.Run(()=> {
                Task delay = Task.Delay(1500);
                delay.Wait();

                Dispatcher.Invoke(() => 
                {
                    DebugCopyButton.Icon = new SymbolIcon(SymbolRegular.ClipboardPaste20, 30, true);
                });
            });
        }

        private void ValidateSDK(string dir)
        {
            if (File.Exists(dir + "\\tools\\bin\\fspackagetool.exe"))
            {
                lgValid = true;
                LabelValidation(labelValidation4, "Found SDK!");
                ConsoleWriteLine("Found SDK!");
            }
            else
            {
                lgValid = false;
                ConsoleWriteLine("Unable to find SDK, Check settings...");
                LabelValidation(labelValidation4, "Unable to find SDK...", true);
            }
        }

        private void ValidateLG(string dir)
        {
            if (File.Exists(dir + "\\MSFSLayoutGenerator.exe"))
            {
                lgValid = true;
                LabelValidation(labelValidation5, "Found Layout Generator!");
                ConsoleWriteLine("Found Layout Generator!");
            }
            else
            {
                lgValid = false;
                ConsoleWriteLine("Unable to find LayoutGenerator, Check settings...");
                LabelValidation(labelValidation5, "Unable to find MSFSLayoutGenerator.exe...", true);
            }
        }

        private void SDKPathButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog folderDialog = new OpenFolderDialog();

            if (folderDialog.ShowDialog() == true)
            {
                string? folderName = folderDialog.FolderName;
                SDKPath.Text = folderName;
                Properties.Settings.Default.sdkPath = folderName;
                Properties.Settings.Default.Save();
                ValidateSDK(folderName);
            }
        }

        private void LGPathButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog folderDialog = new OpenFolderDialog();

            if (folderDialog.ShowDialog() == true)
            {
                string? folderName = folderDialog.FolderName;
                LGPath.Text = folderName;
                Properties.Settings.Default.layoutGenPath = folderName;
                Properties.Settings.Default.Save();
                ValidateLG(folderName);
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {

            lc24.ClearPath(new DirectoryInfo(path)); // cleanup work directory here again (in-case app isn't closed before next use).
            GeneralError = false;
            bool isJsonAvail = false;
            string[] files = Directory.GetFiles(cwd20!, "*.DDS");
            foreach (string f in files)
            {
                ConsoleWriteLine("Uploading textures to project...");
                File.Copy(f, path + "DDSINPUT\\" + System.IO.Path.GetFileName(f));
                // Grab json files for typing if exists
                if (File.Exists(f + ".json"))
                {
                    File.Copy(f + ".json", path + "DDSINPUT\\" + System.IO.Path.GetFileName(f) + ".json");
                    isJsonAvail = true;
                }
            }
            lc24.ProcessHandler(isJsonAvail);
        }
    }
}
