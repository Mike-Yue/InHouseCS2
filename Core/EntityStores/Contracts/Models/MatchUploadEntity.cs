using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace InHouseCS2.Core.EntityStores.Contracts.Models;

[Index(nameof(Status))]
public class MatchUploadEntity : BaseEntity
{
    [Key]
    public int Id { get; set; }
    public MatchUploadStatus Status { get; set; }
    
    public string? DemoMediaStoreUri { get; set; }

    public required string DemoFingerprint { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime MatchPlayedAt { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime LastUpdatedAt { get; set; }
    public MatchEntity? Match { get; set; }
}