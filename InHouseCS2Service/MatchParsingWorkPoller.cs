
using InHouseCS2.Core.Clients.Contracts;
using InHouseCS2.Core.Common.Contracts;
using InHouseCS2.Core.EntityStores.Contracts;
using InHouseCS2.Core.EntityStores.Contracts.Models;

namespace InHouseCS2Service
{
    public class MatchParsingWorkPoller : BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly IBackgroundTaskQueue taskQueue;
        private readonly ILogger<MatchParsingWorkPoller> logger;

        public MatchParsingWorkPoller(IServiceScopeFactory scopeFactory, IBackgroundTaskQueue taskQueue, ILogger<MatchParsingWorkPoller> logger)
        {
            this.scopeFactory = scopeFactory;
            this.taskQueue = taskQueue;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogInformation("Polling service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await this.taskQueue.DequeueBackgroundTask(stoppingToken);
                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var scope = this.scopeFactory.CreateScope();
                        await workItem(scope.ServiceProvider, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogCritical($"Background task failed with {ex.Message}");
                    }
                }, stoppingToken);
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
