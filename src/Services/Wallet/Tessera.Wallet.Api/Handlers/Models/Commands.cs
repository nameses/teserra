using MediatR;

namespace Tessera.Wallet.Api.Handlers.Models;

public record DebitBalanceCommand(Guid PlayerId, Guid IdempotancyKey, decimal Amount) : ICommand<DebitBalanceResponse>;
public record CreditBalanceCommand(Guid PlayerId, Guid IdempotancyKey, decimal Amount) : ICommand<CreditBalanceResponse>;
