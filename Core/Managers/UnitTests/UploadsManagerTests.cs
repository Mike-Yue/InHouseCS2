namespace InHouseCS2.Core.Managers.UnitTests;

using FluentAssertions;
using InHouseCS2.Core.Clients.Contracts;
using InHouseCS2.Core.Common;
using InHouseCS2.Core.EntityStores.Contracts;
using InHouseCS2.Core.EntityStores.Models;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

[TestClass]
public sealed class UploadsManagerTests
{
    [TestMethod]
    public async Task GetUploadURL_ShouldFailIfFileAlreadyExists()
    {
        var uploadsManagerFixture = new UploadsManagerTestFixture();
        var fingerprint = "fingerprint123";
        var entities = new List<MatchUploadEntity>
        {
            new MatchUploadEntity
            {
                Id = 1,
                DemoFingerprint = fingerprint,
            }
        };

        // Mocks
        uploadsManagerFixture.mockMatchUploadEntityStore
            .Setup(s => s.FindAll(It.IsAny<Expression<Func<MatchUploadEntity, bool>>>()))
            .ReturnsAsync(entities);

        // Execute
        await uploadsManagerFixture.TestComponentAndVerifyMocksAsync(async s =>
        {
            var output = (await s.GetUploadURL(fingerprint, "dem"));
            output.Should().BeNull();
        });

    }

    [TestMethod]
    public async Task GetUploadURL_ShouldSucceed()
    {
        var uploadsManagerFixture = new UploadsManagerTestFixture();
        var uploadUri = new Uri("https://uploadUrl.com");
        var mediaUri = new Uri("https://mediaUrl.com");
        var fingerprint = "fingerprint123";

        // Mocks
        uploadsManagerFixture.mockMatchUploadEntityStore
            .Setup(s => s.FindAll(It.IsAny<Expression<Func<MatchUploadEntity, bool>>>()))
            .ReturnsAsync(new List<MatchUploadEntity>());
        uploadsManagerFixture.mockMediaStorageClient
            .Setup(m => m.GetUploadUrl("dem", 1))
            .Returns(new Clients.MediaUploadInfo(uploadUri, mediaUri));
        uploadsManagerFixture.mockMatchUploadEntityStore
            .Setup(m => m.Create(It.IsAny<Func<MatchUploadEntity>>()))
            .Callback<Func<MatchUploadEntity>>(func =>
            {
                var entity = func();
                entity.Should().NotBeNull();
                entity.Status.Should().Be(MatchUploadStatus.Initialized);
                entity.DemoMediaStoreUri.Should().Be(mediaUri.ToString());
                entity.DemoFingerprint.Should().Be(fingerprint);
                entity.CreatedAt.Should().NotBe(null);
                entity.LastUpdatedAt.Should().NotBe(null);
            })
            .ReturnsAsync(1);

        // Execute
        await uploadsManagerFixture.TestComponentAndVerifyMocksAsync(async s =>
        {
            var output = (await s.GetUploadURL(fingerprint, "dem"));
            output.Should().NotBeNull();
            output.uploadUri.Should().BeEquivalentTo(uploadUri);
            output.id.Should().Be(1);
        });
    }

    [TestMethod]
    public async Task TestUpdateMatchStatusAndPersistWork_ShouldTerminateEarlyIfNoMatchMediaUriMatches()
    {
        var uploadsManagerFixture = new UploadsManagerTestFixture();
        var mediaUri = new Uri("https://mediaUrl.com");

        // Mocks
        uploadsManagerFixture.mockMatchUploadEntityStore
            .Setup(s => s.FindAll(It.IsAny<Expression<Func<MatchUploadEntity, bool>>>()))
            .ReturnsAsync(new List<MatchUploadEntity>());

        await uploadsManagerFixture.TestComponentAndVerifyMocksAsync(async s =>
        {
            await s.UpdateMatchStatusAndPersistWork(mediaUri);
        });
    }

    private class UploadsManagerTestFixture : MockFixture<UploadsManager>
    {
        public Mock<IMediaStorageClient> mockMediaStorageClient = new(MockBehavior.Strict);
        public Mock<IEntityStore<MatchUploadEntity>> mockMatchUploadEntityStore = new(MockBehavior.Strict);
        public Mock<IEntityStore<ParseMatchTaskEntity>> mockParseMatchTaskEntityStore = new(MockBehavior.Strict);

        public override UploadsManager SetSubject()
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<UploadsManager>();
            return new UploadsManager(
                mediaStorageClient: this.mockMediaStorageClient.Object,
                matchUploadEntityStore: this.mockMatchUploadEntityStore.Object,
                parseMatchTaskEntityStore: this.mockParseMatchTaskEntityStore.Object,
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
