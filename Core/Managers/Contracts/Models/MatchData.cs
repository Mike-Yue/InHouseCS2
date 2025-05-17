namespace InHouseCS2.Core.Managers.Models;

public record MatchData
{
    public required MatchMetadata MatchMetadata { get; init; }   
    public required List<MatchStatPerPlayer> MatchStatPerPlayers { get; init; }
    public required List<MatchKillData> MatchKills { get; init; }
}
