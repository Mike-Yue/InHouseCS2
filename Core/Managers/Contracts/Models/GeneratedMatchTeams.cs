namespace InHouseCS2.Core.Managers.Contracts.Models;

public record GeneratedMatchTeams
{
    public required double MatchQuality { get; init; }
    public required double Team1WinPercentage { get; init; }
    public required List<PlayerMetadata> Team1 { get; init; }
    public required List<PlayerMetadata> Team2 { get; init; }
}
