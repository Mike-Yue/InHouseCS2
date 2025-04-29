namespace InHouseCS2Service.Controllers.Models;

public record MatchMetadata
{
    public required string Map { get; init; }
    public required int WinningScore { get; init; }
    public required int LosingScore { get; init; }
}
