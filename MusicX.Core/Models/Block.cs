using MusicX.Shared.ListenTogether.Radio;
using Newtonsoft.Json;

namespace MusicX.Core.Models
{
    public class Block
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data_type")]
        public string DataType { get; set; }

        [JsonProperty("layout")]
        public Layout Layout { get; set; }

        [JsonProperty("catalog_banner_ids")]
        public List<int> CatalogBannerIds { get; set; } = new List<int>();

        [JsonProperty("links_ids")]
        public List<string> LinksIds { get; set; } = new List<string>();

        [JsonProperty("buttons")]
        public List<Button> Buttons { get; set; }

        [JsonProperty("actions")]
        public List<Button> Actions { get; set; } = new List<Button> { };

        [JsonProperty("next_from")]
        public string NextFrom { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("listen_events")]
        public List<string> ListenEvents { get; set; }

        [JsonProperty("playlists_ids")]
        public List<string> PlaylistsIds { get; set; } = new List<string>();

        [JsonProperty("suggestions_ids")]
        public List<string> SuggestionsIds { get; set; } = new List<string>();

        [JsonProperty("artists_ids")]
        public List<string> ArtistsIds { get; set; } = new List<string>();

        [JsonProperty("badge")]
        public Badge Badge { get; set; }

        [JsonProperty("audios_ids")]
        public List<string> AudiosIds { get; set; } = new List<string>();

        [JsonProperty("curators_ids")]
        public List<long> CuratorsIds { get; set; } = new List<long>();

        [JsonProperty("group_ids")]
        public List<long> GroupIds { get; set; } = new List<long>();

        [JsonProperty("text_ids")]
        public List<string> TextIds { get; set; } = new List<string>();

        [JsonProperty("podcast_episodes_ids")]
        public List<string> PodcastEpisodesIds { get; set; } = new List<string>();

        [JsonProperty("podcast_slider_items_ids")]
        public List<string> PodcastSliderItemsIds { get; set; } = new List<string>();

        [JsonProperty("longreads_ids")]
        public List<string> LongreadsIds { get; set; } = new List<string>();

        [JsonProperty("videos_ids")]
        public List<string> VideosIds { get; set; } = new List<string>();

        [JsonProperty("artist_videos_ids")]
        public List<string> ArtistVideosIds { get; set; } = new List<string>();

        [JsonProperty("placeholder_ids")]
        public List<string> PlaceholdersIds { get; set; } = new List<string>();

        [JsonProperty("music_owners_ids")]
        public List<string> MusicOwnerIds { get; set; } = new List<string>();

        [JsonProperty("audio_followings_update_info_ids")]
        public List<string> FollowingUpdateInfoIds { get; set; } = new List<string>();

        [JsonProperty("group_items")]
        public List<CuratorGroup> GroupsItemsIds { get; set; } = new List<CuratorGroup>();

        [JsonExtensionData]
        public Dictionary<string, object> AdditionalData { get; set; }

        public List<Curator> Curators { get; set; } = new List<Curator>();
        public List<Text> Texts { get; set; } = new List<Text>();
        public List<Audio> Audios { get; set; } = new List<Audio>();

        [JsonProperty("Playlists")]
        public List<Playlist> Playlists { get; set; } = new List<Playlist>();

        [JsonProperty("Banners")]
        public List<CatalogBanner> Banners { get; set; } = new List<CatalogBanner>();
        public List<Link> Links { get; set; } = new List<Link>();
        public List<Suggestion> Suggestions { get; set; } = new List<Suggestion>();
        public List<Artist> Artists { get; set; } = new List<Artist>();
        public List<Group> Groups { get; set; } = new List<Group>();
        public List<PodcastSliderItem> PodcastSliderItems { get; set; } = new List<PodcastSliderItem>();
        public List<PodcastEpisode> PodcastEpisodes { get; set; } = new List<PodcastEpisode>();
        public List<Longread> Longreads { get; set; } = new List<Longread>();
        public List<RecommendedPlaylist> RecommendedPlaylists { get; set; } = new List<RecommendedPlaylist>();
        public List<Video> Videos { get; set; } = new List<Video>();
        public List<Video> ArtistVideos { get; set; } = new List<Video>();

        public List<Placeholder> Placeholders { get; set; } = new List<Placeholder>();

        public List<MusicOwner> MusicOwners { get; set; } = new List<MusicOwner>();

        public List<AudioFollowingsUpdateInfo> FollowingsUpdateInfos { get; set; } = new List<AudioFollowingsUpdateInfo>();

        public List<Station> Stations { get; set; } = new List<Station>();
    }
}
