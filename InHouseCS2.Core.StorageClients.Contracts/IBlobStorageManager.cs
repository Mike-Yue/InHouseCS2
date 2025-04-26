namespace InHouseCS2.Core.StorageClients.Contracts
{
    public interface IBlobStorageManager
    {
        public string GeneratePresignedUrl(string fileName, string fileExtension);
    }
}
