using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LiveryConverter2024
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public readonly string version = "0.1.99";
        public Window1()
        {
            InitializeComponent();
            string? t_cb = Properties.Settings.Default.store;
            comboBox.SelectedIndex = t_cb switch
            {
                "MS Store" => 1,
                _ => 0,
            };
        }

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

        private void LiveryPathButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog folderDialog = new OpenFolderDialog();
            if (folderDialog.ShowDialog() == true)
            {
                string? folderName = folderDialog.FolderName;
                LiveryPath.Text = folderName;
                try
                {
                    IEnumerable<string>? ddsFiles = Directory.EnumerateFiles(folderName, "*.DDS", SearchOption.AllDirectories);
                    string fullpath = ddsFiles.First().ToString();
                    string dirname = Path.GetDirectoryName(fullpath)!;
                    if (dirname != null)
                    {
                        cwd20 = dirname;
                        LabelValidation(labelValidation1, "Found Texture files successfully!");
                        ConsoleWriteLine("Found Texture files successfully!");
                        ConsoleWriteLine(cwd20);
                    }
                }
                catch (Exception ex)
                {
                    ConsoleWriteLine("Unable to find textures!");
                    ConsoleWriteLine(ex.Message);
                    LabelValidation(labelValidation1, "Unable to find textures!", true);
                }
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
            MessageBox.Show("LiveryConverter2024 Version "+version);
        }

        private void DebugOpenButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Clicked!");
        }
    }
}
