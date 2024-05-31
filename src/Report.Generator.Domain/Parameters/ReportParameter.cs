namespace Report.Generator.Domain.Parameters;

public class ReportParameter(DateOnly referenceDate)
{
    public DateOnly ReferenceDate { get; set; } = referenceDate;
}
