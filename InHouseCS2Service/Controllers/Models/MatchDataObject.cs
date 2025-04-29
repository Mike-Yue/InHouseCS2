namespace InHouseCS2Service.Controllers.Models;

public record MatchDataObject
{
    public required MatchMetadata MatchMetadata { get; init; }
    public required List<MatchStatPerPlayer> MatchStatPerPlayers { get; init; }
    public required List<MatchKillData> MatchKills { get; init; }
}
