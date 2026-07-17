// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation extensions for CollectionExtensions class
// Provides validation methods for parameters used by CollectionExtensions extension methods
// =====================================================================
using System.Collections;

namespace JiraAnalyticsCli.Utils;

/// <summary>
/// Validation helpers for CollectionExtensions operations.
/// Provides methods to validate parameters used by CollectionExtensions extension methods.
/// </summary>
public static class CollectionExtensionsValidation
{
    /// <summary>
    /// Validates a collection source for common issues like null collections or empty collections.
    /// This helps prevent InvalidOperationException when calling CollectionExtensions methods.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection</typeparam>
    /// <param name="source">The collection to validate</param>
    /// <returns>An empty list if valid, otherwise a list of human-readable problem descriptions</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is null</exception>
    public static IReadOnlyList<string> Validate<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source switch
        {
            ICollection collection when collection.Count == 0 => new[] { "Collection is empty" },
            _ => Array.Empty<string>()
        };
    }

    /// <summary>
    /// Determines whether the collection source is valid (non-null and non-empty).
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection</typeparam>
    /// <param name="source">The collection to check</param>
    /// <returns>True if valid; false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is null</exception>
    public static bool IsValid<T>(this IEnumerable<T> source) => source is not null && source switch
    {
        ICollection collection => collection.Count > 0,
        _ => true
    };

    /// <summary>
    /// Ensures that a collection source is valid, throwing an exception if any validation
    /// problems are found (null or empty collection).
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection</typeparam>
    /// <param name="source">The collection to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if collection is empty</exception>
    public static void EnsureValid<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source is ICollection { Count: 0 })
        {
            throw new ArgumentException("Collection cannot be empty");
        }
    }

    /// <summary>
    /// Validates a batch size parameter to ensure it's positive.
    /// Batch size must be positive to avoid infinite loops or incorrect batching behavior.
    /// </summary>
    /// <param name="batchSize">The batch size to validate</param>
    /// <returns>An empty list if valid, otherwise a list of human-readable problem descriptions</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if batchSize is not positive</exception>
    public static IReadOnlyList<string> Validate(this int batchSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(batchSize, 0, nameof(batchSize));

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the batch size is valid (positive).
    /// </summary>
    /// <param name="batchSize">The batch size to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid(this int batchSize) => batchSize > 0;

    /// <summary>
    /// Ensures the batch size is valid, throwing an exception if validation fails.
    /// </summary>
    /// <param name="batchSize">The batch size to validate</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if batch size is not positive</exception>
    public static void EnsureValid(this int batchSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(batchSize, 0, nameof(batchSize));
    }

    /// <summary>
    /// Validates a key selector function to ensure it's not null.
    /// Key selectors are used in GroupByMultiple and DistinctBy operations.
    /// </summary>
    /// <typeparam name="TItem">The type of items being processed</typeparam>
    /// <typeparam name="TKey">The type of keys being selected</typeparam>
    /// <param name="keySelector">The key selector function to validate</param>
    /// <returns>An empty list if valid, otherwise a list of human-readable problem descriptions</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="keySelector"/> is null</exception>
    public static IReadOnlyList<string> Validate<TItem, TKey>(this Func<TItem, TKey> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the key selector function is valid (not null).
    /// </summary>
    /// <typeparam name="TItem">The type of items being processed</typeparam>
    /// <typeparam name="TKey">The type of keys being selected</typeparam>
    /// <param name="keySelector">The key selector function to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid<TItem, TKey>(this Func<TItem, TKey> keySelector) => keySelector != null;

    /// <summary>
    /// Ensures the key selector function is valid, throwing an exception if validation fails.
    /// </summary>
    /// <typeparam name="TItem">The type of items being processed</typeparam>
    /// <typeparam name="TKey">The type of keys being selected</typeparam>
    /// <param name="keySelector">The key selector function to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="keySelector"/> is null</exception>
    public static void EnsureValid<TItem, TKey>(this Func<TItem, TKey> keySelector)
        => ArgumentNullException.ThrowIfNull(keySelector);

    /// <summary>
    /// Validates a list and index for GetAtIndexOrDefault operation.
    /// Ensures the list is not null and the index is non-negative.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list</typeparam>
    /// <param name="list">The list to validate</param>
    /// <param name="index">The index to validate</param>
    /// <returns>An empty list if valid, otherwise a list of human-readable problem descriptions</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="list"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="index"/> is negative</exception>
    public static IReadOnlyList<string> Validate<T>(this IList<T> list, int index)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the list and index are valid for GetAtIndexOrDefault operation.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list</typeparam>
    /// <param name="list">The list to check</param>
    /// <param name="index">The index to check</param>
    /// <returns>True if valid; false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="list"/> is null</exception>
    public static bool IsValid<T>(this IList<T> list, int index) => list != null && index >= 0;

    /// <summary>
    /// Ensures the list and index are valid for GetAtIndexOrDefault operation,
    /// throwing an exception if validation fails.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list</typeparam>
    /// <param name="list">The list to validate</param>
    /// <param name="index">The index to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="list"/> is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is negative</exception>
    public static void EnsureValid<T>(this IList<T> list, int index)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
    }
}