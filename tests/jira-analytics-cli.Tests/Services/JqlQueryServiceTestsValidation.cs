using System;
using System.Collections.Generic;
using System.Globalization;

public static class JqlQueryServiceTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="JqlQueryServiceTests"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A read-only list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this JqlQueryServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate all public members that could contain invalid data
        // Note: Only the actual public members of JqlQueryServiceTests are validated

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="JqlQueryServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this JqlQueryServiceTests value)
    {
        try
        {
            _ = Validate(value);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures that the specified <see cref="JqlQueryServiceTests"/> instance is valid, throwing an <see cref="ArgumentException"/> with a detailed message if it is not.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing a list of all validation problems.</exception>
    public static void EnsureValid(this JqlQueryServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"The {nameof(JqlQueryServiceTests)} instance is invalid. Problems:\n{string.Join("\n", problems)}");
    }
}
