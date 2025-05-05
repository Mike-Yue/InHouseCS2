using InHouseCS2.Core.Managers.Contracts.Models;

namespace InHouseCS2.Core.Managers.Contracts;

public interface IMatchesManager
{
    public Task<MatchSummaryRecord?> GetMatchData(string matchId);
}
