namespace Tessera.Wallet.Api.Handlers.Models;

public abstract record DebitBalanceResponse
{
    public sealed record Ok(decimal Balance) : DebitBalanceResponse;
    public sealed record AlreadyApplied(decimal Balance) : DebitBalanceResponse;
    public sealed record InsufficientFunds(decimal Balance, decimal Need) : DebitBalanceResponse;
    public sealed record WalletNotFound() : DebitBalanceResponse;
}

public abstract record CreditBalanceResponse
{
    public sealed record Ok(decimal Balance) : CreditBalanceResponse;
    public sealed record AlreadyApplied(decimal Balance) : CreditBalanceResponse;
    public sealed record WalletNotFound() : CreditBalanceResponse;
}
