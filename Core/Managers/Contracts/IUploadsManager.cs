namespace InHouseCS2.Core.Managers.Contracts;
public interface IUploadsManager
{
    public string GetUploadURL(string fileName, string fileExtension);
}
