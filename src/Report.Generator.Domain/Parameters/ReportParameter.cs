namespace Report.Generator.Domain.Parameters;

public class ReportParameter(DateTime referenceDate)
{
    public DateTime ReferenceDate { get; set; } = referenceDate;
}
