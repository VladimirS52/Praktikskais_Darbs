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
using Microsoft.Win32;

namespace Music_player
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public string SelectedTheme { get; private set; } = "Light";
        public string SelectedLanguage { get; private set; } = "English";
        public string SavePath { get; private set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public int SelectedFontSize { get; private set; } = 14;
        public string SelectedFontFamily { get; private set; } = "Arial";
        public SettingsWindow()
        {
            InitializeComponent();
            ThemeComboBox.SelectedIndex = 0; // Light
            LanguageComboBox.SelectedIndex = 0; // English
            SavePathTextBox.Text = SavePath;
        }
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            
            var dialog = new SaveFileDialog
            {
                Title = "Select Save Path",
                Filter = "All Files|*.*",
                FileName = "playlist.txt"
            };

            if (dialog.ShowDialog() == true)
            {
                SavePath = dialog.FileName;
                SavePathTextBox.Text = SavePath;
            }
        }

        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            
            var selectedThemeItem = ThemeComboBox.SelectedItem as ComboBoxItem;
            if (selectedThemeItem != null)
            {
                SelectedTheme = selectedThemeItem.Content.ToString();
            }

            
            var selectedLanguageItem = LanguageComboBox.SelectedItem as ComboBoxItem;
            if (selectedLanguageItem != null)
            {
                SelectedLanguage = selectedLanguageItem.Content.ToString();
            }

            
            MessageBox.Show($"Settings saved:\nTheme: {SelectedTheme}\nLanguage: {SelectedLanguage}\nSave Path: {SavePath}",
                            "Settings Saved", MessageBoxButton.OK, MessageBoxImage.Information);

            
            this.Close();

            
            var fontSizeItem = FontSizeComboBox.SelectedItem as ComboBoxItem;
            if (fontSizeItem != null)
            {
                SelectedFontSize = int.Parse(fontSizeItem.Content.ToString());
            }

            
            var fontFamilyItem = FontFamilyComboBox.SelectedItem as ComboBoxItem;
            if (fontFamilyItem != null)
            {
                SelectedFontFamily = fontFamilyItem.Content.ToString();
            }
        }
        private void ResetToDefaultButton_Click(object sender, RoutedEventArgs e)
        {
           
            ThemeComboBox.SelectedIndex = 0; // Light
            LanguageComboBox.SelectedIndex = 0; // English
            FontSizeComboBox.SelectedIndex = 1; // 14
            FontFamilyComboBox.SelectedIndex = 0; // Arial
            SavePathTextBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            MessageBox.Show("Settings have been reset to default.", "Reset Settings", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
    }
}
