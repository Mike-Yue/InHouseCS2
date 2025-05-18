using InHouseCS2.Core.EntityStores.Contracts.Models;
using InHouseCS2.Core.Ratings.Contracts;
using Microsoft.Extensions.Logging;
using Moserware.Skills;
using MathNet.Numerics.Distributions;
using System.Linq;

namespace InHouseCS2.Core.Ratings;

public class RatingCalculator : IRatingCalculator
{
    private readonly ILogger<RatingCalculator> logger;
    private readonly GameInfo gameInfo;

    // TODO Move all gameInfo params into constructor instead of providing them inline
    public RatingCalculator(GameInfo gameInfo, ILogger<RatingCalculator> logger)
    {
        this.gameInfo = gameInfo;
        this.logger = logger;
    }

    public double CalculateMatchQuality(List<PlayerEntity> team1, List<PlayerEntity> team2)
    {
        var trueSkillTeam1 = new Team();
        var trueSkillTeam2 = new Team();
        foreach (var playerEntity in team1)
        {
            trueSkillTeam1.AddPlayer(new Player(playerEntity.SteamId), new Rating((double)playerEntity.Rating, (double)playerEntity.Deviation));
        }
        foreach (var playerEntity in team2)
        {
            trueSkillTeam2.AddPlayer(new Player(playerEntity.SteamId), new Rating((double)playerEntity.Rating, (double)playerEntity.Deviation));
        }
        return TrueSkillCalculator.CalculateMatchQuality(this.gameInfo, Teams.Concat(trueSkillTeam1, trueSkillTeam2));
    }

    public Dictionary<long, (double rating, double deviation)> CalculateMatchRatingChange(
        List<PlayerEntity> team1, 
        List<PlayerEntity> team2,
        int[] teamRanks)
    {
        var output = new Dictionary<long, (double rating, double deviation)>();
        var trueSkillTeam1 = new Team();
        var trueSkillTeam2 = new Team();
        foreach (var playerEntity in team1)
        {
            trueSkillTeam1.AddPlayer(new Player(playerEntity.SteamId), new Rating((double)playerEntity.Rating, (double)playerEntity.Deviation));
        }
        foreach (var playerEntity in team2)
        {
            trueSkillTeam2.AddPlayer(new Player(playerEntity.SteamId), new Rating((double)playerEntity.Rating, (double)playerEntity.Deviation));
        }
        var teams = new List<Team>
        {
            trueSkillTeam1,
            trueSkillTeam2,
        };
        var ratings = TrueSkillCalculator.CalculateNewRatings(this.gameInfo, Teams.Concat(trueSkillTeam1, trueSkillTeam2), teamRanks);
        foreach (var player in ratings.Keys)
        {
            var steamId = Convert.ToInt64(player.Id);
            output.Add(steamId, (ratings[player].Mean, ratings[player].StandardDeviation));
        }
        return output;
    }

    public Dictionary<long, Rating> RecalculateAllGames(List<PlayerEntity> playerEntities, List<MatchEntity> matchEntities)
    {
        var steamIdToPlayerMap = new Dictionary<long, Player>();
        var steamIdToRatingMap = new Dictionary<long, Rating>();
        foreach(var playerEntity in playerEntities)
        {
            var player = new Player(playerEntity.SteamId);
            steamIdToPlayerMap[playerEntity.SteamId] = player;
            steamIdToRatingMap[playerEntity.SteamId] = new Rating(1000, 1000 / 3);
        }
        foreach(var matchEntity in matchEntities)
        {
            if (matchEntity.PlayerMatchStats.Any((pms) => pms.TiedMatch))
            {
                // do tied match stuff
            }
            else
            {
                var winningTeam = new Team();
                var losingTeam = new Team();
                foreach(var winningPlayer in matchEntity.PlayerMatchStats.Where((pms) => pms.WonMatch))
                {
                    winningTeam.AddPlayer(steamIdToPlayerMap[winningPlayer.PlayerId], steamIdToRatingMap[winningPlayer.PlayerId]);
                }
                foreach (var losingPlayer in matchEntity.PlayerMatchStats.Where((pms) => !pms.WonMatch))
                {
                    losingTeam.AddPlayer(steamIdToPlayerMap[losingPlayer.PlayerId], steamIdToRatingMap[losingPlayer.PlayerId]);
                }
                
                var teams = new List<Team>
                {
                    winningTeam,
                    losingTeam,
                };

                var ranks = new int[] { 0, 1 };

                var ratings = TrueSkillCalculator.CalculateNewRatings(this.gameInfo, Teams.Concat(winningTeam, losingTeam), ranks);
                foreach (var player in ratings.Keys)
                {
                    var steamId = Convert.ToInt64(player.Id);
                    this.logger.LogInformation($"Player {steamId}: Rating: {ratings[player].Mean}, Deviation: {ratings[player].StandardDeviation}");
                    steamIdToRatingMap[steamId] = ratings[player];
                }
            }
        }

        return steamIdToRatingMap;
    }

    public double CalculateTeam1WinPercentage(List<PlayerEntity> team1, List<PlayerEntity> team2)//(List<Rating> teamA, List<Rating> teamB)
    {
        var teamA = team1.Select(playerEntity => new Rating((double)playerEntity.Rating, (double)playerEntity.Deviation));
        var teamB = team2.Select(playerEntity => new Rating((double)playerEntity.Rating, (double)playerEntity.Deviation));
        double muA = teamA.Sum(p => p.Mean);
        double sigmaSqA = teamA.Sum(p => p.StandardDeviation * p.StandardDeviation);

        double muB = teamB.Sum(p => p.Mean);
        double sigmaSqB = teamB.Sum(p => p.StandardDeviation * p.StandardDeviation);

        double denom = Math.Sqrt(sigmaSqA + sigmaSqB + 2 * this.gameInfo.Beta * this.gameInfo.Beta);
        double deltaMu = muA - muB;

        return Normal.CDF(0, 1, deltaMu / denom); // P(Team A wins)
    }
}
