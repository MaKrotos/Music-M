using Microsoft.UI.Dispatching;
using MusicX.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using VK_UI3.Controllers;
using VK_UI3.Helpers;
using VkNet.Abstractions;



namespace VK_UI3.VKs.IVK
{
    public interface IVKGetTracksMore
    {
        public abstract void GetTracks(int offset);
    }

    public abstract class IVKGetAudio
    {
        public IVkApi api;
        public string id;
        bool shuffle = false;

        private List<ExtendedAudio> selectedAudio = new List<ExtendedAudio>();


        public Uri photoUri;

        public string name;



        List<int> shuffleList = new List<int>();
        

        public ObservableRangeCollection<ExtendedAudio> listAudio
        {
            get; set;
        } = new ObservableRangeCollection<ExtendedAudio>();

        public void FillAndShuffleList()
        {
          
            for (int i = 0; i <= countTracks - 1; i++)
            {
                shuffleList.Add(i);
            }

            Random rng = new Random();
            int n = shuffleList.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                int value = shuffleList[k];
                shuffleList[k] = shuffleList[n];
                shuffleList[n] = value;
            }

            // Find the index of currentTrack in the shuffled list
            int currentTrackIndex = shuffleList.IndexOf((int)currentTrack);

            // Update currentTrack to the index where its value is stored
            currentTrack = currentTrackIndex;
        }



        public DispatcherQueue DispatcherQueue;

        public EventHandler onListUpdate;



        public WeakEventManager onCountUpDated = new WeakEventManager();

        public void countUpdated() { onCountUpDated?.RaiseEvent(this, EventArgs.Empty); }


        public WeakEventManager onInfoUpdated = new WeakEventManager();

        public void InfoUpdated() { onCountUpDated?.RaiseEvent(this, EventArgs.Empty); }


        public WeakEventManager onNameUpdated = new WeakEventManager();
        public void NameUpdated() { onNameUpdated?.RaiseEvent(this, EventArgs.Empty); }

        public WeakEventManager onPhotoUpdated = new WeakEventManager();


        public void PhotoUpdated() { onPhotoUpdated?.RaiseEvent(this, EventArgs.Empty); }


        private long? _countTracks { get; set; } = null;

        public long? countTracks
        {
            get
            {
                if (_countTracks == null) return -1;
                if (_countTracks != -1) return _countTracks;
                else return listAudio.Count;
            }
            set { _countTracks = (long)value; }
        }


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

        public void PlayThis()
        {
            this.currentTrack = 0;
            AudioPlayer.PlayList(this);
        }

        // Добавляем делегат и событие
        public delegate void ChangedPlayAudio(object sender, EventArgs e);
        public event ChangedPlayAudio AudioPlayedChangeEvent;

        // Метод для вызова события
        public void ChangePlayAudio(ExtendedAudio trackdata)
        {
            AudioPlayedChangeEvent?.Invoke(trackdata, EventArgs.Empty);
        }
        public string Next;


