using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using InHouseCS2.Core.Clients.Contracts;
using Microsoft.Extensions.Logging;

namespace InHouseCS2.Core.Clients;

public class AzureBlobStorageClient : IMediaStorageClient
{
    private readonly BlobContainerClient blobContainerClient;
    private readonly ILogger<AzureBlobStorageClient> logger;

    public AzureBlobStorageClient(BlobContainerClient blobContainerClient, ILogger<AzureBlobStorageClient> logger)
    {
        this.blobContainerClient = blobContainerClient;
        this.logger = logger;
    }

    public Uri GetUploadUrl(string fileName, string fileExtension, double hoursValidFor)
    {
        this.logger.LogInformation($"Can generate SAS URI - {this.blobContainerClient.CanGenerateSasUri}");
        var blobClient = this.blobContainerClient.GetBlobClient($"{fileName}.{fileExtension}");
        this.logger.LogInformation($"Blob URI is: {blobClient.Uri}");

        var blobSasBuilder = new BlobSasBuilder()
        {
            BlobContainerName = this.blobContainerClient.Name,
            // We probably don't want users to tell us the filename, that could cause unintentional overwrites
            // Service should come up with a guid and store that
            BlobName = $"{fileName}.{fileExtension}",
            Resource = "b"
        };

        blobSasBuilder.SetPermissions(BlobContainerSasPermissions.Create | BlobContainerSasPermissions.Write);
        blobSasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(hoursValidFor);

        return blobClient.GenerateSasUri(blobSasBuilder);
    }
}
