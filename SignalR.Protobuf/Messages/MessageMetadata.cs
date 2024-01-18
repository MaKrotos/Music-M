using ProtoBuf;

namespace SignalR.Protobuf.Messages;

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class MessageMetadata
{
    public IList<ItemMetadata> Items { get; set; } = new List<ItemMetadata>();
}