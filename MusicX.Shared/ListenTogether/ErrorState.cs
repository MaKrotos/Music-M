using ProtoBuf;

namespace MusicX.Shared.ListenTogether;

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic, SkipConstructor = true)]
public record ErrorState(bool Success, string? Message = null);