namespace InHouseCS2.Core.Clients.Contracts;

public interface IMediaStorageClient
{
    public Uri GetUploadUrl(string fileName, string fileExtension, double hoursValidFor);
}
