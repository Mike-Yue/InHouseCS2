namespace InHouseCS2.Core.Managers.Contracts.Models;

public record PlayerMatchSummary
{
    public required int Kills { get; init; }
    public required int Assists { get; init; }
    public required int Deaths { get; init; }

    public required decimal KD { get; init; }
    public required decimal HSP { get; init; }
    public required int MVPs { get; init; }
}
