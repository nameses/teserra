namespace Tessera.Wallet.Api.Handlers.Models;

public abstract record WalletOperationResponse
{
    public sealed record Ok(decimal Balance) : WalletOperationResponse;
    public sealed record AlreadyApplied(decimal Balance) : WalletOperationResponse;
    public sealed record InsufficientFunds(decimal Balance, decimal Need) : WalletOperationResponse;
    public sealed record WalletNotFound() : WalletOperationResponse;
}