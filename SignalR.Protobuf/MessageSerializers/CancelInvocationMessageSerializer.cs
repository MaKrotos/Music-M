using Microsoft.AspNetCore.SignalR.Protocol;
using SignalR.Protobuf.Messages;
using SignalR.Protobuf.MessageSerializers.Base;
using SignalR.Protobuf.Util;

namespace SignalR.Protobuf.MessageSerializers;

internal class CancelInvocationMessageSerializer : BaseMessageSerializer
{
    public override HubMessageType HubMessageType => HubMessageType.CancelInvocation;
    public override Type MessageType => typeof(CancelInvocationMessage);

    protected override IEnumerable<object> CreateItems(HubMessage message)
    {
        var cancelInvocationMessage = (CancelInvocationMessage)message;

        yield return new CancelInvocationMessageProtobuf
        {
            InvocationId = cancelInvocationMessage.InvocationId,
            Headers = cancelInvocationMessage.Headers.Flatten().ToList()
        };
    }

    protected override HubMessage CreateHubMessage(IReadOnlyList<object> items)
    {
        var protobuf = (CancelInvocationMessageProtobuf)items.Single();

        return new CancelInvocationMessage(protobuf.InvocationId)
        {
            Headers = protobuf.Headers.Unflatten()
        };
    }
}