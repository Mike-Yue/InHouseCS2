namespace InHouseCS2.Core.Managers.Contracts.Models;

public record MatchPlayerList
{
    public required List<long> PlayerList { get; init; }
}