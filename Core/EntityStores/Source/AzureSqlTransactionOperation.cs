using InHouseCS2.Core.EntityStores.Contracts;
using InHouseCS2.Core.EntityStores.Contracts.Models;

namespace InHouseCS2.Core.EntityStores;

public class AzureSqlTransactionOperation : ITransactionOperation
{
    private readonly AzureSqlDbContext dbContext;

    public AzureSqlTransactionOperation(AzureSqlDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task ExecuteOperationInTransactionAsync(Func<ITransactionOperation, Task> operation)
    {
        using var transaction = await this.dbContext.Database.BeginTransactionAsync();
        try
        {
            await operation(this);
            await this.dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public IEntityStore<T> GetEntityStore<T>() where T : BaseEntity
    {
        return new AzureSqlEntityStore<T>(this.dbContext);
    }
}
