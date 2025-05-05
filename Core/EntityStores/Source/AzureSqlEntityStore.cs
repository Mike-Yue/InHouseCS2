using InHouseCS2.Core.EntityStores.Contracts;
using InHouseCS2.Core.EntityStores.Contracts.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace InHouseCS2.Core.EntityStores;

public class AzureSqlEntityStore<T, TId> : IEntityStore<T, TId> where T : BaseEntity
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

    public async Task Delete(T entity)
    {
        this.dbSet.Remove(entity);
        await this.dbContext.SaveChangesAsync();
    }

    public async Task<List<T>> FindAll(Expression<Func<T, bool>> filterFunc)
    {
        return await this.dbSet.Where(filterFunc).ToListAsync();
    }

    public async Task<T?> FindOnly(Expression<Func<T, bool>> filterFunc)
    {
        return await this.dbSet.Where(filterFunc).SingleOrDefaultAsync();
    }
    public Task<List<TResult>> QueryAsync<TResult>(Func<IQueryable<T>, IQueryable<TResult>> query)
    {
        return query(this.dbContext.Set<T>()).ToListAsync();
    }


    public async Task<T?> Get(TId id)
    {
        return await this.dbSet.FindAsync(id);
    }

    public async Task<T> Update(TId id, Action<T> updateFunc)
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
