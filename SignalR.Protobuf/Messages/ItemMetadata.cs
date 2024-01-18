using ProtoBuf;

namespace SignalR.Protobuf.Messages;

[ProtoContract]
public partial class ItemMetadata
{
    [ProtoMember(1)]
    public IList<int> TypesAndSizes { get; set; } = new List<int>();
}