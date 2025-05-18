using InHouseCS2.Core.EntityStores.Contracts.Models;
using Microsoft.EntityFrameworkCore;

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

            modelBuilder.Entity<PlayerEntity>()
                .Property(p => p.Rating)
                .HasPrecision(8, 4);

            modelBuilder.Entity<PlayerEntity>()
                .Property(p => p.Deviation)
                .HasPrecision(7, 4);

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

            modelBuilder.Entity<PlayerMatchStatEntity>()
                .HasIndex(k => new { k.PlayerId, k.MatchId })
                .IsUnique();

            modelBuilder.Entity<PlayerMatchStatEntity>()
                .Property(p => p.HeadshotPercentage)
                .HasPrecision(6, 3);

            modelBuilder.Entity<PlayerMatchStatEntity>()
                .Property(p => p.HeadshotKillPercentage)
                .HasPrecision(6, 3);

            modelBuilder.Entity<PlayerMatchStatEntity>()
                .Property(p => p.HLTVRating)
                .HasPrecision(4, 2);

            modelBuilder.Entity<PlayerMatchStatEntity>()
                .Property(p => p.KASTRating)
                .HasPrecision(6, 3);

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

            modelBuilder.Entity<KillEventEntity>()
                .HasIndex(k => new { k.KillerId, k.VictimId, k.MatchId });

            modelBuilder.Entity<PlayerEntity>();
        }
    }
}
