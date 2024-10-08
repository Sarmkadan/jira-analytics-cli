using System;
using FluentAssertions;
using JiraAnalyticsCli.Formatters;
using Xunit;

namespace JiraAnalyticsCli.Tests.Formatters
{
    /// <summary>
    /// Extension methods for JsonFormatterTests to provide additional test utilities.
    /// </summary>
    public static class JsonFormatterTestsExtensions
    {
        /// <summary>
        /// Asserts that the formatted JSON contains the expected key-value pair.
        /// </summary>
        /// <param name="test">The JsonFormatterTests instance.</param>
        /// <param name="json">The JSON string to validate.</param>
        /// <param name="key">The key to check for.</param>
        /// <param name="expectedValue">The expected value for the key.</param>
        public static void ShouldContainKeyValue(this JsonFormatterTests test, string json, string key, object expectedValue)
        {
            json.Should().NotBeNullOrEmpty();
            json.Should().Contain($"\"{key}\"");

            var expectedString = expectedValue.ToString();
            if (expectedValue is string stringValue)
            {
                json.Should().Contain($"\"{key}\":\"{stringValue}\"");
            }
            else if (expectedValue is bool boolValue)
            {
                json.Should().Contain($"\"{key}\":{boolValue.ToString().ToLowerInvariant()}");
            }
            else
            {
                json.Should().Contain($"\"{key}\":{expectedValue}");
            }
        }

        /// <summary>
        /// Asserts that the formatted JSON contains the expected key.
        /// </summary>
        /// <param name="test">The JsonFormatterTests instance.</param>
        /// <param name="json">The JSON string to validate.</param>
        /// <param name="key">The key to check for.</param>
        public static void ShouldContainKey(this JsonFormatterTests test, string json, string key)
        {
            json.Should().NotBeNullOrEmpty();
            json.Should().Contain($"\"{key}\"");
        }

        /// <summary>
        /// Asserts that the formatted JSON does not contain the specified key.
        /// </summary>
        /// <param name="test">The JsonFormatterTests instance.</param>
        /// <param name="json">The JSON string to validate.</param>
        /// <param name="key">The key to exclude.</param>
        public static void ShouldNotContainKey(this JsonFormatterTests test, string json, string key)
        {
            json.Should().NotBeNullOrEmpty();
            json.Should().NotContain($"\"{key}\"");
        }

        /// <summary>
        /// Asserts that the validation result indicates valid JSON with no errors.
        /// </summary>
        /// <param name="test">The JsonFormatterTests instance.</param>
        /// <param name="validationResult">The validation result tuple.</param>
        public static void ShouldBeValidWithNoErrors(this JsonFormatterTests test, (bool isValid, string errors) validationResult)
        {
            validationResult.isValid.Should().BeTrue();
            validationResult.errors.Should().BeNullOrEmpty();
        }

        /// <summary>
        /// Asserts that the validation result indicates invalid JSON with errors.
        /// </summary>
        /// <param name="test">The JsonFormatterTests instance.</param>
        /// <param name="validationResult">The validation result tuple.</param>
        public static void ShouldBeInvalidWithErrors(this JsonFormatterTests test, (bool isValid, string errors) validationResult)
        {
            validationResult.isValid.Should().BeFalse();
            validationResult.errors.Should().NotBeNullOrEmpty();
        }
    }
}