using ProtoBuf;

namespace MusicX.Shared.ListenTogether
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic, SkipConstructor = true)]
    public record User(string ConnectionId, long VkId);
}
