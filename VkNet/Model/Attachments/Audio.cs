using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using VkNet.Enums;
using VkNet.Model.GroupUpdate;
using VkNet.Utils;

namespace VkNet.Model.Attachments
{
	/// <summary>
	/// Аудиозапись пользователя или группы.
	/// См. описание http://vk.com/dev/audio_object
	/// </summary>
	[Serializable]
	public class Audio : MediaAttachment, IGroupUpdate
	{
		/// <inheritdoc />
		protected override string Alias => "audio";

		/// <summary>
		/// Исполнитель аудиозаписи.
		/// </summary>
		[JsonProperty("artist")]
		public string Artist { get; set; }

		// Additional properties (for any data not explicitly declared)
		[JsonExtensionData]
		public Dictionary<string, object> AdditionalData { get; set; }

		/// <summary>
		/// Поиск трека выключен
		/// </summary>
        [JsonProperty("no_search")]
        public bool No_search { get; set; } = false;



		/// <summary>
		/// Посталвен ли дизлайе
		/// </summary>
        [JsonProperty("dislike")]
		public	bool Dislike { get; set; } = false;
        /// <summary>
        /// Название композиции.
        /// </summary>
        [JsonProperty("title")]
		public string Title { get; set; }

		/// <summary>
		/// Длительность аудиозаписи в секундах.
		/// </summary>
		[JsonProperty("duration")]
		public int Duration { get; set; }

		/// <summary>
		/// Дата добавления.
		/// </summary>
		[JsonConverter(typeof(UnixDateTimeConverter))]
		[JsonProperty("date")]
		public DateTime? Date { get; set; }

		/// <summary>
		/// Ссылка на аудиозапись (привязана к ip-адресу клиентского приложения).
		/// </summary>
		[JsonProperty("url")]
		public Uri Url { get; set; }

		/// <summary>
		/// Альбом аудиозаписи.
		/// </summary>
		[JsonProperty("album")]
		public AudioAlbum Album { get; set; }

		/// <summary>
		/// <c>true</c>, если аудиозапись лицензируется.
		/// </summary>
		[JsonProperty("is_licensed")]
		public bool? IsLicensed { get; set; }

		/// <summary>
		/// <c>true</c>, если аудиозапись в высоком качестве.
		/// </summary>
		[JsonProperty("is_hq")]
		public bool? IsHq { get; set; }

		/// <summary>
		/// Жанр аудиозаписи.
		/// </summary>
		[JsonProperty("track_genre_id")]
		public AudioGenre? TrackGenre { get; set; }

		/// <summary>
		/// Жанр аудиозаписи.
		/// </summary>
		[JsonProperty("genre_id")]
		public AudioGenre? Genre { get; set; }

		/// <summary>
		/// Идентификатор текста аудиозаписи (если доступно).
		/// </summary>
		[JsonProperty("lyrics_id")]
		public long? LyricsId { get; set; }

		/// <summary>
		/// Имеет ли текст
		/// </summary>
		[JsonProperty("has_lyrics")]
		public bool HasLyrics { get; set; } = false;

        /// <summary>
        /// Содержит ли трек ненормативную лексику.
        /// </summary>
        [JsonProperty("is_explicit")]
		public bool IsExplicit { get; set; }

		/// <summary>
		/// Получено экспериментально
		/// </summary>
		[JsonProperty("is_focus_track")]
		public bool IsFocusTrack { get; set; }

		/// <summary>
		/// Возможно ли использование обложки этого трека в "Историях" (получено экспериментально)
		/// </summary>
		[JsonProperty("stories_cover_allowed")]
		public bool? StoriesCoverAllowed { get; set; }

		/// <summary>
		/// Возможно ли использование этого трека в "Историях" (получено экспериментально)
		/// </summary>
		[JsonProperty("stories_allowed")]
		public bool? StoriesAllowed { get; set; }

		/// <summary>
		/// Возможно ли использование этого трека в "Клипах" (получено экспериментально)
		/// </summary>
		[JsonProperty("short_videos_allowed")]
		public bool? ShortVideosAllowed { get; set; }

		/// <summary>
		/// Список главных исполнителей.
		/// </summary>
		[JsonProperty("main_artists")]
		public IEnumerable<AudioArtist> MainArtists { get; set; }

		/// <summary>
		/// Список второстепенных исполнителей.
		/// </summary>
		[JsonProperty("featured_artists")]
		public IEnumerable<AudioArtist> FeaturedArtists { get; set; }

        //		Key	"release_audio_id"	string

        /// <summary>
        /// ID оригинала
        /// </summary>
		[JsonProperty("release_audio_id")]
        public string? Release_audio_id { get; set; }


        /// <summary>
        /// Подзаголовок(?)  композиции.
        /// </summary>
        [JsonProperty("subtitle")]
		public string Subtitle { get; set; }

		/// <summary>
		/// Неизвестно (получено экспериментально).
		/// </summary>
		[JsonProperty("track_code")]
		public string TrackCode { get; set; }


        [JsonProperty("audio_chart_info")]
		public	AudioChartInfo AudioChartInfo { get; set; }

        [JsonProperty("thumb")]
        public AudioCover? Thumb { get; set; }



        /*
		 * "[ads, {
		  ""content_id"": ""-2001619099_128619099"",
		  ""duration"": ""171"",
		  ""account_age_type"": ""3"",
		  ""puid1"": ""554"",
		  ""puid22"": ""14""
		}]"
		 */

        #region Методы

        /// <summary>
        /// Разобрать из json.
        /// </summary>
        /// <param name="response"> Ответ сервера. </param>
        /// <returns> </returns>
        public static Audio FromJson(VkResponse response)
		{
			return new Audio
			{
				Id = response["id"],
				OwnerId = response["owner_id"],
				Artist = response["artist"],
				Title = response["title"],
				Duration = response["duration"],
				Url = response["url"],
				LyricsId = response["lyrics_id"],
				Album = response["album"],
				AccessKey = response["access_key"],
				IsHq = response["is_hq"],
				IsLicensed = response["is_licensed"],
				TrackGenre = response["track_genre_id"],
				IsExplicit = response["is_explicit"],
				Genre = response["genre_id"] ?? response["genre"],
				Date = response["date"],
				MainArtists = response["main_artists"].ToReadOnlyCollectionOf<AudioArtist>(x => x),
				FeaturedArtists = response["featured_artists"].ToReadOnlyCollectionOf<AudioArtist>(x => x),
				Subtitle = response["subtitle"]
			};
		}

		/// <summary>
		/// Преобразование класса <see cref="Audio" /> в <see cref="VkParameters" />
		/// </summary>
		/// <param name="response"> Ответ сервера. </param>
		/// <returns>Результат преобразования в <see cref="Audio" /></returns>
		public static implicit operator Audio(VkResponse response)
		{
			if (response == null)
			{
				return null;
			}

			return response.HasToken()
				? FromJson(response)
				: null;
		}



	#endregion
	}
}
