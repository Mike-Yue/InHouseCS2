namespace InHouseCS2.Core.Managers.UnitTests;

using FluentAssertions;
using InHouseCS2.Core.Clients.Contracts;
using InHouseCS2.Core.Common;
using InHouseCS2.Core.EntityStores.Contracts;
using InHouseCS2.Core.EntityStores.Models;
using Microsoft.Extensions.Logging;
using Moq;

[TestClass]
public sealed class UploadsManagerTests
{
    [TestMethod]
    public async Task GetUploadURL_ShouldSucceed()
    {
        var uploadsManagerFixture = new UploadsManagerTestFixture();
        uploadsManagerFixture.mockMediaStorageClient
            .Setup(m => m.GetUploadUrl("file1", "dem", 1))
            .Returns(new Clients.MediaUploadInfo(new Uri("https://uploadUrl.com"), new Uri("https://mediaUrl.com")));
        uploadsManagerFixture.mockMatchUploadEntityStore.Setup(m => m.Create(It.IsAny<MatchUploadEntity>())).ReturnsAsync(1);

        await uploadsManagerFixture.TestComponentAndVerifyMocksAsync(async s =>
        {
            var output = (await s.GetUploadURL("file1", "dem"));
            output.uploadUri.Should().BeEquivalentTo(new Uri("https://uploadUrl.com"));
            output.id.Should().Be(1);
        });
    }

    private class UploadsManagerTestFixture : MockFixture<UploadsManager>
    {
        public Mock<IMediaStorageClient> mockMediaStorageClient = new(MockBehavior.Strict);
        public Mock<IEntityStore<MatchUploadEntity>> mockMatchUploadEntityStore = new(MockBehavior.Strict);

        public override UploadsManager SetSubject()
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<UploadsManager>();
            return new UploadsManager(
                mediaStorageClient: this.mockMediaStorageClient.Object,
                matchUploadEntityStore: this.mockMatchUploadEntityStore.Object,
                logger: logger
                );
        }

        public override void VerifyAll()
        {
            this.mockMatchUploadEntityStore.VerifyAll();
            this.mockMediaStorageClient.VerifyAll();
        }
    }
}
