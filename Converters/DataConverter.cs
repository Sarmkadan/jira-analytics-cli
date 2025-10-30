// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;
using Microsoft.Extensions.Logging;

namespace JiraAnalyticsCli.Converters;

/// <summary>
/// Converts data between different types and formats using reflection and mapping.
/// Provides type-safe conversion with error handling and validation.
/// </summary>
public class DataConverter
{
    private readonly ILogger<DataConverter> _logger;
    private readonly Dictionary<(Type From, Type To), Func<object, object>> _customConverters = new();

    public DataConverter(ILogger<DataConverter> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Registers custom converter function for specific type pair.
    /// Used when automatic conversion is not suitable.
    /// </summary>
    public void RegisterConverter<TFrom, TTo>(Func<TFrom, TTo> converter)
    {
        _customConverters[(typeof(TFrom), typeof(TTo))] = obj => converter((TFrom)obj)!;
        _logger.LogDebug("Registered custom converter: {FromType} -> {ToType}", typeof(TFrom).Name, typeof(TTo).Name);
    }

    /// <summary>
    /// Converts object to target type using registered converter or reflection.
    /// Returns default value if conversion fails.
    /// </summary>
    public TTo? Convert<TFrom, TTo>(TFrom source, TTo? defaultValue = default) where TTo : class
    {
        if (source == null)
            return defaultValue;

        try
        {
            // Check for custom converter
            if (_customConverters.TryGetValue((typeof(TFrom), typeof(TTo)), out var converter))
            {
                var result = converter(source);
                return (TTo?)result;
            }

            // Try direct cast
            if (source is TTo directCast)
                return directCast;

            // Try reflection-based conversion
            return ConvertByReflection(source, defaultValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting {FromType} to {ToType}", typeof(TFrom).Name, typeof(TTo).Name);
            return defaultValue;
        }
    }

    /// <summary>
    /// Converts collection of objects from one type to another.
    /// Preserves order and skips items that cannot be converted.
    /// </summary>
    public List<TTo> ConvertCollection<TFrom, TTo>(IEnumerable<TFrom> source) where TTo : class
    {
        var result = new List<TTo>();

        foreach (var item in source)
        {
            var converted = Convert<TFrom, TTo>(item);
            if (converted != null)
            {
                result.Add(converted);
            }
        }

        _logger.LogDebug("Converted {Count} items from {FromType} to {ToType}",
            result.Count, typeof(TFrom).Name, typeof(TTo).Name);

        return result;
    }

    /// <summary>
    /// Maps properties from source object to target object.
    /// Uses reflection to copy matching properties by name.
    /// </summary>
    public TTo Map<TFrom, TTo>(TFrom source) where TTo : class, new()
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var target = new TTo();
        var sourceProperties = typeof(TFrom).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var targetProperties = typeof(TTo).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var sourceProp in sourceProperties)
        {
            var targetProp = targetProperties.FirstOrDefault(p =>
                p.Name.Equals(sourceProp.Name, StringComparison.OrdinalIgnoreCase) &&
                p.CanWrite);

            if (targetProp != null)
            {
                try
                {
                    var value = sourceProp.GetValue(source);

                    if (value != null && targetProp.PropertyType != sourceProp.PropertyType)
                    {
                        value = System.Convert.ChangeType(value, targetProp.PropertyType);
                    }

                    targetProp.SetValue(target, value);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not map property {PropertyName}", sourceProp.Name);
                }
            }
        }

        return target;
    }

    /// <summary>
    /// Converts value to specific type (int, bool, DateTime, etc.).
    /// Handles common type conversions safely.
    /// </summary>
    public object? ConvertValue(object? value, Type targetType)
    {
        if (value == null)
            return null;

        if (targetType.IsAssignableFrom(value.GetType()))
            return value;

        try
        {
            if (targetType == typeof(bool) && value is string boolStr)
            {
                return bool.TryParse(boolStr, out var boolVal) ? boolVal : false;
            }

            if (targetType == typeof(DateTime) && value is string dateStr)
            {
                return DateTime.TryParse(dateStr, out var dateVal) ? dateVal : (object?)null;
            }

            if (targetType == typeof(Guid) && value is string guidStr)
            {
                return Guid.TryParse(guidStr, out var guidVal) ? guidVal : (object?)null;
            }

            return System.Convert.ChangeType(value, targetType);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not convert value to type {TargetType}", targetType.Name);
            return null;
        }
    }

    private TTo? ConvertByReflection<TFrom, TTo>(TFrom source, TTo? defaultValue) where TTo : class
    {
        // Try to create instance and map properties
        try
        {
            var target = Activator.CreateInstance<TTo>();
            if (target == null)
                return defaultValue;

            var sourceProperties = typeof(TFrom).GetProperties();
            var targetProperties = typeof(TTo).GetProperties();

            foreach (var sourceProp in sourceProperties)
            {
                var targetProp = targetProperties.FirstOrDefault(p =>
                    p.Name == sourceProp.Name && p.CanWrite);

                if (targetProp != null)
                {
                    var value = sourceProp.GetValue(source);
                    targetProp.SetValue(target, value);
                }
            }

            return target;
        }
        catch
        {
            return defaultValue;
        }
    }
}
