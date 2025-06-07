namespace InHouseCS2.Core.Managers.Contracts.Models;

public record PlayerMetadata
{
    public required long SteamId { get; init; }
    public required string SteamUsername { get; init; }

    public required decimal Rating { get; init; }
}
