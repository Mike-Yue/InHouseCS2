
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
    }
}
