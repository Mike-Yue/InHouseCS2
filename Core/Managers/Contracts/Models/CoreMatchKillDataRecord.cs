namespace InHouseCS2.Core.Managers.Models;

public record CoreMatchKillDataRecord
{
    public required string KillerSteamId { get; init; }
    public required string VictimSteamId { get; init; }
    public required bool Headshot { get; init; }
    public required bool Wallbang { get; init; }
    public required bool NoScope { get; init; }
    public required bool ThroughSmoke { get; init; }
    public required bool Midair { get; init; }
    public required bool KillerFlashed { get; init; }
    public required bool VictimFlashed { get; init; }
    public required string WeaponUsed { get; init; }
}
