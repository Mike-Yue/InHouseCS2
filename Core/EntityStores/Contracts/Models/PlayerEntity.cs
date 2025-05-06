using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace InHouseCS2.Core.EntityStores.Contracts.Models;
public class PlayerEntity : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required long SteamId { get; set; }
    public double Rating { get; set; } = 1000;
    public double Deviation { get; set; } = 1000 / 3;
    public ICollection<KillEventEntity> Kills { get; set; } = new List<KillEventEntity>();
    public ICollection<KillEventEntity> Deaths { get; set; } = new List<KillEventEntity>();
    public ICollection<PlayerMatchStatEntity> PlayerMatchStats { get; set; } = new List<PlayerMatchStatEntity>();
}
