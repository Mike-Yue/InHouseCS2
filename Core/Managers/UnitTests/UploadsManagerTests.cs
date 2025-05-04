namespace InHouseCS2.Core.Managers.UnitTests;

using FluentAssertions;
using InHouseCS2.Core.Clients.Contracts;
using InHouseCS2.Core.Common;
using InHouseCS2.Core.EntityStores.Contracts;
using InHouseCS2.Core.EntityStores.Contracts.Models;
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
                Status = MatchUploadStatus.Initialized,
            }
        };

        // Mocks
        uploadsManagerFixture.mockTransactionOperation
            .Setup(s => s.GetEntityStore<MatchUploadEntity, int>())
            .Returns(uploadsManagerFixture.mockMatchUploadEntityStore.Object);

        uploadsManagerFixture.mockMatchUploadEntityStore
            .Setup(s => s.FindAll(It.IsAny<Expression<Func<MatchUploadEntity, bool>>>()))
            .ReturnsAsync(entities);

        // Execute
        await uploadsManagerFixture.TestComponentAndVerifyMocksAsync(async s =>
        {
            var output = (await s.GetUploadURL(fingerprint, DateTime.Now));
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
        var matchUploadEntity = new MatchUploadEntity
        {
            DemoFingerprint = fingerprint,
            Id = 1,
        };

        // Mocks
        uploadsManagerFixture.mockTransactionOperation
            .Setup(s => s.GetEntityStore<MatchUploadEntity, int>())
            .Returns(uploadsManagerFixture.mockMatchUploadEntityStore.Object);
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
            .ReturnsAsync(matchUploadEntity);

        // Execute
        await uploadsManagerFixture.TestComponentAndVerifyMocksAsync(async s =>
        {
            var output = (await s.GetUploadURL(fingerprint, DateTime.Now));
            output.Should().NotBeNull();
            output.uploadUri.Should().BeEquivalentTo(uploadUri);
            output.id.Should().Be(matchUploadEntity.Id);
        });
    }

    [TestMethod]
    public async Task TestUpdateMatchStatusAndPersistWork_ShouldTerminateEarlyIfNoMatchMediaUriMatches()
    {
        var uploadsManagerFixture = new UploadsManagerTestFixture();
        var mediaUri = new Uri("https://mediaUrl.com");

        // Mocks
        uploadsManagerFixture.mockTransactionOperation
            .Setup(s => s.GetEntityStore<MatchUploadEntity, int>())
            .Returns(uploadsManagerFixture.mockMatchUploadEntityStore.Object);
        uploadsManagerFixture.mockMatchUploadEntityStore
            .Setup(s => s.FindAll(It.IsAny<Expression<Func<MatchUploadEntity, bool>>>()))
            .ReturnsAsync(new List<MatchUploadEntity>());

        await uploadsManagerFixture.TestComponentAndVerifyMocksAsync(async s =>
        {
            await s.UpdateMatchStatusAndPersistWork(mediaUri);
        });
    }

    [TestMethod]
    public async Task TestUpdateMatchStatusAndPersistWork_ShouldThrowExceptionIfMoreThanOneMatch()
    {
        var uploadsManagerFixture = new UploadsManagerTestFixture();
        var mediaUri = new Uri("https://mediaUrl.com");

        // Mocks
        uploadsManagerFixture.mockTransactionOperation
            .Setup(s => s.GetEntityStore<MatchUploadEntity, int>())
            .Returns(uploadsManagerFixture.mockMatchUploadEntityStore.Object);
        uploadsManagerFixture.mockMatchUploadEntityStore
            .Setup(s => s.FindAll(It.IsAny<Expression<Func<MatchUploadEntity, bool>>>()))
            .ReturnsAsync(new List<MatchUploadEntity>()
            {
                new MatchUploadEntity
                {
                    Id = 1,
                    DemoFingerprint ="123"
                },
                new MatchUploadEntity
                {
                    Id = 2,
                    DemoFingerprint ="123"
                },
            });

        await uploadsManagerFixture.TestComponentAndVerifyMocksAsync(s =>
        {
            Func<Task> act = async () => await s.UpdateMatchStatusAndPersistWork(mediaUri);
            act.Should().ThrowAsync<Exception>().WithMessage("Multiple rows found with the same media storage Uri");
            return Task.CompletedTask;
        });
    }

    [TestMethod]
    public async Task TestUpdateMatchStatusAndPersistWork_ShouldSucceed()
    {
        var uploadsManagerFixture = new UploadsManagerTestFixture();
        var mediaUri = new Uri("https://mediaUrl.com");
        var matchingMatchUploadEntity = new MatchUploadEntity
        {
            Id = 1,
            DemoFingerprint = "123"
        };

        // Mocks
        uploadsManagerFixture.mockTransactionOperation
            .Setup(s => s.GetEntityStore<MatchUploadEntity, int>())
            .Returns(uploadsManagerFixture.mockMatchUploadEntityStore.Object);
        uploadsManagerFixture.mockMatchUploadEntityStore
            .Setup(s => s.FindAll(It.IsAny<Expression<Func<MatchUploadEntity, bool>>>()))
            .ReturnsAsync(new List<MatchUploadEntity>()
            {
                matchingMatchUploadEntity
            });
        uploadsManagerFixture.mockMatchUploadEntityStore
            .Setup(s => s.Update(matchingMatchUploadEntity.Id, It.IsAny<Action<MatchUploadEntity>>()))
            .Callback<int, Action<MatchUploadEntity>>((id, action) =>
            {
                action(matchingMatchUploadEntity);
                matchingMatchUploadEntity.Status.Should().Be(MatchUploadStatus.Uploaded);
                matchingMatchUploadEntity.LastUpdatedAt.Should().NotBe(null);
            })
            .ReturnsAsync(matchingMatchUploadEntity);

        await uploadsManagerFixture.TestComponentAndVerifyMocksAsync(async s =>
        {
            await s.UpdateMatchStatusAndPersistWork(mediaUri);
        });
    }

    private class UploadsManagerTestFixture : MockFixture<UploadsManager>
    {
        public Mock<IMediaStorageClient> mockMediaStorageClient = new(MockBehavior.Strict);
        public Mock<ITransactionOperation> mockTransactionOperation = new(MockBehavior.Strict);
        public Mock<IMatchParserServiceClient> mockCatchParserServiceClient = new(MockBehavior.Strict);
        public Mock<IEntityStore<MatchUploadEntity, int>> mockMatchUploadEntityStore = new(MockBehavior.Strict);

        public override UploadsManager SetSubject()
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<UploadsManager>();
            return new UploadsManager(
                mediaStorageClient: this.mockMediaStorageClient.Object,
                transacationOperation: this.mockTransactionOperation.Object,
                matchParserServiceClient: this.mockCatchParserServiceClient.Object,
                logger: logger
                );
        }

        public override void VerifyAll()
        {
            this.mockTransactionOperation.VerifyAll();
            this.mockMediaStorageClient.VerifyAll();
            this.mockMatchUploadEntityStore.VerifyAll();
        }
    }
}
