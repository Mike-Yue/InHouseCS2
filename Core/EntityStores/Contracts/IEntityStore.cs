namespace InHouseCS2.Core.EntityStores.Contracts;

public interface IEntityStore<T> where T: class
{
    public Task<T> Get(int id);

    public Task<int> Create(T entity);

    public Task<int> Update(int id, Action<T> updateFunc);

    public Task Delete(int id);
}
