using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Tessera.Orchestrator.Db;

public class OrchestratorDbContext : DbContext
{
    public OrchestratorDbContext(DbContextOptions<OrchestratorDbContext> options) 
        : base(options) 
    { }

    public DbSet<BetState> BetStates { get; set; }

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.AddInboxStateEntity();
        b.AddOutboxMessageEntity();
        b.AddOutboxStateEntity();

        b.Entity<BetState>(e =>
        {
            e.HasKey(w => w.CorrelationId);
            e.Property(w => w.Balance).HasPrecision(19, 4);
            e.Property(w => w.Stake).HasPrecision(19, 4);
            e.Property(w => w.Payout).HasPrecision(19, 4);
        });
    }
}
