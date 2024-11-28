using System;
using System.Globalization;
using JiraAnalyticsCli.Utils;

namespace JiraAnalyticsCli.Benchmarks
{
    /// <summary>
    /// Extension methods for string manipulation used in benchmark scenarios.
    /// </summary>
    public static class StringBenchmarksExtensions
    {
        /// <summary>
        /// Converts the string to title case using invariant culture.
        /// </summary>
        /// <param name="str">The input string to convert.</param>
        /// <returns>The string in title case format.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="str"/> is null.</exception>
        public static string ToTitleCase(this string str)
        {
            ArgumentNullException.ThrowIfNull(str);

            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(str.ToLowerInvariant());
        }

        /// <summary>
        /// Reverses the characters in the string.
        /// </summary>
        /// <param name="str">The input string to reverse.</param>
        /// <returns>The reversed string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="str"/> is null.</exception>
        public static string ReverseString(this string str)
        {
            ArgumentNullException.ThrowIfNull(str);

            var arr = str.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }

        /// <summary>
        /// Determines whether the string is a palindrome (reads the same forwards and backwards).
        /// </summary>
        /// <param name="str">The input string to check.</param>
        /// <returns>True if the string is a palindrome; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="str"/> is null.</exception>
        public static bool IsPalindrome(this string str)
        {
            ArgumentNullException.ThrowIfNull(str);

            var normalized = str.RemoveWhitespace();
            var arr = normalized.ToCharArray();
            Array.Reverse(arr);
            return new string(arr) == normalized;
        }
    }
}
