using Report.Generator.Domain.Parameters;

namespace Report.Generator.Domain.Interfaces;

public interface IReportGeneratorStrategy
{
    public Task ExecuteAsync(ReportParameter parameter);
}
