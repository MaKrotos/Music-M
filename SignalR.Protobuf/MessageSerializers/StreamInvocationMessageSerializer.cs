using Microsoft.AspNetCore.SignalR.Protocol;
using SignalR.Protobuf.Messages;
using SignalR.Protobuf.MessageSerializers.Base;
using SignalR.Protobuf.Util;

namespace SignalR.Protobuf.MessageSerializers;

internal class StreamInvocationMessageSerializer : BaseMessageSerializer
{
    public override HubMessageType HubMessageType => HubMessageType.StreamInvocation;
    public override Type MessageType => typeof(StreamInvocationMessage);

    protected override IEnumerable<object> CreateItems(HubMessage message)
    {
        var invocationMessage = (StreamInvocationMessage) message;

        yield return new StreamInvocationMessageProtobuf
        {
            Headers = invocationMessage.Headers.Flatten().ToList(),
            InvocationId = invocationMessage.InvocationId,
            Target = invocationMessage.Target,
            StreamIds = new List<string>(invocationMessage.StreamIds ?? Enumerable.Empty<string>())
        };

        foreach (var argument in invocationMessage.Arguments)
        {
            yield return argument;
        }
    }

    protected override HubMessage CreateHubMessage(IReadOnlyList<object> items)
    {
        var protobuf = (StreamInvocationMessageProtobuf) items.First();
        var argumentProtobufs = items.Skip(1).ToArray();

        return new StreamInvocationMessage(
            protobuf.InvocationId, 
            protobuf.Target, 
            argumentProtobufs,
            protobuf.StreamIds.ToArray()
        )
        {
            Headers = protobuf.Headers.Unflatten()
        };
    }
}