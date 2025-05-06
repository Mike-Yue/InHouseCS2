using InHouseCS2.Core.EntityStores.Contracts.Models;
using InHouseCS2.Core.Ratings.Contracts;
using Microsoft.Extensions.Logging;
using Moserware.Skills;

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
}
