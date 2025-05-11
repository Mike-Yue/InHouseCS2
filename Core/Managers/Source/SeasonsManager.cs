using InHouseCS2.Core.EntityStores.Contracts;
using InHouseCS2.Core.EntityStores.Contracts.Models;
using InHouseCS2.Core.Managers.Contracts;
using InHouseCS2.Core.Managers.Contracts.Models;
using Microsoft.EntityFrameworkCore;
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

    public async Task<SeasonLeaderboardData?> GetLeaderboardData(int seasonId)
    {
        var seasonEntityStore = this.transactionOperation.GetEntityStore<SeasonEntity, int>();
        var playerMatchStatEntityStore = this.transactionOperation.GetEntityStore<PlayerMatchStatEntity, int>();
        var seasonEntity = await seasonEntityStore.Get(seasonId);
        if (seasonEntity is null)
        {
            return null;
        }
        var playerData = await playerMatchStatEntityStore.QueryAsync(pmse =>
        {
            return pmse
            .Include(pmse => pmse.Match).Where(pmse => pmse.Match.SeasonEntityId == seasonId)
            .Include(pmse => pmse.Player)
            .GroupBy(pmse => pmse.PlayerId)
            .Select(g => new SeasonLeaderboardPlayerData
            {
                SteamId = g.Key,
                Kills = g.Sum(pmse => pmse.Kills),
                Assists = g.Sum(pmse => pmse.DamageAssists) + g.Sum(pmse => pmse.FlashAssists),
                Deaths = g.Sum(pmse => pmse.Deaths),
                KD = g.Average(pms => pms.Kills) / g.Average(pms => pms.Deaths),
                HSP = g.Average(pms => pms.HeadshotKillPercentage),
                WinPercentage = Convert.ToDouble(g.Count(pms => pms.WonMatch)) * 100 / Convert.ToDouble(g.Count()),
                Rating = g.Select(pmse => pmse.Player.Rating).First(),
            })
            .OrderByDescending(seasonLeaderboardData => seasonLeaderboardData.Rating);
        });
        return new SeasonLeaderboardData
        {
            Name = seasonEntity.Name,
            StartDate = seasonEntity.StartDate,
            EndDate = seasonEntity.EndDate,
            playerData = playerData,
        };
    }
}
