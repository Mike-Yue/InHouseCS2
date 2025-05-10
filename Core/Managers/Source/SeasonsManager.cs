using InHouseCS2.Core.EntityStores.Contracts;
using InHouseCS2.Core.EntityStores.Contracts.Models;
using InHouseCS2.Core.Managers.Contracts;
using InHouseCS2.Core.Managers.Contracts.Models;
using Microsoft.Extensions.Logging;

namespace InHouseCS2.Core.Managers;

public class SeasonsManager : ISeasonsManager
{
    private readonly ITransactionOperation transactionOperation;
    private readonly ILogger<SeasonsManager> logger;

    public SeasonsManager(ITransactionOperation transactionOperation, ILogger<SeasonsManager> logger)
    {
        this.transactionOperation = transactionOperation;
        this.logger = logger;
    }

    public async Task CreateNextSeason()
    {
        var seasonEntityStore = this.transactionOperation.GetEntityStore<SeasonEntity, int>();
        var mostRecentSeason = (await seasonEntityStore.QueryAsync(seasons => seasons.OrderByDescending(season => season.EndDate))).First();
        var startDate = mostRecentSeason.EndDate.AddDays(1);
        var endDate = startDate.AddMonths(3);
        await seasonEntityStore.Create(() =>
        {
            return new SeasonEntity
            {
                Name = "Worthy",
                StartDate = startDate,
                EndDate = endDate,
            };
        });
    }

    public Task<SeasonLeaderboardData?> GetLeaderboardData(int seasonId)
    {
        throw new NotImplementedException();
    }
}
