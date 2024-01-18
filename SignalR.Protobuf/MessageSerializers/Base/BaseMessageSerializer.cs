using System.Buffers;
using Microsoft.AspNetCore.SignalR.Protocol;
using ProtoBuf;
using ProtoBuf.Meta;
using SignalR.Protobuf.Messages;
using SignalR.Protobuf.Util;

namespace SignalR.Protobuf.MessageSerializers.Base;

internal abstract class BaseMessageSerializer : IMessageSerializer
{
    public abstract HubMessageType HubMessageType { get; }
    public abstract Type MessageType { get; }

    protected abstract IEnumerable<object> CreateItems(HubMessage message);
    protected abstract HubMessage CreateHubMessage(IReadOnlyList<object> items);

    public void WriteMessage(
        HubMessage message,
        IBufferWriter<byte> output,
        IReadOnlyDictionary<Type, int> protobufTypeToIndexMap
    )
    {
        var itemsMetadata = CreateItems(message)
                            .Select(item => ItemMetadata.Create(item, protobufTypeToIndexMap))
                            .ToList();

        var metadata = new MessageMetadata
        {
            Items = itemsMetadata
        };

        var metadataByteSize = (int) Serializer.Measure(metadata).LengthOnly();

        var totalByteSize = 4 // Total byte size (int)
                            + 4 + metadataByteSize // Metadata byte size (int) and metadata itself
                            + itemsMetadata
                              .Select(itemMetadata => itemMetadata.CalculateTotalSizeBytes())
                              .Sum(); // Total bytes of the protobuf models

        var byteArray = ArrayPool<byte>.Shared.Rent(totalByteSize);
        try
        {
            using (var outputStream = new MemoryStream(byteArray))
            {
                // Total byte size
                outputStream.Write(BitConverter.GetBytes(totalByteSize), 0, 4);

                // Metadata byte size
                outputStream.Write(BitConverter.GetBytes(metadataByteSize), 0, 4);

                // Metadata
                Serializer.Serialize(outputStream, metadata);

                // Other protobufs
                foreach (var protobufItem in metadata.Items.SelectMany(item => item.NonNullProtobufs))
                {
                    RuntimeTypeModel.Default.Serialize(outputStream, protobufItem);
                }
            }

            output.Write(new ReadOnlySpan<byte>(byteArray, 0, totalByteSize));
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(byteArray);
        }
    }

    public bool TryParseMessage(
        ref ReadOnlySequence<byte> input,
        out HubMessage message,
        IReadOnlyDictionary<int, Type> protobufIndexToTypeMap
    )
    {
        // 4 bytes are required to read the length of the message
        if (input.Length < 4)
        {
            message = null;
            return false;
        }

        var totalByteSize = BitConverter.ToInt32(input.Slice(0, 4).ToArray(), 0);
        if (input.Length < totalByteSize)
        {
            message = null;
            return false;
        }

        var protobufInput = input.Slice(4);

        var arrayPool = ArrayPool<byte>.Shared;
        var byteArray = arrayPool.Rent((int) protobufInput.Length);
        try
        {
            protobufInput.CopyTo(byteArray);

            using (var inputStream = new MemoryStream(byteArray))
            {
                var metadata = new MessageMetadata().MergeFixedDelimitedFrom(inputStream);

                var items = new List<object>(metadata.Items.Count);
                foreach (var itemMetadata in metadata.Items)
                {
                    var item = itemMetadata.CreateItem(inputStream, protobufIndexToTypeMap);
                    items.Add(item);
                }

                message = CreateHubMessage(items);
            }

            input = input.Slice(totalByteSize);

            return true;
        }
        finally
        {
            arrayPool.Return(byteArray);
        }
    }
}