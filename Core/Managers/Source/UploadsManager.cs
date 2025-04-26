using InHouseCS2.Core.Clients.Contracts;
using InHouseCS2.Core.Managers.Contracts;
using Microsoft.Extensions.Logging;

namespace InHouseCS2.Core.Managers;

public class UploadsManager : IUploadsManager
{
    private readonly IMediaStorageClient mediaStorageClient;
    private readonly ILogger<UploadsManager> logger;

    public UploadsManager(IMediaStorageClient mediaStorageClient, ILogger<UploadsManager> logger)
    {
        this.mediaStorageClient = mediaStorageClient;
        this.logger = logger;
    }

    public string GetUploadURL(string fileName, string fileExtension)
    {
        this.logger.LogInformation($"Getting upload URL for {fileName}.{fileExtension}");
        return this.mediaStorageClient.GetUploadUrl(fileName, fileExtension, 300).ToString();
    }
}
