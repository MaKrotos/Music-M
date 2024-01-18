using ProtoBuf;

namespace SignalR.Protobuf.Messages;

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class CompletionMessageProtobuf
{
    public string InvocationId { get; set; }
    public IList<string> Headers { get; set; } = new List<string>();
    public string? Error { get; set; }
    public string? Result { get; set; }
    public bool HasResult { get; set; }
}