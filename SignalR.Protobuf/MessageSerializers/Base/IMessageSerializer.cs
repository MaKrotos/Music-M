using System.Buffers;
using Microsoft.AspNetCore.SignalR.Protocol;

namespace SignalR.Protobuf.MessageSerializers.Base;

internal interface IMessageSerializer
{
    HubMessageType HubMessageType { get; }
    Type MessageType { get; }

    void WriteMessage(
        HubMessage message, 
        IBufferWriter<byte> output, 
        IReadOnlyDictionary<Type, int> protobufTypeToIndexMap
    );

    bool TryParseMessage(
        ref ReadOnlySequence<byte> input, 
        out HubMessage message,
        IReadOnlyDictionary<int, Type> protobufIndexToTypeMap
    );
}