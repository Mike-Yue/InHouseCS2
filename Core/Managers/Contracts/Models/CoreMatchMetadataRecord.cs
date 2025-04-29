namespace InHouseCS2.Core.Managers.Models;

public record CoreMatchMetadataRecord
{
    public required string Map { get; init; }
    public required int WinningScore { get; init; }
    public required int LosingScore { get; init; }
}
