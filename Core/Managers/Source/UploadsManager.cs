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
    private readonly IEntityStore<ParseMatchTaskEntity> parseMatchTaskEntityStore;
    private readonly ILogger<UploadsManager> logger;

    public UploadsManager(
        IMediaStorageClient mediaStorageClient,
        IEntityStore<MatchUploadEntity> matchUploadEntityStore,
        IEntityStore<ParseMatchTaskEntity> parseMatchTaskEntityStore,
        ILogger<UploadsManager> logger)
    {
        this.mediaStorageClient = mediaStorageClient;
        this.matchUploadEntityStore = matchUploadEntityStore;
        this.parseMatchTaskEntityStore = parseMatchTaskEntityStore;
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

        var id = await this.matchUploadEntityStore.Create(() =>
        {
            MatchUploadEntity newMatchUpload = new MatchUploadEntity();
            newMatchUpload.Status = MatchUploadStatus.Initialized;
            newMatchUpload.DemoMediaStoreUri = uploadInfo.mediaUri.ToString();
            newMatchUpload.CreatedAt = DateTime.Now;
            newMatchUpload.LastUpdatedAt = DateTime.Now;
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

            // Get all currently recorded matchParse tasks for this matchUploadId
            // If any are in a valid state (not failed) then we don't do anything
            var parseMatchTasks = await this.parseMatchTaskEntityStore
                .FindAll((x) => x.MatchUploadEntityId == entities[0].Id && x.Status != ParseMatchTaskStatus.Failed);

            if (parseMatchTasks.Count == 0)
            {
                await this.parseMatchTaskEntityStore.Create(() =>
                {
                    return new ParseMatchTaskEntity
                    {
                        DemoMediaStoreUri = mediaStorageUri,
                        Status = ParseMatchTaskStatus.Initialized,
                        MatchUploadEntityId = entities[0].Id,
                        MatchUploadEntity = entities[0]
                    };
                });
            }
        }
        else
        {
            throw new Exception("Multiple rows found with the same media storage Uri");
        }
    }
}
