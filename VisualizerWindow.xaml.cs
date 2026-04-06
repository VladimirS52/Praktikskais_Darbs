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
using NAudio.Dsp;
using System.Windows.Threading;

namespace Music_player
{
    /// <summary>
    /// Interaction logic for VisualizerWindow.xaml
    /// </summary>
    public partial class VisualizerWindow : Window
    {
        private WasapiLoopbackCapture _audioCapture;
        private DispatcherTimer _visualizerTimer;
        private float[] _fftBuffer;
        public VisualizerWindow()
        {
            InitializeComponent();

            _audioCapture = new WasapiLoopbackCapture();
            _audioCapture.DataAvailable += OnAudioDataAvailable;

            _fftBuffer = new float[1024];

            _visualizerTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50) 
            };
            _visualizerTimer.Tick += UpdateEqualizer;

            Loaded += (s, e) => StartAudioCapture();
            Closing += (s, e) => StopAudioCapture();
        }

        private void StartAudioCapture()
        {
            _audioCapture.StartRecording();
            _visualizerTimer.Start();
        }

        private void StopAudioCapture()
        {
            _visualizerTimer.Stop();
            _audioCapture.StopRecording();
            _audioCapture.Dispose();
        }

        private void OnAudioDataAvailable(object sender, WaveInEventArgs e)
        {
            
            var buffer = new Complex[512];
            for (int i = 0; i < e.BytesRecorded / 2; i++)
            {
                short sample = BitConverter.ToInt16(e.Buffer, i * 2);
                buffer[i].X = sample / 32768.0f; 
                buffer[i].Y = 0; 
            }

            
            FastFourierTransform.FFT(true, (int)Math.Log(512, 2), buffer);

            
            for (int i = 0; i < buffer.Length; i++)
            {
                _fftBuffer[i] = (float)Math.Sqrt(buffer[i].X * buffer[i].X + buffer[i].Y * buffer[i].Y);
            }
        }

        private void UpdateEqualizer(object sender, EventArgs e)
        {
            
            int barIndex = 0;
            foreach (var child in EqualizerPanel.Children)
            {
                if (child is Rectangle rect && barIndex < _fftBuffer.Length)
                {
                    rect.Height = Math.Max(20, _fftBuffer[barIndex] * 200); 
                    barIndex += 50; 
                }
            }
        }

    }
}
