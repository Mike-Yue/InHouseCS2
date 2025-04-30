using InHouseCS2.Core.Clients.Contracts;
using InHouseCS2.Core.EntityStores.Contracts;
using InHouseCS2.Core.EntityStores.Models;
using InHouseCS2.Core.Managers.Contracts;
using InHouseCS2.Core.Managers.Contracts.Models;
using InHouseCS2.Core.Managers.Models;
using Microsoft.Extensions.Logging;

namespace InHouseCS2.Core.Managers;

public class UploadsManager : IUploadsManager
{
    private readonly IMediaStorageClient mediaStorageClient;
    private readonly IEntityStore<MatchUploadEntity> matchUploadEntityStore;
    private readonly ILogger<UploadsManager> logger;

    public UploadsManager(
        IMediaStorageClient mediaStorageClient,
        IEntityStore<MatchUploadEntity> matchUploadEntityStore,
        ILogger<UploadsManager> logger)
    {
        this.mediaStorageClient = mediaStorageClient;
        this.matchUploadEntityStore = matchUploadEntityStore;
        this.logger = logger;
    }

    public Task FinalizeMatchUploadEntityAndRecordData(CoreMatchDataRecord coreMatchDataRecord)
    {
        throw new NotImplementedException();
    }

    public async Task<string> GetMatchUploadStatus(int id)
    {
        var matchUpload = await this.matchUploadEntityStore.Get(id);
        return matchUpload.CreatedAt.ToString();
    }

    public async Task<UploadMetaData?> GetUploadURL(string fileFingerprint, string fileExtension)
    {
        var filesWithSameFingerPrint = await this.matchUploadEntityStore.FindAll((x) => x.DemoFingerprint == fileFingerprint && x.Status != MatchUploadStatus.FailedToUpload);
        if (filesWithSameFingerPrint.Count > 0)
        {
            return null;
        }

        var uploadInfo = this.mediaStorageClient.GetUploadUrl(fileExtension, 1);

        var id = await this.matchUploadEntityStore.Create(() =>
        {
            var newMatchUpload = new MatchUploadEntity
            {
                DemoFingerprint = fileFingerprint,
                Status = MatchUploadStatus.Initialized,
                DemoMediaStoreUri = uploadInfo.mediaUri.ToString(),
                CreatedAt = DateTime.Now,
                LastUpdatedAt = DateTime.Now
            };
            return newMatchUpload;
        });
        return new UploadMetaData(uploadInfo.uploadUri, id);
    }

    public async Task UpdateMatchStatusAndPersistWork(Uri mediaStorageUri)
    {
        var entities = await this.matchUploadEntityStore.FindAll((x) => x.DemoMediaStoreUri == mediaStorageUri.ToString());
        if (entities.Count == 0)
        {
            this.logger.LogInformation("No entities found. Returning");
            return;
        }

        if (entities.Count == 1)
        {
            this.logger.LogInformation("1 entity found. Working");

            await this.matchUploadEntityStore.Update(entities[0].Id, (matchUploadEntity) =>
            {
                matchUploadEntity.Status = MatchUploadStatus.Uploaded;
            });
        }
        else
        {
            throw new Exception("Multiple rows found with the same media storage Uri");
        }
    }
}
