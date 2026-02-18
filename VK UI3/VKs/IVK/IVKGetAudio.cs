using Microsoft.UI.Dispatching;
using MusicX.Core.Models;
using System;
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

    public class ErrorLoad : EventArgs
    {
        public Exception exception;

        public ErrorLoad(Exception exception)
        {
            this.exception = exception;
        }
    }

    public abstract class IVKGetAudio
    {
        #region Поля и свойства
        
        /// <summary>
        /// API для работы с ВКонтакте
        /// </summary>
        public IVkApi api;
        
        /// <summary>
        /// Идентификатор
        /// </summary>
        public string id;
        
        /// <summary>
        /// Флаг перемешивания треков
        /// </summary>
        protected bool shuffle = false;
        
        /// <summary>
        /// Список выбранных аудиозаписей
        /// </summary>
        private List<ExtendedAudio> selectedAudio = new List<ExtendedAudio>();
        
        /// <summary>
        /// URI фотографии
        /// </summary>
        public Uri photoUri;
        
        /// <summary>
        /// Название
        /// </summary>
        public string name;
        
        /// <summary>
        /// Список индексов для перемешивания
        /// </summary>
        private List<int> shuffleList = new List<int>();
        
        /// <summary>
        /// Коллекция аудиозаписей
        /// </summary>
        public ObservableRangeCollection<ExtendedAudio> listAudio { get; set; } = new ObservableRangeCollection<ExtendedAudio>();
        
        /// <summary>
        /// Очередь диспетчера
        /// </summary>
        public DispatcherQueue DispatcherQueue;
        
        /// <summary>
        /// Событие обновления списка
        /// </summary>
        public EventHandler onListUpdate;
        
        /// <summary>
        /// Событие ошибки загрузки
        /// </summary>
        public EventHandler onErrorLoad;
        
        /// <summary>
        /// Менеджер событий обновления количества треков
        /// </summary>
        public WeakEventManager onCountUpDated = new WeakEventManager();
        
        /// <summary>
        /// Менеджер событий обновления информации
        /// </summary>
        public WeakEventManager onInfoUpdated = new WeakEventManager();
        
        /// <summary>
        /// Менеджер событий обновления названия
        /// </summary>
        public WeakEventManager onNameUpdated = new WeakEventManager();
        
        /// <summary>
        /// Менеджер событий обновления фотографии
        /// </summary>
        public WeakEventManager onPhotoUpdated = new WeakEventManager();
        
        /// <summary>
        /// Количество треков
        /// </summary>
        private long? _countTracks { get; set; } = null;
        public long? countTracks
        {
            get
            {
                if (_countTracks == null) return -1;
                if (_countTracks != -1) return _countTracks;
                else return listAudio.Count;
            }
            set { _countTracks = value; }
        }
        
        /// <summary>
        /// Индекс текущего трека
        /// </summary>
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
        
        /// <summary>
        /// Следующий идентификатор для загрузки
        /// </summary>
        public string Next;
        
        /// <summary>
        /// Флаг завершения загрузки всех треков
        /// </summary>
        private bool _itsAll = false;
        public bool itsAll
        {
            get
            {
                return _itsAll;
            }
            set { _itsAll = value; }
        }
        
        /// <summary>
        /// Список задач для ожидания завершения загрузки
        /// </summary>
        public List<TaskCompletionSource<bool>> tcs = new();
        
        /// <summary>
        /// Флаг загрузки треков
        /// </summary>
        public bool getLoadedTracks = false;
        
        /// <summary>
        /// Текущая задача загрузки
        /// </summary>
        public Task task = null;
        
        #endregion
        
        #region События
        
        /// <summary>
        /// Делегат события изменения воспроизводимого аудио
        /// </summary>
        public delegate void ChangedPlayAudio(object sender, EventArgs e);
        
        /// <summary>
        /// Событие изменения воспроизводимого аудио
        /// </summary>
        public event ChangedPlayAudio AudioPlayedChangeEvent;
        
        #endregion
        
        #region Конструкторы
        
        /// <summary>
        /// Конструктор с диспетчером
        /// </summary>
        /// <param name="dispatcher">Диспетчер</param>
        public IVKGetAudio(DispatcherQueue dispatcher)
        {
            DispatcherQueue = dispatcher;
        }
        
        /// <summary>
        /// Конструктор с идентификатором раздела
        /// </summary>
        /// <param name="sectionID">Идентификатор раздела</param>
        /// <param name="dispatcher">Диспетчер</param>
        /// <param name="photoLink">Ссылка на фото</param>
        /// <param name="name">Название</param>
        /// <param name="audios">Список аудиозаписей</param>
        /// <param name="next">Следующий идентификатор</param>
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
        
        /// <summary>
        /// Конструктор с идентификатором пользователя
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <param name="dispatcher">Диспетчер</param>
        public IVKGetAudio(long id, DispatcherQueue dispatcher)
        {
            api = new VK().getVKAPI();
            this.id = id.ToString();
            
            DispatcherQueue = dispatcher;
            Task.Run(() =>
            {
                try
                {
                    name = getName();
                    onNameUpdated?.RaiseEvent(this, EventArgs.Empty);
                    photoUri = getPhoto();
                    onPhotoUpdated?.RaiseEvent(this, EventArgs.Empty);
                }
                catch (Exception e)
                {
                    // Обработка исключения
                }
                
                try
                {
                    countTracks = getCount();
                    onCountUpDated?.RaiseEvent(this, EventArgs.Empty);
                    GetTracks();
                }
                catch (Exception e)
                {
                    onErrorLoad?.Invoke(this, new ErrorLoad(e));
                }
            });
        }
        
        /// <summary>
        /// Конструктор с блоком
        /// </summary>
        /// <param name="block">Блок</param>
        /// <param name="dispatcher">Диспетчер</param>
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
        
        #endregion
        
        #region Методы для работы с треками
        
        /// <summary>
        /// Получает количество треков
        /// </summary>
        /// <returns>Количество треков</returns>
        public abstract long? getCount();
        
        /// <summary>
        /// Получает треки
        /// </summary>
        public abstract void GetTracks();
        
        /// <summary>
        /// Получает трек для воспроизведения асинхронно
        /// </summary>
        /// <param name="tracI">Индекс трека</param>
        /// <param name="prinud">Флаг принудительного получения</param>
        /// <returns>Расширенная аудиозапись</returns>
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
        
        /// <summary>
        /// Получает трек для воспроизведения
        /// </summary>
        /// <param name="prinud">Флаг принудительного получения</param>
        /// <returns>Расширенная аудиозапись</returns>
        public async Task<ExtendedAudio> GetTrackPlay(bool prinud = false)
        {
            if (countTracks < currentTrack && countTracks != -1) return null;
            if (currentTrack == null)
                currentTrack = 0;
            
            if (shuffle)
            {
                if (shuffleList.Count < listAudio.Count)
                {
                    FillAndShuffleList();
                }
                return await GetTrackPlayAsync(shuffleList[(int)currentTrack], prinud);
            }
            
            return await GetTrackPlayAsync((long)currentTrack, prinud);
        }
        
        /// <summary>
        /// Устанавливает следующий трек для воспроизведения
        /// </summary>
        public void setNextTrackForPlay()
        {
            if (currentTrack >= countTracks - 1 && countTracks != -1)
                currentTrack = 0;
            else
            {
                currentTrack++;
            }
        }
        
        /// <summary>
        /// Устанавливает предыдущий трек для воспроизведения
        /// </summary>
        public void setPreviusTrackForPlay()
        {
            if (currentTrack <= 0)
                currentTrack = countTracks - 1;
            else
            {
                currentTrack--;
            }
        }
        
        /// <summary>
        /// Воспроизводит плейлист
        /// </summary>
        public void PlayThis()
        {
            this.currentTrack = 0;
            AudioPlayer.PlayList(this);
        }
        
        /// <summary>
        /// Воспроизводит трек
        /// </summary>
        /// <param name="numberInList">Номер трека в списке</param>
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
        
        /// <summary>
        /// Делится треком во ВКонтакте
        /// </summary>
        public async void shareToVK()
        {
            try
            {
                var a = (await GetTrackPlay()).audio;
                if (a != null)
                    await VK.api.Audio.SetBroadcastAsync(
                       a.OwnerId + "_" + a.Id
                        );
            }
            catch (Exception e)
            {
                // Обработка исключения
            }
        }
        
        #endregion
        
        #region Методы для работы с выбором треков
        
        /// <summary>
        /// Выбирает аудиозапись
        /// </summary>
        /// <param name="extended">Расширенная аудиозапись</param>
        public void SelectAudio(ExtendedAudio extended)
        {
            if (extended.iVKGetAudio != this)
                return;
            
            if (selectedAudio.Contains(extended))
                return;
            
            selectedAudio.Add(extended);
            extended.trackSelectChangedInvoke(new ExtendedAudio.SelectedChange(true));
        }
        
        /// <summary>
        /// Отменяет выбор аудиозаписи
        /// </summary>
        /// <param name="extended">Расширенная аудиозапись</param>
        public void unselectAudio(ExtendedAudio extended)
        {
            if (extended.iVKGetAudio != this)
                return;
            
            if (!selectedAudio.Contains(extended))
                return;
            
            selectedAudio.Remove(extended);
            extended.trackSelectChangedInvoke(new ExtendedAudio.SelectedChange(false));
        }
        
        /// <summary>
        /// Проверяет, выбрана ли аудиозапись
        /// </summary>
        /// <param name="extended">Расширенная аудиозапись</param>
        /// <returns>Флаг выбора</returns>
        public bool AudioIsSelect(ExtendedAudio extended)
        {
            return selectedAudio.Contains(extended);
        }
        
        /// <summary>
        /// Последняя выбранная аудиозапись
        /// </summary>
        private ExtendedAudio lastSelected = null;
        
        /// <summary>
        /// Изменяет выбор аудиозаписи
        /// </summary>
        /// <param name="dataTrack">Расширенная аудиозапись</param>
        /// <param name="shiftPressend">Флаг нажатия Shift</param>
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
        
        /// <summary>
        /// Получает список выбранных аудиозаписей
        /// </summary>
        /// <returns>Список расширенных аудиозаписей</returns>
        public List<ExtendedAudio> getSelectedList()
        {
            return new List<ExtendedAudio>(selectedAudio.ToList());
        }
        
        /// <summary>
        /// Отменяет выбор всех аудиозаписей
        /// </summary>
        internal void deselectAll()
        {
            foreach (var item in selectedAudio)
            {
                item.trackSelectChangedInvoke(new ExtendedAudio.SelectedChange(false));
            }
            
            selectedAudio.Clear();
        }
        
        #endregion
        
        #region Методы для работы с перемешиванием
        
        /// <summary>
        /// Заполняет и перемешивает список
        /// </summary>
        public void FillAndShuffleList()
        {
            int trackCount = countTracks == -1 ? listAudio.Count : (int)countTracks;
            
            // Инициализируем shuffleList
            shuffleList = Enumerable.Range(0, trackCount).ToList();
            
            // Перемешиваем список
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
            
            // Находим индекс текущего трека в перемешанном списке
            if (currentTrack.HasValue)
            {
                int currentTrackIndex = shuffleList.IndexOf((int)currentTrack);
                // Обновляем currentTrack на индекс в перемешанном списке
                currentTrack = currentTrackIndex;
            }
        }
        
        ///summary>
        /// Устанавливает перемешивание
        /// </summary>
        public void setShuffle()
        {
            if (!shuffle)
            {
                FillAndShuffleList();
                shuffle = true;
            }
            NotifyOnListUpdate();
        }
        
        /// <summary>
        /// Отменяет перемешивание списка
        /// </summary>
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
        
        #endregion
        
        #region Методы для работы с фотографиями
        
        /// <summary>
        /// Получает URI фотографии
        /// </summary>
        /// <returns>URI фотографии</returns>
        public abstract Uri getPhoto();
        
        /// <summary>
        /// Получает список URI фотографий
        /// </summary>
        /// <returns>Список URI фотографий</returns>
        public abstract List<string> getPhotosList();
        
        #endregion
        
        #region Методы для работы с названием
        
        /// <summary>
        /// Получает название
        /// </summary>
        /// <returns>Название</returns>
        public abstract string getName();
        
        #endregion
        
        #region Методы для уведомлений
        
        /// <summary>
        /// Уведомляет об обновлении количества
        /// </summary>
        public void countUpdated() { onCountUpDated?.RaiseEvent(this, EventArgs.Empty); }
        
        /// <summary>
        /// Уведомляет об обновлении информации
        /// </summary>
        public void InfoUpdated() { onCountUpDated?.RaiseEvent(this, EventArgs.Empty); }
        
        /// <summary>
        /// Уведомляет об обновлении названия
        /// </summary>
        public void NameUpdated() { onNameUpdated?.RaiseEvent(this, EventArgs.Empty); }
        
        /// <summary>
        /// Уведомляет об обновлении фотографии
        /// </summary>
        public void PhotoUpdated() { onPhotoUpdated?.RaiseEvent(this, EventArgs.Empty); }
        
        /// <summary>
        /// Уведомляет об ошибке загрузки
        /// </summary>
        /// <param name="e">Исключение</param>
        public void NotifyonErrorLoad(Exception e)
        {
            onErrorLoad?.Invoke(this, new ErrorLoad(e));
        }
        
        /// <summary>
        /// Уведомляет об обновлении списка
        /// </summary>
        public void NotifyOnListUpdate()
        {
            if (shuffle && countTracks == -1)
            {
                List<int> shuffleListex = new List<int>();
                
                for (int i = shuffleList.Count; listAudio.Count - 1 > i; i++)
                {
                    shuffleListex.Add(i);
                }
                
                Random rng = new Random();
                int n = shuffleListex.Count;
                while (n > 1)
                {
                    n--;
                    int k = rng.Next(n + 1);
                    int value = shuffleListex[k];
                    shuffleListex[k] = shuffleListex[n];
                    shuffleListex[n] = value;
                }
                
                foreach (var item in shuffleListex)
                {
                    shuffleList.Add(item);
                }
            }
            
            var tempList = new List<TaskCompletionSource<bool>>(tcs);
            tcs.Clear();
            foreach (var item in tempList)
            {
                item.TrySetResult(true);
            }
            
            getLoadedTracks = false;
            onListUpdate?.Invoke(this, EventArgs.Empty);
        }
        
        #endregion
        
        #region Методы для работы с номерами
        
        /// <summary>
        /// Обновляет номера аудиозаписей
        /// </summary>
        public void updateNumbers()
        {
            for (int i = 0; i < listAudio.Count; i++)
            {
                listAudio[i].NumberInList = i;
            }
        }
        
        /// <summary>
        /// Удаляет трек из списка
        /// </summary>
        /// <param name="dataTrack">Расширенная аудиозапись</param>
        public void DeleteTrackFromList(ExtendedAudio dataTrack)
        {
            if (shuffle)
            {
                shuffleList.RemoveAt(shuffleList.IndexOf((int)dataTrack.NumberInList));
            }
            listAudio.Remove(dataTrack);
            selectedAudio.Remove(dataTrack);
            
            updateNumbers();
        }
        
        #endregion
        
        #region Методы для вызова событий
        
        /// <summary>
        /// Вызывает событие изменения воспроизводимого аудио
        /// </summary>
        /// <param name="trackdata">Расширенная аудиозапись</param>
        public void ChangePlayAudio(ExtendedAudio trackdata)
        {
            AudioPlayedChangeEvent?.Invoke(trackdata, EventArgs.Empty);
        }
        
        #endregion
        
        #region Методы для сохранения и восстановления
        
        /// <summary>
        /// Сохраняет в файл
        /// </summary>
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
        
        /// <summary>
        /// Восстанавливает из файла
        /// </summary>
        /// <param name="id">Идентификатор</param>
        /// <returns>Объект IVKGetAudio</returns>
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
        
        #endregion
    }
}