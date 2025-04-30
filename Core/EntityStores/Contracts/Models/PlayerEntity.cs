using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace InHouseCS2.Core.EntityStores.Contracts.Models;
public class PlayerEntity : BaseEntity
{
    public required long SteamId { get; set; }
    public int Elo { get; set; }

    public ICollection<KillEventEntity> Kills { get; set; } = new List<KillEventEntity>();
    public ICollection<KillEventEntity> Deaths { get; set; } = new List<KillEventEntity>();
    public ICollection<PlayerMatchStatEntity> PlayerMatchStats { get; set; } = new List<PlayerMatchStatEntity>();
}
