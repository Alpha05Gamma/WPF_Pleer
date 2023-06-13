using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Windows.Threading;
using MahApps.Metro.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

namespace Player
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            
        }
        

        bool isPlaying = false;
        bool isShufled = false;
        bool isLooped = false;

        string Fpath;

        int songindex = 0;

        List<int> AlreadyPlayedSongs = new List<int>();

        Playlist playlist = new Playlist();
        Playlist shufllist = new Playlist();

        public List<string> songs = new List<string>();



        private string selectFolder()
        {
            string path = null;
            
                var dialog = new CommonOpenFileDialog { IsFolderPicker=true };
                dialog.Title = "Выберите папку с музыкой";
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    var fileExtensions = new List<string> { ".mp3", ".wav", ".flac", ".mp4", ".m4a" };
                    
                    playlist.audioList.Clear();

                    foreach (string file in Directory.GetFiles(dialog.FileName))
                    {
                        if (fileExtensions.Contains(System.IO.Path.GetExtension(file)))
                        {
                            playlist.audioList.Add(new audio(System.IO.Path.GetFullPath(file)));
                        }
                    }                   
                    
                }            

            return path;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            isPlaying = !isPlaying;
            if(isPlaying)
            {
                PlayButtonIcon.Kind = MahApps.Metro.IconPacks.PackIconFontaudioKind.Pause;
            }
            else
            {
                PlayButtonIcon.Kind = MahApps.Metro.IconPacks.PackIconFontaudioKind.Play;
            }
        }

        private void DialogButton_Click(object sender, RoutedEventArgs e)
        {
            Fpath = selectFolder();
            SyLists("a");
        }
        private void MediaList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectionAudio();                       
        }
        private void CicleButton_Click(object sender, RoutedEventArgs e)
        {
            isLooped = !isLooped;
        }

        private void MixerButton_Click(object sender, RoutedEventArgs e)
        {
            isShufled = !isShufled;
            if(isShufled)
            {
                shufllist.audioList.Clear();
                shufllist.audioList = shufllist.Shufle(playlist.audioList);
                SyLists("b");
            }
            else
            {
                SyLists("a");
            }
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
           
            if (0 <= (songindex -= 1))
            {
                
                MediaList.SelectedIndex = songindex;
                selectionAudio();
            }
            else
            {
                songindex = songs.Count - 1;
                MediaList.SelectedIndex = songindex;
                selectionAudio();
            }
        }

        private void AudioSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            player.Position = new TimeSpan(Convert.ToInt64(AudioSlider.Value));
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (songs.Count > (songindex += 1))
            {
                
                MediaList.SelectedIndex = songindex;
                selectionAudio();
            }
            else
            {
                songindex = 0;
                MediaList.SelectedIndex = songindex;
                selectionAudio();
            }
        }

        private void SyLists(string flag)
        {
            if(flag == "a")
            {
                songs.Clear();
                foreach(audio audio in playlist.audioList)
                {
                    songs.Add(audio.FileName);
                }

                MediaList.SelectedIndex = 0;
                MediaList.ItemsSource = songs;
                MediaList.Items.Refresh();
            }
            if(flag == "b")
            {
                songs.Clear();
                foreach (audio audio in shufllist.audioList)
                {
                    songs.Add(audio.FileName);
                }

                MediaList.SelectedIndex = 0;
                MediaList.ItemsSource = songs;
                MediaList.Items.Refresh();
            }
        }

        private void selectionAudio()
        {
            try
            {
                songindex = MediaList.SelectedIndex;             

                string target = songs[songindex];

                foreach(audio audio in playlist.audioList)
                {
                    if(audio.FileName == target)
                    {
                        player.Source = new Uri(audio.FilePath);                    

                        Name.Text = audio.FileName;
                        Artist.Text = audio.Artist ?? "Unknown Artist";
                        Album.Text = audio.Album ?? "Unknown Album";
                        Year.Text = audio.Year.ToString() ?? "Unknown Year";

                        if(audio.Picture.Length > 0)
                        {
                            var picture = audio.Picture[0];
                            using (var stream = new MemoryStream(picture.Data.Data))
                            {
                                var bitmap = new BitmapImage();
                                bitmap.BeginInit();
                                bitmap.StreamSource = stream;
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap.EndInit();
                                Picture.Source = bitmap;
                            }                            
                        }
                        else
                        {
                            Picture.Source = null;
                        }

                        MessageBox.Show("Песня успешно выбрана"); //без него не работает, но не понимаю почему
                        

                        AudioSlider.Value = 0;
                        AudioSlider.Maximum = player.NaturalDuration.TimeSpan.Ticks;
                        DispatcherTimer timer = new DispatcherTimer();
                        timer.Interval = TimeSpan.FromSeconds(1);
                        timer.Tick += timer_Tick;
                        timer.Start();



                        if (player.NaturalDuration.TimeSpan.Seconds < 10)
                        {
                            RemainBlock.Text = player.NaturalDuration.TimeSpan.Minutes.ToString() + ":" + "0" + player.NaturalDuration.TimeSpan.Seconds.ToString();
                        }
                        else
                        {
                            RemainBlock.Text = player.NaturalDuration.TimeSpan.Minutes.ToString() + ":" + player.NaturalDuration.TimeSpan.Seconds.ToString();
                        }

                        if (player.Position.Seconds < 10)
                        {
                            PassedBlock.Text = player.Position.Minutes.ToString() + ":" + "0" + player.Position.Seconds.ToString();
                        }
                        else
                        {
                            PassedBlock.Text = player.Position.Minutes.ToString() + ":" + player.Position.Seconds.ToString();
                        }

                        AudioSlider.Value = player.Position.Ticks;

                    }
                }

            }
            catch
            {

            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (player.NaturalDuration.HasTimeSpan)
            {
                
                AudioSlider.Minimum = 0;
                AudioSlider.Maximum = player.NaturalDuration.TimeSpan.Ticks;
                AudioSlider.Value = player.Position.Ticks;

                if (player.Position.Seconds < 10)
                {
                    PassedBlock.Text = player.Position.Minutes.ToString() + ":" + "0" + player.Position.Seconds.ToString();
                }
                else
                {
                    PassedBlock.Text = player.Position.Minutes.ToString() + ":" + player.Position.Seconds.ToString();
                }
            }
        }



        void Media_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                player.Play();
            }
        }

        void Media_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (isLooped)
            {
                selectionAudio();
            }
            else
            {
                songindex += 1;
                MediaList.SelectedIndex = songindex;
                selectionAudio();
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        
    }
}
