using Microsoft.UI.Dispatching;
using MusicX.Core.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK_UI3.Controllers;
using VkNet.Model.Attachments;
using VkNet.Utils;

namespace VK_UI3.VKs.IVK
{
    /// <summary>
    /// Опции для микса
    /// </summary>
    public record MixOptions(
        string Id,
        int Append = 0,
        ImmutableDictionary<string, ImmutableArray<string>>? Options = null,
        string? PromptEvents = null,
        string? Ref = null,
        string? EntityId = null)
    {
        /// <summary>
        /// Получает хэш-код
        /// </summary>
        /// <returns>Хэш-код</returns>
        public override int GetHashCode()
        {
            var hashCode = new HashCode();

            hashCode.Add(Id);
            hashCode.Add(Append);

            if (Options is not null)
            {
                foreach (var (key, values) in Options)
                {
                    hashCode.Add(key);
                    foreach (var item in values)
                    {
                        hashCode.Add(item);
                    }
                }
            }

            hashCode.Add(PromptEvents);
            hashCode.Add(Ref);
            hashCode.Add(EntityId);

            return hashCode.ToHashCode();
        }
    }

    /// <summary>
    /// Класс для работы с миксами аудиозаписей
    /// </summary>
    public class MixAudio : IVKGetAudio
    {
        #region Поля и свойства

        /// <summary>
        /// Текущее значение append для пагинации
        /// </summary>
        private int append = 0;

        /// <summary>
        /// Общее количество загруженных треков (используется для определения момента сброса append)
        /// </summary>
        private int totalLoadedTracks = 0;

        /// <summary>
        /// Флаг загрузки (защита от множественных вызовов)
        /// </summary>
        private bool isLoading = false;

        /// <summary>
        /// Объект для синхронизации доступа к общим ресурсам
        /// </summary>
        private readonly object lockObject = new object();

        /// <summary>
        /// Данные микса
        /// </summary>
        public MixOptions data;

        /// <summary>
        /// Максимальное количество попыток загрузки при ошибках
        /// </summary>
        private const int MAX_RETRY_ATTEMPTS = 3;

        /// <summary>
        /// Счетчик неудачных попыток
        /// </summary>
        private int failedAttempts = 0;

        /// <summary>
        /// Количество треков, после которого сбрасываем append в 0
        /// При достижении этого лимита API начнет выдавать новые треки
        /// </summary>
        private const int RESET_APPEND_AFTER = 100;

        #endregion

        #region Конструкторы

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="data">Данные микса</param>
        /// <param name="dispatcher">Диспетчер</param>
        public MixAudio(MixOptions data, DispatcherQueue dispatcher) : base(dispatcher)
        {
            this.data = data;
            this.append = data.Append; // Используем начальное значение append из параметров
            this.countTracks = -1; // Бесконечный плейлист
            this.itsAll = false; // Микс никогда не заканчивается

            // Загружаем первую порцию треков
            LoadInitialTracks();
        }

