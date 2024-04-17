using Microsoft.AspNetCore.SignalR.Protocol;
using SignalR.Protobuf.Messages;
using SignalR.Protobuf.MessageSerializers.Base;
using SignalR.Protobuf.Util;

namespace SignalR.Protobuf.MessageSerializers;

internal class StreamItemMessageSerializer : BaseMessageSerializer
{
    public override HubMessageType HubMessageType => HubMessageType.StreamItem;
    public override Type MessageType => typeof(StreamItemMessage);

    protected override IEnumerable<object> CreateItems(HubMessage message)
    {
        var streamItemMessage = (StreamItemMessage)message;

        yield return new StreamItemMessageProtobuf
        {
            Headers = streamItemMessage.Headers.Flatten().ToList(),
            InvocationId = streamItemMessage.InvocationId
        };

        yield return streamItemMessage.Item;
    }

    protected override HubMessage CreateHubMessage(IReadOnlyList<object> items)
    {
        var protobuf = (StreamItemMessageProtobuf)items[0];
        var itemProtobuf = items[1];

        return new StreamItemMessage(protobuf.InvocationId, itemProtobuf)
        {
            Headers = protobuf.Headers.Unflatten()
        };
    }
}