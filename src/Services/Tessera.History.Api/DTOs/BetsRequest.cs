using Tessera.History.Api.DTOs.Common;

namespace Tessera.History.Api.DTOs;

public class BetsRequest
{
    public int Size { get; set; }
    //filters
    public string? GameType { get; set; }
    public decimal? ScoreFrom { get; set; }
    public decimal? ScoreTo { get; set; }
    public decimal? PayoutFrom { get; set; }
    public decimal? PayoutTo { get; set; }
    
    public BetStatus BetStatus { get; set; }
    public SortDirection SortDirection { get; set; }

    /// <summary>
    /// Pagination cursor. If null - first page of pagination
    /// </summary>
    public string? Cursor { get; set; }
}
