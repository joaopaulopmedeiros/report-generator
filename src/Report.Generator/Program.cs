using Report.Generator.Infrastructure.Contexts;

namespace Report.Generator;

internal class Program
{
    static async Task Main()
    {
        DateTime referenceDate = UI.GetReferenceDateUserInput();
        string providerName = "AWS";

        Console.WriteLine($"Data de referência: {referenceDate:dd/MM/yyyy}");
        Console.WriteLine($"Cloud Provider: {providerName}");

        ReportGeneratorContext context = new(providerName);
        await context.ExecuteAsync(new(referenceDate));
    }
}