        /// <summary>
        /// Асинхронная загрузка начальных треков
        /// </summary>
        private async void LoadInitialTracks()
        {
            try
            {
                // Запрашиваем первую порцию треков с append = 0
                var audios = await VK.vkService.GetStreamMixAudios(
                    data.Id,
                    append,
                    50,
                    options: data.Options,
                    data.PromptEvents,
                    data.Ref,
                    data.EntityId);

                if (audios != null && audios.Any())
                {
                    lock (lockObject)
                    {
                        // Добавляем треки в список
                        foreach (var item in audios)
                        {
                            listAudio.Add(new Helpers.ExtendedAudio(item, this));
                        }
                        totalLoadedTracks += audios.Count;
                        append++; // Увеличиваем append для следующей загрузки
                        failedAttempts = 0; // Сбрасываем счетчик ошибок
                    }

                    // Уведомляем об обновлении списка
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        NotifyOnListUpdate();
                    });

                    // Запускаем воспроизведение с первого трека
                    this.currentTrack = 0;
                    VK_UI3.Services.MediaPlayerService.PlayList(this);
                }
                else
                {
                    // Если первый запрос вернул пустой список, пробуем еще раз с append = 0
                    await Task.Delay(1000);
                    append = 0;
                    LoadInitialTracks();
                }
            }
            catch (Exception e)
            {
                // Логируем ошибку и вызываем событие
                Console.WriteLine($"Ошибка при инициализации микса: {e.Message}");
                onErrorLoad?.Invoke(this, new ErrorLoad(e));

                // Пробуем перезагрузить через 2 секунды
                await Task.Delay(2000);
                LoadInitialTracks();
            }
        }

        #endregion

        #region Методы для получения данных

        /// <summary>
        /// Получает количество треков
        /// </summary>
        /// <returns>Количество треков</returns>
        public override long? getCount()
        {
            lock (lockObject)
            {
                return listAudio.Count;
            }
        }

        /// <summary>
        /// Получает название микса
        /// </summary>
        /// <returns>Название</returns>
        public override string getName() => null;

        /// <summary>
        /// Получает список URI фотографий
        /// </summary>
        /// <returns>Список URI фотографий</returns>
        public override List<string> getPhotosList() => null;

        /// <summary>
        /// Получает URI фотографии
        /// </summary>
        /// <returns>URI фотографии</returns>
        public override Uri getPhoto() => null;

        #endregion

        #region Методы для получения треков

        /// <summary>
        /// Переопределяем свойство currentTrack для ранней подгрузки треков
        /// </summary>
        public new long? currentTrack
        {
            get { return base.currentTrack; }
            set
            {
                base.currentTrack = value;

                // Проверяем, нужно ли подгрузить новые треки
                if (value.HasValue && listAudio.Count > 0)
                {
                    var remaining = listAudio.Count - (int)value.Value - 1;

                    // Если осталось меньше 5 треков до конца - запускаем подгрузку
                    if (remaining < 5 && !isLoading)
                    {
                        lock (lockObject)
                        {
                            if (!isLoading && !getLoadedTracks)
                            {
                                Task.Run(() => GetTracks());
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Получает следующую порцию треков
        /// </summary>
        public override async void GetTracks()
        {
            // Блокируем повторные вызовы
            lock (lockObject)
            {
                if (isLoading || getLoadedTracks)
                    return;

                isLoading = true;
                getLoadedTracks = true;
            }

            try
            {
                // Если загружено много треков - сбрасываем append в 0
                // Это заставит API выдать новую порцию треков, а не продолжать старую
                if (totalLoadedTracks >= RESET_APPEND_AFTER)
                {
                    lock (lockObject)
                    {
                        Console.WriteLine($"Сброс append с {append} на 0 (загружено {totalLoadedTracks} треков)");
                        append = 0;
                        totalLoadedTracks = 0;
                        // Старые треки остаются в списке, новые будут добавлены к ним
                    }
                }

                var currentAppend = append;
                Console.WriteLine($"Запрос треков: append={currentAppend}, всего загружено: {totalLoadedTracks}");

                // Запрашиваем треки с текущим значением append
                var tracks = await VK.vkService.GetStreamMixAudios(
                    data.Id,
                    currentAppend,
                    50,
                    options: data.Options,
                    data.PromptEvents,
                    data.Ref,
                    data.EntityId);

                lock (lockObject)
                {
                    if (tracks != null && tracks.Any())
                    {
                        // Добавляем новые треки в список
                        foreach (var item in tracks)
                        {
                            listAudio.Add(new Helpers.ExtendedAudio(item, this));
                        }

                        totalLoadedTracks += tracks.Count;
                        append++; // Увеличиваем append для следующей загрузки
                        failedAttempts = 0; // Сбрасываем счетчик ошибок

                        Console.WriteLine($"Загружено {tracks.Count} треков. Всего: {listAudio.Count}");

                        // Уведомляем об обновлении списка
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            NotifyOnListUpdate();
                            onCountUpDated?.RaiseEvent(this, EventArgs.Empty);
                        });
                    }
                    else
                    {
                        // Если API вернул пустой список - увеличиваем счетчик ошибок
                        failedAttempts++;
                        Console.WriteLine($"Пустой ответ от API (попытка {failedAttempts}/{MAX_RETRY_ATTEMPTS})");

                        // Если превышено количество попыток - сбрасываем append
                        if (failedAttempts >= MAX_RETRY_ATTEMPTS)
                        {
                            Console.WriteLine("Сброс append на 0 после пустых ответов");
                            append = 0;
                            totalLoadedTracks = 0;
                            failedAttempts = 0;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // Логируем ошибку
                Console.WriteLine($"Ошибка при загрузке треков микса: {e.Message}");

                lock (lockObject)
                {
                    failedAttempts++; // Увеличиваем счетчик ошибок
                }

                // Вызываем событие ошибки
                DispatcherQueue.TryEnqueue(() =>
                {
                    onErrorLoad?.Invoke(this, new ErrorLoad(e));
                });
            }
            finally
            {
                // Снимаем блокировки
                lock (lockObject)
                {
                    isLoading = false;
                    getLoadedTracks = false;
                }
            }
        }

        #endregion
    }
}