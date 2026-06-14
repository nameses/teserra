using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Tessera.Wallet.Api.Db;

public class WalletDbContext : DbContext
{
    public WalletDbContext(DbContextOptions<WalletDbContext> options) : base(options) { }

    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.AddInboxStateEntity();
        b.AddOutboxMessageEntity();
        b.AddOutboxStateEntity();

        b.Entity<Wallet>(e =>
        {
            e.HasKey(w => w.Id);
            e.HasIndex(w => w.PlayerId).IsUnique();
            e.Property(w => w.Balance).HasPrecision(19, 4);
            e.Property(p => p.Xmin).HasColumnName("xmin").HasColumnType("xid").IsRowVersion();

            e.HasMany(x => x.Transactions)
                .WithOne(x => x.Wallet)
                .HasForeignKey(x => x.WalletId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<LedgerEntry>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasPrecision(19, 4);
            e.Property(w => w.BalanceAfter).HasPrecision(19, 4);
            e.HasIndex(x => new { x.WalletId, x.IdempotencyKey }).IsUnique();
            e.HasIndex(x => new { x.WalletId, x.CreatedAt });
        });
    }
}
