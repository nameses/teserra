using MassTransit;
using Tessera.Contracts.Betting;
using Tessera.History.Api.Repositories;

namespace Tessera.History.Api.Consumers;

public class BetFailedConsumer(IBetsRepository repo) : IConsumer<BetFailedEvent>
{
    public async Task Consume(ConsumeContext<BetFailedEvent> ctx)
    {
        var m = ctx.Message;
        await repo.UpsertAsync(
            m.RoundId, 
            m.PlayerId, 
            bet =>
            {
                bet.FailedReason = m.Reason;
                bet.BalanceAfter = m.Balance;
                bet.FailedAt = DateTime.UtcNow;
            },
            ctx.CancellationToken);
    }
}