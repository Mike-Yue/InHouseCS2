using InHouseCS2.Core.Managers.Contracts.Models;
using InHouseCS2.Core.Managers.Models;

namespace InHouseCS2.Core.Managers.Contracts;
public interface IUploadsManager
{
    public Task<UploadMetaData?> GetUploadURL(string fileFingerprint, string fileExtension);

    public Task<string> GetMatchUploadStatus(int id);

    public Task UpdateMatchStatusAndPersistWork(Uri mediaStorageUri);

    public Task FinalizeMatchUploadEntityAndRecordData(int matchUploadId, CoreMatchDataRecord coreMatchDataRecord);
}
