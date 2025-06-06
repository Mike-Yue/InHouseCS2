﻿using InHouseCS2.Core.Managers.Contracts.Models;

namespace InHouseCS2.Core.Managers.Contracts;

public interface IMatchesManager
{
    public Task<MatchSummaryRecord?> GetMatchData(string matchId);

    public Task<GeneratedMatchTeams?> GenerateMatchTeams(MatchPlayerList matchPlayerList);

    public Task DeleteMatch(string matchId);
}
