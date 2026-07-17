using System;
using System.Reflection;
using System.Threading.Tasks;
using JiraAnalyticsCli.Tests.Services;

/// <summary>
/// Provides extension methods for <see cref="HtmlReportServiceTests"/> to facilitate test discovery and execution.
/// </summary>
public static class HtmlReportServiceTestsExtensions
{
	/// <summary>
	/// Gets the number of test methods defined in the test class.
	/// </summary>
	/// <param name="tests">The test instance.</param>
	/// <returns>The count of test methods.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
	public static int GetTestCount(this HtmlReportServiceTests tests)
	{
		ArgumentNullException.ThrowIfNull(tests);

		return tests.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
			.Where(m => m.ReturnType == typeof(void) || m.ReturnType == typeof(Task))
			.Count();
	}

	/// <summary>
	/// Gets the name of the test method at the specified index.
	/// </summary>
	/// <param name="tests">The test instance.</param>
	/// <param name="index">The zero-based index of the test method.</param>
	/// <returns>The name of the test method.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0 or greater than or equal to the test count.</exception>
	public static string GetTestName(this HtmlReportServiceTests tests, int index)
	{
		ArgumentNullException.ThrowIfNull(tests);

		var methods = tests.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
			.Where(m => m.ReturnType == typeof(void) || m.ReturnType == typeof(Task))
			.ToArray();

		if (index < 0 || index >= methods.Length)
		{
			throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be within the valid range of test methods.");
		}

		return methods[index].Name;
	}

	/// <summary>
	/// Executes all test methods asynchronously.
	/// </summary>
	/// <param name="tests">The test instance.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
	public static async Task RunAllTestsAsync(this HtmlReportServiceTests tests)
	{
		ArgumentNullException.ThrowIfNull(tests);

		var methods = tests.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
			.Where(m => m.ReturnType == typeof(void) || m.ReturnType == typeof(Task))
			.ToArray();

		foreach (var method in methods)
		{
			try
			{
				var result = method.Invoke(tests, null);

				if (result is Task task)
				{
					await task;
				}
			}
			catch (TargetInvocationException ex) when (ex.InnerException != null)
			{
				throw ex.InnerException;
			}
		}
	}
}