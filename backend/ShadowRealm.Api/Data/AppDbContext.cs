using Microsoft.EntityFrameworkCore;
using ShadowRealm.Api.Models;

namespace ShadowRealm.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<Player> Players { get; set; }
    public DbSet<Quest> Quests { get; set; }
    public DbSet<PlayerQuest> PlayerQuests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PlayerQuest>()
            .HasOne(pq => pq.Player)
            .WithMany(p => p.CompletedQuest)
            .HasForeignKey(pq => pq.PlayerId);
    }
    
}