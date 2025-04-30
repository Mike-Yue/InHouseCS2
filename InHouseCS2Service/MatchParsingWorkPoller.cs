
using InHouseCS2.Core.EntityStores.Contracts;
using InHouseCS2.Core.EntityStores.Models;

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
                var matchParserEntityStore = scope.ServiceProvider.GetRequiredService<IEntityStore<MatchUploadEntity>>();

                var pendingWork = await matchParserEntityStore.FindAll((x) => x.Status == MatchUploadStatus.Uploaded);

                foreach (var work in pendingWork)
                {
                    try
                    {
                        // Sending the work twice is ok, the webhook Uri MatchParserService updates will be idempotent
                        await this.SendToMatchParserService(work);
                        await matchParserEntityStore.Update(work.Id, (entity) =>
                        {
                            entity.Status = MatchUploadStatus.Processing;
                            entity.LastUpdatedAt = DateTime.UtcNow;
                        });
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

        private async Task SendToMatchParserService(MatchUploadEntity task)
        {
            this.logger.LogInformation($"Executing work on {task.Id}");
            var associatedMatchUploadId = task.Id;
            // Get Presigned URL and associatedMatchUploadId and send to Match service
            await Task.CompletedTask;
        }
    }
}
