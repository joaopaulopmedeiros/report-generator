using CsvHelper;
using Google.Cloud.Storage.V1;
using System.Buffers;
using System.Globalization;

using Report.Generator.Domain.Interfaces;
using Report.Generator.Domain.Parameters;
using Report.Generator.Infrastructure.Repositories;
using Google.Apis.Storage.v1.Data;

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

        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream);
        using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);

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
                    var partName = await UploadPartAsync(buffer, bufferPosition, bucketName, fileName, partNumber++);
                    uploadedParts.Add(partName);
                    bufferPosition = 0;
                }
            }
        }

        if (bufferPosition > 0)
        {
            var partName = await UploadPartAsync(buffer, bufferPosition, bucketName, fileName, partNumber);
            uploadedParts.Add(partName);
        }

        ArrayPool<byte>.Shared.Return(buffer);

        await ComposeObjectAsync(bucketName, uploadedParts, fileName);
    }

    private async Task<string> UploadPartAsync(byte[] buffer, int bufferLength, string bucketName, string fileName, int partNumber)
    {
        var partName = $"{fileName}_part_{partNumber}";
        using var partStream = new MemoryStream(buffer, 0, bufferLength);
        await _storageClient.UploadObjectAsync(bucketName, partName, "text/csv", partStream);
        Console.WriteLine($"Uploaded part {partNumber}");
        return partName;
    }

    private async Task ComposeObjectAsync(string bucketName, List<string> partNames, string finalFileName)
    {
        var sourceObjects = partNames.Select(part => new ComposeRequest.SourceObjectsData { Name = part }).ToList();

        var targetObjectName = $"{finalFileName}.csv";

        var composeRequest = new ComposeRequest
        {
            Destination = new Google.Apis.Storage.v1.Data.Object { Bucket = bucketName, Name = targetObjectName },
            SourceObjects = sourceObjects
        };

        await _storageClient.Service.Objects.Compose(composeRequest, bucketName, targetObjectName).ExecuteAsync();

        await DeletePartsAsync(bucketName, partNames);
    }

    private async Task DeletePartsAsync(string bucketName, List<string> partNames)
    {
        var tasks = new List<Task>();

        foreach (var partName in partNames)
        {
            tasks.Add(_storageClient.DeleteObjectAsync(bucketName, partName));
        }

        await Task.WhenAll(tasks);
    }
}
