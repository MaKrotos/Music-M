using ProtoBuf;

namespace SignalR.Protobuf.Messages;

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class HandshakeRequestMessageProtobuf
{
    public string Protocol { get; set; }
    public int Version { get; set; }
}