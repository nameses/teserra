namespace Tessera.Contracts.Wallet;
//todo: it's example
public sealed record FundsReserved(Guid PlayerId, Guid RoundId, decimal Amount, DateTimeOffset OccurredAt);