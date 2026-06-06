using MediatR;
using Microsoft.EntityFrameworkCore;
using Tessera.Wallet.Api.Db;

namespace Tessera.Wallet.Api.Handlers;

public record GetBalanceQuery(Guid PlayerId) : IRequest<decimal>;

public class GetBalanceHandler : IRequestHandler<GetBalanceQuery, decimal>
{
    private WalletDbContext _db { get; set; }

    public GetBalanceHandler(WalletDbContext db) => _db = db; 

    public async Task<decimal> Handle(GetBalanceQuery request, CancellationToken cancellationToken)
    {
        var wallet = await _db.Wallets.Where(w => w.PlayerId == request.PlayerId).FirstOrDefaultAsync(cancellationToken);
        return wallet?.Balance ?? 0m;
    }
}
