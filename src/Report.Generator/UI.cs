namespace Report.Generator;

public class UI
{
    public static DateOnly GetReferenceDateUserInput()
    {
        Console.WriteLine("Please input a reference date in format: dd/mm/yyyy:");
        string? referenceDateInput = Console.ReadLine();
        DateOnly referenceDate;

        while (!DateOnly.TryParseExact(referenceDateInput, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out referenceDate))
        {
            Console.WriteLine("Invalid input. Please add a reference date in format: dd/mm/yyyy:");
            referenceDateInput = Console.ReadLine();
        }

        return referenceDate;
    }

    public static string GetCloudProviderUserInput()
    {
        Console.WriteLine("Please input a cloud provider (aws, gcp ou azure):");
        string? cloudProviderInput = Console.ReadLine()?.ToLower();

        while (cloudProviderInput != "aws" && cloudProviderInput != "gcp" && cloudProviderInput != "azure")
        {
            Console.WriteLine("Invalid input. Please add 'aws', 'gcp' or 'azure' in lower case:");
            cloudProviderInput = Console.ReadLine()?.ToLower();
        }

        return cloudProviderInput;
    }
}
