using ProtoBuf;
using System.Text.Json.Serialization;

namespace MusicX.Shared.Player;

[JsonDerivedType(typeof(VkAlbumId), "vk")]
[JsonDerivedType(typeof(BoomAlbumId), "boom")]
[ProtoContract(ImplicitFields = ImplicitFields.AllPublic, SkipConstructor = true)]
[ProtoInclude(100, typeof(BoomAlbumId))]
[ProtoInclude(101, typeof(VkAlbumId))]
public abstract record AlbumId(string Name, string CoverUrl, string? BigCoverUrl);

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic, SkipConstructor = true)]
public sealed record VkAlbumId
    (long Id, long OwnerId, string AccessKey, string Name, string CoverUrl, string? BigCoverUrl) : AlbumId(Name, CoverUrl, BigCoverUrl)
{
    public bool Equals(VkAlbumId? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return Id == other.Id && OwnerId == other.OwnerId && AccessKey == other.AccessKey;
    }
    public override int GetHashCode() =>
        HashCode.Combine(Id.GetHashCode(), OwnerId.GetHashCode(), AccessKey.GetHashCode());
}

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic, SkipConstructor = true)]
public sealed record BoomAlbumId(string Id, string Name, string CoverUrl, string? BigCoverUrl) : AlbumId(Name, CoverUrl, BigCoverUrl)
{
    public bool Equals(BoomAlbumId? other) => Id == other?.Id;
    public override int GetHashCode() => Id.GetHashCode();
}