using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InHouseCS2.Core.EntityStores.Contracts.Models;

public abstract class BaseEntity
{
    public int Id { get; set; }

    [Timestamp]
    [Column(TypeName = "rowversion")]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
