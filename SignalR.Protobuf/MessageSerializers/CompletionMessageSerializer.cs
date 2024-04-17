using Microsoft.AspNetCore.SignalR.Protocol;
using SignalR.Protobuf.Messages;
using SignalR.Protobuf.MessageSerializers.Base;
using SignalR.Protobuf.Util;

namespace SignalR.Protobuf.MessageSerializers;

internal class CompletionMessageSerializer : BaseMessageSerializer
{
    public override HubMessageType HubMessageType => HubMessageType.Completion;
    public override Type MessageType => typeof(CompletionMessage);

    protected override IEnumerable<object> CreateItems(HubMessage message)
    {
        var completionMessage = (CompletionMessage)message;

        yield return new CompletionMessageProtobuf
        {
            InvocationId = completionMessage.InvocationId,
            Headers = completionMessage.Headers.Flatten().ToList(),
            Error = completionMessage.Error,
            HasResult = completionMessage.HasResult
        };

        yield return completionMessage.Result;
    }

    protected override HubMessage CreateHubMessage(IReadOnlyList<object> items)
    {
        var protobuf = (CompletionMessageProtobuf)items[0];
        var resultProtobuf = items[1];

        return new CompletionMessage(
            protobuf.InvocationId,
            protobuf.Error,
            resultProtobuf,
            protobuf.HasResult
        );
    }
}