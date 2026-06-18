using System.Reflection.Emit;

using MassTransit;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tessera.History.Api.Db;

public class HistoryApiDbContext : DbContext
{
    public HistoryApiDbContext(DbContextOptions<HistoryApiDbContext> options) : base(options) { }

    public DbSet<BetDetail> BetDetails => Set<BetDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddInboxStateEntity();

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HistoryApiDbContext).Assembly);
    }
}

public sealed class BetDetailConfig : IEntityTypeConfiguration<BetDetail>
{
    public void Configure(EntityTypeBuilder<BetDetail> b)
    {
        b.HasKey(w => w.RoundId);
        b.Property(w => w.BalanceAfter).HasPrecision(19, 4);
        b.Property(w => w.Stake).HasPrecision(19, 4);
        b.Property(w => w.Payout).HasPrecision(19, 4);

        b.ToTable("bet_history");
        b.HasKey(x => x.RoundId);

        b.HasIndex(x => new { x.PlayerId, x.PlacedAt, x.RoundId })
         .HasDatabaseName("ix_bet_history_player_placed")
         .HasFilter("placed_at IS NOT NULL");
    }
}