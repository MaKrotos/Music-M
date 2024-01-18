using ProtoBuf;

namespace MusicX.Shared.Player;

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic, SkipConstructor = true)]
public record TrackArtist(string Name, ArtistId? Id);