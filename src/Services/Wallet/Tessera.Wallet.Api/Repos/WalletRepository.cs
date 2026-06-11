using MediatR;
using Microsoft.EntityFrameworkCore;
using Tessera.Wallet.Api.Db;

namespace Tessera.Wallet.Api.Repos;

public interface IWalletRepository
{
    Task CreateTransactionAsync(LedgerEntry entry, CancellationToken cancellationToken);

    Task<bool> TransactionExistsAsync(Guid playerId, string key, CancellationToken cancellationToken);
    Task<Db.Wallet?> GetAsync(Guid playerId, CancellationToken cancellationToken);
    Task<Db.Wallet?> GetOrCreateAsync(Guid playerId, CancellationToken cancellationToken);
    Task<IEnumerable<LedgerEntry>> GetTransactionsAsync(Guid playerId, CancellationToken cancellationToken);
}

public class WalletRepository : IWalletRepository
{
    private Db.WalletDbContext _db { get; set; }
    public WalletRepository(WalletDbContext db) => _db = db;

    public async Task CreateTransactionAsync(LedgerEntry entry, CancellationToken cancellationToken)
    {
        await _db.LedgerEntries.AddAsync(entry, cancellationToken);
    }

    public async Task<bool> TransactionExistsAsync(Guid playerId, string key, CancellationToken cancellationToken)
    {
        var existingIdempotancyKey = await _db.LedgerEntries.FirstOrDefaultAsync(
            x => x.IdempotencyKey == key && x.Wallet!.PlayerId == playerId, 
            cancellationToken);

        return existingIdempotancyKey != null;
    }

    public async Task<Db.Wallet?> GetAsync(Guid playerId, CancellationToken cancellationToken)
    {
        var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.PlayerId == playerId, cancellationToken);
        return wallet;
    }

    public async Task<Db.Wallet?> GetOrCreateAsync(Guid playerId, CancellationToken cancellationToken)
    {
        var wallet = await GetAsync(playerId, cancellationToken);
        if(wallet == null)
        {
            wallet = await createAsync(playerId, cancellationToken);
        }
        return wallet;
    }

    private async Task<Db.Wallet> createAsync(Guid playerId, CancellationToken cancellationToken)
    {
        var wallet = new Db.Wallet() { Balance = 0m, CreatedAt = DateTime.UtcNow, PlayerId = playerId };

        await _db.Wallets.AddAsync(wallet, cancellationToken);

        return wallet;
    }

    public async Task<IEnumerable<LedgerEntry>> GetTransactionsAsync(Guid playerId, CancellationToken cancellationToken)
    {
        var transactions = await _db.LedgerEntries.Where(le => le.Wallet!.PlayerId == playerId)
            .ToListAsync(cancellationToken);

        return transactions;
    }
}
