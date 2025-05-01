using InHouseCS2.Core.Managers.Models;

namespace InHouseCS2.Core.Managers.Contracts.Models;

public class CoreMatchDataWrapperRecord
{
    public required bool FailedToParse { get; init; }
    public CoreMatchDataRecord? MatchDataObject { get; init; }
}
