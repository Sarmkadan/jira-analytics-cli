# CollectionExtensions

Extension methods that provide common collection‑wise operations not found in the base LINQ API.

## API

### `Batch<T>(this IEnumerable<T> source, int size)`
**Purpose**  
Splits the source sequence into consecutive batches, each containing up to `size` elements.

**Parameters**  
- `source`: The sequence to batch.  
- `size`: The maximum number of elements per batch; must be greater than zero.

**Return value**  
An `IEnumerable<IEnumerable<T>>` where each inner enumerable yields a batch of elements from `source`. The final batch may contain fewer than `size` elements if the source does not divide evenly.

**Exceptions**  
- `ArgumentNullException` if `source` is `null`.  
- `ArgumentOutOfRangeException` if `size` is less than or equal to zero.

### `GroupByMultiple<TItem, TKey>(this IEnumerable<TItem> source, params Func<TItem, TKey>[] keySelectors)`
**Purpose**  
Groups elements by a composite key formed from the values returned by the supplied key selectors.

**Parameters**  
- `source`: The sequence to group.  
- `keySelectors`: One or more functions that extract a key part from each element. The combined key is a `ValueTuple` of the selector results (type `TKey`).

**Return value**  
An `IEnumerable<IGrouping<TKey, TItem>>` where each group’s key represents the tuple of key values shared by its elements.

**Exceptions**  
- `ArgumentNullException` if `source` is `null` or any element in `keySelectors` is `null`.  
- `ArgumentException` if `keySelectors` contains zero selectors.

### `DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)`
**Purpose**  
Returns distinct elements from the source sequence, using the specified key selector to determine uniqueness.

**Parameters**  
- `source`: The sequence to deduplicate.  
- `keySelector`: A function that extracts the key used for comparison.

**Return value**  
An `IEnumerable<T>` containing the first occurrence of each distinct key value encountered in `source`.

**Exceptions**  
- `ArgumentNullException` if `source` or `keySelector` is `null`.

### `GetAtIndexOrDefault<T>(this IEnumerable<T> source, int index)`
**Purpose**  
Retrieves the element at the specified zero‑based index, or returns the default value for `T` if the index is out of range.

**Parameters**  
- `source`: The sequence to index.  
- `index`: The zero‑based position of the desired element.

**Return value**  
The element at `index` if it exists; otherwise `default(T)`.

**Exceptions**  
- `ArgumentNullException` if `source` is `null`.

### `IsEmpty<T>(this IEnumerable<T> source)`
**Purpose**  
Determines whether the source sequence contains no elements.

**Parameters**  
- `source`: The sequence to test.

**Return value**  
`true` if `source` is `null` or does not yield any elements; otherwise `false`.

**Exceptions**  
- None (the method treats a `null` source as empty).

### `HasExactlyOne<T>(this IEnumerable<T> source)`
**Purpose**  
Determines whether the source sequence contains exactly one element.

**Parameters**  
- `source`: The sequence to test.

**Return value**  
`true` if `source` yields precisely one element; otherwise `false`.

**Exceptions**  
- `ArgumentNullException` if `source` is `null`.

### `Merge<T>(this IEnumerable<IEnumerable<T>> sources)`
**Purpose**  
Concatenates multiple sequences into a single sequence, preserving the order of the supplied sequences and the order within each sequence.

**Parameters**  
- `sources`: A sequence of sequences to merge.

**Return value**  
An `IEnumerable<T>` that yields all elements from each inner sequence in turn.

**Exceptions**  
- `ArgumentNullException` if `sources` is `null` or any element within `sources` is `null`.

### `ToTuples<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)`
**Purpose**  
Projects each `KeyValuePair<TKey, TValue>` in the source sequence to a value tuple `(Key, Value)`.

**Parameters**  
- `source`: The sequence of key‑value pairs to convert.

**Return value**  
An `IEnumerable<(TKey Key, TValue Value)>` containing the projected tuples.

**Exceptions**  
- `ArgumentNullException` if `source` is `null`.

## Usage

```csharp
// Process a large list of work items in batches of 100.
IEnumerable<WorkItem> allItems = GetWorkItems();
foreach (var batch in allItems.Batch(100))
{
    ProcessBatch(batch.ToList()); // Assume ProcessBatch accepts a List<WorkItem>
}

// Obtain a distinct set of users by their email address.
IEnumerable<User> users = GetUsers();
IEnumerable<User> distinctUsers = users.DistinctBy(u => u.Email);
```

## Notes

- All extension operators are **pure**: they do not modify the source sequence. Consequently, they are safe to use concurrently with other read‑only operations, provided the underlying collections are not mutated during enumeration.
- `Batch` will produce an empty outer sequence when the source is empty, regardless of the batch size.
- `GroupByMultiple` creates a composite key using `ValueTuple<TKey,...>`; if only one selector is supplied the behavior mirrors that of `Enumerable.GroupBy`.
- `GetAtIndexOrDefault` walks the source sequence up to the requested index each time it is called; for indexed access on large sequences consider materialising the source first (e.g., `source.ToList()`).
- `Merge` enumerates each inner sequence lazily; if any inner sequence throws during enumeration, the exception is propagated directly from the merged enumerator.
- `ToTuples` simply rewraps each `KeyValuePair`; it does not perform any key‑based sorting or deduplication.
