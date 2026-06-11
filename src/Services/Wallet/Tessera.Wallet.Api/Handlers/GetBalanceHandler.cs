using MediatR;
using Tessera.Wallet.Api.Handlers.Models;
using Tessera.Wallet.Api.Repos;

namespace Tessera.Wallet.Api.Handlers;

public class GetBalanceHandler : IRequestHandler<GetBalanceQuery, decimal>
{
    private readonly IWalletRepository _repo;

    public GetBalanceHandler(IWalletRepository repo) => _repo = repo; 

    public async Task<decimal> Handle(GetBalanceQuery request, CancellationToken cancellationToken)
    {
        var wallet = await _repo.GetAsync(request.PlayerId, cancellationToken);
        return wallet?.Balance ?? 0m;
    }
}
