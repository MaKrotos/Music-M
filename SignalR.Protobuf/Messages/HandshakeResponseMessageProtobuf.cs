using ProtoBuf;

namespace SignalR.Protobuf.Messages;

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class HandshakeResponseMessageProtobuf
{
    public string? Error { get; set; }
}