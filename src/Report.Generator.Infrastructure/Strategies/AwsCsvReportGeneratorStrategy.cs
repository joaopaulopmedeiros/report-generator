using System.Buffers;
using System.Globalization;

using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

using CsvHelper;

using Report.Generator.Domain.Entities;
using Report.Generator.Domain.Interfaces;
using Report.Generator.Domain.Parameters;
using Report.Generator.Infrastructure.Mappers;
using Report.Generator.Infrastructure.Repositories;

using Serilog;

namespace Report.Generator.Infrastructure.Strategies;

public class AwsCsvReportGeneratorStrategy(IAmazonS3 s3Client) : IReportGeneratorStrategy
{
    private readonly IAmazonS3 _s3Client = s3Client;

    public async Task ExecuteAsync(ReportParameter parameter)
    {
        string bucketName = Environment.GetEnvironmentVariable("AWS_S3_BUCKET_NAME") ?? "report-bucket";
        string keyName = "relatorio.csv";
        int blockSize = 5 * 1024 * 1024;
        int partNumber = 1;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(blockSize);
        int bufferPosition = 0;

        List<UploadPartResponse> uploadResponses = [];
        InitiateMultipartUploadRequest initiateRequest = new()
        {
            BucketName = bucketName,
            Key = keyName
        };

        InitiateMultipartUploadResponse initResponse =
            await _s3Client.InitiateMultipartUploadAsync(initiateRequest);

        try
        {
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream);
            using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);

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
                        await UploadPartAsync(buffer, bufferPosition, bucketName, keyName, initResponse.UploadId, partNumber++, uploadResponses);
                        bufferPosition = 0;
                    }
                }
            }

            if (bufferPosition > 0)
            {
                await UploadPartAsync(buffer, bufferPosition, bucketName, keyName, initResponse.UploadId, partNumber, uploadResponses);
            }

            CompleteMultipartUploadRequest completeRequest = new()
            {
                BucketName = bucketName,
                Key = keyName,
                UploadId = initResponse.UploadId
            };

            completeRequest.AddPartETags(uploadResponses);

            CompleteMultipartUploadResponse completeUploadResponse =
                await _s3Client.CompleteMultipartUploadAsync(completeRequest);
        }
        catch (Exception exception)
        {
            Log.Error("An Exception was thrown: {0}", exception.Message);

            AbortMultipartUploadRequest abortMPURequest = new()
            {
                BucketName = bucketName,
                Key = keyName,
                UploadId = initResponse.UploadId
            };
            await _s3Client.AbortMultipartUploadAsync(abortMPURequest);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private async Task UploadPartAsync(byte[] buffer, int bufferLength, string bucketName, string keyName, string uploadId, int partNumber, List<UploadPartResponse> uploadResponses)
    {
        using var partStream = new MemoryStream(buffer, 0, bufferLength);
        UploadPartRequest uploadRequest = new()
        {
            BucketName = bucketName,
            Key = keyName,
            UploadId = uploadId,
            PartNumber = partNumber,
            PartSize = bufferLength,
            InputStream = partStream
        };

        uploadRequest.StreamTransferProgress += new EventHandler<StreamTransferProgressArgs>(UploadPartProgressEventCallback);
        uploadResponses.Add(await _s3Client.UploadPartAsync(uploadRequest));
    }

    public void UploadPartProgressEventCallback(object? sender, StreamTransferProgressArgs e)
    {
        Log.Information("{0}/{1}", e.TransferredBytes, e.TotalBytes);
    }
}
