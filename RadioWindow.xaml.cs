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
using NAudio.Wave;

namespace Music_player
{
    /// <summary>
    /// Interaction logic for RadioWindow.xaml
    /// </summary>
    public partial class RadioWindow : Window
    {
        private IWavePlayer waveOut;
        private MediaFoundationReader mediaReader;
        public RadioWindow()
        {
            InitializeComponent();
        }
//sdasdasdasd
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            strin radioUrl = RadioUrlTextBox.Text;

            if (string.IsNullOrWhiteSpace(radioUrl))
            {
                MessageBox.Show("Please enter a valid radio URL.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                
                StopPlayback();
                mediaReader = new MediaFoundationReader(radioUrl);
                waveOut = new WaveOutEvent();
                waveOut.Init(mediaReader);
                waveOut.Play();

                StatusTextBlock.Text = "Status: Playing";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error playing radio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBlock.Text = "Status: Error";
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopPlayback();
        }

        private void StopPlayback()
        {
            if (waveOut != null)
            {
                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
            }

            if (mediaReader != null)
            {
                mediaReader.Dispose();
                mediaReader = null;
            }

            StatusTextBlock.Text = "Status: Stopped";
        }

        private void RadioStationsComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (RadioStationsComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem)
            {
                string selectedUrl = selectedItem.Tag as string;

                if (selectedUrl != null)
                {
                    RadioUrlTextBox.Text = selectedUrl;
                }

                if (string.IsNullOrEmpty(selectedUrl))
                {
                    RadioUrlTextBox.IsEnabled = true; 
                }
                else
                {
                    RadioUrlTextBox.IsEnabled = false;
                }
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            StopPlayback();
            base.OnClosing(e);
        }
    }
}

