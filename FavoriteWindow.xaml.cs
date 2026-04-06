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

namespace Music_player
{
    /// <summary>
    /// Interaction logic for Favorite.xaml
    /// </summary>
    public partial class FavoriteWindow : Window
    {
        
        public FavoriteWindow(List<string> favoriteTracks)
        {
            InitializeComponent();
            FavoriteTracksListBox.ItemsSource = favoriteTracks;
        }

        

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Закрываем окно
        }
    }
}
