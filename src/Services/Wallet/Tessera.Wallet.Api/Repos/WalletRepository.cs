using MediatR;
using Microsoft.EntityFrameworkCore;
using Db = Tessera.Wallet.Api.Db;

namespace Tessera.Wallet.Api.Repos;

public interface IWalletRepository
{
    Task<bool> TransactionExistsAsync(Guid playerId, string key, CancellationToken cancellationToken);
    Task<Db.Wallet?> GetAsync(Guid playerId, CancellationToken cancellationToken);
    Task<IEnumerable<Db.LedgerEntry>> GetTransactionsAsync(Guid playerId, CancellationToken cancellationToken);
}

public class WalletRepository : IWalletRepository
{
    private Db.WalletDbContext _db { get; set; }
    public WalletRepository(Db.WalletDbContext db) => _db = db;

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

    public async Task<IEnumerable<Db.LedgerEntry>> GetTransactionsAsync(Guid playerId, CancellationToken cancellationToken)
    {
        var transactions = await _db.LedgerEntries.Where(le => le.Wallet!.PlayerId == playerId)
            .ToListAsync(cancellationToken);

        return transactions;
    }
}

public static class WalletExtensions
{
    public static Db.LedgerEntry? Debit(this Db.Wallet wallet, decimal amount, string key, Guid idempotencyKey)
    {
        var balanceAfter = wallet.Balance - amount;

        if(balanceAfter < 0)
        {
            return null;
        }

        return new Db.LedgerEntry()
        {
            WalletId = wallet.Id,
            Type = Db.OperationType.BetStake,
            ReferenceId = idempotencyKey,
            IdempotencyKey = key,
            BalanceAfter = balanceAfter,
            Amount = amount,
            CreatedAt = DateTime.UtcNow,
        };
    }
}