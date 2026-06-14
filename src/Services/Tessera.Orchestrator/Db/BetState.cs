using MassTransit;

namespace Tessera.Orchestrator.Db;

public class BetState : SagaStateMachineInstance, ISagaVersion
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = null!;
    public int Version { get; set; }

    public Guid PlayerId { get; set; }
    public string GameType { get; set; } = null!;
    public decimal Stake { get; set; }
    public string GameDetails { get; set; } = null!;
    public string Outcome { get; set; } = null!;
    public decimal Payout { get; set; }
    public decimal Balance { get; set; }
    public Guid? TimeoutTokenId { get; set; }
}
