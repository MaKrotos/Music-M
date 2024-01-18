using ProtoBuf;

namespace SignalR.Protobuf.Messages;

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class StreamInvocationMessageProtobuf
{
    public string? InvocationId { get; set; }
    public string Target { get; set; }
    public IList<string> Headers { get; set; } = new List<string>();
    public IList<string> StreamIds { get; set; } = new List<string>();
}