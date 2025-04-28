using InHouseCS2.Core.EntityStores.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace InHouseCS2.Core.EntityStores
{
    public class AzureSqlDbContext : DbContext
    {
        public AzureSqlDbContext(DbContextOptions<AzureSqlDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var entityTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(BaseEntity).IsAssignableFrom(t)).ToList();

            foreach (var entityType in entityTypes)
            {
                modelBuilder.Entity(entityType);
            }
        }
    }
}
