using FluentAssertions;

namespace InHouseCS2.Core.StorageClients.UnitTests
{
    [TestClass]
    public sealed class BlobStorageManagerTests
    {
        [TestMethod]
        public void TestGeneratePresignedUrl()
        {
            var blobStorageManager = new BlobStorageManager();
            var output = blobStorageManager.GeneratePresignedUrl("file", ".dem");
            output.Should().Be("hello");
        }
    }
}
