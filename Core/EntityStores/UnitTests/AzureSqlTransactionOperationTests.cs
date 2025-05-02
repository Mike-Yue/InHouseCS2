using InHouseCS2.Core.EntityStores.Contracts.Models;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace InHouseCS2.Core.EntityStores.UnitTests;

[TestClass]
public class AzureSqlTransactionOperationTests
{
    private static DbContextOptions<AzureSqlDbContext> dbContext;

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("testsettings.json")
            .Build();
        dbContext = new DbContextOptionsBuilder<AzureSqlDbContext>()
            .UseSqlServer(configuration["ConnectionStrings:TestDb"])
            .Options;
        using var db = new AzureSqlDbContext(dbContext);
        db.Database.EnsureCreated();

        // Thank you chatgpt
        db.Database.ExecuteSqlRaw(@"
            DECLARE @sql NVARCHAR(MAX) = N'';
            SELECT @sql += 'ALTER TABLE [' + SCHEMA_NAME(schema_id) + '].[' + OBJECT_NAME(parent_object_id) + '] DROP CONSTRAINT [' + name + '];'
            FROM sys.foreign_keys;
            EXEC sp_executesql @sql;
        ");
        db.Database.ExecuteSqlRaw(@"
            DECLARE @sql NVARCHAR(MAX) = N'';
            SELECT @sql += 'DROP TABLE [' + SCHEMA_NAME(schema_id) + '].[' + name + '];'
            FROM sys.tables;
            EXEC sp_executesql @sql;
        ");
        db.Database.EnsureCreated();
    }

    [TestMethod]
    public async Task SeasonEntityStoreTest()
    {
        AzureSqlTransactionOperation transactionOperation = new(new AzureSqlDbContext(dbContext));
        var seasonEntityStore = transactionOperation.GetEntityStore<SeasonEntity, int>();

        var season = await seasonEntityStore.Create(() =>
        {
            return new SeasonEntity
            {
                Name = "TestSeason",
            };
        });

        int id = season.Id;

        var readSeason = await seasonEntityStore.Get(season.Id);
        readSeason.Should().NotBeNull();
        readSeason.Name.Should().BeEquivalentTo(season.Name);

        var updateSeason = await seasonEntityStore.Update(season.Id, (season) =>
        {
            season.Name = "ChangedSeason";
        });

        updateSeason.Name.Should().Be("ChangedSeason");

        await seasonEntityStore.Delete(updateSeason);

        (await seasonEntityStore.Get(id)).Should().BeNull();
    }
}
