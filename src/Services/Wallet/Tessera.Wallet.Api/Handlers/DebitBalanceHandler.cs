using MediatR;

using Tessera.Wallet.Api.Handlers.Models;
using Tessera.Wallet.Api.Repos;

namespace Tessera.Wallet.Api.Handlers;

public class DebitBalanceHandler : IRequestHandler<DebitBalanceCommand, DebitBalanceResponse>
{
    private readonly IWalletRepository _repo;

    public DebitBalanceHandler(IWalletRepository repo) => _repo = repo;

    public async Task<DebitBalanceResponse> Handle(DebitBalanceCommand request, CancellationToken cancellationToken)
    {
        var key = $"{request.IdempotancyKey}:withdraw";

        var wallet = await _repo.GetAsync(request.PlayerId, cancellationToken);
        if (wallet == null) return new DebitBalanceResponse.WalletNotFound();

        var existingTransaction = await _repo.TransactionExistsAsync(request.PlayerId, key, cancellationToken);
        if(existingTransaction) return new DebitBalanceResponse.AlreadyApplied(wallet.Balance);

        var transaction = wallet.Debit(request.Amount, key, request.IdempotancyKey);
        
        return transaction == null 
            ? new DebitBalanceResponse.InsufficientFunds(wallet.Balance, request.Amount) 
            : new DebitBalanceResponse.Ok(transaction.BalanceAfter);
    }
}

