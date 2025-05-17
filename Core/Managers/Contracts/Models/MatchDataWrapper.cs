using InHouseCS2.Core.Managers.Models;

namespace InHouseCS2.Core.Managers.Contracts.Models;

public class MatchDataWrapper
{
    public required bool FailedToParse { get; init; }
    public MatchData? MatchDataObject { get; init; }
}
