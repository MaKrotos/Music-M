using System.Collections;
using System.Reflection;
using ProtoBuf;
using SignalR.Protobuf.Util;

// ReSharper disable once CheckNamespace
namespace SignalR.Protobuf.Messages;

public partial class ItemMetadata
{
    private static readonly MethodInfo MeasureMethod = typeof(Serializer).GetMethod(nameof(Serializer.Measure))!;

    public static ItemMetadata Create(object? obj, IReadOnlyDictionary<Type, int> protobufTypeToIndexMap)
    {
        var result = new ItemMetadata
        {
            TypesAndSizes = new List<int>()
        };

        if (obj is null or not IEnumerable)
        {
            AddTypeAndSize(obj);
        }
        else
        {
            result.TypesAndSizes.Add(-2);
            foreach (var item in (IEnumerable) obj)
            {
                AddTypeAndSize(item);
            }
        }

        return result;

        void AddTypeAndSize(object? protobuf)
        {
            if (protobuf == null)
            {
                result.TypesAndSizes.Add(-1);
                result.TypesAndSizes.Add(0);
            }
            else
            {
                var typeInt = protobufTypeToIndexMap[protobuf.GetType()];
                result.TypesAndSizes.Add(typeInt);
                result.TypesAndSizes.Add((int)(long)typeof(MeasureState<>).MakeGenericType(protobuf.GetType())
                                                                    .GetMethod(nameof(MeasureState<int>.LengthOnly))!
                                                                    .Invoke(MeasureMethod
                                                                            .MakeGenericMethod(protobuf.GetType())
                                                                            .Invoke(null, new[] { protobuf, null, -1L }),
                                                                            Array.Empty<object>())!);

                result._nonNullProtobufs.Add(protobuf);
            }
        }
    }

    private readonly List<object> _nonNullProtobufs = new List<object>();
    public IEnumerable<object> NonNullProtobufs => _nonNullProtobufs;

    public int CalculateTotalSizeBytes()
    {
        switch (TypesAndSizes[0])
        {
            case -2:
            {
                var sum = 0;
                for (var i = 2; i < TypesAndSizes.Count; i += 2)
                {
                    sum += TypesAndSizes[i];
                }
                return sum;
            }
            default:
            {
                return TypesAndSizes[1];
            }
        }
    }

    public object? CreateItem(Stream stream, IReadOnlyDictionary<int, Type> protobufIndexToTypeMap)
    {
        switch (TypesAndSizes[0])
        {
            case -2:
            {
                var result = new List<object?>(TypesAndSizes.Count / 2);
                for (var i = 1; i < TypesAndSizes.Count; i += 2)
                {
                    var typeIndex = TypesAndSizes[i];
                    var sizeBytes = TypesAndSizes[i + 1];

                    switch (typeIndex)
                    {
                        case -1:
                            result.Add(null);
                            break;
                        default:
                            // Only add the item to the list if it is mapped.
                            // This ensures backwards-compatibility
                            if (protobufIndexToTypeMap.TryGetValue(typeIndex, out var type))
                            {
                                using var inputStream = new LimitedInputStream(stream, sizeBytes);
                                return Serializer.Deserialize(type, inputStream);
                            }
                            break;
                    }
                }

                return result;
            }
            default:
            {
                var typeIndex = TypesAndSizes[0];
                var sizeBytes = TypesAndSizes[1];

                switch (typeIndex)
                {
                    case -1:
                        return null;
                    default:
                    {
                        using var inputStream = new LimitedInputStream(stream, sizeBytes);
                        return protobufIndexToTypeMap.TryGetValue(typeIndex, out var protoType) ? Serializer.Deserialize(protoType, inputStream) : null;
                    }
                }
            }
        }
    }
}