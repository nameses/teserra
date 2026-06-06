using System.ComponentModel.DataAnnotations;

namespace Tessera.Wallet.Api.Db;

public class Wallet
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public decimal Balance { get; set; }
    [Timestamp]
    public uint Xmin { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
