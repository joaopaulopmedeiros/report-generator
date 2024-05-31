namespace Report.Generator;

internal class Program
{
    static void Main()
    {
        Console.WriteLine("Por favor, especifique o número total de produtos do relatório:");
        string? totalProdutosInput = Console.ReadLine();
        int totalProdutos;

        while (!int.TryParse(totalProdutosInput, out totalProdutos) || totalProdutos <= 0)
        {
            Console.WriteLine("Entrada inválida. Por favor, insira um número inteiro positivo para o número total de produtos:");
            totalProdutosInput = Console.ReadLine();
        }

        Console.WriteLine("Por favor, especifique a data de referência (formato: dd/mm/aaaa):");
        string? dataReferenciaInput = Console.ReadLine();
        DateTime dataReferencia;

        while (!DateTime.TryParseExact(dataReferenciaInput, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dataReferencia))
        {
            Console.WriteLine("Entrada inválida. Por favor, insira uma data no formato dd/mm/aaaa:");
            dataReferenciaInput = Console.ReadLine();
        }

        Console.WriteLine($"\nNúmero total de produtos: {totalProdutos}");
        Console.WriteLine($"Data de referência: {dataReferencia:dd/MM/yyyy}");
    }
}
