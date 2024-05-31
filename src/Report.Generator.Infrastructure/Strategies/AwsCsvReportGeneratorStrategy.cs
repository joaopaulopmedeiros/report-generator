using Report.Generator.Domain.Interfaces;
using Report.Generator.Domain.Parameters;

namespace Report.Generator.Infrastructure.Strategies;

public class AwsCsvReportGeneratorStrategy : IReportGeneratorStrategy
{
    public async Task ExecuteAsync(ReportParameter parameter)
    {
        await Task.Delay(500);
    }
}
