using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Tessera.History.Api.Db;

public class HistoryApiDbContext : DbContext
{
    public HistoryApiDbContext(DbContextOptions<HistoryApiDbContext> options) : base(options) { }

    public DbSet<BetDetail> BetDetails => Set<BetDetail>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.AddInboxStateEntity();

        b.Entity<BetDetail>(e =>
        {
            e.HasKey(w => w.RoundId);
            e.Property(w => w.BalanceAfter).HasPrecision(19, 4);
            e.Property(w => w.Stake).HasPrecision(19, 4);
            e.Property(w => w.Payout).HasPrecision(19, 4);
        });
    }
}
