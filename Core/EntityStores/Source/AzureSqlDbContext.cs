using InHouseCS2.Core.EntityStores.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text.RegularExpressions;

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

            //var entityTypes = Assembly.GetExecutingAssembly()
            //    .GetTypes()
            //    .Where(t => t.IsClass && !t.IsAbstract && typeof(BaseEntity).IsAssignableFrom(t)).ToList();

            //foreach (var entityType in entityTypes)
            //{
            //    modelBuilder.Entity(entityType);
            //}

            modelBuilder.Entity<MatchEntity>()
                .HasOne(m => m.MatchUpload)
                .WithOne(mu => mu.Match)
                .HasForeignKey<MatchEntity>(m => m.MatchUploadEntityId);

            // Season → Match (1:N)
            modelBuilder.Entity<MatchEntity>()
                .HasOne(m => m.Season)
                .WithMany(s => s.Matches)
                .HasForeignKey(m => m.SeasonEntityId);

            // PlayerMatchStats (many-to-many with payload)
            modelBuilder.Entity<PlayerMatchStatEntity>()
                .HasKey(pms => new { pms.PlayerId, pms.MatchId });

            modelBuilder.Entity<PlayerMatchStatEntity>()
                .HasOne(pms => pms.Player)
                .WithMany(p => p.PlayerMatchStats)
                .HasForeignKey(pms => pms.PlayerId);

            modelBuilder.Entity<PlayerMatchStatEntity>()
                .HasOne(pms => pms.Match)
                .WithMany(m => m.PlayerMatchStats)
                .HasForeignKey(pms => pms.MatchId);

            // KillEvent → Player (Killer)
            modelBuilder.Entity<KillEventEntity>()
                .HasOne(ke => ke.Killer)
                .WithMany(p => p.Kills)
                .HasForeignKey(ke => ke.KillerId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading deletes on self-referencing FKs

            // KillEvent → Player (Victim)
            modelBuilder.Entity<KillEventEntity>()
                .HasOne(ke => ke.Victim)
                .WithMany(p => p.Deaths)
                .HasForeignKey(ke => ke.VictimId)
                .OnDelete(DeleteBehavior.Restrict); // Same reason as above

            // KillEvent → Match
            modelBuilder.Entity<KillEventEntity>()
                .HasOne(ke => ke.Match)
                .WithMany(m => m.KillEvents)
                .HasForeignKey(ke => ke.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PlayerEntity>()
                .HasIndex(pe => pe.SteamId)
                .IsUnique();
        }
    }
}
