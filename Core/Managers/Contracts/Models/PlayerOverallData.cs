namespace InHouseCS2.Core.Managers.Contracts.Models;

public record PlayerOverallData
{
    public required List<PlayerMapAggregatedData> MapAggregatedDataList { get; init; }
    // Add room for funny stats like wallbangs or smokes or whatever
}
