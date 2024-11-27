using System;
using System.Threading.Tasks;
using JiraAnalyticsCli.Tests.Services;

public static class HtmlReportServiceTestsExtensions
{
    public static bool HasValidTestMethods(this HtmlReportServiceTests tests)
    {
        return tests.GetType().GetMethods().Length > 0;
    }

    public static async Task RunAllTestsAsync(this HtmlReportServiceTests tests)
    {
        foreach (var method in tests.GetType().GetMethods())
        {
            if (method.ReturnType == typeof(void))
            {
                method.Invoke(tests, null);
            }
            else if (method.ReturnType == typeof(Task))
            {
                await (Task)method.Invoke(tests, null);
            }
        }
    }

    public static int GetTestCount(this HtmlReportServiceTests tests)
    {
        return tests.GetType().GetMethods().Length;
    }

    public static string GetTestName(this HtmlReportServiceTests tests, int index)
    {
        var methods = tests.GetType().GetMethods();
        if (index < 0 || index >= methods.Length)
        {
            throw new IndexOutOfRangeException("Index out of range");
        }
        return methods[index].Name;
    }
}
