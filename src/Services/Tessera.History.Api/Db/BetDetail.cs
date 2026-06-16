namespace Tessera.History.Api.Db;

public class BetDetail
{
    public Guid RoundId { get; set; }
    public Guid PlayerId { get; set; }
    
    public decimal Stake { get; set; }
    public decimal Payout { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? GameType { get; set; }

    public DateTime? PlacedAt { get; set; }
    public DateTime? SettledAt { get; set; }
    
    public string? FailedReason { get; set; }
    public DateTime? FailedAt { get; set; }
}
