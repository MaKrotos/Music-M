using System.Text.Json.Serialization;
using ProtoBuf;

namespace MusicX.Shared.Player;

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic, SkipConstructor = true)]
public sealed record PlaylistTrack(string Title, string Subtitle, AlbumId? AlbumId,
                                   ICollection<TrackArtist> MainArtists,
                                   ICollection<TrackArtist>? FeaturedArtists, TrackData Data)
{
    public bool Equals(PlaylistTrack? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return AlbumId == other.AlbumId && Data == other.Data;
    }
    public override int GetHashCode() => HashCode.Combine(AlbumId?.GetHashCode(), Data.GetHashCode());
}

[JsonDerivedType(typeof(VkTrackData), "vk")]
[JsonDerivedType(typeof(BoomTrackData), "boom")]
[ProtoContract(ImplicitFields = ImplicitFields.AllPublic, SkipConstructor = true)]
[ProtoInclude(100, typeof(BoomTrackData))]
[ProtoInclude(101, typeof(VkTrackData))]
[ProtoInclude(102, typeof(DownloaderData))]
public abstract record TrackData(string Url, bool IsLiked, bool IsExplicit, TimeSpan Duration);

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic, SkipConstructor = true)]
public sealed record BoomTrackData(string Url, bool IsLiked, bool IsExplicit, TimeSpan Duration, string Id) : TrackData(
    Url, IsLiked, IsExplicit, Duration)
{
    public bool Equals(BoomTrackData? other) => Id == other?.Id;
    public override int GetHashCode() => Id.GetHashCode();
}

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic, SkipConstructor = true)]
public record IdInfo(long Id, long OwnerId, string AccessKey)
{
    public string ToOwnerIdString() => $"{OwnerId}_{Id}";
}

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic, SkipConstructor = true)]
public sealed record VkTrackData(string Url, bool IsLiked, bool IsExplicit, bool? HasLyrics, TimeSpan Duration,
                                 IdInfo Info, string TrackCode, string? ParentBlockId,
                                 IdInfo? Playlist) : TrackData(Url, IsLiked, IsExplicit, Duration)
{
    public bool Equals(VkTrackData? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        
        return Info == other.Info;
    }
    public override int GetHashCode() => Info.GetHashCode();
}
                          
[ProtoContract(ImplicitFields = ImplicitFields.AllPublic, SkipConstructor = true)]
public record DownloaderData
    (string Url, bool IsLiked, bool IsExplicit, TimeSpan Duration, string PlaylistName) : TrackData(
        Url, IsLiked, IsExplicit, Duration);