// ... rest of README content ...
## CachePolicyExtensions

The `CachePolicyExtensions` class provides a set of extension methods for working with `CachePolicy` objects. These methods allow you to create and modify cache policies with specific conditions, maximum sizes, and expiration times.

### Usage Example

```csharp
using JiraAnalyticsCli.Caching;

// Create a test instance of CachePolicy
var policy = CachePolicyExtensions.WithMaxSize(new CachePolicy(), 100);

// Add a condition to the policy
policy = CachePolicyExtensions.WithCondition(policy, issue => issue.IsOverdue());

// Set a combined expiration time
policy = CachePolicyExtensions.WithCombinedExpiration(policy, TimeSpan.FromHours(2));

// Check if the policy has an expiration
bool hasExpiration = CachePolicyExtensions.HasExpiration(policy);

// Get the effective expiration time
TimeSpan? effectiveExpiration = CachePolicyExtensions.GetEffectiveExpiration(policy);

Console.WriteLine($"Has expiration: {hasExpiration}");
Console.WriteLine($"Effective expiration: {effectiveExpiration}");
```

# ... rest of README content ...
