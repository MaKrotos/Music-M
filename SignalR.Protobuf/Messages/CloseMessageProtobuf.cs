using ProtoBuf;

namespace SignalR.Protobuf.Messages;

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class CloseMessageProtobuf
{
    public string? Error { get; set; }
}