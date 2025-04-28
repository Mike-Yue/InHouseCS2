using InHouseCS2.Core.Clients;
using InHouseCS2.Core.Clients.Contracts;
using InHouseCS2.Core.EntityStores.Contracts;
using InHouseCS2.Core.EntityStores.Models;
using InHouseCS2.Core.Managers.Contracts;
using Microsoft.Extensions.Logging;

namespace InHouseCS2.Core.Managers;

public class UploadsManager : IUploadsManager
{
    private readonly IMediaStorageClient mediaStorageClient;
    private readonly IEntityStore<MatchUploadEntity> matchUploadEntityStore;
    private readonly ILogger<UploadsManager> logger;

    public UploadsManager(IMediaStorageClient mediaStorageClient, IEntityStore<MatchUploadEntity> matchUploadEntityStore, ILogger<UploadsManager> logger)
    {
        this.mediaStorageClient = mediaStorageClient;
        this.matchUploadEntityStore = matchUploadEntityStore;
        this.logger = logger;
    }

    public async Task<string> GetMatchUploadStatus(int id)
    {
        var matchUpload = await this.matchUploadEntityStore.Get(id);
        return matchUpload.CreatedAt.ToString();
    }

    public async Task<UploadMetaData> GetUploadURL(string fileName, string fileExtension)
    {
        this.logger.LogInformation($"Getting upload URL for {fileName}.{fileExtension}");
        var uploadInfo = this.mediaStorageClient.GetUploadUrl(fileName, fileExtension, 1);

        MatchUploadEntity newMatchUpload = this.CreateNewMatchUploadEntity(uploadInfo.mediaUri);
        var id = await this.matchUploadEntityStore.Create(newMatchUpload);
        return new UploadMetaData(uploadInfo.uploadUri, id);
    }

    public async Task SetMatchUploadStatusToUploaded(int id)
    {
        await this.matchUploadEntityStore.Update(id, (matchUploadEntity) =>
        {
            matchUploadEntity.Status = MatchUploadStatus.Uploaded;
        });
    }

    private MatchUploadEntity CreateNewMatchUploadEntity(Uri mediaStorageUri)
    {
        MatchUploadEntity newMatchUpload = new MatchUploadEntity();
        newMatchUpload.Status = MatchUploadStatus.Initialized;
        newMatchUpload.DemoMediaStoreUri = mediaStorageUri;
        newMatchUpload.CreatedAt = DateTime.Now;
        newMatchUpload.LastUpdatedAt = DateTime.Now;
        return newMatchUpload;
    }
}
