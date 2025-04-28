namespace InHouseCS2.Core.EntityStores.Models;

public enum MatchUploadStatus
{
    Initialized,
    FailedToUpload,
    Uploaded,
    Duplicate,
    Processing,
    FailedToProcess,
    Processed,
    Completed
}
