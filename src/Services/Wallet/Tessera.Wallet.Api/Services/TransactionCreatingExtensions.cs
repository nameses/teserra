using Tessera.Wallet.Api.Db;

namespace Tessera.Wallet.Api.Services;


public static class WalletExtensions
{
    extension(Db.Wallet wallet)
    {
        public LedgerEntry? Debit(decimal amount,
        string key,
        Guid idempotencyKey,
        OperationType type)
        {
            var balanceAfter = wallet.Balance - amount;

            if (balanceAfter < 0)
            {
                return null;
            }

            wallet.Balance = balanceAfter;

            return new LedgerEntry()
            {
                WalletId = wallet.Id,
                Type = type,
                ReferenceId = idempotencyKey,
                IdempotencyKey = key,
                BalanceAfter = balanceAfter,
                Amount = amount,
                CreatedAt = DateTime.UtcNow,
            };
        }
        public LedgerEntry Credit(decimal amount,
            string key,
            Guid idempotencyKey,
            OperationType type)
        {
            var balanceAfter = wallet.Balance + amount;

            wallet.Balance = balanceAfter;

            return new LedgerEntry()
            {
                WalletId = wallet.Id,
                Type = type,
                ReferenceId = idempotencyKey,
                IdempotencyKey = key,
                BalanceAfter = balanceAfter,
                Amount = amount,
                CreatedAt = DateTime.UtcNow,
            };
        }
    }
}