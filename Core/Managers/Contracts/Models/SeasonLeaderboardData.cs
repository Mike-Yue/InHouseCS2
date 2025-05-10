namespace InHouseCS2.Core.Managers.Contracts.Models;

public record SeasonLeaderboardData
{
    public required string Name { get; init; }
    public required DateTime StartDate { get; init; }

    public required DateTime EndDate { get; init; }

    public required List<SeasonLeaderboardPlayerData> playerData { get; init; }
}
