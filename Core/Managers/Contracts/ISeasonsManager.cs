using InHouseCS2.Core.Managers.Contracts.Models;

namespace InHouseCS2.Core.Managers.Contracts;

public interface ISeasonsManager
{
    public Task CreateNextSeason();

    public Task<SeasonLeaderboardData?> GetLeaderboardData(int seasonId);
}
