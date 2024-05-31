namespace Report.Generator;

internal class Program
{
    static void Main()
    {
        int totalProducts = GetTotalProductsUserInput();
        DateTime referenceDate = GetReferenceDateUserInput();
        Console.WriteLine($"Número total de produtos: {totalProducts}");
        Console.WriteLine($"Data de referência: {referenceDate:dd/MM/yyyy}");
    }

    private static DateTime GetReferenceDateUserInput()
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

    private static int GetTotalProductsUserInput()
    {
        Console.WriteLine("Por favor, especifique o número total de produtos do relatório:");
        string? totalProductsInput = Console.ReadLine();
        int totalProducts;

        while (!int.TryParse(totalProductsInput, out totalProducts) || totalProducts <= 0)
        {
            Console.WriteLine("Entrada inválida. Por favor, insira um número inteiro positivo para o número total de produtos:");
            totalProductsInput = Console.ReadLine();
        }

        return totalProducts;
    }
}
