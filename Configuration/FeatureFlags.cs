// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Configuration;

/// <summary>
/// Manages feature flags for enabling/disabling functionality at runtime.
/// Supports percentage-based rollout and condition-based feature gating.
/// </summary>
public class FeatureFlags
{
    private readonly Dictionary<string, FeatureFlag> _flags = new();
    private readonly ILogger<FeatureFlags> _logger;

    public FeatureFlags(ILogger<FeatureFlags> logger)
    {
        _logger = logger;
        InitializeDefaultFlags();
    }

    /// <summary>
    /// Checks if feature is enabled.
    /// Returns false if feature doesn't exist.
    /// </summary>
    public bool IsEnabled(string featureName)
    {
        if (_flags.TryGetValue(featureName, out var flag))
        {
            var enabled = flag.Enabled && (flag.Condition == null || flag.Condition());

            _logger.LogDebug("Feature flag check: {Feature} = {Enabled}", featureName, enabled);
            return enabled;
        }

        _logger.LogWarning("Feature flag not found: {Feature}", featureName);
        return false;
    }

    /// <summary>
    /// Checks if feature is enabled with percentage-based rollout.
    /// Useful for gradual feature releases.
    /// </summary>
    public bool IsEnabledForPercentage(string featureName, double percentage)
    {
        if (!IsEnabled(featureName))
            return false;

        var randomValue = new Random().NextDouble() * 100;
        return randomValue <= percentage;
    }

    /// <summary>
    /// Enables or disables feature.
    /// </summary>
    public void SetFeatureEnabled(string featureName, bool enabled)
    {
        if (_flags.ContainsKey(featureName))
        {
            _flags[featureName].Enabled = enabled;
            _logger.LogInformation("Feature {Feature} set to {Status}", featureName, enabled ? "enabled" : "disabled");
        }
    }

    /// <summary>
    /// Registers feature with optional condition.
    /// </summary>
    public void RegisterFeature(string featureName, bool enabled = true, Func<bool>? condition = null)
    {
        _flags[featureName] = new FeatureFlag
        {
            Name = featureName,
            Enabled = enabled,
            Condition = condition
        };

        _logger.LogDebug("Feature registered: {Feature}", featureName);
    }

    /// <summary>
    /// Gets all feature flags and their status.
    /// </summary>
    public Dictionary<string, bool> GetAllFeatures()
    {
        return _flags.ToDictionary(
            kv => kv.Key,
            kv => IsEnabled(kv.Key));
    }

    private void InitializeDefaultFlags()
    {
        RegisterFeature("enable_caching", enabled: true);
        RegisterFeature("enable_background_tasks", enabled: true);
        RegisterFeature("enable_webhooks", enabled: false);
        RegisterFeature("enable_advanced_analytics", enabled: true);
        RegisterFeature("enable_performance_tracking", enabled: true);
        RegisterFeature("enable_detailed_logging", enabled: false);
        RegisterFeature("enable_rate_limiting", enabled: true);
        RegisterFeature("enable_data_compression", enabled: true);

        _logger.LogDebug("Default feature flags initialized: {Count} flags", _flags.Count);
    }

    private class FeatureFlag
    {
        public string Name { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public Func<bool>? Condition { get; set; }
    }
}
