using CsvHelper;
using Google.Cloud.Storage.V1;
using System.Buffers;
using System.Globalization;

using Report.Generator.Domain.Interfaces;
using Report.Generator.Domain.Parameters;
using Report.Generator.Infrastructure.Repositories;
using Google.Apis.Storage.v1.Data;
using Serilog;

namespace Report.Generator.Infrastructure.Strategies;

public class GcpCsvReportGeneratorStrategy(StorageClient storageClient) : IReportGeneratorStrategy
{
    private readonly StorageClient _storageClient = storageClient;

    public async Task ExecuteAsync(ReportParameter parameter)
    {
        string bucketName = Environment.GetEnvironmentVariable("GCP_BUCKET_NAME") ?? "report-bucket";
        string fileName = "relatorio";
        int blockSize = 5 * 1024 * 1024;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(blockSize);
        int bufferPosition = 0;
        int partNumber = 1;
        var uploadedParts = new List<string>();

        try
        {
            using MemoryStream memoryStream = new();
            using StreamWriter writer = new(memoryStream);
            using CsvWriter csvWriter = new(writer, CultureInfo.InvariantCulture);

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
                        string partName = await UploadPartAsync(buffer, bufferPosition, bucketName, fileName, partNumber++);
                        uploadedParts.Add(partName);
                        bufferPosition = 0;
                    }
                }
            }

            if (bufferPosition > 0)
            {
                string partName = await UploadPartAsync(buffer, bufferPosition, bucketName, fileName, partNumber);
                uploadedParts.Add(partName);
            }

            await ComposeObjectAsync(bucketName, uploadedParts, fileName);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private async Task<string> UploadPartAsync(byte[] buffer, int bufferLength, string bucketName, string fileName, int partNumber)
    {
        string partName = $"{fileName}_part_{partNumber}";
        using MemoryStream partStream = new(buffer, 0, bufferLength);
        await _storageClient.UploadObjectAsync(bucketName, partName, "text/csv", partStream);
        Log.Information($"Uploaded part {partNumber}");
        return partName;
    }

    private async Task ComposeObjectAsync(string bucketName, List<string> partNames, string finalFileName)
    {
        List<ComposeRequest.SourceObjectsData> sourceObjects = partNames.Select(part => new ComposeRequest.SourceObjectsData { Name = part }).ToList();

        string targetObjectName = $"{finalFileName}.csv";

        ComposeRequest composeRequest = new()
        {
            Destination = new Google.Apis.Storage.v1.Data.Object { Bucket = bucketName, Name = targetObjectName },
            SourceObjects = sourceObjects
        };

        await _storageClient.Service.Objects.Compose(composeRequest, bucketName, targetObjectName).ExecuteAsync();

        await DeletePartsAsync(bucketName, partNames);
    }

    private async Task DeletePartsAsync(string bucketName, List<string> partNames)
    {
        List<Task> tasks = [];

        foreach (var partName in partNames)
        {
            tasks.Add(_storageClient.DeleteObjectAsync(bucketName, partName));
        }

        await Task.WhenAll(tasks);
    }
}
