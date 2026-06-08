using System.ComponentModel.DataAnnotations;

namespace Tessera.Wallet.Api.Db;

public class Wallet
{
    public required Guid Id { get; set; }
    public required Guid PlayerId { get; set; }
    public required decimal Balance { get; set; }
    [Timestamp]
    public uint Xmin { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<LedgerEntry> Transactions { get; set; } = new List<LedgerEntry>();
}
