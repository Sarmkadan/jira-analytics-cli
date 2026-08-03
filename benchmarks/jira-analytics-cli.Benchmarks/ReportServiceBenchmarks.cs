[MemoryDiagnoser]
public class ReportServiceBenchmarks
{
    [Benchmark]
    public void Benchmark_ReportService_GetReport([Params(10)] int reportId)
    {
        // setup test data in [GlobalSetup]
        var reportService = new ReportService();
        var report = reportService.GetReport(reportId);
        // assert
    }

    [Benchmark]
    public void Benchmark_ReportService_GetReports([Params(100)] int startIndex, [Params(100)] int count)
    {
        // setup test data in [GlobalSetup]
        var reportService = new ReportService();
        var reports = reportService.GetReports(startIndex, count);
        // assert
    }

    [Benchmark]
    public void Benchmark_ReportService_CreateReport([Params(1000)] Report report)
    {
        // setup test data in [GlobalSetup]
        var reportService = new ReportService();
        reportService.CreateReport(report);
        // assert
    }
}