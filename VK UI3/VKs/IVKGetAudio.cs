using Microsoft.VisualBasic;
using MusicX.Core.Models;
using MusicX.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VK_UI3.Helpers;
using VK_UI3.VKs;
using VkNet.Abstractions;
using VkNet.Model.Attachments;
using Windows.Foundation;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using static VK_UI3.VKs.VK;



namespace VK_UI3.Interfaces
{
    public abstract class IVKGetAudio
    {
        public IVkApi api;
        public string id;
        bool shuffle = false;


        Uri photoUri;

        public string name;

        protected List<ExtendedAudio> listAudioShuffle = new List<ExtendedAudio>();
        protected List<ExtendedAudio> listAudioTrue = new List<ExtendedAudio>();

        public List<ExtendedAudio> listAudio
        {
            get
            {
                if (shuffle) return listAudioShuffle;
                else return listAudioTrue;

            }
        }
        public event EventHandler onListUpdate; // Событие OnDeviceAttached
        public long? countTracks { get; set; }
        public long? currentTrack { get; set; }


        // Добавляем делегат и событие
        public delegate void ChangedPlayAudio(object sender, EventArgs e);
        public event ChangedPlayAudio AudioPlayedChangeEvent;

        // Метод для вызова события
        public void ChangePlayAudio()
        {
            AudioPlayedChangeEvent?.Invoke(this, EventArgs.Empty);
        }
        public string? Next;

        public IVKGetAudio(string sectionID, Uri photoLink = null, string name = null, List<MusicX.Core.Models.Audio> audios = null, string next = null)
        {
            this.api = new VK().getVKAPI();
            this.id = sectionID;

            this.photoUri = photoLink ?? getPhoto();
            this.name = name ?? getName();
            this.Next = next;

            if (audios != null)
            {
                foreach (var audio in audios)
                {
                    ExtendedAudio extendedAudio = new ExtendedAudio((VkNet.Model.Attachments.Audio)audio, this);
                    listAudioTrue.Add(extendedAudio);
                }
            }
            else
            {
                countTracks = getCount();
                this.GetTracks();
            }
        }



        public IVKGetAudio(MusicX.Core.Models.Block block)
        {
            this.api = new VK().getVKAPI();
            this.id = block.Id;

            this.photoUri = null;
            this.name = null;
            this.Next = block.NextFrom;


            if (block.Audios != null)
            {
                foreach (var audio in block.Audios)
                {
                    ExtendedAudio extendedAudio = new ExtendedAudio(audio, this);
                    listAudioTrue.Add(extendedAudio);
                    NotifyOnListUpdate();
                }
            }
            else
            {

                countTracks = getCount();
                this.GetTracks();

            }
        }


        public void shareToVK()
        {
            var a = GetTrackPlay().Audio;
            api.Audio.SetBroadcastAsync(
               a.OwnerId + "_" + a.Id
                ); ;
        }

        public void NotifyOnListUpdate()
        {

            for (int i = 0; i < listAudio.Count; i++)
            {
                listAudio[i].NumberInList = i;
            }

            onListUpdate?.Invoke(this, EventArgs.Empty);
        }
        public IVKGetAudio(long id)
        {
            this.api = new VK().getVKAPI();
            this.id = id.ToString();

            Task.Run(() =>
            {
                countTracks = getCount();
                this.GetTracks();
                NotifyOnListUpdate();
            });
            name = getName();
            photoUri = getPhoto();



        }


        public abstract Uri? getPhoto();

        public abstract string? getName();


        public abstract long? getCount();





        public void setShuffle()
        {
            if (!shuffle)
            {
                shuffle = true;
                ShuffleList();
                NotifyOnListUpdate();
            }
        }

        public void ShuffleList()
        {
            Random rng = new Random();
            listAudioShuffle = listAudioTrue.OrderBy(x => rng.Next()).ToList();

            currentTrack = listAudioShuffle.FindIndex(x => x.NumberInList == currentTrack);
        }

        internal void UnShuffleList()
        {
            if (shuffle)
            {
                shuffle = false;
                listAudioShuffle.Clear();

                currentTrack = listAudio.FindIndex(x => x.NumberInList == currentTrack);

                NotifyOnListUpdate();
            }
        }



        public ExtendedAudio getNextTrackForPlay()
        {

            if (currentTrack >= countTracks - 1)
                currentTrack = 0;
            else
            {
                currentTrack++;
            }

            return GetTrackPlay();

        }

        public ExtendedAudio getPreviusTrackForPlay()

        {

            if (currentTrack <= 0)
                currentTrack = countTracks - 1;
            else
            {
                currentTrack--;
            }

            return GetTrackPlay();

        }
        public bool itsAll = false;
        public ExtendedAudio GetTrackPlay()
        {
            if (!itsAll && currentTrack == listAudio.Count() -1)
            {
                Task.Run(() =>
                {
                    countTracks = getCount();
                    this.GetTracks();
                });
            }
            if (currentTrack == null) currentTrack = 0;
            return GetTrackPlay((long)currentTrack);
        }


        public bool getLoadedTracks = false;
        public ExtendedAudio GetTrackPlay(long tracI)
        {
            while (currentTrack + 1 > listAudio.Count)
            {
                GetTracks();
            }

            if (tracI > countTracks)
            {
                return null;
            }

            if (listAudio.Count < (tracI))
            {
                GetTracks();
                return GetTrackPlay(tracI);
            }

            return listAudio[(int)tracI];
        }

        public abstract void GetTracks();



        public void SaveToFile()
        {
            // Reset the shuffle state
            shuffle = false;
            listAudioShuffle.Clear();

            // Get the path to the AppData folder
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Get the path to the database folder in the AppData folder
            var databaseFolderPath = Path.Combine(appDataPath, "classesListCache");

            // Create the database folder if it doesn't exist
            Directory.CreateDirectory(databaseFolderPath);

            // Create a file stream
            using (var stream = new FileStream(Path.Combine(databaseFolderPath, $"{id}.json"), FileMode.Create))
            {
                // Serialize this object using JsonSerializer
                JsonSerializer.Serialize(stream, this);
            }
        }

        public static IVKGetAudio RestoreFromFile(long id)
        {
            // Get the path to the AppData folder
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Get the path to the database folder in the AppData folder
            var databaseFolderPath = Path.Combine(appDataPath, "classesListCache");

            // Create a file stream
            using (var stream = new FileStream(Path.Combine(databaseFolderPath, $"{id}.json"), FileMode.Open))
            {
                // Deserialize this object using JsonSerializer
                return JsonSerializer.Deserialize<IVKGetAudio>(stream);
            }
        }


    }

}