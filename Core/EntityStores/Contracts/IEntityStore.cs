using InHouseCS2.Core.EntityStores.Contracts.Models;
using System.Linq.Expressions;

namespace InHouseCS2.Core.EntityStores.Contracts;

public interface IEntityStore<T, TId> where T: BaseEntity
{
    public Task<T?> Get(TId id);

    // This probably works better as an enumerator instead of a list
    public Task<List<T>> FindAll(Expression<Func<T, bool>> filterFunc);
    public Task<T?> FindOnly(Expression<Func<T, bool>> filterFunc);

    public Task<T> Create(Func<T> createFunc);

    public Task<T> Update(TId id, Action<T> updateFunc);

    public Task Delete(TId id);
}
