using Microsoft.AspNetCore.SignalR.Protocol;
using SignalR.Protobuf.Messages;
using SignalR.Protobuf.MessageSerializers.Base;
using SignalR.Protobuf.Util;

namespace SignalR.Protobuf.MessageSerializers;

internal class InvocationMessageSerializer : BaseMessageSerializer
{
    public override HubMessageType HubMessageType => HubMessageType.Invocation;
    public override Type MessageType => typeof(InvocationMessage);

    protected override IEnumerable<object> CreateItems(HubMessage message)
    {
        var invocationMessage = (InvocationMessage)message;

        yield return new InvocationMessageProtobuf
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
        var protobuf = (InvocationMessageProtobuf)items.First();
        var argumentProtobufs = items.Skip(1).ToArray();

        return new InvocationMessage(
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