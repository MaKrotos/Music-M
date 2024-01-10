using Microsoft.VisualBasic;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;



namespace VK_UI3.Interfaces
{
    public abstract class IVKGetAudio
    {
        public IVkApi api;
        public long id;
        bool shuffle = false;
        Uri photoUri;

        public string name;

        protected List<ExtendedAudio> listAudioShuffle = new List<ExtendedAudio>();
        protected List<ExtendedAudio> listAudioTrue = new List<ExtendedAudio>();

        public List<ExtendedAudio> listAudio { get {
                if (shuffle) return listAudioShuffle;
                else return listAudioTrue;
               
                        } 
        }
        public event EventHandler onListUpdate; // Событие OnDeviceAttached
        public long countTracks { get; set; }
        public long? currentTrack { get; set; }

        // Добавляем делегат и событие
        public delegate void ChangedPlayAudio(object sender, EventArgs e);
        public event ChangedPlayAudio AudioPlayedChangeEvent;

        // Метод для вызова события
        public void ChangePlayAudio()
        {
            AudioPlayedChangeEvent?.Invoke(this, EventArgs.Empty);
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
            this.id = id;
            Task.Run(() =>
            {
                countTracks = getCount();
                this.GetTracks();
            });
            name = getName();
            photoUri = getPhoto();
        }
        public abstract Uri getPhoto();

        public abstract string getName();
       

        public abstract long getCount();





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

        public ExtendedAudio GetTrackPlay()
        {
            return GetTrackPlay((long)currentTrack);
        }
        public ExtendedAudio GetTrackPlay(long tracI)
        {
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
