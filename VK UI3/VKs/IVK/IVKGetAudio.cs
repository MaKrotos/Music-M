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



namespace VK_UI3.VKs.IVK
{
    public abstract class IVKGetAudio
    {
        public IVkApi api;
        public string id;
        bool shuffle = false;



        public Uri photoUri;

        public string name;

        public ObservableRangeCollection<ExtendedAudio> listAudioShuffle = new ObservableRangeCollection<ExtendedAudio>();
        public ObservableRangeCollection<ExtendedAudio> listAudioTrue = new ObservableRangeCollection<ExtendedAudio>();

        public ObservableRangeCollection<ExtendedAudio> listAudio
        {
            get
            {
                if (shuffle) return listAudioShuffle;
                return listAudioTrue;
            }
            set { listAudioTrue = value; }
        }


        public DispatcherQueue DispatcherQueue;


        public event EventHandler onListUpdate;

        public event EventHandler onCountUpDated;

        public void countUpdated() { onCountUpDated?.Invoke(this, EventArgs.Empty); }

        public event EventHandler onNameUpdated;

        public void NameUpdated() { onNameUpdated?.Invoke(this, EventArgs.Empty); }

        public event EventHandler onPhotoUpdated;
        public void PhotoUpdated() { onPhotoUpdated?.Invoke(this, EventArgs.Empty); }

        public long? countTracks { get; set; }


        private long? _currentTrack;

        public long? currentTrack
        {
            get { return _currentTrack; }
            set
            {

                if (!itsAll && value == listAudio.Count() - 1 && _currentTrack != value)
                {
                    Task.Run(() =>
                    {
                        if (countTracks == null)
                            countTracks = getCount();
                        GetTracks();
                    });
                }

                _currentTrack = value;
            }
        }



        // Добавляем делегат и событие
        public delegate void ChangedPlayAudio(object sender, EventArgs e);
        public event ChangedPlayAudio AudioPlayedChangeEvent;

        // Метод для вызова события
        public void ChangePlayAudio()
        {
            AudioPlayedChangeEvent?.Invoke(this, EventArgs.Empty);
        }
        public string Next;


        public IVKGetAudio(DispatcherQueue dispatcher)
        {

            DispatcherQueue = dispatcher;
        }

        public IVKGetAudio(string sectionID, DispatcherQueue dispatcher, Uri photoLink = null, string name = null, List<MusicX.Core.Models.Audio> audios = null, string next = null)
        {
            api = new VK().getVKAPI();
            id = sectionID;
            photoUri = photoLink ?? getPhoto();
            this.name = name ?? getName();
            Next = next;
            DispatcherQueue = dispatcher;

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
                GetTracks();
            }
        }
        public IVKGetAudio(long id, DispatcherQueue dispatcher)
        {
            api = new VK().getVKAPI();
            this.id = id.ToString();

            DispatcherQueue = dispatcher;
            Task.Run(() =>
            {
                name = getName();
                onNameUpdated?.Invoke(this, EventArgs.Empty);
                photoUri = getPhoto();
                onPhotoUpdated?.Invoke(this, EventArgs.Empty);
            });

            Task.Run(() =>
            {
                countTracks = getCount();
                onCountUpDated?.Invoke(this, EventArgs.Empty);
                GetTracks();
            });

        }




        public IVKGetAudio(Block block, DispatcherQueue dispatcher)
        {
            api = new VK().getVKAPI();
            id = block.Id;
            DispatcherQueue = dispatcher;
            photoUri = null;
            name = null;
            if (block.NextFrom == null) itsAll = true;
            Next = block.NextFrom;


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
                GetTracks();

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
            onListUpdate?.Invoke(this, EventArgs.Empty);
        }
        public void updateNumbers() {
            for (int i = 0; i < listAudio.Count; i++)
            {
                listAudio[i].NumberInList = i;
            }
        }


        public abstract Uri getPhoto();

        public abstract string getName();


        public abstract long? getCount();





        public void setShuffle()
        {
            if (!shuffle)
            {
                // shuffle = true;
                //  ShuffleList();
                //  NotifyOnListUpdate();
            }
        }

        public void ShuffleList()
        {
            Random rng = new Random();
            listAudioShuffle = new ObservableRangeCollection<ExtendedAudio>(listAudio.OrderBy(x => rng.Next()).ToList());

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

        public bool itsAll
        {
            get
            {
                if (_itsAll)
                    return true;
                return false;
            }
            set { _itsAll = value; }
        }

        public bool HasMoreItems => throw new NotImplementedException();

        public ExtendedAudio GetTrackPlay()
        {
            if (currentTrack == null)
                currentTrack = 0;
            return GetTrackPlay((long)currentTrack);
        }





        public bool getLoadedTracks = false;
        public ExtendedAudio GetTrackPlay(long tracI)
        {
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