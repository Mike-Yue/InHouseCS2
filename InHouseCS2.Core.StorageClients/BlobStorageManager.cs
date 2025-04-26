using InHouseCS2.Core.StorageClients.Contracts;

namespace InHouseCS2.Core.StorageClients
{
    public class BlobStorageManager : IBlobStorageManager
    {
        public string GeneratePresignedUrl(string fileName, string fileExtension)
        {
            return "test";
        }
    }
}
