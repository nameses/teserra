using System.Linq.Expressions;

using Tessera.History.Api.Db;
using Tessera.History.Api.DTOs.Common;

namespace Tessera.History.Api.DTOs;

internal static class BetDetailsResponseHelpers
{
    public static readonly Expression<Func<BetDetail, BetDetailsResponse>> ToResponse =
        bet => new BetDetailsResponse
        {
            Payout = bet.Payout,
            BalanceAfter = bet.BalanceAfter,
            FailedReason = bet.FailedReason,
            GameType = bet.GameType,
            Stake = bet.Stake,
            PlacedAt = bet.PlacedAt,
            BetStatus = bet.FailedAt != null
                ? BetStatus.Refunded
                : bet.SettledAt != null ? BetStatus.Settled : BetStatus.Placed
        };
}