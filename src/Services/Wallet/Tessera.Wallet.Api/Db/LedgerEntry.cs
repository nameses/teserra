namespace Tessera.Wallet.Api.Db;

public class LedgerEntry
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public decimal Amount { get; set; }
    public OperationType Type { get; set; }
    public Guid ReferenceId { get; set; }
    public Guid IdempotencyKey { get; set; }
    public decimal BalanceAfter { get; set; }
    public DateTime CreatedAt { get; set; }

    public Wallet? Wallet { get; set; }
}
