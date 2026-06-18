using Microsoft.EntityFrameworkCore;
using Tessera.History.Api.Db;
using Tessera.History.Api.DTOs;
using Tessera.History.Api.DTOs.Common;
using Tessera.History.Api.Services;

namespace Tessera.History.Api.Repositories;

public interface IBetsRepository
{
    public Task<BetDetailsResponse?> GetAsync(Guid roundId, Guid playerId, CancellationToken cancellationToken);
    public Task<BetsResponse> GetBulkAsync(BetsRequest request, Guid playerId, CancellationToken cancellationToken);
}

public class BetsRepository : IBetsRepository
{
    private readonly HistoryApiDbContext _db;

    public BetsRepository(HistoryApiDbContext db)
    {
        _db = db;
    }
    public async Task<BetDetailsResponse?> GetAsync(Guid roundId, Guid playerId, CancellationToken cancellationToken)
    {
        var bet = await _db.BetDetails
            .Where(b => b.PlayerId == playerId && b.RoundId == roundId)
            .Select(BetDetailsResponseHelpers.ToResponse)
            .FirstOrDefaultAsync(cancellationToken);

        return bet;
    }

    public async Task<BetsResponse> GetBulkAsync(BetsRequest request, Guid playerId, CancellationToken cancellationToken)
    {
        var betsCommand = _db.BetDetails.Where(b => b.PlayerId == playerId && b.PlacedAt != null);
        var cursor = BetCursor.Decode(request.Cursor, request.SortDirection);

        if (request.PayoutFrom != 0)
            betsCommand = betsCommand.Where(b => b.Payout >= request.PayoutFrom);
        if (request.PayoutTo != 0)
            betsCommand = betsCommand.Where(b => b.Payout <= request.PayoutTo);

        if (request.StakeFrom != 0)
            betsCommand = betsCommand.Where(b => b.Stake >= request.StakeFrom);
        if (request.StakeTo != 0)
            betsCommand = betsCommand.Where(b => b.Stake <= request.StakeTo);

        if (!string.IsNullOrEmpty(request.GameType))
            betsCommand = betsCommand.Where(b => b.GameType == request.GameType);

        if (request.BetStatus != null)
        {
            betsCommand = request.BetStatus switch
            {
                BetStatus.Settled => betsCommand.Where(b => b.SettledAt != null),
                BetStatus.Refunded => betsCommand.Where(b => b.FailedAt != null),
                BetStatus.Placed => betsCommand.Where(b => b.SettledAt == null && b.FailedAt == null),
                _ => betsCommand
            };
        }

        switch (request.SortDirection)
        {
            case SortDirection.Descending:
                if (cursor is not null)
                    betsCommand = betsCommand.Where(b => b.PlacedAt < cursor.PlacedAt
                        || (b.PlacedAt == cursor.PlacedAt && b.RoundId.CompareTo(cursor.RoundId) < 0));

                betsCommand = betsCommand.OrderByDescending(b => b.PlacedAt)
                    .ThenByDescending(b => b.RoundId);
                break;

            case SortDirection.Ascending:
                if (cursor is not null)
                    betsCommand = betsCommand.Where(b => b.PlacedAt > cursor.PlacedAt
                        || (b.PlacedAt == cursor.PlacedAt && b.RoundId.CompareTo(cursor.RoundId) > 0));

                betsCommand = betsCommand.OrderBy(b => b.PlacedAt)
                    .ThenBy(b => b.RoundId);
                break;
        }

        var bets = await betsCommand
            .Take(request.Size + 1)
            .Select(BetDetailsResponseHelpers.ToResponse)
            .ToListAsync(cancellationToken);

        var hasMore = bets.Count > request.Size;
        if (hasMore) bets.RemoveAt(bets.Count - 1);

        var nextCursor = hasMore
            ? new BetCursor(bets[^1].PlacedAt!.Value.Ticks, bets[^1].RoundId, request.SortDirection).Encode()
            : null;

        return new BetsResponse(bets, nextCursor);
    }
}
