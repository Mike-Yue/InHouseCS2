using InHouseCS2.Core.EntityStores.Contracts.Models;
using InHouseCS2.Core.Managers.Contracts.Models;
using Moserware.Skills;

namespace InHouseCS2.Core.Ratings.Contracts;

public interface IRatingCalculator
{
    public Dictionary<long, Rating> RecalculateAllGames(List<PlayerEntity> players, List<MatchEntity> matches);

    public Dictionary<long, (double rating, double deviation)> CalculateMatchRatingChange(List<PlayerEntity> team1, List<PlayerEntity> team2, int[] teamRanks);

    public double CalculateMatchQuality(List<PlayerEntity> team1, List<PlayerEntity> team2);

    public double CalculateTeam1WinPercentage(List<PlayerEntity> team1, List<PlayerEntity> team2);
}
