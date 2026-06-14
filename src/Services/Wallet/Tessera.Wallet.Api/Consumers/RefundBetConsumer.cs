using MassTransit;
using MediatR;
using Tessera.Contracts.Betting;
using Tessera.Wallet.Api.Db;
using Tessera.Wallet.Api.Handlers.Models;

namespace Tessera.Wallet.Api.Consumers;

public class RefundBetConsumer(ISender mediator) : IConsumer<RefundBetCommand>
{
    public async Task Consume(ConsumeContext<RefundBetCommand> ctx)
    {
        var m = ctx.Message;
        var result = await mediator.Send(
            new WalletOperationCommand(m.PlayerId, m.RoundId, m.Amount, OperationType.Refund));

        var balance = result switch
        {
            WalletOperationResponse.Ok r => r.Balance,
            WalletOperationResponse.AlreadyApplied r => r.Balance,
            _ => 0m
        };

        await ctx.Publish(new BetRefundedEvent(m.RoundId, m.PlayerId, m.Amount, balance));
    }
}
