using ProtoBuf;
using ProtoBuf.Meta;

namespace SignalR.Protobuf.Util;

// ReSharper disable once InconsistentNaming
internal static class MessageExtensions
{
    internal static T MergeFixedDelimitedFrom<T>(this T protobufMessage, Stream stream) where T : class
    {
        Span<byte> lengthBytes = stackalloc byte[4];
        var readLength = stream.Read(lengthBytes);

        var numberOfBytes = BitConverter.ToInt32(lengthBytes[..readLength]);
        return protobufMessage.MergeFrom(stream, numberOfBytes);
    }

    internal static T MergeFrom<T>(this T protobufMessage, Stream stream, int numberOfBytes) where T : class
    {
        return Serializer.Deserialize(stream, protobufMessage, length: numberOfBytes);
    }
}