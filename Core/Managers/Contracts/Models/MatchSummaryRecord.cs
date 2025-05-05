namespace InHouseCS2.Core.Managers.Contracts.Models;

public record MatchSummaryRecord
{
    public required DateTime MatchPlayedAt { get; init; }
    public required string MapName { get; init; }

    public required int WinningScore { get; init; }
    public required int LosingScore { get; init; }

    public required List<PlayerMatchSummary> WinningTeamStats { get; init; }
    public required List<PlayerMatchSummary> LosingTeamStats { get; init; }
}
