using MediatR;

using Tessera.Wallet.Api.Db;

namespace Tessera.Wallet.Api.Handlers.Models;

public record GetBalanceQuery(Guid PlayerId) : IQuery<decimal>;

public record GetTransactionsQuery(Guid PlayerId) : IQuery<IEnumerable<LedgerEntry>>;