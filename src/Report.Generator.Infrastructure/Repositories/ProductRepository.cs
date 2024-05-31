using System.Data;
using System.Runtime.CompilerServices;
using Dapper;
using Npgsql;
using Report.Generator.Domain.Entities;
using Report.Generator.Domain.Parameters;

namespace Report.Generator.Infrastructure.Repositories;

public class ProductRepository
{
    public static async IAsyncEnumerable<Product> FetchProductsAsync(ReportParameter parameter, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        using var connection = new NpgsqlConnection(Environment.GetEnvironmentVariable("DATABASE_CONNECTION"));

        await connection.OpenAsync(cancellationToken);

        try
        {
            string sqlQuery = @"SELECT id, title, price, created_at FROM products WHERE DATE(created_at) >= @referenceDate ORDER BY id ASC OFFSET @offset LIMIT @limit";
            int maxSize = 500_000;
            short limit = 10_000;
            long offset = 0;
            int totalFoundItems = 0;

            while (true)
            {
                var parameters = new DynamicParameters();
                parameters.Add("offset", offset, DbType.Int64);
                parameters.Add("limit", limit, DbType.Int16);
                parameters.Add("referenceDate", parameter.ReferenceDate, DbType.Date);

                bool hasAvailableData = false;

                await foreach (var product in connection.QueryUnbufferedAsync<Product>(sqlQuery, parameters, commandTimeout: 60).WithCancellation(cancellationToken))
                {
                    hasAvailableData = true;
                    totalFoundItems++;
                    yield return product;
                }

                if (!hasAvailableData || totalFoundItems >= maxSize) break;

                offset += limit;
            }
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}