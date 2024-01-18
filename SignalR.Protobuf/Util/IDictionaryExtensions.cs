namespace SignalR.Protobuf.Util;

// ReSharper disable once InconsistentNaming
internal static class IDictionaryExtensions
{
    internal static IEnumerable<T> Flatten<T>(this IDictionary<T, T> dictionary)
    {
        if (dictionary == null)
        {
            yield break;
        }

        foreach (var pair in dictionary)
        {
            yield return pair.Key;
            yield return pair.Value;
        }
    }

    internal static IDictionary<T, T> Unflatten<T>(this IEnumerable<T> enumerable)
    {
        var result = new Dictionary<T, T>();

        using (var enumerator = enumerable.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                var key = enumerator.Current;
                enumerator.MoveNext();
                var value = enumerator.Current;

                result[key] = value;
            }
        }

        return result;
    }
}