using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using TagLib;

namespace Player
{
    internal class audio
    {
        public audio(string filePath)
        {
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    this.FilePath = filePath;
                    this.FileName = Path.GetFileNameWithoutExtension(filePath);

                    this.File = TagLib.File.Create(filePath);

                    if(File.Tag != null)
                    {
                        Artist = File.Tag.FirstAlbumArtist;
                        Album = File.Tag.Album;
                        Year = File.Tag.Year;
                        Year = File.Tag.Year;
                        Picture = File.Tag.Pictures;
                    }
                }
                else
                {
                    throw new FileNotFoundException("File not found.", filePath);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        public string FilePath { get; set; }
        public string FileName { get; set; }
        public TagLib.File File { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public uint Year { get; set; }
        public IPicture[] Picture { get; set; }

    }

    internal class Playlist
    {
        public BindingList<audio> audioList { get; set; }

        public Playlist()
        {
            audioList = new BindingList<audio>();
        }

        public BindingList<audio> Shufle(BindingList<audio> audioList)
        {
            Random rand = new Random();
            
            BindingList <audio> shuffledList = new BindingList<audio>(audioList);

            for (int i = 0; i < shuffledList.Count; i++)
            {
                int randomIndex = rand.Next(shuffledList.Count);
                audio temp = shuffledList[i];
                shuffledList[i] = shuffledList[randomIndex];
                shuffledList[randomIndex] = temp;
            }

            return shuffledList;
        }
    }
}
