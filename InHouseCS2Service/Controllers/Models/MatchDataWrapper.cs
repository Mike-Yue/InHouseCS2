namespace InHouseCS2Service.Controllers.Models;

public record MatchDataWrapper
{
    public required bool FailedToParse { get; init; }
    public MatchDataObject? MatchDataObject { get; init; }
}
