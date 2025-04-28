using System.Linq.Expressions;

namespace InHouseCS2.Core.EntityStores.Contracts;

public interface IEntityStore<T> where T: class
{
    public Task<T> Get(int id);

    // This probably works better as an enumerator instead of a list
    public Task<List<T>> FindAll(Expression<Func<T, bool>> filterFunc);

    public Task<int> Create(T entity);

    public Task<int> Update(int id, Action<T> updateFunc);

    public Task Delete(int id);
}
