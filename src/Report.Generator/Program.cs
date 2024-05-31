﻿using Report.Generator.Infrastructure.Contexts;

using Serilog;

namespace Report.Generator;

internal class Program
{
    static async Task Main()
    {
        Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
        DateTime referenceDate = UI.GetReferenceDateUserInput();
        string providerName = "AWS";

        Log.Information($"Iniciando processamento de relatório em formato .csv referente à data: {referenceDate:dd/MM/yyyy} em {providerName}");

        ReportGeneratorContext context = new(providerName);
        await context.ExecuteAsync(new(referenceDate));

        Log.Information("Finalizado processamento de relatório");
    }
}
