using InHouseCS2.Core.Managers.Contracts.Models;

namespace InHouseCS2.Core.Managers.Contracts;
public interface IUploadsManager
{
    public Task<UploadMetaData?> GetUploadURL(string fileFingerprint, DateTime matchPlayedAt);

    public Task<string?> GetMatchUploadStatus(int id);

    public Task UpdateMatchStatusAndPersistWork(Uri mediaStorageUri);

    public Task FinalizeMatchUploadEntityAndRecordData(int matchUploadId, MatchDataWrapper coreMatchDataWrapperRecord);
}
