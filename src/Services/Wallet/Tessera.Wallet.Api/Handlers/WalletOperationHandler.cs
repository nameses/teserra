using MediatR;
using Tessera.Wallet.Api.Handlers.Models;
using Tessera.Wallet.Api.Repos;
using Tessera.Wallet.Api.Services;

namespace Tessera.Wallet.Api.Handlers;

public class WalletOperationHandler : IRequestHandler<WalletOperationCommand, WalletOperationResponse>
{
    private readonly IWalletRepository _repo;

    public WalletOperationHandler(IWalletRepository repo) => _repo = repo;

    public async Task<WalletOperationResponse> Handle(WalletOperationCommand request, CancellationToken cancellationToken)
    {
        var operation = OperationMetadata.For(request.Type);
        var key = $"{request.IdempotancyKey}:{operation.Suffix}";

        var wallet = operation.CreateIfMissing
            ? await _repo.GetOrCreateAsync(request.PlayerId, cancellationToken)
            : await _repo.GetAsync(request.PlayerId, cancellationToken);
        if (wallet == null) 
            return new WalletOperationResponse.WalletNotFound();

        if (await _repo.TransactionExistsAsync(request.PlayerId, key, cancellationToken)) 
            return new WalletOperationResponse.AlreadyApplied(wallet.Balance);

        var transaction = operation.Direction == Direction.Debit 
            ? wallet.Debit(request.Amount, key, request.IdempotancyKey, request.Type)
            : wallet.Credit(request.Amount, key, request.IdempotancyKey, request.Type);

        if (transaction is null) 
            return new WalletOperationResponse.InsufficientFunds(wallet.Balance, request.Amount);

        await _repo.CreateTransactionAsync(transaction, cancellationToken);

        return new WalletOperationResponse.Ok(transaction.BalanceAfter);
    }
}
