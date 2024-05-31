using Amazon.S3;

using Azure.Storage.Blobs;

using Report.Generator.Domain.Interfaces;
using Report.Generator.Domain.Parameters;

namespace Report.Generator.Infrastructure.Strategies;

public class ReportGeneratorContext
{
    private readonly IReportGeneratorStrategy _strategy;

    public ReportGeneratorContext(string providerName)
    {
        if (providerName == "aws")
        {
            string? publicKey = Environment.GetEnvironmentVariable("AWS_S3_PUBLIC_KEY");
            string? privateKey = Environment.GetEnvironmentVariable("AWS_S3_PRIVATE_KEY");
            _strategy = new AwsCsvReportGeneratorStrategy(new AmazonS3Client(publicKey, privateKey, Amazon.RegionEndpoint.SAEast1));
        }
        else if (providerName == "azure")
        {
            string? connString = Environment.GetEnvironmentVariable("AZURE_BLOB_CONNECTION");
            BlobContainerClient blobContainerClient = new(connString, "dotnet-report-container");
            _strategy = new AzureCsvReportGeneratorStrategy(blobContainerClient);
        }
        else
        {
            throw new ArgumentException(null, nameof(providerName));
        }
    }

    public async Task ExecuteAsync(ReportParameter parameter) => await _strategy.ExecuteAsync(parameter);
}
