using Tessera.History.Api.Db;
using Tessera.History.Api.DTOs.Common;

namespace Tessera.History.Api.DTOs;

public class BetDetailsResponse
{
    public Guid RoundId { get; set; 
    }
    public decimal Stake { get; set; }
    public decimal Payout { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? GameType { get; set; }
    public DateTime? PlacedAt { get; set; }

    public BetStatus BetStatus { get; set; }
    public string? FailedReason { get; set; }
}
