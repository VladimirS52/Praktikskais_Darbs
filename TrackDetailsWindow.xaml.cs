using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
using TagLib;

namespace Music_player
{
    /// <summary>
    /// Interaction logic for TrackDetailsWindow.xaml
    /// </summary>
    public partial class TrackDetailsWindow : Window
    {
        
        public TrackDetailsWindow(string filePath)
        {
            InitializeComponent();
            LoadTrackDetails(filePath);
        }
        private void LoadTrackDetails(string filePath)
        {
            try
            {
                var file = TagLib.File.Create(filePath);

                TitleTextBox.Text = file.Tag.Title ?? "Unknown Title";
                ArtistTextBox.Text = file.Tag.FirstPerformer ?? "Unknown Artist";
                AlbumTextBox.Text = file.Tag.Album ?? "Unknown Album";
                DurationTextBox.Text = file.Properties.Duration.ToString(@"mm\:ss");
                SizeTextBox.Text = new FileInfo(filePath).Length / 1024 + " KB";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading track details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
