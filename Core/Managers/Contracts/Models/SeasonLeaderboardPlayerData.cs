namespace InHouseCS2.Core.Managers.Contracts.Models;

public class SeasonLeaderboardPlayerData
{
    public required long SteamId { get; init; }
    public required string Username { get; init; }
    public required double Kills { get; init; }
    public required double Assists { get; init; }
    public required double Deaths { get; init; }
    public required double KD { get; init; }
    public required decimal HSP { get; init; }

    public required int MatchesPlayed { get; init; }
    public required double WinPercentage { get; init; }
    public required decimal Rating { get; init; }
}
