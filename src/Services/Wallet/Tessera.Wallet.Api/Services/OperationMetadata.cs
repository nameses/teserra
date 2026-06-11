using Tessera.Wallet.Api.Db;

namespace Tessera.Wallet.Api.Services;

public enum Direction { Credit, Debit }
public readonly record struct OpMeta(Direction Direction, string Suffix, bool CreateIfMissing = false);
public static class OperationMetadata
{

    public static OpMeta For(OperationType type)
    {
        return type switch
        {
            OperationType.Deposit => new OpMeta(Direction.Credit, "deposit", CreateIfMissing: true),
            OperationType.Withdrawal => new OpMeta(Direction.Debit, "withdraw"),
            OperationType.BetStake => new OpMeta(Direction.Debit, "stake"),
            OperationType.BetPayout => new OpMeta(Direction.Credit, "payout"),
            OperationType.Refund => new OpMeta(Direction.Credit, "refund"),
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };
    }
}
