
using InHouseCS2.Core.Clients.Contracts;
using InHouseCS2.Core.EntityStores.Contracts;
using InHouseCS2.Core.EntityStores.Contracts.Models;

namespace InHouseCS2Service
{
    public class MatchParsingWorkPoller : BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<MatchParsingWorkPoller> logger;

        public MatchParsingWorkPoller(IServiceScopeFactory scopeFactory, ILogger<MatchParsingWorkPoller> logger)
        {
            this.scopeFactory = scopeFactory;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogInformation("Polling service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = this.scopeFactory.CreateScope();
                var matchUploadEntityStore = scope.ServiceProvider.GetRequiredService<IEntityStore<MatchUploadEntity, int>>();
                var matchParserServiceClient = scope.ServiceProvider.GetRequiredService<IMatchParserServiceClient>();
                var mediaStorageClient = scope.ServiceProvider.GetRequiredService<IMediaStorageClient>();

                var pendingWork = await matchUploadEntityStore.FindAll((x) => x.Status == MatchUploadStatus.Uploaded);

                foreach (var work in pendingWork)
                {
                    try
                    {
                        // Sending the work twice is ok, the webhook Uri MatchParserService updates will be idempotent
                        await this.SendToMatchParserService(work, matchUploadEntityStore, matchParserServiceClient, mediaStorageClient);
                    }
                    catch (InvalidOperationException ex)
                    {
                        this.logger.LogCritical($"Starting Match Parser work failed with {ex.Message}");
                        continue;
                    }

                }
                await Task.Delay(30000, stoppingToken);
            }
            this.logger.LogInformation("Polling service stopped");
        }

        private async Task SendToMatchParserService(
            MatchUploadEntity task,
            IEntityStore<MatchUploadEntity, int> matchUploadEntityStore,
            IMatchParserServiceClient matchParserServiceClient,
            IMediaStorageClient mediaStorageClient)
        {
            this.logger.LogInformation($"Executing work on {task.Id}");

            var callbackUri = new Uri($"https://inhousecs2.azurewebsites.net/uploads/{task.Id}");

            var downloadUri = mediaStorageClient.GetDownloadUrl(task.DemoMediaStoreUri!, 1);

            var response = await matchParserServiceClient.SendMatchForParsing("/parse", downloadUri, callbackUri);

            if (response.Success)
            {
                await matchUploadEntityStore.Update(task.Id, (entity) =>
                {
                    entity.Status = MatchUploadStatus.Processing;
                    entity.LastUpdatedAt = DateTime.UtcNow;
                });
            }
        }
    }
}
