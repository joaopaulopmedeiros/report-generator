namespace Report.Generator;

public class UI
{
    public static DateTime GetReferenceDateUserInput()
    {
        Console.WriteLine("Por favor, especifique a data de referência no formato: dd/mm/aaaa:");
        string? dataReferenciaInput = Console.ReadLine();
        DateTime dataReferencia;

        while (!DateTime.TryParseExact(dataReferenciaInput, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dataReferencia))
        {
            Console.WriteLine("Entrada inválida. Por favor, insira uma data no formato dd/mm/aaaa:");
            dataReferenciaInput = Console.ReadLine();
        }

        return dataReferencia;
    }

    public static string GetCloudProviderUserInput()
    {
        Console.WriteLine("Por favor, especifique o provedor de nuvem (aws, gcp ou azure):");
        string? cloudProviderInput = Console.ReadLine()?.ToLower();

        while (cloudProviderInput != "aws" && cloudProviderInput != "gcp" && cloudProviderInput != "azure")
        {
            Console.WriteLine("Entrada inválida. Por favor, insira 'aws', 'gcp' ou 'azure':");
            cloudProviderInput = Console.ReadLine()?.ToLower();
        }

        return cloudProviderInput;
    }
}
