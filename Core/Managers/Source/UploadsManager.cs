using InHouseCS2.Core.Clients.Contracts;
using InHouseCS2.Core.EntityStores.Contracts;
using InHouseCS2.Core.EntityStores.Contracts.Models;
using InHouseCS2.Core.Managers.Contracts;
using InHouseCS2.Core.Managers.Contracts.Models;
using InHouseCS2.Core.Managers.Models;
using Microsoft.Extensions.Logging;

namespace InHouseCS2.Core.Managers;

public class UploadsManager : IUploadsManager
{
    private readonly IMediaStorageClient mediaStorageClient;
    private readonly IEntityStore<MatchUploadEntity, int> matchUploadEntityStore;
    private readonly IEntityStore<MatchEntity, string> matchEntityStore;
    private readonly IEntityStore<PlayerEntity, long> playerEntityStore;
    private readonly IEntityStore<PlayerMatchStatEntity, int> playerMatchStatEntityStore;
    private readonly IEntityStore<KillEventEntity, int> killEventEntityStore;
    private readonly ITransactionOperation transactionOperation;
    private readonly ILogger<UploadsManager> logger;

    public UploadsManager(
        IMediaStorageClient mediaStorageClient,
        IEntityStore<MatchUploadEntity, int> matchUploadEntityStore,
        IEntityStore<MatchEntity, string> matchEntityStore,
        IEntityStore<PlayerEntity, long> playerEntityStore,
        IEntityStore<PlayerMatchStatEntity, int> playerMatchStatEntityStore,
        IEntityStore<KillEventEntity, int> killEventEntityStore,
        ITransactionOperation transacationOperation,
        ILogger<UploadsManager> logger)
    {
        this.mediaStorageClient = mediaStorageClient;
        this.matchUploadEntityStore = matchUploadEntityStore;
        this.matchEntityStore = matchEntityStore;
        this.playerEntityStore = playerEntityStore;
        this.playerMatchStatEntityStore = playerMatchStatEntityStore;
        this.killEventEntityStore = killEventEntityStore;
        this.transactionOperation = transacationOperation;
        this.logger = logger;
    }

    public async Task FinalizeMatchUploadEntityAndRecordData(int matchUploadId, CoreMatchDataRecord coreMatchDataRecord)
    {
        var matchUploadEntity = await this.matchUploadEntityStore.Update(matchUploadId, (entity) =>
        {
            entity.Status = MatchUploadStatus.Processed;
            entity.LastUpdatedAt = DateTime.Now;
        });

        var match = await this.matchEntityStore.Create(() =>
        {
            return new MatchEntity
            {
                DemoFileHash = matchUploadEntity.DemoFingerprint,
                Map = coreMatchDataRecord.MatchMetadata.Map,
                WinScore = coreMatchDataRecord.MatchMetadata.WinningScore,
                LoseScore = coreMatchDataRecord.MatchMetadata.LosingScore,
                MatchUploadEntityId = matchUploadId,
                SeasonEntityId = 1
            };
        });

        foreach (var entry in coreMatchDataRecord.MatchStatPerPlayers)
        {
            var playerEntity = await this.playerEntityStore.FindOnly((x) => x.SteamId == Convert.ToInt64(entry.SteamId));
            if (playerEntity is null)
            {
                playerEntity = await this.playerEntityStore.Create(() =>
                {
                    return new PlayerEntity
                    {
                        SteamId = Convert.ToInt64(entry.SteamId),
                        Elo = 1000
                    };
                });
            }
            await this.playerMatchStatEntityStore.Create(() =>
            {
                return new PlayerMatchStatEntity
                {
                    PlayerId = playerEntity.SteamId,
                    MatchId = match.DemoFileHash,
                    Kills = entry.Kills,
                    DamageAssists = entry.DamageAssists,
                    Deaths = entry.Deaths,
                    DamageDealt = entry.DamageDealt,
                    Mvps = entry.Mvps,
                    HeadshotPercentage = entry.HeadshotPercentage,
                    HeadshotKillPercentage = entry.HeadshotKillPercentage,
                    FlashAssists = entry.FlashAssists,
                    EnemiesFlashed = entry.EnemiesFlashed,
                    HighExplosiveGrenadeDamage = entry.HighExplosiveGrenadeDamage,
                    FireGrenadeDamage = entry.FireGrenadeDamage,
                    WonMatch = entry.WonMatch,
                    StartingTeam = entry.StartingTeam
                };
            });
        }

        foreach (var entry in coreMatchDataRecord.MatchKills)
        {
            var killer = await this.playerEntityStore.FindOnly((x) => x.SteamId == Convert.ToInt64(entry.KillerSteamId));
            var victim = await this.playerEntityStore.FindOnly((x) => x.SteamId == Convert.ToInt64(entry.VictimSteamId));
            await this.killEventEntityStore.Create(() =>
            {
                return new KillEventEntity
                {
                    KillerId = killer!.SteamId,
                    VictimId = victim!.SteamId,
                    MatchId = match.DemoFileHash,
                    Weapon = entry.WeaponUsed,
                    Headshot = entry.Headshot,
                    Wallbang = entry.Wallbang,
                    NoScope = entry.NoScope,
                    ThroughSmoke = entry.ThroughSmoke,
                    Midair = entry.Midair,
                    AttackerBlind = entry.KillerFlashed,
                    VictimBlind = entry.VictimFlashed
                };
            });
        }

        await this.matchUploadEntityStore.Update(matchUploadId, (entity) =>
        {
            entity.Status = MatchUploadStatus.Completed;
            entity.LastUpdatedAt = DateTime.Now;
        });
    }

    public async Task<string> GetMatchUploadStatus(int id)
    {
        await this.transactionOperation.ExecuteOperationInTransactionAsync(async (operation) =>
        {
            var playerStore = operation.GetEntityStore<PlayerEntity, long>();
            var seasonStore = operation.GetEntityStore<SeasonEntity, int>();
            await playerStore.Create(() =>
            {
                return new PlayerEntity
                {
                    SteamId = 123456
                };
            });
            throw new Exception("Test exception");
            await seasonStore.Create(() =>
            {
                return new SeasonEntity
                {
                    Name = "Test1"
                };
            });

        });
        return "123";
    }

    public async Task<UploadMetaData?> GetUploadURL(string fileFingerprint, string fileExtension)
    {
        var filesWithSameFingerPrint = await this.matchUploadEntityStore.FindAll((x) => x.DemoFingerprint == fileFingerprint && x.Status != MatchUploadStatus.FailedToUpload);
        if (filesWithSameFingerPrint.Count > 0)
        {
            return null;
        }

        var uploadInfo = this.mediaStorageClient.GetUploadUrl(fileExtension, 1);

        var matchUploadEntity = await this.matchUploadEntityStore.Create(() =>
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
        return new UploadMetaData(uploadInfo.uploadUri, matchUploadEntity.Id);
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
