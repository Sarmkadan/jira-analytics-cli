using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        // Validate that the instance has the expected test methods
        var testMethods = GetTestMethods(value);
        if (testMethods.Count == 0)
        {
            problems.Add("The JqlQueryServiceTests instance has no public parameterless test methods.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="JqlQueryServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
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
            $"The {nameof(JqlQueryServiceTests)} instance is invalid. Problems:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
    }

    private static List<string> GetTestMethods(JqlQueryServiceTests value)
    {
        var methods = typeof(JqlQueryServiceTests)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(m => m.GetParameters().Length == 0 &&
                   (m.ReturnType == typeof(void) || m.ReturnType == typeof(Task)))
            .ToList();

        return methods.Select(m => m.Name).ToList();
    }
}
