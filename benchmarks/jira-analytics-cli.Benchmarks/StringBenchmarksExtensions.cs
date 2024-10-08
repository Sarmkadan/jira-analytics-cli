using System;
using System.Text.RegularExpressions;

namespace jira_analytics_cli.Benchmarks
{
    public static class StringBenchmarksExtensions
    {
        public static string ToTitleCase(this StringBenchmarks str)
        {
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
        }

        public static string ReverseString(this StringBenchmarks str)
        {
            char[] arr = str.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }

        public static bool IsPalindrome(this StringBenchmarks str)
        {
            str = str.RemoveWhitespace;
            char[] arr = str.ToCharArray();
            Array.Reverse(arr);
            return new string(arr) == str;
        }
    }
}
