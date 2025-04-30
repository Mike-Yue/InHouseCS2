using System.ComponentModel.DataAnnotations;

namespace InHouseCS2.Core.EntityStores.Contracts.Models;

public class SeasonEntity : BaseEntity
{
    public required string Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public ICollection<MatchEntity> Matches { get; set; } = new List<MatchEntity>();
}
