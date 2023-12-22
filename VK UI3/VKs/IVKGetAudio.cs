using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using VK_UI3.Helpers;
using VK_UI3.VKs;
using VkNet;
using VkNet.Model;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;



namespace VK_UI3.Interfaces
{
    public abstract class IVKGetAudio
    {
        public VkApi api;
        public long id;
        bool shuffle = false;
        Uri photoUri;

        public string name;

        protected ObservableCollection<ExtendedAudio> listAudioShuffle = new ObservableCollection<ExtendedAudio>();
        protected ObservableCollection<ExtendedAudio> listAudioTrue = new ObservableCollection<ExtendedAudio>();

        public ObservableCollection<ExtendedAudio> listAudio { get {
                if (shuffle) return listAudioShuffle;
                else return listAudioTrue;
               
                        } 
        }
        public event EventHandler onListUpdate; // Событие OnDeviceAttached
        public long countTracks { get; set; }
        public long currentTrack { get; set; }


        public void shareToVK() 
        {

            var a = GetTrackPlay().Audio;
            api.Audio.SetBroadcastAsync(
               a.OwnerId + "_" + a.Id
                ); ;
        }

        public void NotifyOnListUpdate()
        {
            onListUpdate?.Invoke(this, EventArgs.Empty);
        }
        public IVKGetAudio(long id)
        {
            this.api = VK.getVKAPI();
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

        
        public void getSetNumberPlay() 
        {
            int i = 0;
            foreach (var item in listAudio)
            {
                if (item.PlayThis) {

                 
                    currentTrack = i;
                }
                item.PlayThis = false;
                i++;
            }
        }



        public void setShuffle() {

            if (shuffle) return;
            GetTrackPlay().PlayThis = true;
            shuffle = true;
            ShuffleList();
            getSetNumberPlay();
            NotifyOnListUpdate();
        }


        public void ShuffleList()
        {
            Random rng = new Random();
            listAudioShuffle = new ObservableCollection<ExtendedAudio>(listAudioTrue.OrderBy(x => rng.Next()));
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
            return GetTrackPlay(currentTrack);
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

        internal void UnShuffleList()
        {
            GetTrackPlay().PlayThis = true;
            shuffle = false;
            listAudioShuffle.Clear();
            getSetNumberPlay();
            NotifyOnListUpdate();
        }


        public void SaveToFile()
        {
            // Reset the shuffle state
            shuffle = false;
            listAudioShuffle.Clear();

            // Create a binary formatter
            var formatter = new BinaryFormatter();

            // Get the path to the AppData folder
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Get the path to the database folder in the AppData folder
            var databaseFolderPath = Path.Combine(appDataPath, "classesListCache");

            // Create the database folder if it doesn't exist
            Directory.CreateDirectory(databaseFolderPath);

            // Create a file stream
            using (var stream = new FileStream(Path.Combine(databaseFolderPath, $"{id}.bin"), FileMode.Create, FileAccess.Write))
            {
                // Serialize this object
                formatter.Serialize(stream, this);
            }
        }

        public static IVKGetAudio RestoreFromFile(long id)
        {
            // Create a binary formatter
            var formatter = new BinaryFormatter();

            // Get the path to the AppData folder
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Get the path to the database folder in the AppData folder
            var databaseFolderPath = Path.Combine(appDataPath, "classesListCache");

            // Create a file stream
            using (var stream = new FileStream(Path.Combine(databaseFolderPath, $"{id}.bin"), FileMode.Open, FileAccess.Read))
            {
                // Deserialize the object
                return (IVKGetAudio)formatter.Deserialize(stream);
            }
        }
    }

}
