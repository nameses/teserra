using MassTransit;
using Tessera.Contracts.Betting;
using Tessera.History.Api.Repositories;

namespace Tessera.History.Api.Consumers;

public class BetSettledConsumer(IBetsRepository repo) : IConsumer<BetSettledEvent>
{
    public async Task Consume(ConsumeContext<BetSettledEvent> ctx)
    {
        var m = ctx.Message;
        await repo.UpsertAsync(
            m.RoundId,
            m.PlayerId,
            bet =>
            {
                bet.Payout = m.Payout;
                bet.Outcome = m.Outcome;
                bet.BalanceAfter = m.Balance;
                bet.SettledAt = DateTime.UtcNow;
            },
            ctx.CancellationToken);
    }
}