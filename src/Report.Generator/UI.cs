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
}
