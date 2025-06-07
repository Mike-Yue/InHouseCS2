using InHouseCS2.Core.Managers.Contracts.Models;

namespace InHouseCS2.Core.Managers.Contracts;

public interface IPlayersManager
{
    public Task<PlayerOverallData?> GetOverallPlayerData(long playerId);

    public Task<List<PlayerMetadata>> GetAllPlayers();
}
