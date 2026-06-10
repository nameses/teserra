using MediatR;
using Tessera.Wallet.Api.Handlers.Models;
using Tessera.Wallet.Api.Repos;

namespace Tessera.Wallet.Api.Handlers;

public class CreditBalanceHandler : IRequestHandler<CreditBalanceCommand, CreditBalanceResponse>
{
    private readonly IWalletRepository _repo;

    public CreditBalanceHandler(IWalletRepository repo) => _repo = repo;

    public async Task<CreditBalanceResponse> Handle(CreditBalanceCommand request, CancellationToken cancellationToken)
    {
        var key = $"{request.IdempotancyKey}:deposit";

        var wallet = await _repo.GetOrCreateAsync(request.PlayerId, cancellationToken);
        if (wallet == null) return new CreditBalanceResponse.WalletNotFound();

        var existingTransaction = await _repo.TransactionExistsAsync(request.PlayerId, key, cancellationToken);
        if (existingTransaction) return new CreditBalanceResponse.AlreadyApplied(wallet.Balance);

        var transaction = wallet.Credit(request.Amount, key, request.IdempotancyKey);
        await _repo.CreateTransactionAsync(transaction, cancellationToken);

        return new CreditBalanceResponse.Ok(transaction.BalanceAfter);
    }
}

