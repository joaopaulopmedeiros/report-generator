using CsvHelper.Configuration;

using Report.Generator.Domain.Entities;

namespace Report.Generator.Infrastructure.Mappers;

public class ProductCsvMapper : ClassMap<Product>
{
    public ProductCsvMapper()
    {
        Map(m => m.Id).Index(0).Name("id");
        Map(m => m.Title).Index(1).Name("titulo");
        Map(m => m.Price).Index(2).Name("preço");
        Map(m => m.CreatedAt).Index(3).Name("data de criação").TypeConverterOption.Format("dd/MM/yyyy");
    }
}