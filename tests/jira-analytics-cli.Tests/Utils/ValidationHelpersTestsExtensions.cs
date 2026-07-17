namespace JiraAnalyticsCli.Tests.Utils;

public static class ValidationHelpersTestsExtensions
{
    /// <summary>
    /// Gets a list of test method names from the <see cref="ValidationHelpersTests"/> instance.
    /// </summary>
    /// <param name="tests">The <see cref="ValidationHelpersTests"/> instance.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> of test method names.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> GetTestMethodNames(this ValidationHelpersTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        return new[]
        {
            nameof(ValidationHelpersTests.IsValidJiraIssueKey_VariousFormats_ReturnsExpected),
            nameof(ValidationHelpersTests.IsValidProjectKey_VariousFormats_ReturnsExpected),
            nameof(ValidationHelpersTests.IsValidUrl_VariousFormats_ReturnsExpected),
            nameof(ValidationHelpersTests.IsValidEmail_VariousFormats_ReturnsExpected),
            nameof(ValidationHelpersTests.IsValidSprintId_BoundaryValues_ReturnsExpected),
            nameof(ValidationHelpersTests.IsValidStoryPoints_Null_ReturnsTrue),
            nameof(ValidationHelpersTests.IsValidStoryPoints_Zero_ReturnsTrue),
            nameof(ValidationHelpersTests.IsValidStoryPoints_Negative_ReturnsFalse),
            nameof(ValidationHelpersTests.IsValidDateRange_StartBeforeEnd_ReturnsTrue),
            nameof(ValidationHelpersTests.IsValidDateRange_StartEqualsEnd_ReturnsFalse),
            nameof(ValidationHelpersTests.IsValidDateRange_StartAfterEnd_ReturnsFalse),
            nameof(ValidationHelpersTests.IsValidPercentage_BoundaryValues_ReturnsExpected),
            nameof(ValidationHelpersTests.TruncateWithEllipsis_NullInput_ReturnsEmpty),
            nameof(ValidationHelpersTests.TruncateWithEllipsis_EmptyInput_ReturnsEmpty),
            nameof(ValidationHelpersTests.TruncateWithEllipsis_ShortInput_ReturnsUnchanged),
            nameof(ValidationHelpersTests.TruncateWithEllipsis_LongInput_TruncatesWithDots),
            nameof(ValidationHelpersTests.SanitizeForCsv_NullInput_ReturnsEmpty),
            nameof(ValidationHelpersTests.SanitizeForCsv_StringWithCommas_RemovesCommas),
            nameof(ValidationHelpersTests.SanitizeForCsv_StringWithNewlines_RemovesNewlines),
            nameof(ValidationHelpersTests.SanitizeForCsv_StringWithQuotes_RemovesQuotes),
        };
    }

    /// <summary>
    /// Runs all tests in the <see cref="ValidationHelpersTests"/> instance and returns the results.
    /// </summary>
    /// <param name="tests">The <see cref="ValidationHelpersTests"/> instance.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> of test results.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown if any test fails.</exception>
    public static IReadOnlyList<TestResult> RunAllTests(this ValidationHelpersTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        var testMethodNames = tests.GetTestMethodNames();
        var testResults = new List<TestResult>();

        foreach (var testMethodName in testMethodNames)
        {
            var testMethod = tests.GetType().GetMethod(testMethodName);

            if (testMethod is null)
            {
                testResults.Add(new TestResult(testMethodName, false));
                continue;
            }

            try
            {
                var result = testMethod.Invoke(tests, null);
                var success = result is not null && (bool)result;
                testResults.Add(new TestResult(testMethodName, success));
            }
            catch
            {
                testResults.Add(new TestResult(testMethodName, false));
            }
        }

        if (testResults.Any(r => !r.Success))
        {
            throw new InvalidOperationException("One or more tests failed.");
        }

        return testResults.AsReadOnly();
    }
}

public class TestResult
{
    public string TestMethodName { get; }
    public bool Success { get; }

    public TestResult(string testMethodName, bool success)
    {
        TestMethodName = testMethodName;
        Success = success;
    }
}