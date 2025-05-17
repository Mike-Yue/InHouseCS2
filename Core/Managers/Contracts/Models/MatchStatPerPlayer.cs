namespace InHouseCS2.Core.Managers.Models;

public record MatchStatPerPlayer
{
    public required string SteamId { get; init; }
    public required string SteamUsername { get; init; }

    // Individual skill stats
    public required int Kills { get; init; }
    public required int DamageAssists { get; init; }
    public required int Deaths { get; init; }
    public required int DamageDealt { get; init; }
    public required int Mvps { get; init; }
    public required decimal HeadshotPercentage { get; init; }
    public required decimal HeadshotKillPercentage { get; init; }
    public required decimal KASTRating { get; init; }
    public required decimal HLTVRating { get; init; }

    // Utility stats
    public required int FlashAssists { get; init; }
    public required int EnemiesFlashed { get; init; }
    public required int HighExplosiveGrenadeDamage { get; init; }
    public required int FireGrenadeDamage { get; init; }

    // Misc stats
    public required bool WonMatch { get; init; }
    public required bool TiedMatch { get; set; }
    public required string StartingTeam { get; init; } //T or CT
}
