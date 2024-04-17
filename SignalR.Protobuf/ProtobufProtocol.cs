using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using ProtoBuf.Meta;
using SignalR.Protobuf.Messages;
using SignalR.Protobuf.MessageSerializers;
using SignalR.Protobuf.MessageSerializers.Base;
using System.Buffers;

namespace SignalR.Protobuf;

public class ProtobufProtocol : IHubProtocol
{
    private static readonly Dictionary<Type, IMessageSerializer> TypeToSerializerMap = new Dictionary<Type, IMessageSerializer>();
    private static readonly Dictionary<HubMessageType, IMessageSerializer> EnumTypeToSerializerMap = new Dictionary<HubMessageType, IMessageSerializer>();

    static ProtobufProtocol()
    {
        var serializers = new IMessageSerializer[]
        {
            new CancelInvocationMessageSerializer(),
            new CloseMessageSerializer(),
            new CompletionMessageSerializer(),
            new HandshakeRequestMessageSerializer(),
            new HandshakeResponseMessageSerializer(),
            new InvocationMessageSerializer(),
            new PingMessageSerializer(),
            new StreamInvocationMessageSerializer(),
            new StreamItemMessageSerializer()
        };

        foreach (var serializer in serializers)
        {
            EnumTypeToSerializerMap[serializer.HubMessageType] = serializer;
            TypeToSerializerMap[serializer.MessageType] = serializer;
        }
    }

    private readonly Dictionary<int, Type> _protobufIndexToTypeMap = new Dictionary<int, Type>();
    private readonly Dictionary<Type, int> _protobufTypeToIndexMap = new Dictionary<Type, int>();

    public ProtobufProtocol(IReadOnlyDictionary<int, Type> protobufTypes)
    {
        foreach (var pair in protobufTypes)
        {
            var index = pair.Key;
            var type = pair.Value;

            if (index < 0)
            {
                throw new ArgumentException(
                    $"Index \"{index}\" for type {type} is less than 0",
                    nameof(protobufTypes)
                );
            }
        }

        var allPairs = protobufTypes
                       .Select(pair => (pair.Key, pair.Value))
                       .Concat(
                           new[]
                           {
                               // Leave a 64 int gap for special type cases
                               // (ex: nulls and enumerables)
                               (-65, typeof(MessageMetadata)),
                               (-66, typeof(ItemMetadata)),
                               (-67, typeof(CancelInvocationMessageProtobuf)),
                               (-68, typeof(CloseMessageProtobuf)),
                               (-69, typeof(CompletionMessageProtobuf)),
                               (-70, typeof(HandshakeRequestMessageProtobuf)),
                               (-71, typeof(HandshakeResponseMessageProtobuf)),
                               (-72, typeof(InvocationMessageProtobuf)),
                               (-73, typeof(StreamInvocationMessageProtobuf)),
                               (-74, typeof(StreamItemMessageProtobuf))
                           }
                       );
        foreach (var (index, type) in allPairs)
        {
            _protobufIndexToTypeMap[index] = type;
            _protobufTypeToIndexMap[type] = index;
            RuntimeTypeModel.Default.Add(type);
        }

        foreach (var modelsMessageType in _protobufTypeToIndexMap.Keys)
        {
            if (!RuntimeTypeModel.Default.CanSerialize(modelsMessageType))
            {
                throw new Exception($"{modelsMessageType} is not mapped in {nameof(ProtobufProtocol)}");
            }
        }
    }

    public string Name => nameof(ProtobufProtocol);
    public int Version => 3;
    public TransferFormat TransferFormat => TransferFormat.Binary;
    public bool IsVersionSupported(int version) => version == Version;

    public ReadOnlyMemory<byte> GetMessageBytes(HubMessage message)
    {
        return HubProtocolExtensions.GetMessageBytes(this, message);
    }

    public void WriteMessage(HubMessage message, IBufferWriter<byte> output)
    {
        var serializer = TypeToSerializerMap[message.GetType()];
        output.Write(new[] { (byte)serializer.HubMessageType });
        serializer.WriteMessage(message, output, _protobufTypeToIndexMap);
    }

    public bool TryParseMessage(ref ReadOnlySequence<byte> input, IInvocationBinder binder, out HubMessage message)
    {
        // At least one byte is needed to determine the type of message
        if (input.IsEmpty)
        {
            message = null;
            return false;
        }

        var enumType = (HubMessageType)input.Slice(0, 1).ToArray()[0];
        var processedSequence = input.Slice(1);

        var serializer = EnumTypeToSerializerMap[enumType];
        var successfullyParsed = serializer.TryParseMessage(
            ref processedSequence,
            out message,
            _protobufIndexToTypeMap
        );

        if (successfullyParsed)
        {
            input = processedSequence;
        }

        return successfullyParsed;
    }
}