using MassTransit;
using MediatR;
using Tessera.Contracts.Betting;
using Tessera.Wallet.Api.Db;
using Tessera.Wallet.Api.Handlers.Models;

namespace Tessera.Wallet.Api.Consumers;

public class ReserveFundsConsumer(ISender mediator) : IConsumer<ReserveFundsCommand>
{
    public async Task Consume(ConsumeContext<ReserveFundsCommand> ctx)
    {
        var m = ctx.Message;
        var result = await mediator.Send(
            new WalletOperationCommand(m.PlayerId, m.RoundId, m.Amount, OperationType.BetStake));

        FundsReservationEvent evt = result switch
        {
            WalletOperationResponse.Ok r => new FundsReservedEvent(m.RoundId, m.PlayerId, m.Amount, r.Balance),
            WalletOperationResponse.AlreadyApplied r => new FundsReservedEvent(m.RoundId, m.PlayerId, m.Amount, r.Balance),
            WalletOperationResponse.InsufficientFunds r => new FundsReservationRejectedEvent(m.RoundId, m.PlayerId, "insufficient_funds", r.Balance),
            WalletOperationResponse.WalletNotFound => new FundsReservationRejectedEvent(m.RoundId, m.PlayerId, "wallet_not_found", 0m),
            _ => throw new InvalidOperationException("unknown wallet result")
        };

        await ctx.Publish(evt, evt.GetType());
    }
}
