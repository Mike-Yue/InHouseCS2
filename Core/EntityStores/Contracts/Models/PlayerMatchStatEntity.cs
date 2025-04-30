namespace InHouseCS2.Core.EntityStores.Contracts.Models;

public class PlayerMatchStatEntity : BaseEntity
{
    public required int PlayerId { get; set; }
    public PlayerEntity Player { get; set; } = null!;

    public required int MatchId { get; set; }
    public MatchEntity Match { get; set; } = null!;

    // Individual skill stats
    public required int Kills { get; set; }
    public required int DamageAssists { get; set; }
    public required int Deaths { get; set; }
    public required int DamageDealt { get; set; }
    public required int Mvps { get; set; }
    public required decimal HeadshotPercentage { get; set; }
    public required decimal HeadshotKillPercentage { get; set; }

    // Utility stats
    public required int FlashAssists { get; set; }
    public required int EnemiesFlashed { get; set; }
    public required int HighExplosiveGrenadeDamage { get; set; }
    public required int FireGrenadeDamage { get; set; }

    // Misc stats
    public required bool WonMatch { get; set; }
    public required string StartingTeam { get; set; } //T or CT
}
