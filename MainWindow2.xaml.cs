using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LiveryConverter2024
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
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
        public string? cwd20;

        private void LiveryPathButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog folderDialog = new();
            if (folderDialog.ShowDialog() == true)
            {
                string? folderName = folderDialog.FolderName;
                LiveryPath.Text = folderName;
                cwd20 = folderName;
            }
        }

        private void SimVersion_Changed(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.store = ((ComboBoxItem)comboBox.SelectedItem).Content.ToString();
            Properties.Settings.Default.Save();
            if (((ComboBoxItem)comboBox.SelectedItem).Content.ToString() == Properties.Settings.Default.store)
            {
                labelValidation6.Visibility = Visibility.Visible;
            }
        }
    }
}
