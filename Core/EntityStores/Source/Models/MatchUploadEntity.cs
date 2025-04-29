namespace InHouseCS2.Core.EntityStores.Models;

public class MatchUploadEntity : BaseEntity
{
    public MatchUploadStatus Status { get; set; }
    
    public Uri? DemoMediaStoreUri { get; set; }

    public string? DemoFingerprint { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime LastUpdatedAt { get; set; }

    public ICollection<ParseMatchTaskEntity>? ParseMatchTasks { get; }
}