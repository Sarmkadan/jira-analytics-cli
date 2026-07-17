using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace JiraAnalyticsCli.Tests.Services
{
    /// <summary>
    /// Extension methods that make it easier to work with <see cref="JqlQueryServiceTests"/>.
    /// </summary>
    public static class JqlQueryServiceTestsExtensions
    {
        /// <summary>
        /// Executes every parameter-less test method on the supplied <see cref="JqlQueryServiceTests"/>
        /// instance and returns the names of the tests that completed without throwing.
        /// </summary>
        /// <param name="tests">The test instance to run.</param>
        /// <returns>A read-only list containing the names of the successful test methods.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
        public static async Task<IReadOnlyList<string>> GetPassingTestMethodNamesAsync(this JqlQueryServiceTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            var methods = typeof(JqlQueryServiceTests)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => m.GetParameters().Length == 0 &&
                    (m.ReturnType == typeof(Task) || m.ReturnType == typeof(void)));

            var passing = new List<string>();

            foreach (var method in methods)
            {
                try
                {
                    var result = method.Invoke(tests, null);
                    if (result is Task task)
                    {
                        await task.ConfigureAwait(false);
                    }

                    passing.Add(method.Name);
                }
                catch (TargetInvocationException tie) when (tie.InnerException != null)
                {
                    // The test threw; treat as failure – do not add to the passing list.
                }
            }

            return passing.AsReadOnly();
        }

        /// <summary>
        /// Executes every parameter-less test method on the supplied <see cref="JqlQueryServiceTests"/>
        /// instance and returns the names of the tests that threw an exception.
        /// </summary>
        /// <param name="tests">The test instance to run.</param>
        /// <returns>A read-only list containing the names of the failed test methods.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
        public static async Task<IReadOnlyList<string>> GetFailingTestMethodNamesAsync(this JqlQueryServiceTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            var methods = typeof(JqlQueryServiceTests)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => m.GetParameters().Length == 0 &&
                    (m.ReturnType == typeof(Task) || m.ReturnType == typeof(void)));

            var failing = new List<string>();

            foreach (var method in methods)
            {
                try
                {
                    var result = method.Invoke(tests, null);
                    if (result is Task task)
                    {
                        await task.ConfigureAwait(false);
                    }
                }
                catch (TargetInvocationException tie) when (tie.InnerException != null)
                {
                    failing.Add(method.Name);
                }
            }

            return failing.AsReadOnly();
        }

        /// <summary>
        /// Runs all parameter-less test methods on the supplied <see cref="JqlQueryServiceTests"/>
        /// instance. If any test fails, an <see cref="AggregateException"/> containing all
        /// inner exceptions is thrown.
        /// </summary>
        /// <param name="tests">The test instance to run.</param>
        /// <returns>A task that completes when all tests have been executed.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
        /// <exception cref="AggregateException">Thrown when one or more tests throw.</exception>
        public static async Task RunAllAsync(this JqlQueryServiceTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            var methods = typeof(JqlQueryServiceTests)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => m.GetParameters().Length == 0 &&
                    (m.ReturnType == typeof(Task) || m.ReturnType == typeof(void)));

            var exceptions = new List<Exception>();

            foreach (var method in methods)
            {
                try
                {
                    var result = method.Invoke(tests, null);
                    if (result is Task task)
                    {
                        await task.ConfigureAwait(false);
                    }
                }
                catch (TargetInvocationException tie) when (tie.InnerException != null)
                {
                    exceptions.Add(tie.InnerException);
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}