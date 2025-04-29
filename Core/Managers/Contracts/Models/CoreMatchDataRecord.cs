namespace InHouseCS2.Core.Managers.Models;

public record CoreMatchDataRecord
{
    public required CoreMatchMetadataRecord MatchMetadata { get; init; }   
    public required List<CoreMatchStatPerPlayerRecord> MatchStatPerPlayers { get; init; }
    public required List<CoreMatchKillDataRecord> MatchKills { get; init; }
}
