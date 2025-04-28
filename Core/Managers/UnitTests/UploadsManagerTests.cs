namespace InHouseCS2.Core.Managers.UnitTests;

using FluentAssertions;
using InHouseCS2.Core.Clients.Contracts;
using InHouseCS2.Core.EntityStores.Contracts;
using InHouseCS2.Core.EntityStores.Models;
using Microsoft.Extensions.Logging;
using Moq;

[TestClass]
public sealed class UploadsManagerTests
{
    [TestMethod]
    public async Task TestMethod1()
    {
        var uploadsManagerFixture = new UploadsManagerTestFixture();
        uploadsManagerFixture.mockMediaStorageClient
            .Setup(m => m.GetUploadUrl("file1", "dem", 1))
            .Returns(new Clients.MediaUploadInfo(new Uri("https://uploadUrl.com"), new Uri("https://mediaUrl.com")));
        uploadsManagerFixture.mockMatchUploadEntityStore.Setup(m => m.Create(It.IsAny<MatchUploadEntity>())).ReturnsAsync(1);

        var output = (await uploadsManagerFixture.uploadsManager.GetUploadURL("file1", "dem"));
        output.uploadUri.Should().BeEquivalentTo(new Uri("https://uploadUrl.com"));
        output.id.Should().Be(1);

        uploadsManagerFixture.VerifyAll();
    }

    private class UploadsManagerTestFixture
    {
        public Mock<IMediaStorageClient> mockMediaStorageClient = new(MockBehavior.Strict);
        public Mock<IEntityStore<MatchUploadEntity>> mockMatchUploadEntityStore = new(MockBehavior.Strict);
        public UploadsManager uploadsManager;

        public UploadsManagerTestFixture()
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<UploadsManager>();
            this.uploadsManager = new UploadsManager(
                mediaStorageClient: this.mockMediaStorageClient.Object,
                matchUploadEntityStore: this.mockMatchUploadEntityStore.Object,
                logger: logger
                );
        }

        public void VerifyAll()
        {
            this.mockMatchUploadEntityStore.VerifyAll();
            this.mockMediaStorageClient.VerifyAll();
        }
    }
}
