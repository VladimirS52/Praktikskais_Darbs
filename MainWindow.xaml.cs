using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
using System.Configuration;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using Newtonsoft.Json;
using System.Diagnostics;


namespace Music_player
{
    public class PlaylistData
    {
        public required string Name { get; set; }
        public required List<string> Tracks { get; set; }
    }
    public partial class MainWindow : Window
    {
        private Dictionary<string, List<string>> playlists = new Dictionary<string, List<string>>();
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private DispatcherTimer timer = new DispatcherTimer();
        private int currentTrackIndex = 0;
        private bool isDraggingSlider = false;
        private double previousVolume = 0.5;
        private bool isMuted = false;
        private List<string> favoriteTracks = new List<string>();
        private SpotifyService spotifyService = new SpotifyService();
        private DispatcherTimer _dateTimeTimer;



        public MainWindow()
        {
            InitializeComponent();    
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            LoadPlaylists();
            LoadSettings();
            AuthenticateSpotify();

            _dateTimeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1) 
            };
            _dateTimeTimer.Tick += UpdateDateTime;


            mediaPlayer.Volume = VolumeSlider.Value;

            foreach (var child in LogicalTreeHelper.GetChildren(this))
            {
                if (child is Button button)
                {
                    AddHoverAnimation(button);
                }
            }

