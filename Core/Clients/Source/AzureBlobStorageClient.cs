﻿using Azure.Storage.Blobs;
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

    public MediaUploadInfo GetUploadUrl(string fileExtension, double hoursValidFor)
    {
        var guid = Guid.NewGuid();
        var blobClient = this.blobContainerClient.GetBlobClient($"{guid}.{fileExtension}");
        this.logger.LogInformation($"Blob URI is: {blobClient.Uri} - Generating Upload URL");

        var blobSasBuilder = new BlobSasBuilder()
        {
            BlobContainerName = this.blobContainerClient.Name,
            // Service should come up with a guid and store that to prevent users from uploading the same file name with different files
            BlobName = $"{guid}.{fileExtension}",
            Resource = "b"
        };

        blobSasBuilder.SetPermissions(BlobContainerSasPermissions.Create | BlobContainerSasPermissions.Write);
        blobSasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(hoursValidFor);

        return new MediaUploadInfo(blobClient.GenerateSasUri(blobSasBuilder), blobClient.Uri);
    }

    public Uri GetDownloadUrl(string fileUri, double hoursValidFor)
    {
        var guid = Guid.NewGuid();
        var blobClient = this.blobContainerClient.GetBlobClient(fileUri);
        this.logger.LogInformation($"Blob URI is: {blobClient.Uri} - Generating Download URL");

        var blobSasBuilder = new BlobSasBuilder()
        {
            BlobContainerName = this.blobContainerClient.Name,
            BlobName = fileUri,
            Resource = "b"
        };

        blobSasBuilder.SetPermissions(BlobContainerSasPermissions.Read);
        blobSasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(hoursValidFor);

        return blobClient.GenerateSasUri(blobSasBuilder);
    }
}
