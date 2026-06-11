namespace Tessera.Wallet.Api.Handlers.Models;

public record GetBalanceQuery(Guid PlayerId) : IQuery<decimal>;