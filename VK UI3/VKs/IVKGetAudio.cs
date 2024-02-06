using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Data;
using Microsoft.VisualBasic;
using MusicX.Core.Models;
using MusicX.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading;
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
    public abstract class IVKGetAudio : ObservableCollection<ExtendedAudio>, ISupportIncrementalLoading
    {
        public IVkApi api;
        public string id;
        bool shuffle = false;
         
 

        Uri photoUri;

        public string name;

        public ObservableCollection<ExtendedAudio> listAudioShuffle = new ObservableCollection<ExtendedAudio>();
        public ObservableCollection<ExtendedAudio> listAudio = new ObservableCollection<ExtendedAudio>();
        public DispatcherQueue dispatcherQueue;

      
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

        public IVKGetAudio(string sectionID, DispatcherQueue dispatcherQueue ,  Uri photoLink = null, string name = null, List<MusicX.Core.Models.Audio> audios = null, string next = null)
        {
            this.api = new VK().getVKAPI();
            this.id = sectionID;
            this.dispatcherQueue = dispatcherQueue;
            this.photoUri = photoLink ?? getPhoto();
            this.name = name ?? getName();
            this.Next = next;

            if (audios != null)
            {
                foreach (var audio in audios)
                {
                    ExtendedAudio extendedAudio = new ExtendedAudio(audio, this);
                    listAudio.Add(extendedAudio);
                }
            }
            else
            {
                name = getName();
                photoUri = getPhoto();
                countTracks = getCount();
                this.GetTracks();
            }
        }



        public IVKGetAudio(MusicX.Core.Models.Block block, DispatcherQueue dispatcherQueue)
        {
            this.api = new VK().getVKAPI();
            this.id = block.Id;
            this.dispatcherQueue = dispatcherQueue;
            this.photoUri = null;
            this.name = null;
            this.Next = block.NextFrom;


            if (block.Audios != null)
            {
                foreach (var audio in block.Audios)
                {
                    ExtendedAudio extendedAudio = new ExtendedAudio(audio, this);
                    listAudio.Add(extendedAudio);
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
            var a = GetTrackPlay().audio;
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
        public IVKGetAudio(long id, DispatcherQueue dispatcher)
        {
            this.api = new VK().getVKAPI();
            this.id = id.ToString();
            this.dispatcherQueue = dispatcher;
            Task.Run(() =>
            {
                name = getName();
                photoUri = getPhoto();
                countTracks = getCount();
                this.GetTracks();
                NotifyOnListUpdate();
            });
          
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
            listAudioShuffle = new ObservableCollection<VK_UI3.Helpers.ExtendedAudio>(listAudio.OrderBy(x => rng.Next()));

            currentTrack = listAudioShuffle.ToList().FindIndex(x => x.NumberInList == currentTrack);
        }


        internal void UnShuffleList()
        {
            if (shuffle)
            {
                shuffle = false;
                listAudioShuffle.Clear();

                currentTrack = listAudio.ToList().FindIndex(x => x.NumberInList == currentTrack);

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


        bool _itsAll = false;

        public bool itsAll { get {
                if (_itsAll || (countTracks != null &&  listAudio.Count == countTracks)  ) return true;
                return false;
            } set { _itsAll = value; } }

        public bool HasMoreItems => throw new NotImplementedException();

        public ExtendedAudio GetTrackPlay()
        {
          
            if (currentTrack == null) currentTrack = 0;
            return GetTrackPlay((long)currentTrack);
        }


        public bool getLoadedTracks = false;
        public ExtendedAudio GetTrackPlay(long tracI)
        {
            if (!itsAll && currentTrack == listAudio.Count() - 1)
            {
                Task.Run(() =>
                {
                    if (countTracks == null)
                        countTracks = getCount();
                    this.GetTracks();
                });
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

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            throw new NotImplementedException();
        }
    }






}