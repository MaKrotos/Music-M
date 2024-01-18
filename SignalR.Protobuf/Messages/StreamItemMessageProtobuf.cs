using ProtoBuf;

namespace SignalR.Protobuf.Messages;

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class StreamItemMessageProtobuf
{
    public string InvocationId { get; set; }
    public IList<string> Headers { get; set; } = new List<string>();
}