        public void DeleteTrackFromList(ExtendedAudio dataTrack) {
            if (shuffle)
            {
               shuffleList.RemoveAt(shuffleList.IndexOf((int)dataTrack.NumberInList));
            }
            listAudio.Remove(dataTrack);
            selectedAudio.Remove(dataTrack);

            updateNumbers();
        }

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
                onNameUpdated?.RaiseEvent(this, EventArgs.Empty);
                photoUri = getPhoto();
                onPhotoUpdated?.RaiseEvent(this, EventArgs.Empty);
            });

            Task.Run(() =>
            {
                countTracks = getCount();
                onCountUpDated?.RaiseEvent(this, EventArgs.Empty);
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


        public async void shareToVK()
        {
            try
            {

                var a = (await GetTrackPlay()).audio;
                if (a != null)
                    VK.api.Audio.SetBroadcastAsync(
                       a.OwnerId + "_" + a.Id
                        );
            }
            catch (Exception e)
            {

            }
        }

        public void NotifyOnListUpdate()
        {


            var tempList = new List<TaskCompletionSource<bool>>(tcs);
            tcs.Clear();
            foreach (var item in tempList)
            {
                item.TrySetResult(true);
            }


            getLoadedTracks = false;
            onListUpdate?.Invoke(this, EventArgs.Empty); 
        }
        public void updateNumbers()
        {
            for (int i = 0; i < listAudio.Count; i++)
            {
                listAudio[i].NumberInList = i;
            }
        }


        public abstract Uri getPhoto();

        public abstract string getName();


        public abstract long? getCount();

        public abstract List<string> getPhotosList();



        public void setShuffle()
        {
            if (!shuffle)
            {
               
                FillAndShuffleList();
                shuffle = true;
            }
            NotifyOnListUpdate();
        }




        internal void UnShuffleList()
        {
            if (shuffle)
            {
             
                shuffleList.Clear();

                currentTrack = listAudio.ToList().FindIndex(x => x.NumberInList == currentTrack);

                shuffle = false;
                NotifyOnListUpdate();
            }
          
        }



        public void setNextTrackForPlay()
        {

            if (currentTrack >= countTracks - 1 && countTracks != -1)
                currentTrack = 0;
            else
            {
                currentTrack++;
            }

          

        }

        public void setPreviusTrackForPlay()
        {
            if (currentTrack <= 0)
                currentTrack = countTracks - 1;
            else
            {
                currentTrack--;
            }

            

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




        public List<TaskCompletionSource<bool>> tcs = new();

        public bool getLoadedTracks = false;
        public async Task<ExtendedAudio> GetTrackPlayAsync(long tracI, bool prinud = false)
        {

            if (!prinud && tracI > listAudio.Count() - 1) return null;
            while (tracI > listAudio.Count() - 1)
            {
                if (task == null)
                    if (this is IVKGetTracksMore ivl)
                    {
                        ivl.GetTracks(int.Min((int)tracI - listAudio.Count + 1, 1000));
                    }
                    else
                    {
                        GetTracks();
                    }
                if (task != null)
                    await task;
            }


            return listAudio[(int)tracI];
        }


        public Task task = null;
        public abstract void GetTracks();


        public async Task<ExtendedAudio> GetTrackPlay(bool prinud = false)
        {
            if (countTracks < currentTrack && countTracks != -1) return null;
            if (currentTrack == null)
                currentTrack = 0;

            if (shuffle)
            {
                return await GetTrackPlayAsync(shuffleList[(int)currentTrack], prinud);
            }
            

            return await GetTrackPlayAsync((long)currentTrack, prinud);
        }

        public void SaveToFile()
        {
            // Reset the shuffle state
            shuffle = false;
            listAudio.Clear();

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

        internal void playTrack(long? numberInList)
        {
            if (shuffle)
            {
                currentTrack = shuffleList.IndexOf((int)numberInList);
            }
            else
            {
                currentTrack = numberInList;
            }
            AudioPlayer.PlayList(this);
        }

        public void SelectAudio(ExtendedAudio extended)
        {
            if (extended.iVKGetAudio != this)
                return;

            if (selectedAudio.Contains(extended))
                return;

            selectedAudio.Add(extended);
            extended.trackSelectChangedInvoke(new ExtendedAudio.SelectedChange(true));

        }




        public void unselectAudio(ExtendedAudio extended)
        {
            if (extended.iVKGetAudio != this)
                return;

            if (!selectedAudio.Contains(extended))
                return;

            selectedAudio.Remove(extended);
            extended.trackSelectChangedInvoke(new ExtendedAudio.SelectedChange(false));

        }

        public bool AudioIsSelect(ExtendedAudio extended)
        {
            return selectedAudio.Contains(extended);
        }


        ExtendedAudio lastSelected = null;
        internal void changeSelect(ExtendedAudio dataTrack, bool shiftPressend)
        {
            if (dataTrack.iVKGetAudio != this)
                return;

            var trackSelected = this.selectedAudio.Contains(dataTrack);

            if (lastSelected == null || !shiftPressend || !this.listAudio.Contains(lastSelected))
            {
                if (trackSelected)
                {
                    unselectAudio(dataTrack);
                }
                else
                {
                    SelectAudio(dataTrack);
                }
                lastSelected = dataTrack;
                return;
            }

            var newSelectedIndex = listAudio.IndexOf(dataTrack);



            var lastSelectedIndex = listAudio.IndexOf(lastSelected);
      

            int step = lastSelectedIndex < newSelectedIndex ? 1 : -1;

            for (int i = lastSelectedIndex; i != newSelectedIndex + step; i += step)
            {
                if (trackSelected)
                {
                    unselectAudio(listAudio[i]);
                }
                else
                {
                    SelectAudio(listAudio[i]);
                }
            }

            lastSelected = dataTrack;
        }

        public List<ExtendedAudio> getSelectedList() 
        {
            return new List<ExtendedAudio>(selectedAudio.ToList());
        }

        internal void deselectAll()
        {
            foreach (var item in selectedAudio)
            {
                item.trackSelectChangedInvoke(new ExtendedAudio.SelectedChange(false));
            }

            selectedAudio.Clear();
        }
    }

   

}