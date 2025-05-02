using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InHouseCS2.Core.EntityStores.Contracts.Models;

public class MatchEntity : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required string DemoFileHash { get; set; }
    public required string Map { get; set; }
    public required DateTime DatePlayed { get; set; }

    public int WinScore { get; set; }
    public int LoseScore { get; set; }
    public int MatchUploadEntityId { get; set; }
    public MatchUploadEntity MatchUpload { get; set; } = null!;

    public ICollection<PlayerMatchStatEntity> PlayerMatchStats { get; set; } = new List<PlayerMatchStatEntity>();
    public ICollection<KillEventEntity> KillEvents { get; set; } = new List<KillEventEntity>();

    public int SeasonEntityId { get; set; }
    public SeasonEntity Season { get; set; } = null!;
}
