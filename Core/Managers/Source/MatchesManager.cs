using InHouseCS2.Core.EntityStores.Contracts;
using InHouseCS2.Core.EntityStores.Contracts.Models;
using InHouseCS2.Core.Managers.Contracts;
using InHouseCS2.Core.Managers.Contracts.Models;
using InHouseCS2.Core.Ratings.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Combinatorics.Collections;

namespace InHouseCS2.Core.Managers;

public class MatchesManager : IMatchesManager
{
    private readonly ITransactionOperation transactionOperation;
    private readonly IRatingCalculator ratingCalculator;
    private readonly ILogger<MatchesManager> logger;

    public MatchesManager(ITransactionOperation transactionOperation, IRatingCalculator ratingCalculator, ILogger<MatchesManager> logger)
    {
        this.transactionOperation = transactionOperation;
        this.ratingCalculator = ratingCalculator;
        this.logger = logger;
    }

    public async Task DeleteMatch(string matchId)
    {
        await this.transactionOperation.ExecuteOperationInTransactionAsync(async (operation) =>
        {
            var matchEntityStore = operation.GetEntityStore<MatchEntity, string>();
            var match = await matchEntityStore.Get(matchId);
            if (match != null)
            {
                await matchEntityStore.Delete(match);
            }

            // Also delete all attempted matchupload entities of this match
            var matchUploadEntityStore = operation.GetEntityStore<MatchUploadEntity, int>();
            var matchUploadEntities = await matchUploadEntityStore.FindAll(m => m.DemoFingerprint == matchId);
            foreach (var matchUpload in matchUploadEntities)
            {
                await matchUploadEntityStore.Delete(matchUpload);
            }
        });
    }

    public async Task<GeneratedMatchTeams?> GenerateMatchTeams(MatchPlayerList matchPlayerList)
    {
        if (matchPlayerList.PlayerList.Count != 10)
        {
            this.logger.LogWarning($"Passed in {matchPlayerList.PlayerList.Count} players. Must only pass in 10 players to form CS2 teams");
            return null;
        }
        var playerEntityStore = this.transactionOperation.GetEntityStore<PlayerEntity, long>();
        var playerEntityList = new List<PlayerEntity>();
        foreach (var playerId in matchPlayerList.PlayerList)
        {
            var playerEntity = await playerEntityStore.Get(playerId);
            if (playerEntity is null)
            {
                throw new Exception($"Player with steam Id {playerId} not found. Aborting Team Generation");
            }
            playerEntityList.Add(playerEntity);
        }
        var combinations = this.GenerateAllTeamCombinations(playerEntityList);

        var priorityQueue = new PriorityQueue<MatchQualityData, double>();
        foreach (var combination in combinations)
        {
            var team1 = combination.team1.ToList();
            var team2 = combination.team2.ToList();
            var matchQualityScore = this.ratingCalculator.CalculateMatchQuality(team1, team2);
            var team1WinPercentage = this.ratingCalculator.CalculateTeam1WinPercentage(team1, team2);
            var matchQualityData = new MatchQualityData
            {
                MatchQualityScore = matchQualityScore,
                Team1WinPercentage = team1WinPercentage,
                Team1 = team1,
                Team2 = team2,
            };
            priorityQueue.Enqueue(matchQualityData, matchQualityScore * -1);
        }

        var bestMatchData = priorityQueue.Peek();

        return new GeneratedMatchTeams
        {
            MatchQuality = bestMatchData.MatchQualityScore,
            Team1WinPercentage = bestMatchData.Team1WinPercentage,
            Team1 = bestMatchData.Team1.Select(
                playerEntity => new PlayerMetadata { SteamId = playerEntity.SteamId, SteamUsername = playerEntity.SteamUsername }).ToList(),
            Team2 = bestMatchData.Team2.Select(
                playerEntity => new PlayerMetadata { SteamId = playerEntity.SteamId, SteamUsername = playerEntity.SteamUsername }).ToList(),
        };
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
                    KD = pms.Deaths == 0 ? pms.Kills : pms.Kills / pms.Deaths,
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

    private List<(HashSet<PlayerEntity> team1, HashSet<PlayerEntity> team2)> GenerateAllTeamCombinations(List<PlayerEntity> players)
    {
        var allPlayersSet = players.ToHashSet();
        var seen = new List<HashSet<PlayerEntity>>();
        var matchups = new List<(HashSet<PlayerEntity>, HashSet<PlayerEntity>)>();

        var combos = new Combinations<PlayerEntity>(players, 5); // Generates all 5-player teams

        foreach (var team1 in combos)
        {
            var team1Set = team1.ToHashSet();
            if (seen.Any(set => set.SetEquals(team1Set)))
            {
                continue;
            }
            else
            {
                var team2Set = allPlayersSet.Except(team1).ToHashSet();
                seen.Add(team1Set);
                seen.Add(team2Set);
                matchups.Add((team1Set, team2Set));
            }
        }
        return matchups;
    }

    private record MatchQualityData
    {
        public required double MatchQualityScore { get; init; }
        public required double Team1WinPercentage { get; init; }
        public required List<PlayerEntity> Team1 { get; init; }
        public required List<PlayerEntity> Team2 { get; init; }
    }
}
