using InHouseCS2.Core.Clients.Contracts;
using InHouseCS2.Core.EntityStores.Contracts;
using InHouseCS2.Core.EntityStores.Contracts.Models;
using InHouseCS2.Core.Managers.Contracts;
using InHouseCS2.Core.Managers.Contracts.Models;
using Microsoft.Extensions.Logging;

namespace InHouseCS2.Core.Managers;

public class UploadsManager : IUploadsManager
{
    private readonly IMediaStorageClient mediaStorageClient;
    private readonly ITransactionOperation transactionOperation;
    private readonly IMatchParserServiceClient matchParserServiceClient;
    private readonly ILogger<UploadsManager> logger;

    public UploadsManager(
        IMediaStorageClient mediaStorageClient,
        ITransactionOperation transacationOperation,
        IMatchParserServiceClient matchParserServiceClient,
        ILogger<UploadsManager> logger)
    {
        this.mediaStorageClient = mediaStorageClient;
        this.transactionOperation = transacationOperation;
        this.matchParserServiceClient = matchParserServiceClient;
        this.logger = logger;
    }

    public async Task FinalizeMatchUploadEntityAndRecordData(int matchUploadId, CoreMatchDataWrapperRecord coreMatchDataWrapperRecord)
    {
        if (coreMatchDataWrapperRecord.FailedToParse)
        {
            await this.transactionOperation.ExecuteOperationInTransactionAsync(async (operation) =>
            {
                var matchUploadEntityStore = operation.GetEntityStore<MatchUploadEntity, int>();
                var matchUploadEntity = await matchUploadEntityStore.Update(matchUploadId, (entity) =>
                {
                    entity.Status = MatchUploadStatus.FailedToProcess;
                    entity.LastUpdatedAt = DateTime.Now;
                });
            });
            return;
        }

        if (coreMatchDataWrapperRecord.FailedToParse == false && coreMatchDataWrapperRecord.MatchDataObject is null)
        {
            throw new Exception("Cannot have empty MatchDataObject if FailedToParse is false");
        }

        var coreMatchDataRecord = coreMatchDataWrapperRecord.MatchDataObject!;
        await this.transactionOperation.ExecuteOperationInTransactionAsync(async (operation) =>
        {
            var matchUploadEntityStore = operation.GetEntityStore<MatchUploadEntity, int>();
            var matchEntityStore = operation.GetEntityStore<MatchEntity, string>();
            var playerEntityStore = operation.GetEntityStore<PlayerEntity, long>();
            var playerMatchStatEntityStore = operation.GetEntityStore<PlayerMatchStatEntity, int>();
            var killEventEntityStore = operation.GetEntityStore<KillEventEntity, int>();

            var matchUploadEntity = await matchUploadEntityStore.Update(matchUploadId, (entity) =>
            {
                entity.Status = MatchUploadStatus.Processed;
                entity.LastUpdatedAt = DateTime.Now;
            });

            var match = await matchEntityStore.Create(() =>
            {
                return new MatchEntity
                {
                    DemoFileHash = matchUploadEntity.DemoFingerprint,
                    Map = coreMatchDataRecord.MatchMetadata.Map,
                    DatePlayed = matchUploadEntity.MatchPlayedAt,
                    WinScore = coreMatchDataRecord.MatchMetadata.WinningScore,
                    LoseScore = coreMatchDataRecord.MatchMetadata.LosingScore,
                    MatchUploadEntityId = matchUploadId,
                    SeasonEntityId = 1
                };
            });

            foreach (var entry in coreMatchDataRecord.MatchStatPerPlayers)
            {
                var playerEntity = await playerEntityStore.FindOnly((x) => x.SteamId == Convert.ToInt64(entry.SteamId));
                if (playerEntity is null)
                {
                    playerEntity = await playerEntityStore.Create(() =>
                    {
                        return new PlayerEntity
                        {
                            SteamId = Convert.ToInt64(entry.SteamId),
                            Elo = 1000
                        };
                    });
                }
                await playerMatchStatEntityStore.Create(() =>
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
                        KASTRating = entry.KASTRating,
                        HLTVRating = entry.HLTVRating,
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
                var killer = await playerEntityStore.FindOnly((x) => x.SteamId == Convert.ToInt64(entry.KillerSteamId));
                var victim = await playerEntityStore.FindOnly((x) => x.SteamId == Convert.ToInt64(entry.VictimSteamId));
                await killEventEntityStore.Create(() =>
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

            await matchUploadEntityStore.Update(matchUploadId, (entity) =>
            {
                entity.Status = MatchUploadStatus.Completed;
                entity.LastUpdatedAt = DateTime.Now;
            });
        });
    }

    public async Task<string?> GetMatchUploadStatus(int id)
    {
        this.logger.LogInformation("Got here info");
        this.logger.LogWarning("Got here warning");
        var matchUploadEntityStore = this.transactionOperation.GetEntityStore<MatchUploadEntity, int>();
        var matchUploadEntity = await matchUploadEntityStore.Get(id);
        return matchUploadEntity is null ? null : matchUploadEntity.Status.ToString();
    }

    public async Task<UploadMetaData?> GetUploadURL(string fileFingerprint, DateTime matchPlayedAt)
    {
        var matchUploadEntityStore = this.transactionOperation.GetEntityStore<MatchUploadEntity, int>();

        // Only allowed to try and upload demo if 
        // 1: No MatchUploadEntities with the same fingerprint exist
        // 2: If the same fingerprint exists in other rows, make sure they're all in terminal failed state, and that no Match Entity already exists for any of them
        var filesWithSameFingerPrint = await matchUploadEntityStore.FindAll(
            (x) => x.DemoFingerprint == fileFingerprint && x.Status != MatchUploadStatus.FailedToUpload && x.Status != MatchUploadStatus.FailedToProcess);
        if (filesWithSameFingerPrint.Count > 0)
        {
            return null;
        }

        var uploadInfo = this.mediaStorageClient.GetUploadUrl("dem", 0.5);

        var matchUploadEntity = await matchUploadEntityStore.Create(() =>
        {
            var newMatchUpload = new MatchUploadEntity
            {
                DemoFingerprint = fileFingerprint,
                Status = MatchUploadStatus.Initialized,
                MatchPlayedAt = matchPlayedAt,
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
        var matchUploadEntityStore = this.transactionOperation.GetEntityStore<MatchUploadEntity, int>();
        var entities = await matchUploadEntityStore.FindAll((x) => x.DemoMediaStoreUri == mediaStorageUri.ToString());
        if (entities.Count == 0)
        {
            this.logger.LogInformation("No entities found. Returning");
            return;
        }

        if (entities.Count == 1)
        {
            this.logger.LogInformation("1 entity found. Working");

            await this.transactionOperation.ExecuteOperationInTransactionAsync(async (operation) =>
            {
                var matchUploadEntity = await matchUploadEntityStore.Update(entities[0].Id, (matchUploadEntity) =>
                {
                    matchUploadEntity.Status = MatchUploadStatus.Uploaded;
                    matchUploadEntity.LastUpdatedAt = DateTime.Now;
                });
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await this.SendToMatchParserService(matchUploadEntity);
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError($"Error sending Match demo to MatchParserService: {ex.Message}");
                    }
                });
            });
        }
        else
        {
            throw new Exception("Multiple rows found with the same media storage Uri");
        }
    }

    private async Task SendToMatchParserService(MatchUploadEntity task)
    {
        this.logger.LogInformation($"Executing work on {task.Id}");

        var callbackUri = new Uri($"https://inhousecs2.azurewebsites.net/uploads/{task.Id}");

        var downloadUri = this.mediaStorageClient.GetDownloadUrl(task.DemoMediaStoreUri!, 1);

        // var response = await this.matchParserServiceClient.SendMatchForParsing("/parse", downloadUri, callbackUri);
        // Keep this here until Andrew sets up match parser service
        var response = new MatchParserServiceResponse { Success = true };

        if (response.Success)
        {
            var matchUploadEntityStore = this.transactionOperation.GetEntityStore<MatchUploadEntity, int>();
            await matchUploadEntityStore.Update(task.Id, (entity) =>
            {
                entity.Status = MatchUploadStatus.Processing;
                entity.LastUpdatedAt = DateTime.UtcNow;
            });
        }
    }
}
