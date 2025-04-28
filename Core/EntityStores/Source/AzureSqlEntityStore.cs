using InHouseCS2.Core.EntityStores.Contracts;
using InHouseCS2.Core.EntityStores.Models;
using Microsoft.EntityFrameworkCore;

namespace InHouseCS2.Core.EntityStores;

public class AzureSqlEntityStore<T> : IEntityStore<T> where T : BaseEntity
{
    // Don't like this, it's tightly coupled. The entityStore layer should be able to save changes regardless of what underlying DB technology is used
    // Maybe add another layer
    private readonly DbContext dbContext;
    private readonly DbSet<T> dbSet;

    public AzureSqlEntityStore(AzureSqlDbContext dbContext) {
        this.dbContext = dbContext;
        this.dbSet = this.dbContext.Set<T>();
    }

    public Task<int> Create(T entity)
    {
        this.dbContext.Add(entity);
        this.dbContext.SaveChanges();
        var id = entity.Id;
        return Task.FromResult(id);
    }

    public Task Delete(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<T> Get(int id)
    {
        return await this.dbSet.FindAsync(id);
    }

    public async Task<int> Update(int id, Action<T> updateFunc)
    {
        var entity = await this.dbSet.FindAsync(id);
        if (entity == null)
        {
            throw new Exception($"Entity with Id {id} not found");
        }

        updateFunc(entity);
        await this.dbContext.SaveChangesAsync();
        return entity.Id;
    }
}
