using Report.Generator.Domain.Interfaces;
using Report.Generator.Domain.Parameters;
using Report.Generator.Infrastructure.Strategies;

namespace Report.Generator.Infrastructure.Contexts;

public class ReportGeneratorContext
{
    private readonly IReportGeneratorStrategy _strategy;

    public ReportGeneratorContext(string providerName)
    {
        if (providerName == null || providerName != "AWS") throw new ArgumentNullException(nameof(providerName));

        _strategy = new AwsCsvReportGeneratorStrategy();
    }

    public async Task ExecuteAsync(ReportParameter parameter) => await _strategy.ExecuteAsync(parameter);
}
