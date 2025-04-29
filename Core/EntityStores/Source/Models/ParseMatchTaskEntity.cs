
namespace InHouseCS2.Core.EntityStores.Models;

public class ParseMatchTaskEntity : BaseEntity
{
    public required Uri DemoMediaStoreUri { get; set; }

    public required ParseMatchTaskStatus Status { get; set; }

    // Foreign Key reference to MatchUploadEntity. Every job must be correlated with a MatchUploadEntity
    public int MatchUploadEntityId { get; set; }
    public required MatchUploadEntity MatchUploadEntity { get; set; }
}
