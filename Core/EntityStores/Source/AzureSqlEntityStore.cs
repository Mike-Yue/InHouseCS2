using InHouseCS2.Core.EntityStores.Contracts;
using InHouseCS2.Core.EntityStores.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace InHouseCS2.Core.EntityStores;

public class AzureSqlEntityStore<T> : IEntityStore<T> where T : BaseEntity
{
    private readonly DbContext dbContext;
    private readonly DbSet<T> dbSet;

    public AzureSqlEntityStore(AzureSqlDbContext dbContext) {
        this.dbContext = dbContext;
        this.dbSet = this.dbContext.Set<T>();
    }

    public async Task<T> Create(Func<T> createFunc)
    {
        var entity = createFunc();
        this.dbContext.Add(entity);
        await this.dbContext.SaveChangesAsync();
        return entity;
    }

    public Task Delete(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<List<T>> FindAll(Expression<Func<T, bool>> filterFunc)
    {
        return await this.dbSet.Where(filterFunc).ToListAsync();
    }

    public async Task<T?> FindOnly(Expression<Func<T, bool>> filterFunc)
    {
        return await this.dbSet.Where(filterFunc).SingleOrDefaultAsync();
    }

    public async Task<T?> Get(int id)
    {
        return await this.dbSet.FindAsync(id);
    }

    public async Task<T> Update(int id, Action<T> updateFunc)
    {
        var entity = await this.dbSet.FindAsync(id);
        if (entity == null)
        {
            throw new Exception($"Entity with Id {id} not found");
        }

        updateFunc(entity);

        try
        {
            await this.dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException($"Update failed due to a concurrency conflict.", ex);
        }

        return entity;
    }
}
