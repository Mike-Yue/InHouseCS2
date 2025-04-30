using System.ComponentModel.DataAnnotations;

namespace InHouseCS2.Core.EntityStores.Contracts.Models;

public class KillEventEntity : BaseEntity
{
    [Key]
    public int Id { get; set; }
    public required long KillerId { get; set; }
    public PlayerEntity Killer { get; set; } = null!;

    public required long VictimId { get; set; }
    public PlayerEntity Victim { get; set; } = null!;

    public required string MatchId { get; set; }
    public MatchEntity Match { get; set; } = null!;

    public required string Weapon { get; set; }

    public required bool Headshot { get; set; }
    public required bool Wallbang { get; set; }
    public required bool NoScope { get; set; }
    public required bool ThroughSmoke { get; set; }
    public required bool Midair { get; set; }
    public required bool AttackerBlind { get; set; }
    public required bool VictimBlind { get; set; }
}