            Style buttonStyle = new Style(typeof(Button));
            buttonStyle.Setters.Add(new Setter(Button.MarginProperty, new Thickness(5)));
            buttonStyle.Setters.Add(new Setter(Button.FontSizeProperty, 14.0));
            buttonStyle.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(60, 63, 65))));
            buttonStyle.Setters.Add(new Setter(Button.ForegroundProperty, Brushes.White));
            buttonStyle.Setters.Add(new Setter(Button.ForegroundProperty, Brushes.White));
            
            Style textBlockStyle = new Style(typeof(TextBlock));
            textBlockStyle.Setters.Add(new Setter(TextBlock.FontSizeProperty, 14.0));
            textBlockStyle.Setters.Add(new Setter(TextBlock.MarginProperty, new Thickness(5)));
            textBlockStyle.Setters.Add(new Setter(TextBlock.ForegroundProperty, Brushes.White));

            
            this.Resources.Add(typeof(Button), buttonStyle);
            this.Resources.Add(typeof(TextBlock), textBlockStyle);

            
            ApplyStyleToElements<Button>(this, buttonStyle);
            
            ApplyStyleToElements<TextBlock>(this, textBlockStyle);

        }
        private void ApplyStyleToElements<T>(DependencyObject parent, Style style) where T : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T element)
                {
                    element.Style = style;
                }
                ApplyStyleToElements<T>(child, style);
            }
        }
        private void AddHoverAnimation(Button button)
        {
            
            ScaleTransform scaleTransform = new ScaleTransform(1.0, 1.0);
            button.RenderTransform = scaleTransform;
            button.RenderTransformOrigin = new Point(0.5, 0.5);

            
            var growAnimation = new DoubleAnimation(1.0, 1.1, TimeSpan.FromMilliseconds(200))
            {
                AutoReverse = true
            };

            
            button.MouseEnter += (s, e) => scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, growAnimation);
            button.MouseEnter += (s, e) => scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, growAnimation);
        }
        private void AddListBoxItemHoverAnimation(ListBox listBox)
        {
            listBox.MouseMove += (s, e) =>
            {
                if (e.OriginalSource is ListBoxItem item)
                {
                    ColorAnimation colorAnimation = new ColorAnimation
                    {
                        To = Colors.Gold,
                        Duration = TimeSpan.FromMilliseconds(300),
                        AutoReverse = true
                    };

                    SolidColorBrush brush = new SolidColorBrush(Colors.White);
                    item.Background = brush;

                    brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
                }
            };
        }
        private void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (TracksListBox.SelectedItem != null)
            {
                string selectedTrack = TracksListBox.SelectedItem.ToString();

                
                if (!favoriteTracks.Contains(selectedTrack))
                {
                    favoriteTracks.Add(selectedTrack);
                    MessageBox.Show($"Трек \"{selectedTrack}\" добавлен в избранное!", "Избранное", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Трек \"{selectedTrack}\" уже в избранном.", "Избранное", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите трек для добавления в избранное.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ViewFavoritesButton_Click(object sender, RoutedEventArgs e)
        {
            if (favoriteTracks.Count == 0)
            {
                MessageBox.Show("Список избранного пуст.", "Избранное", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            
            FavoriteWindow favoritesWindow = new FavoriteWindow(favoriteTracks);
            favoritesWindow.ShowDialog();
        }
        private void AddFiles_Click(object sender, RoutedEventArgs e)
        {
            if (PlaylistComboBox.SelectedItem == null)
            {
                MessageBox.Show("First, select or create a playlist.");
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Audio files|*.mp3;*.wav;*.wma;*.mp4" ;
            if (openFileDialog.ShowDialog() == true)
            {
                var selectedPlaylist = PlaylistComboBox.SelectedItem.ToString()!;
                playlists[selectedPlaylist].AddRange(openFileDialog.FileNames);
                UpdateTrackList();
            }
        }

        private void CreatePlaylist_Click(object sender, RoutedEventArgs e)
        {
            string playlistName = "Playlist " + (playlists.Count + 1);
            playlists.Add(playlistName, new List<string>());
            PlaylistComboBox.Items.Add(playlistName);
            PlaylistComboBox.SelectedItem = playlistName;
        }

        private void DeletePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (PlaylistComboBox.SelectedItem != null)
            {
                var selectedPlaylist = PlaylistComboBox.SelectedItem.ToString()!;
                playlists.Remove(selectedPlaylist);
                PlaylistComboBox.Items.Remove(selectedPlaylist);
                TracksListBox.Items.Clear();
            }
        }

        private void PlaylistComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateTrackList();
        }

        private void UpdateTrackList()
        {
            TracksListBox.Items.Clear();
            if (PlaylistComboBox.SelectedItem != null)
            {
                var selectedPlaylist = PlaylistComboBox.SelectedItem.ToString()!;
                foreach (var track in playlists[selectedPlaylist])
                {
                    TracksListBox.Items.Add(System.IO.Path.GetFileName(track));
                }
            }
        }

        private void TracksListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            currentTrackIndex = TracksListBox.SelectedIndex;
            PlaySelectedTrack();
            
        }

        private void PlaySelectedTrack()
        {
            if (PlaylistComboBox.SelectedItem != null && TracksListBox.SelectedIndex >= 0)
            {
                var selectedPlaylist = PlaylistComboBox.SelectedItem.ToString()!;
                var trackPath = playlists[selectedPlaylist][currentTrackIndex];
                mediaPlayer.Open(new Uri(trackPath));
                mediaPlayer.Volume = VolumeSlider.Value;
                mediaPlayer.Play();
                timer.Start();

                AddToPlaybackHistory(trackPath); 
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.Source == null && TracksListBox.Items.Count > 0)
            {
                currentTrackIndex = 0;
                TracksListBox.SelectedIndex = currentTrackIndex;
            }
            else
            {
                mediaPlayer.Play();
            }
            timer.Start();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
            timer.Stop();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
            timer.Stop();
            ProgressSlider.Value = 0;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                ProgressSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;

                if (!isDraggingSlider)
                {
                    ProgressSlider.Value = mediaPlayer.Position.TotalSeconds;
                }

                CurrentTimeTextBlock.Text = mediaPlayer.Position.ToString(@"mm\:ss");
                TotalTimeTextBlock.Text = mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");

                if (mediaPlayer.Position >= mediaPlayer.NaturalDuration.TimeSpan)
                {
                    PlayNextTrack();
                }
            }
        }

        private void PlayNextTrack()
        {
            if (RepeatRadioButton.IsChecked == true)
            {
                PlaySelectedTrack();
            }
            else if (ShuffleRadioButton.IsChecked == true)
            {
                Random rand = new Random();
                currentTrackIndex = rand.Next(TracksListBox.Items.Count);
                TracksListBox.SelectedIndex = currentTrackIndex;
            }
            else
            {
                currentTrackIndex++;
                if (currentTrackIndex >= TracksListBox.Items.Count)
                {
                    currentTrackIndex = 0;
                }
                TracksListBox.SelectedIndex = currentTrackIndex;
            }
        }

        private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isDraggingSlider)
            {
                CurrentTimeTextBlock.Text = TimeSpan.FromSeconds(ProgressSlider.Value).ToString(@"mm\:ss");
            }
        }

        private void ProgressSlider_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isDraggingSlider = true;
            mediaPlayer.Pause();
            timer.Stop();
        }

        private void ProgressSlider_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isDraggingSlider = false;
            mediaPlayer.Position = TimeSpan.FromSeconds(ProgressSlider.Value);
            mediaPlayer.Play();
            timer.Start();
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isMuted)
            {
                mediaPlayer.Volume = VolumeSlider.Value;
                previousVolume = VolumeSlider.Value;
            }
        }

        private void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            if (isMuted)
            {
                
                mediaPlayer.Volume = previousVolume;
                VolumeSlider.Value = previousVolume;
                MuteButton.Content = "Mute";
                isMuted = false;
            }
            else
            {
                
                previousVolume = VolumeSlider.Value;
                mediaPlayer.Volume = 0;
                VolumeSlider.Value = 0;
                MuteButton.Content = "Mute off";
                isMuted = true;
            }
        }

        private void DeleteTrack_Click(object sender, RoutedEventArgs e)
        {
            if (PlaylistComboBox.SelectedItem != null && TracksListBox.SelectedIndex >= 0)
            {
                var selectedPlaylist = PlaylistComboBox.SelectedItem.ToString()!;
                int selectedIndex = TracksListBox.SelectedIndex;
                playlists[selectedPlaylist].RemoveAt(selectedIndex);
                UpdateTrackList();
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            SavePlaylists();

            
        }

        private void SavePlaylists()
        {
            List<PlaylistData> playlistDataList = playlists.Select(pl => new PlaylistData
            {
                Name = pl.Key,
                Tracks = pl.Value
            }).ToList();

            XmlSerializer serializer = new XmlSerializer(typeof(List<PlaylistData>));
            using (FileStream fs = new FileStream("playlists.xml", FileMode.Create))
            {
                serializer.Serialize(fs, playlistDataList);
            }
        }

        private void LoadPlaylists()
        {
            if (File.Exists("playlists.xml"))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<PlaylistData>));
                using (FileStream fs = new FileStream("playlists.xml", FileMode.Open))
                {
                    List<PlaylistData> playlistDataList = (List<PlaylistData>)serializer.Deserialize(fs);
                    playlists = playlistDataList.ToDictionary(pl => pl.Name, pl => pl.Tracks);

                    foreach (var playlistName in playlists.Keys)
                    {
                        PlaylistComboBox.Items.Add(playlistName);
                    }
                }
            }
        }
        private void SearchTrack_Click(object sender, RoutedEventArgs e)
        {
            if (PlaylistComboBox.SelectedItem != null)
            {
                var selectedPlaylist = PlaylistComboBox.SelectedItem.ToString()!;
                var searchQuery = SearchTextBox.Text.ToLower();

                var searchResults = playlists[selectedPlaylist]
                    .Where(track => System.IO.Path.GetFileName(track).ToLower().Contains(searchQuery))
                    .ToList();

                TracksListBox.Items.Clear();
                foreach (var track in searchResults)
                {
                    TracksListBox.Items.Add(System.IO.Path.GetFileName(track));
                }
            }
        }
        private List<string> playbackHistory = new List<string>();

        private void AddToPlaybackHistory(string track)
        {
            playbackHistory.Add(track);
            if (playbackHistory.Count > 100)
            {
                playbackHistory.RemoveAt(0); 
            }
        }

        private void ShowPlaybackHistory_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder history = new StringBuilder("Playback History:\n");
            foreach (var track in playbackHistory)
            {
                history.AppendLine(System.IO.Path.GetFileName(track));
            }
            MessageBox.Show(history.ToString(), "Playback History");
        }
        private void OpenSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            
            var settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();

            
            ApplySettings(settingsWindow.SelectedTheme, settingsWindow.SelectedLanguage, settingsWindow.SavePath);
        }

       
        private void ApplySettings(string theme, string language, string savePath)
        {
            
            if (theme == "Dark")
            {
                this.Background = new SolidColorBrush(Colors.DarkSlateGray);
            }
            else
            {
                this.Background = new SolidColorBrush(Colors.LightGray);
            }

            
            if (language == "Russian")
            {
                Title = "Музыкальный проигрыватель";
            }
            else
            {
                Title = "Music Player";
            }

            
            Console.WriteLine($"Save Path: {savePath}");
        }
        private void SaveSettings(string theme, string language, string savePath)
        {
            var settings = new
            {
                Theme = theme,
                Language = language,
                SavePath = savePath
            };

            File.WriteAllText("settings.json", JsonConvert.SerializeObject(settings));
        }

        private void LoadSettings()
        {
            if (File.Exists("settings.json"))
            {
                var settings = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText("settings.json"));
                ApplySettings((string)settings.Theme, (string)settings.Language, (string)settings.SavePath);
            }
        }
        private void ApplySettings(string theme, string language, string savePath, int fontSize, string fontFamily)
        {
            
            this.FontSize = fontSize;
            this.FontFamily = new FontFamily(fontFamily);

            
        }
        private async void AuthenticateSpotify()
        {
            bool isAuthenticated = await spotifyService.AuthenticateAsync();
            if (!isAuthenticated)
            {
                MessageBox.Show("Failed to authenticate with Spotify.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SearchSpotifyButton_Click(object sender, RoutedEventArgs e)
        {
            string query = SpotifySearchTextBox.Text;
            if (string.IsNullOrWhiteSpace(query))
            {
                MessageBox.Show("Please enter a search query.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var results = await spotifyService.SearchTracksAsync(query);
            SpotifyResultsListBox.ItemsSource = results;
        }
        
        private void OpenSleepTimerButton_Click(object sender, RoutedEventArgs e)
        {
            var sleepTimerWindow = new SleepTimerWindow();
            sleepTimerWindow.ShowDialog();
        }

        private void OpenAudioRecordingWindow_Click(object sender, RoutedEventArgs e)
        {
            var audioRecordingWindow = new AudioRecordingWindow();
            audioRecordingWindow.ShowDialog();
        }

        private void OpenRadioWindow_Click(object sender, RoutedEventArgs e)
        {
            var radioWindow = new RadioWindow();
            radioWindow.ShowDialog();
        }

        private void SortTracksButton_Click(object sender, RoutedEventArgs e)
        {
            var sortedTracks = TracksListBox.Items.Cast<string>().OrderBy(track => track).ToList();

            TracksListBox.Items.Clear();
            foreach (var track in sortedTracks)
            {
                TracksListBox.Items.Add(track);
            }
        }

        private void SearchTracksTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = SearchTracksTextBox.Text.ToLower();

            foreach (var item in TracksListBox.Items.Cast<string>().ToList())
            {
                var listBoxItem = (ListBoxItem)TracksListBox.ItemContainerGenerator.ContainerFromItem(item);
                if (listBoxItem != null)
                {
                    listBoxItem.Visibility = item.ToLower().Contains(keyword) ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private void ShowTrackDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (TracksListBox.SelectedItem is string selectedTrack)
            {
                var detailsWindow = new TrackDetailsWindow(selectedTrack);
                detailsWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Please select a track to view details.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ToggleDateTimeVisibility_Click(object sender, RoutedEventArgs e)
        {
            if (DateTimeTextBlock.Visibility == Visibility.Collapsed)
            {
                DateTimeTextBlock.Visibility = Visibility.Visible;
                _dateTimeTimer.Start(); 
            }
            else
            {
                DateTimeTextBlock.Visibility = Visibility.Collapsed;
                _dateTimeTimer.Stop(); 
            }
        }

        private void UpdateDateTime(object sender, EventArgs e)
        {
            DateTimeTextBlock.Text = DateTime.Now.ToString("F"); 
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                PlayPause(); 
            }
            else if (e.Key == Key.Right && Keyboard.Modifiers == ModifierKeys.Control)
            {
                NextTrack(); 
            }
            else if (e.Key == Key.Left && Keyboard.Modifiers == ModifierKeys.Control)
            {
                PreviousTrack(); 
            }
            else if (e.Key == Key.Up && Keyboard.Modifiers == ModifierKeys.Control)
            {
                VolumeUp(); 
            }
            else if (e.Key == Key.Down && Keyboard.Modifiers == ModifierKeys.Control)
            {
                VolumeDown();
            }
        }

        
        private void ShowUserGuide_Click(object sender, RoutedEventArgs e)
        {
            UserGuideWindow userGuideWindow = new UserGuideWindow();
            userGuideWindow.ShowDialog();
        }

        
        private void PlayPause()
        {
            MessageBox.Show("Play/Pause triggered!");
        }

        private void NextTrack()
        {
            MessageBox.Show("Next track triggered!");
        }

        private void PreviousTrack()
        {
            MessageBox.Show("Previous track triggered!");
        }

        private void VolumeUp()
        {
            MessageBox.Show("Volume Up triggered!");
        }

        private void VolumeDown()
        {
            MessageBox.Show("Volume Down triggered!");
        }

        private void OpenVisualizer_Click(object sender, RoutedEventArgs e)
        {
            VisualizerWindow visualizerWindow = new VisualizerWindow();
            visualizerWindow.Show();
        }
    }
}