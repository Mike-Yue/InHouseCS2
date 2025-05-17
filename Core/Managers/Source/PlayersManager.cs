using InHouseCS2.Core.EntityStores.Contracts;
using InHouseCS2.Core.EntityStores.Contracts.Models;
using InHouseCS2.Core.Managers.Contracts;
using InHouseCS2.Core.Managers.Contracts.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InHouseCS2.Core.Managers;

public class PlayersManager : IPlayersManager
{
    private readonly ITransactionOperation transactionOperation;
    private readonly ILogger<PlayersManager> logger;

    public PlayersManager(ITransactionOperation transactionOperation, ILogger<PlayersManager> logger)
    {
        this.transactionOperation = transactionOperation;
        this.logger = logger;
    }

    public async Task<PlayerOverallData?> GetOverallPlayerData(long playerId)
    {
        var playerMatchEntityStore = this.transactionOperation.GetEntityStore<PlayerMatchStatEntity, int>();
        var playerEntityStore = this.transactionOperation.GetEntityStore<PlayerEntity, long>();
        var playerEntity = await playerEntityStore.Get(playerId);
        if (playerEntity is null)
        {
            return null;
        }
        var playerMetadata = new PlayerMetadata
        {
            SteamId = playerId,
            SteamUsername = playerEntity.SteamUsername,
        };

        var playerMapAggregatedData = await playerMatchEntityStore.QueryAsync<PlayerMapAggregatedData>(q =>
        {
            return q.Where(pms => pms.PlayerId == playerId)
                .Include(pms => pms.Match)
                .GroupBy(pms => pms.Match.Map)
                .Select(g => new PlayerMapAggregatedData
                {
                    Map = g.Key,
                    Kills = g.Average(pms => pms.Kills),
                    Assists = g.Average(pms => pms.DamageAssists + pms.FlashAssists),
                    Deaths = g.Average(pms => pms.Deaths),
                    KD = g.Average(pms => pms.Kills) / g.Average(pms => pms.Deaths),
                    KAD = (g.Average(pms => pms.Kills) + g.Average(pms => pms.DamageAssists + pms.FlashAssists)) / g.Average(pms => pms.Deaths),
                    HSP = g.Average(pms => pms.HeadshotKillPercentage),
                    HEDamage = g.Average(pms => pms.HighExplosiveGrenadeDamage),
                    BurnDamage = g.Average(pms => pms.FireGrenadeDamage),
                    EnemiesFlashed = g.Average(pms => pms.EnemiesFlashed),
                    GamesWon = g.Count(pms => pms.WonMatch),
                    GamesLost = g.Count(pms => !pms.WonMatch && !pms.TiedMatch),
                    GamesTied = g.Count(pms => pms.TiedMatch), // need to fix this
                    WinPercentage = Convert.ToDouble(g.Count(pms => pms.WonMatch))*100 / Convert.ToDouble(g.Count())
                });
        });
        return new PlayerOverallData
        {
            PlayerMetadata = playerMetadata,
            MapAggregatedDataList = playerMapAggregatedData
        };
    }
}
