namespace Report.Generator.Infrastructure.Queries;

public static class ProductReportQuery
{
    public static string Get()
    {
        return @"
        SELECT id, title, price, created_at 
        FROM products 
        WHERE DATE(created_at) >= @referenceDate 
        ORDER BY id ASC 
        OFFSET @offset LIMIT @limit";
    }
}
