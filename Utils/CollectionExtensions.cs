// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace JiraAnalyticsCli.Utils;

/// <summary>
/// Extension methods for working with collections and enumerables.
/// Provides convenient aggregation, filtering, and transformation utilities.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Batches items into groups of specified size.
    /// Useful for pagination and batch processing of large datasets.
    /// </summary>
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
    {
        if (batchSize <= 0)
            throw new ArgumentException("Batch size must be positive", nameof(batchSize));

        var batch = new List<T>(batchSize);

        foreach (var item in source)
        {
            batch.Add(item);

            if (batch.Count == batchSize)
            {
                yield return batch;
                batch = new List<T>(batchSize);
            }
        }

        if (batch.Count > 0)
            yield return batch;
    }

    /// <summary>
    /// Groups items by key and returns groups with count > 1.
    /// Useful for finding duplicates or grouped occurrences.
    /// </summary>
    public static IEnumerable<IGrouping<TKey, TItem>> GroupByMultiple<TItem, TKey>(
        this IEnumerable<TItem> source, Func<TItem, TKey> keySelector)
        where TKey : notnull
    {
        return source
            .GroupBy(keySelector)
            .Where(g => g.Count() > 1);
    }

    /// <summary>
    /// Returns distinct items based on a key selector.
    /// More efficient than GroupBy when only distinct values are needed.
    /// </summary>
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        where TKey : notnull
    {
        var seen = new HashSet<TKey>();

        foreach (var item in source)
        {
            if (seen.Add(keySelector(item)))
                yield return item;
        }
    }

    /// <summary>
    /// Safely gets an item at index or returns default value.
    /// Prevents IndexOutOfRangeException when accessing potentially invalid indices.
    /// </summary>
    public static T? GetAtIndexOrDefault<T>(this IList<T> list, int index, T? defaultValue = default)
    {
        if (list == null)
            throw new ArgumentNullException(nameof(list));

        return index >= 0 && index < list.Count ? list[index] : defaultValue;
    }

    /// <summary>
    /// Determines if collection is empty without forcing enumeration.
    /// More efficient than checking Count() for lazy enumerables.
    /// </summary>
    public static bool IsEmpty<T>(this IEnumerable<T> source)
    {
        return !source.Any();
    }

    /// <summary>
    /// Determines if collection has exactly one element.
    /// Stops enumeration early once determination is made.
    /// </summary>
    public static bool HasExactlyOne<T>(this IEnumerable<T> source)
    {
        using var enumerator = source.GetEnumerator();

        if (!enumerator.MoveNext())
            return false;

        return !enumerator.MoveNext();
    }

    /// <summary>
    /// Merges multiple enumerables into single sequence preserving order.
    /// Useful for combining paginated results or multiple data sources.
    /// </summary>
    public static IEnumerable<T> Merge<T>(params IEnumerable<T>[] sources)
    {
        foreach (var source in sources)
        {
            foreach (var item in source)
            {
                yield return item;
            }
        }
    }

    /// <summary>
    /// Converts dictionary to flattened key-value pairs for easier processing.
    /// Useful for transforming dictionaries into collections.
    /// </summary>
    public static IEnumerable<(TKey Key, TValue Value)> ToTuples<TKey, TValue>(this Dictionary<TKey, TValue> dict)
        where TKey : notnull
    {
        return dict.Select(kvp => (kvp.Key, kvp.Value));
    }
}
