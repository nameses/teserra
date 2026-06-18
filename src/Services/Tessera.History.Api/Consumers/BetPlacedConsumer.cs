using MassTransit;
using Tessera.Contracts.Betting;
using Tessera.History.Api.Repositories;

namespace Tessera.History.Api.Consumers;

public class BetPlacedConsumer(IBetsRepository repo) : IConsumer<BetPlacedEvent>
{
    public async Task Consume(ConsumeContext<BetPlacedEvent> ctx)
    {
        var m = ctx.Message;
        await repo.UpsertAsync(
            m.RoundId,
            m.PlayerId,
            bet =>
            {
                bet.GameType = m.GameType;
                bet.Stake = m.Stake;
                bet.PlacedAt = DateTime.UtcNow;
            },
            ctx.CancellationToken);
    }
}