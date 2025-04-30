namespace InHouseCS2.Core.EntityStores.Contracts.Models;

public enum MatchUploadStatus
{
    Initialized,
    FailedToUpload,
    Uploaded,
    Processing,
    FailedToProcess,
    Processed,
    Completed
}
