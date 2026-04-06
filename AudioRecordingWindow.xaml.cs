using NAudio.Wave;
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
using System.IO;
using System.Windows.Threading;
using Microsoft.Win32;
using NAudio.Lame;

namespace Music_player
{
    /// <summary>
    /// Interaction logic for AudioRecordingWindow.xaml
    /// </summary>
    public partial class AudioRecordingWindow : Window
    {
        private WaveInEvent waveIn;
        private WaveFileWriter writer;
        private MemoryStream mp3Stream;
        private string outputFilePath;
        private DispatcherTimer notificationTimer;
        private DateTime recordingStartTime;
        private bool saveAsMp3 = false;

        public AudioRecordingWindow()
        {
            InitializeComponent();
            InitializeNotificationTimer();
        }
        private void InitializeNotificationTimer()
        {
            notificationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            notificationTimer.Tick += UpdateRecordingStatus;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Audio files (*.wav;*.mp3)|*.wav;*.mp3|WAV files (*.wav)|*.wav|MP3 files (*.mp3)|*.mp3",
                Title = "Select Save Location",
                FileName = "Recording.wav"
            };

            if (dialog.ShowDialog() == true)
            {
                SaveLocationTextBox.Text = dialog.FileName;
                saveAsMp3 = dialog.FileName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase);
            }
        }

        private void StartRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FileNameTextBox.Text))
            {
                MessageBox.Show("Please enter a valid file name.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(SaveLocationTextBox.Text))
            {
                MessageBox.Show("Please select a save location.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            outputFilePath = SaveLocationTextBox.Text;

            try
            {
                waveIn = new WaveInEvent();
                waveIn.WaveFormat = new WaveFormat(44100, 1);
                waveIn.DataAvailable += OnDataAvailable;
                waveIn.RecordingStopped += OnRecordingStopped;

                if (saveAsMp3)
                {
                    mp3Stream = new MemoryStream();
                }
                else
                {
                    writer = new WaveFileWriter(outputFilePath, waveIn.WaveFormat);
                }

                waveIn.StartRecording();
                recordingStartTime = DateTime.Now;
                notificationTimer.Start();
                StatusTextBlock.Text = "Status: Recording...";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StopRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            if (waveIn != null)
            {
                waveIn.StopRecording();
                notificationTimer.Stop();
                StatusTextBlock.Text = "Status: Stopped";
            }
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            if (saveAsMp3 && mp3Stream != null)
            {
                using (var mp3Writer = new LameMP3FileWriter(mp3Stream, waveIn.WaveFormat, LAMEPreset.STANDARD))
                {
                    mp3Writer.Write(e.Buffer, 0, e.BytesRecorded);
                }
            }
            else if (writer != null)
            {
                writer.Write(e.Buffer, 0, e.BytesRecorded);
                writer.Flush();
            }
        }

        private void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            waveIn.Dispose();
            waveIn = null;

            if (saveAsMp3 && mp3Stream != null)
            {
                File.WriteAllBytes(outputFilePath, mp3Stream.ToArray());
                mp3Stream.Close();
                mp3Stream = null;
            }
            else if (writer != null)
            {
                writer.Close();
                writer = null;
            }

            Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"Recording saved to {outputFilePath}", "Recording Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        private void UpdateRecordingStatus(object sender, EventArgs e)
        {
            TimeSpan elapsed = DateTime.Now - recordingStartTime;
            StatusTextBlock.Text = $"Status: Recording... {elapsed:mm:ss}";
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (waveIn != null)
            {
                waveIn.StopRecording();
            }
            base.OnClosing(e);
        }
    }
}

