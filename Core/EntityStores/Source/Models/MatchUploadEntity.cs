using Microsoft.EntityFrameworkCore;

namespace InHouseCS2.Core.EntityStores.Models;

[Index(nameof(Status))]
public class MatchUploadEntity : BaseEntity
{
    public MatchUploadStatus Status { get; set; }
    
    public string? DemoMediaStoreUri { get; set; }

    public required string DemoFingerprint { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime LastUpdatedAt { get; set; }
    public MatchEntity? Match { get; set; }
}