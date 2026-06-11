using Tessera.Wallet.Api.Db;

namespace Tessera.Wallet.Api.Handlers.Models;

public record GetTransactionsQuery(Guid PlayerId) : IQuery<IEnumerable<LedgerEntry>>;