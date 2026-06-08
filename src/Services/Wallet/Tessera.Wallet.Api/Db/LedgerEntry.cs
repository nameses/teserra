namespace Tessera.Wallet.Api.Db;

public class LedgerEntry
{
    public Guid Id { get; set; }
    public required Guid WalletId { get; set; }
    public required decimal Amount { get; set; }
    public required OperationType Type { get; set; }
    public Guid ReferenceId { get; set; }
    public required string IdempotencyKey { get; set; }
    public required decimal BalanceAfter { get; set; }
    public required DateTime CreatedAt { get; set; }

    public Wallet? Wallet { get; set; }
}
