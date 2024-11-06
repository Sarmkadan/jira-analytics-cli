using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JiraAnalyticsCli.Tests.Utils
{
    /// <summary>
    /// Extension methods that aid in working with <see cref="FormattingHelpersTests"/>.
    /// </summary>
    public static class FormattingHelpersTestsExtensions
    {
        /// <summary>
        /// Returns the names of all public instance test methods declared on <see cref="FormattingHelpersTests"/>.
        /// </summary>
        /// <param name="tests">The test class instance.</param>
        /// <returns>An <see cref="IReadOnlyList{T}"/> of method names.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> GetTestMethodNames(this FormattingHelpersTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            var methods = tests.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => m.ReturnType == typeof(void) && m.GetParameters().Length == 0)
                .Select(m => m.Name)
                .ToArray();
            return methods;
        }

        /// <summary>
        /// Executes all parameter‑less public test methods on the supplied <see cref="FormattingHelpersTests"/> instance.
        /// </summary>
        /// <param name="tests">The test class instance.</param>
        /// <returns>
        /// An <see cref="IReadOnlyList{T}"/> of <see cref="TestResult"/> records indicating the method name and whether it completed without throwing.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <c>null</c>.</exception>
        public static IReadOnlyList<TestResult> RunAllTests(this FormattingHelpersTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            var results = new List<TestResult>();
            foreach (var method in tests.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => m.ReturnType == typeof(void) && m.GetParameters().Length == 0))
            {
                try
                {
                    method.Invoke(tests, null);
                    results.Add(new TestResult(method.Name, true));
                }
                catch
                {
                    results.Add(new TestResult(method.Name, false));
                }
            }
            return results;
        }

        /// <summary>
        /// Represents the outcome of a single test method execution.
        /// </summary>
        /// <param name="MethodName">The name of the test method.</param>
        /// <param name="Succeeded">True if the method completed without throwing; otherwise false.</param>
        public sealed record TestResult(string MethodName, bool Succeeded);
    }
}
