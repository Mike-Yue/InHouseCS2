using InHouseCS2.Core.EntityStores.Contracts.Models;
using Moserware.Skills;

namespace InHouseCS2.Core.Ratings.Contracts;

public interface IRatingCalculator
{
    public Dictionary<long, Rating> RecalculateAllGames(List<PlayerEntity> players, List<MatchEntity> matches);
}
