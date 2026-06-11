using MediatR;

using Tessera.Wallet.Api.Db;

namespace Tessera.Wallet.Api.Handlers.Models;

public record WalletOperationCommand(
    Guid PlayerId, 
    Guid IdempotancyKey, 
    decimal Amount, 
    OperationType Type) : ICommand<WalletOperationResponse>;
