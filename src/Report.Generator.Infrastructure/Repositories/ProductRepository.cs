using Dapper;
using Npgsql;

using Report.Generator.Domain.Entities;

using System.Runtime.CompilerServices;

namespace Report.Generator.Infrastructure.Repositories;

public class ProductRepository
{
    public static async IAsyncEnumerable<Product> FetchProductsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        using var connection = new NpgsqlConnection(Environment.GetEnvironmentVariable("DATABASE_CONNECTION"));
        await connection.OpenAsync(cancellationToken);

        try
        {
            long limit = 10_000;
            long offset = 0;
            long totalFoundItems = 0;
            string sqlQuery = "SELECT id, title, price, created_at FROM products ORDER BY id ASC OFFSET @offset LIMIT @limit";

            while (true)
            {
                var parameters = new DynamicParameters();
                parameters.Add("offset", offset);
                parameters.Add("limit", limit);

                bool hasAvailableData = false;

                await foreach (var product in connection.QueryUnbufferedAsync<Product>(sqlQuery, parameters, commandTimeout: 60).WithCancellation(cancellationToken))
                {
                    hasAvailableData = true;
                    totalFoundItems++;
                    yield return product;
                }

                if (!hasAvailableData || totalFoundItems >= 500_000) break;

                offset += limit;
            }
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}