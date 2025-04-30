using InHouseCS2.Core.EntityStores.Contracts.Models;

namespace InHouseCS2.Core.EntityStores.Contracts;

public interface ITransactionOperation
{
    public Task ExecuteOperationInTransactionAsync(
        Func<ITransactionOperation, Task> operation);

    public IEntityStore<T> GetEntityStore<T>() where T : BaseEntity;
}
