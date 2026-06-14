using MassTransit;
using MediatR;
using Tessera.Contracts.Betting;
using Tessera.Wallet.Api.Db;
using Tessera.Wallet.Api.Handlers.Models;

namespace Tessera.Wallet.Api.Consumers;

public class SettleBetConsumer(ISender mediator) : IConsumer<SettleBetCommand>
{
    public async Task Consume(ConsumeContext<SettleBetCommand> ctx)
    {
        var m = ctx.Message;
        var result = await mediator.Send(
            new WalletOperationCommand(m.PlayerId, m.RoundId, m.Payout, OperationType.BetPayout));

        var balance = result switch
        {
            WalletOperationResponse.Ok r => r.Balance,
            WalletOperationResponse.AlreadyApplied r => r.Balance,
            _ => 0m
        };

        await ctx.Publish(new PayoutCreditedEvent(m.RoundId, m.PlayerId, m.Payout, balance));
    }
}
