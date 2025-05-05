using InHouseCS2.Core.EntityStores.Contracts;
using InHouseCS2.Core.EntityStores.Contracts.Models;
using InHouseCS2.Core.Managers.Contracts;
using InHouseCS2.Core.Managers.Contracts.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InHouseCS2.Core.Managers;

public class MatchesManager : IMatchesManager
{
    private readonly ITransactionOperation transactionOperation;
    private readonly ILogger<MatchesManager> logger;

    public MatchesManager(ITransactionOperation transactionOperation, ILogger<MatchesManager> logger)
    {
        this.transactionOperation = transactionOperation;
        this.logger = logger;
    }

    public async Task<MatchSummaryRecord?> GetMatchData(string matchId)
    {
        var playerMatchEntityStore = this.transactionOperation.GetEntityStore<PlayerMatchStatEntity, int>();
        var matchEntityStore = this.transactionOperation.GetEntityStore<MatchEntity, string>();
        var matchData = await matchEntityStore.FindOnly((m) => m.DemoFileHash == matchId);

        if (matchData is null)
        {
            return null;
        }
        var winningTeamPlayers = await playerMatchEntityStore.QueryAsync<PlayerMatchSummary>(q =>
        {
            return q.Where(pms => pms.MatchId == matchId)
                .Include(pms => pms.Match).Where(pms => pms.WonMatch)
                .Select(pms => new PlayerMatchSummary
                {
                    Kills = pms.Kills,
                    Deaths = pms.Deaths,
                    Assists = pms.FlashAssists + pms.DamageAssists,
                    KD = pms.Deaths == 0 ? pms.Kills : pms.Kills/pms.Deaths,
                    MVPs = pms.Mvps,
                    HSP = pms.HeadshotPercentage,
                });
        });
        var losingTeamPlayers = await playerMatchEntityStore.QueryAsync<PlayerMatchSummary>(q =>
        {
            return q.Where(pms => pms.MatchId == matchId)
                .Include(pms => pms.Match).Where(pms => !pms.WonMatch)
                .Select(pms => new PlayerMatchSummary
                {
                    Kills = pms.Kills,
                    Deaths = pms.Deaths,
                    Assists = pms.FlashAssists + pms.DamageAssists,
                    KD = pms.Kills / pms.Deaths,
                    MVPs = pms.Mvps,
                    HSP = pms.HeadshotPercentage,
                });
        });

        return new MatchSummaryRecord
        {
            MapName = matchData.Map,
            WinningScore = matchData.WinScore,
            LosingScore = matchData.LoseScore,
            MatchPlayedAt = matchData.DatePlayed,
            WinningTeamStats = winningTeamPlayers,
            LosingTeamStats = losingTeamPlayers,
        };
    }
}
