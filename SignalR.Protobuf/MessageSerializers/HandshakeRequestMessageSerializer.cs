using Microsoft.AspNetCore.SignalR.Protocol;
using SignalR.Protobuf.Messages;
using SignalR.Protobuf.MessageSerializers.Base;

namespace SignalR.Protobuf.MessageSerializers;

internal class HandshakeRequestMessageSerializer : BaseMessageSerializer
{
    public override HubMessageType HubMessageType => HubMessageType.HandshakeRequest;
    public override Type MessageType => typeof(HandshakeRequestMessage);

    protected override IEnumerable<object> CreateItems(HubMessage message)
    {
        var handshakeRequestMessage = (HandshakeRequestMessage)message;
        yield return new HandshakeRequestMessageProtobuf
        {
            Protocol = handshakeRequestMessage.Protocol,
            Version = handshakeRequestMessage.Version
        };
    }

    protected override HubMessage CreateHubMessage(IReadOnlyList<object> items)
    {
        var protobuf = (HandshakeRequestMessageProtobuf)items.Single();
        return new HandshakeRequestMessage(protobuf.Protocol, protobuf.Version);
    }
}