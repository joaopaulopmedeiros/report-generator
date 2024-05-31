using Report.Generator.Infrastructure.Strategies;

using Serilog;

namespace Report.Generator;

internal class Program
{
    static async Task Main()
    {
        Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

        DateTime referenceDate = UI.GetReferenceDateUserInput();
        string providerName = UI.GetCloudProviderUserInput();

        Log.Information($"Started report about products created in {referenceDate:dd/MM/yyyy}. Available at {providerName}");

        ReportGeneratorContext context = new(providerName);
        await context.ExecuteAsync(new(referenceDate));

        Log.Information("Finished report");
    }
}
