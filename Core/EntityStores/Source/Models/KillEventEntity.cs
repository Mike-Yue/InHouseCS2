namespace InHouseCS2.Core.EntityStores.Models;

public class KillEventEntity : BaseEntity
{
    public int KillerId { get; set; }
    public PlayerEntity Killer { get; set; } = null!;

    public int VictimId { get; set; }
    public PlayerEntity Victim { get; set; } = null!;

    public int MatchId { get; set; }
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
