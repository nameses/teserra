using MediatR;
using Tessera.Wallet.Api.Db;
using Tessera.Wallet.Api.Handlers.Models;
using Tessera.Wallet.Api.Repos;

namespace Tessera.Wallet.Api.Handlers;

public class GetTransactionsHandler : IRequestHandler<GetTransactionsQuery, IEnumerable<LedgerEntry>>
{
    private readonly IWalletRepository _repo;

    public GetTransactionsHandler(IWalletRepository repo) => _repo = repo;

    public async Task<IEnumerable<LedgerEntry>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var transactions = await _repo.GetTransactionsAsync(request.PlayerId, cancellationToken);
        return transactions;
    }
}
