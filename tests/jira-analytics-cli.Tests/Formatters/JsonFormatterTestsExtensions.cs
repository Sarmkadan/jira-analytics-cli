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
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> or <paramref name="key"/> is null.</exception>
        public static void ShouldContainKeyValue(this JsonFormatterTests test, string json, string key, object expectedValue)
        {
            ArgumentNullException.ThrowIfNull(json);
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(expectedValue);

            json.Should().NotBeEmpty();
            json.Should().Contain($"\"{key}\"");

            switch (expectedValue)
            {
                case string stringValue:
                    json.Should().Contain($"\"{key}\":\"{stringValue}\"");
                    break;
                case bool boolValue:
                    json.Should().Contain($"\"{key}\":{boolValue.ToString().ToLowerInvariant()}");
                    break;
                default:
                    json.Should().Contain($"\"{key}\":{expectedValue}");
                    break;
            }
        }

        /// <summary>
        /// Asserts that the formatted JSON contains the expected key.
        /// </summary>
        /// <param name="test">The JsonFormatterTests instance.</param>
        /// <param name="json">The JSON string to validate.</param>
        /// <param name="key">The key to check for.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> or <paramref name="key"/> is null.</exception>
        public static void ShouldContainKey(this JsonFormatterTests test, string json, string key)
        {
            ArgumentNullException.ThrowIfNull(json);
            ArgumentNullException.ThrowIfNull(key);

            json.Should().NotBeEmpty();
            json.Should().Contain($"\"{key}\"");
        }

        /// <summary>
        /// Asserts that the formatted JSON does not contain the specified key.
        /// </summary>
        /// <param name="test">The JsonFormatterTests instance.</param>
        /// <param name="json">The JSON string to validate.</param>
        /// <param name="key">The key to exclude.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> or <paramref name="key"/> is null.</exception>
        public static void ShouldNotContainKey(this JsonFormatterTests test, string json, string key)
        {
            ArgumentNullException.ThrowIfNull(json);
            ArgumentNullException.ThrowIfNull(key);

            json.Should().NotBeEmpty();
            json.Should().NotContain($"\"{key}\"");
        }

        /// <summary>
        /// Asserts that the validation result indicates valid JSON with no errors.
        /// </summary>
        /// <param name="test">The JsonFormatterTests instance.</param>
        /// <param name="validationResult">The validation result tuple.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="validationResult"/> is default.</exception>
        public static void ShouldBeValidWithNoErrors(this JsonFormatterTests test, (bool isValid, string errors) validationResult)
        {
            ArgumentNullException.ThrowIfNull(test);
            validationResult.isValid.Should().BeTrue();
            validationResult.errors.Should().BeNullOrEmpty();
        }

        /// <summary>
        /// Asserts that the validation result indicates invalid JSON with errors.
        /// </summary>
        /// <param name="test">The JsonFormatterTests instance.</param>
        /// <param name="validationResult">The validation result tuple.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="validationResult"/> is default.</exception>
        public static void ShouldBeInvalidWithErrors(this JsonFormatterTests test, (bool isValid, string errors) validationResult)
        {
            ArgumentNullException.ThrowIfNull(test);
            validationResult.isValid.Should().BeFalse();
            validationResult.errors.Should().NotBeNullOrEmpty();
        }
    }
}