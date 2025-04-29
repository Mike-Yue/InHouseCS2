namespace InHouseCS2.Core.Managers.Contracts;
public interface IUploadsManager
{
    public Task<UploadMetaData?> GetUploadURL(string fileFingerprint, string fileExtension);

    public Task<string> GetMatchUploadStatus(int id);

    public Task UpdateMatchStatusAndPersistWork(Uri mediaStorageUri);
}
