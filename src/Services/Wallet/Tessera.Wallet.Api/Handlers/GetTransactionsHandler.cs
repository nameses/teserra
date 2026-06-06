using MediatR;
using Microsoft.EntityFrameworkCore;
using Tessera.Wallet.Api.Db;

namespace Tessera.Wallet.Api.Handlers;

public record GetTransactionsQuery(Guid PlayerId) : IRequest<IEnumerable<LedgerEntry>>;

public class GetTransactionsHandler : IRequestHandler<GetTransactionsQuery, IEnumerable<LedgerEntry>>
{
    private WalletDbContext _db { get; set; }

    public GetTransactionsHandler(WalletDbContext db) => _db = db;

    public async Task<IEnumerable<LedgerEntry>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var transactions = await _db.LedgerEntries.Where(le => le.Wallet!.PlayerId == request.PlayerId)
            .ToListAsync(cancellationToken);
        return transactions;
    }
}
