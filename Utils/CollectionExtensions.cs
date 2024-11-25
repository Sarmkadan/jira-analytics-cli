// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

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
    /// <param name="source">The source enumerable to batch.</param>
    /// <param name="batchSize">The maximum size of each batch.</param>
    /// <returns>An enumerable of batches, each containing up to <paramref name="batchSize"/> items.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="batchSize"/> is not positive.</exception>
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
    {
        ArgumentNullException.ThrowIfNull(source);

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
    /// <param name="source">The source enumerable to group.</param>
    /// <param name="keySelector">Function to extract the key from each element.</param>
    /// <returns>An enumerable of groupings where each group has more than one item.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <see langword="null"/></exception>
    public static IEnumerable<IGrouping<TKey, TItem>> GroupByMultiple<TItem, TKey>(
        this IEnumerable<TItem> source, Func<TItem, TKey> keySelector)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        return source
            .GroupBy(keySelector)
            .Where(g => g.Count() > 1);
    }

    /// <summary>
    /// Returns distinct items based on a key selector.
    /// More efficient than GroupBy when only distinct values are needed.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <typeparam name="TKey">The type of key used to determine uniqueness.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="keySelector">A function to extract the key for each element.</param>
    /// <returns>An enumerable that contains only distinct elements.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <see langword="null"/></exception>
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

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
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to access.</param>
    /// <param name="index">The index to retrieve.</param>
    /// <param name="defaultValue">The value to return if the index is invalid. Defaults to <see langword="default"/>.</param>
    /// <returns>The element at the specified index, or <paramref name="defaultValue"/> if the index is out of range.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="list"/> is <see langword="null"/></exception>
    public static T? GetAtIndexOrDefault<T>(this IList<T> list, int index, T? defaultValue = default)
    {
        ArgumentNullException.ThrowIfNull(list);

        return index >= 0 && index < list.Count ? list[index] : defaultValue;
    }

    /// <summary>
    /// Determines if collection is empty without forcing enumeration.
    /// More efficient than checking Count() for lazy enumerables.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source collection.</param>
    /// <returns><see langword="true"/> if the collection is empty; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    public static bool IsEmpty<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return !source.Any();
    }

    /// <summary>
    /// Determines if collection has exactly one element.
    /// Stops enumeration early once determination is made.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source collection.</param>
    /// <returns><see langword="true"/> if the collection contains exactly one element; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    public static bool HasExactlyOne<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        using var enumerator = source.GetEnumerator();

        if (!enumerator.MoveNext())
            return false;

        return !enumerator.MoveNext();
    }

    /// <summary>
    /// Merges multiple enumerables into single sequence preserving order.
    /// Useful for combining paginated results or multiple data sources.
    /// </summary>
    /// <typeparam name="T">The type of elements in the enumerables.</typeparam>
    /// <param name="sources">The enumerables to merge.</param>
    /// <returns>An enumerable containing all elements from all sources in order.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sources"/> is <see langword="null"/></exception>
    public static IEnumerable<T> Merge<T>(params IEnumerable<T>[] sources)
    {
        ArgumentNullException.ThrowIfNull(sources);

        return sources.SelectMany(source => source ?? Enumerable.Empty<T>());
    }

    /// <summary>
    /// Converts dictionary to flattened key-value pairs for easier processing.
    /// Useful for transforming dictionaries into collections.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    /// <param name="dict">The dictionary to convert.</param>
    /// <returns>An enumerable of key-value tuples.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dict"/> is <see langword="null"/></exception>
    public static IEnumerable<(TKey Key, TValue Value)> ToTuples<TKey, TValue>(this Dictionary<TKey, TValue> dict)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dict);

        return dict.Select(kvp => (kvp.Key, kvp.Value));
    }
}