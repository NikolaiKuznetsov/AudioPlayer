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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Microsoft.Toolkit.Wpf.UI.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Windows.Interop;

namespace AudioPlayer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int refreshAll = 0;
        bool pause = false;
        private DispatcherTimer timer;
        List<TrackLists> trackLists = new List<TrackLists>();

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            volume.Value = mediaElement.Volume * 100;
            timer = new DispatcherTimer(DispatcherPriority.Render);
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += new EventHandler(timer_Tick);
            mediaElement.Stop();
        }

        private void openFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "MP3, WAV|*.mp3;*.wav|All|*",
                Multiselect = true,
                ValidateNames = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                for (int i = 0; i < openFileDialog.FileNames.Count(); i++)
                {
                    TrackLists trackList = new TrackLists()
                    {
                        name = openFileDialog.SafeFileNames[i],
                        adress = openFileDialog.FileNames[i]
                    };
                    trackLists.Add(trackList);
                    listTracks.Items.Add(openFileDialog.SafeFileNames[i]);
                }
            }
        }

        private void delTrack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                listTracks.Items.Remove(listTracks.SelectedItem);
                TrackLists track = trackLists.FirstOrDefault(x => x.name == listTracks.SelectedItem.ToString());
                trackLists.Remove(track);
            }
            catch (Exception)
            {
                MessageBox.Show("Элементы не выбраны", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void delListTracks_Click(object sender, RoutedEventArgs e)
        {
            listTracks.Items.Clear();
            trackLists.Clear();
            timer.Stop();
            mediaElement.Stop();
        }

        private void playTrack_Click(object sender, MouseButtonEventArgs e)
        {
            startTrack();
        }

        private void pauseTrack_Click(object sender, MouseButtonEventArgs e)
        {
            if (pause)
            {
                pause = false;
                mediaElement.Play();
                timer.Start();
            }
            else
            {
                pause = true;
                mediaElement.Pause();
                timer.Stop();
            }
        }

        private void stopTrack_Click(object sender, MouseButtonEventArgs e)
        {
            mediaElement.Stop();
            timer.Stop();
        }

        private void nextTrack_Click(object sender, MouseButtonEventArgs e)
        {
            mediaElement.Stop();
            timer.Stop();
            if (listTracks.SelectedIndex == listTracks.Items.Count - 1)
            {
                listTracks.SelectedIndex = 0;
            }
            else
            {
                listTracks.SelectedIndex++;
            }
            startTrack();
        }

        private void backTrack_Click(object sender, MouseButtonEventArgs e)
        {
            if (listTracks.SelectedIndex == 0)
            {
                return;
            }
            mediaElement.Stop();
            timer.Stop();
            listTracks.SelectedIndex--;
            startTrack();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mediaElement.Close();
        }

        private void refreshing_Click(object sender, MouseButtonEventArgs e)
        {
            if (refreshAll == 0)
            {
                refreshAll = 1;
                IntPtr hBitmap = Properties.Resources.refreshingTrackList.GetHbitmap();
                refreshing.Background = new ImageBrush(Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()));
                DeleteObject(hBitmap);
            }
            else if (refreshAll == 1)
            {
                refreshAll = 2;
                IntPtr hBitmap = Properties.Resources.refreshingOneTrack.GetHbitmap();
                refreshing.Background = new ImageBrush(Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()));
                DeleteObject(hBitmap);
            }
            else
            {
                refreshAll = 0;
                IntPtr hBitmap = Properties.Resources.refreshing.GetHbitmap();
                refreshing.Background = new ImageBrush(Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()));
                DeleteObject(hBitmap);
            }
        }

        private void volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaElement.Volume = this.volume.Value / 100;
            volumeValueText.Text = this.volume.Value.ToString("0");
        }

        private void time_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaElement.Pause();
            mediaElement.Position = TimeSpan.FromSeconds(time.Value);
            mediaElement.Play();
        }

        private void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            time.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            mediaElement.Position = TimeSpan.FromSeconds(0);
            time.Value = 0;
        }

        private void mediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MessageBox.Show(e.ErrorException.Message);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            time.Value = mediaElement.Position.TotalSeconds;
            timeValueText.Text = mediaElement.Position.ToString(@"mm\:ss");
            if (mediaElement.Position.TotalSeconds == time.Maximum)
            {
                timer.Stop();
                mediaElement.Position = TimeSpan.FromSeconds(0);
                if (refreshAll == 0)
                {
                    if (listTracks.SelectedIndex == listTracks.Items.Count - 1)
                    {
                        mediaElement.Stop();
                        timer.Stop();
                        return;
                    }
                    else
                    {
                        listTracks.SelectedIndex++;
                    }
                    startTrack();
                }
                else if (refreshAll == 1)
                {
                    if (listTracks.SelectedIndex == listTracks.Items.Count - 1)
                    {
                        listTracks.SelectedIndex = 0;
                    }
                    else
                    {
                        listTracks.SelectedIndex++;
                    }
                    startTrack();
                }
                else
                {
                    mediaElement.Position = TimeSpan.FromSeconds(0);
                    mediaElement.Play();
                    timer.Start();
                }
            }
        }

        private void startTrack()
        {
            try
            {
                TrackLists track = trackLists.FirstOrDefault(x => x.name == listTracks.SelectedItem.ToString());
                mediaElement.Source = new Uri(track.adress, UriKind.Relative);
                mediaElement.Play();
                timer.Start();
            }
            catch (Exception)
            {
                MessageBox.Show("Трек для воспроизведения не выбран", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }

    public class TrackLists
    {
        public string name { get; set; }

        public string adress { get; set; }
    }
}
