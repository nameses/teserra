using Microsoft.EntityFrameworkCore;

namespace Tessera.Wallet.Api.Db;

public class WalletDbContext : DbContext
{
    public WalletDbContext(DbContextOptions<WalletDbContext> options) : base(options) { }

    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Wallet>(e =>
        {
            e.HasKey(w => w.Id);
            e.HasIndex(w => w.PlayerId).IsUnique();
            e.Property(w => w.Balance).HasPrecision(19, 4);
            e.Property(p => p.Xmin).HasColumnName("xmin").HasColumnType("xid").IsRowVersion();
        });

        b.Entity<LedgerEntry>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasPrecision(19, 4);
            e.HasIndex(x => new { x.WalletId, x.IdempotencyKey }).IsUnique();
            e.HasIndex(x => new { x.WalletId, x.CreatedAt });
        });
    }
}
