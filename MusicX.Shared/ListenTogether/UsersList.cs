using ProtoBuf;

namespace MusicX.Shared.ListenTogether
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic, SkipConstructor = true)]
    public record UsersList(List<User> Users);
}
