using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace LiveryConverter2024
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow_old : Window
    {
        public MainWindow_old()
        {
            InitializeComponent();
            lc24 = new LC24(new MainWindow(), path);
            lc24.PrepareDirs();
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
        private readonly string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "com.budzique.livery-converter.app\\");

        private readonly LC24 lc24;

        public bool GeneralError = false;

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

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            lc24.ClearPath(new DirectoryInfo(path)); // cleanup work directory here again (in-case app isn't closed before next use).
            GeneralError = false;
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
                lc24.ProcessHandler(isJsonAvail);
            }
        }

        private void uploadButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {

        }
    }
}