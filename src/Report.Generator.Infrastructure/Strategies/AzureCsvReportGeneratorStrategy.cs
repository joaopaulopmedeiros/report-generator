using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Blobs;

using Report.Generator.Domain.Interfaces;
using Report.Generator.Domain.Parameters;
using CsvHelper;
using System.Buffers;
using System.Collections;
using System.Globalization;
using Report.Generator.Infrastructure.Repositories;
using System.Text;
using Report.Generator.Domain.Entities;
using Report.Generator.Infrastructure.Mappers;

namespace Report.Generator.Infrastructure.Strategies;

public class AzureCsvReportGeneratorStrategy(BlobContainerClient blobContainerClient) : IReportGeneratorStrategy
{
    private readonly BlobContainerClient _blobContainerClient = blobContainerClient;

    public async Task ExecuteAsync(ReportParameter parameter)
    {
        BlockBlobClient blobClient = _blobContainerClient.GetBlockBlobClient("relatorio.csv");

        int blockSize = 5 * 1024 * 1024;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(blockSize);
        int bufferPosition = 0;

        ArrayList blockIDArrayList = [];

        try
        {
            using MemoryStream memoryStream = new();
            using StreamWriter writer = new(memoryStream);
            using CsvWriter csvWriter = new(writer, CultureInfo.InvariantCulture);

            csvWriter.Context.RegisterClassMap<ProductCsvMapper>();
            csvWriter.WriteHeader<Product>();
            await csvWriter.NextRecordAsync();

            await foreach (var product in ProductRepository.FetchProductsAsync(parameter))
            {
                memoryStream.SetLength(0);
                csvWriter.WriteRecord(product);
                await csvWriter.NextRecordAsync();
                await writer.FlushAsync();
                memoryStream.Position = 0;

                while (memoryStream.Position < memoryStream.Length)
                {
                    int bytesToRead = Math.Min(blockSize - bufferPosition, (int)(memoryStream.Length - memoryStream.Position));
                    int bytesRead = await memoryStream.ReadAsync(buffer.AsMemory(bufferPosition, bytesToRead));
                    bufferPosition += bytesRead;

                    if (bufferPosition == blockSize)
                    {
                        await StageBlockAsync(blobClient, buffer, bufferPosition, blockIDArrayList);
                        bufferPosition = 0;
                    }
                }
            }

            if (bufferPosition > 0)
            {
                await StageBlockAsync(blobClient, buffer, bufferPosition, blockIDArrayList);
            }

            string[] blockIDArray = (string[])blockIDArrayList.ToArray(typeof(string));
            await blobClient.CommitBlockListAsync(blockIDArray);
        } 
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static async Task StageBlockAsync(BlockBlobClient blobClient, byte[] buffer, int bufferSize, ArrayList blockIDArrayList)
    {
        using MemoryStream blockStream = new(buffer, 0, bufferSize);
        string blockID = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
        blockIDArrayList.Add(blockID);
        await blobClient.StageBlockAsync(blockID, blockStream);
    }
}
