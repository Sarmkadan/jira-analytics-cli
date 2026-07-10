// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation extensions for CollectionExtensions class
// =====================================================================

using System.Collections;

namespace JiraAnalyticsCli.Utils;

/// <summary>
/// Provides validation methods for collection operations to ensure proper usage
/// and detect common issues with collection operations.
/// </summary>
public static class CollectionExtensionsValidation
{
    /// <summary>
    /// Validates collection parameters for common issues like null collections,
    /// empty collections, or collections with null elements.
    /// </summary>
    /// <param name="source">The collection to validate</param>
    /// <returns>List of validation problems; empty if valid</returns>
    public static IReadOnlyList<string> Validate(this IEnumerable? source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var problems = new List<string>();

        if (source is ICollection collection && collection.Count == 0)
        {
            problems.Add("Collection is empty");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the collection is valid (non-null and non-empty).
    /// </summary>
    /// <param name="source">The collection to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid(this IEnumerable? source)
    {
        return source != null && source switch
        {
            ICollection collection => collection.Count > 0,
            _ => true
        };
    }

    /// <summary>
    /// Ensures the collection is valid, throwing an exception if any validation
    /// problems are found (null or empty collection).
    /// </summary>
    /// <param name="source">The collection to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if source is null</exception>
    /// <exception cref="ArgumentException">Thrown if collection is empty</exception>
    public static void EnsureValid(this IEnumerable source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source is ICollection collection && collection.Count == 0)
        {
            throw new ArgumentException("Collection cannot be empty");
        }
    }

    /// <summary>
    /// Validates batch parameters to ensure batch size is positive.
    /// </summary>
    /// <param name="batchSize">The batch size to validate</param>
    /// <returns>List of validation problems; empty if valid</returns>
    public static IReadOnlyList<string> Validate(this int batchSize)
    {
        var problems = new List<string>();

        if (batchSize <= 0)
        {
            problems.Add("Batch size must be positive");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the batch size is valid (positive).
    /// </summary>
    /// <param name="batchSize">The batch size to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid(this int batchSize)
    {
        return batchSize > 0;
    }

    /// <summary>
    /// Ensures the batch size is valid, throwing an exception if validation fails.
    /// </summary>
    /// <param name="batchSize">The batch size to validate</param>
    /// <exception cref="ArgumentException">Thrown if batch size is not positive</exception>
    public static void EnsureValid(this int batchSize)
    {
        if (batchSize <= 0)
        {
            throw new ArgumentException("Batch size must be positive");
        }
    }
}