using MassTransit;
using Tessera.Contracts.Betting;
using Tessera.Orchestrator.Db;

namespace Tessera.Orchestrator.Services;

public class BetStateMachine : MassTransitStateMachine<BetState>
{
    /// <summary>
    /// awaiting funds reserves via wallet (res: reserved, rejected or timeout)
    /// </summary>
    public State AwaitingFunds { get; private set; } = null!;
    /// <summary>
    /// awaiting results of game (won/loss/timeout)
    /// </summary>
    public State AwaitingResults { get; private set; } = null!;
    /// <summary>
    /// when user won game, awaiting services to complete processes (res: PayoutCredit only)
    /// </summary>
    public State Settling { get; private set; } = null!;
    /// <summary>
    /// Refund if algorithm gone wrong (res: refunded only)
    /// </summary>
    public State Refunding { get; private set; } = null!;
   
    //initial event
    public Event<BetPlacedEvent> BetPlaced { get; private set; } = null!;
    //2nd - stake reservation via wallet
    public Event<FundsReservedEvent> FundsReserved { get; private set; } = null!;
    public Event<FundsReservationRejectedEvent> FundsReservationRejected { get; private set; } = null!;
    //3rd - game processing
    public Event<RoundPlayedEvent> RoundPlayed { get; private set; } = null!;
    public Event<PayoutCreditedEvent> PayoutCredited { get; private set; } = null!;
    //negative outcome
    public Event<BetRefundedEvent> BetRefunded { get; private set; } = null!;
    //timeout if microservice not responding
    public Schedule<BetState, RoundTimeout> RoundTimeout { get; private set; } = null!;


    public BetStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => BetPlaced, e => e.CorrelateById(m => m.Message.RoundId));
        Event(() => FundsReserved, e => e.CorrelateById(m => m.Message.RoundId));
        Event(() => FundsReservationRejected, e => e.CorrelateById(m => m.Message.RoundId));
        Event(() => RoundPlayed, e => e.CorrelateById(m => m.Message.RoundId));
        Event(() => PayoutCredited, e => e.CorrelateById(m => m.Message.RoundId));
        Event(() => BetRefunded, e => e.CorrelateById(m => m.Message.RoundId));
        Schedule(() => RoundTimeout, x => x.TimeoutTokenId, s =>
        {
            s.Delay = TimeSpan.FromSeconds(30);
            s.Received = r => r.CorrelateById(m => m.Message.RoundId);
        });

        Initially(
            When(BetPlaced)
                .Then(ctx => 
                { 
                    ctx.Saga.PlayerId = ctx.Message.PlayerId; 
                    ctx.Saga.Stake = ctx.Message.Stake; 
                    ctx.Saga.GameType = ctx.Message.GameType; 
                    ctx.Saga.GameDetails = ctx.Message.GameDetails; 
                })
                .Send(ctx => new ReserveFundsCommand(ctx.Message.RoundId, ctx.Message.PlayerId, ctx.Message.Stake))
                .TransitionTo(AwaitingFunds));

        During(AwaitingFunds,
            When(FundsReserved)
                .Then(ctx => ctx.Saga.Balance = ctx.Message.Balance)
                //send command to separate queue of current game
                // (all games - separate queues)
                .ThenAsync(async ctx =>
                {
                    var ep = await ctx.GetSendEndpoint(new Uri($"queue:game-{ctx.Saga.GameType}"));
                    await ep.Send(new PlayRoundCommand(ctx.Saga.CorrelationId, ctx.Saga.GameType, ctx.Saga.Stake, ctx.Saga.GameDetails));
                })
                .Schedule(RoundTimeout, ctx => ctx.Init<RoundTimeout>(new { RoundId = ctx.Saga.CorrelationId }))
                .TransitionTo(AwaitingResults),
            When(FundsReservationRejected)
                .Publish(ctx => new BetFailedEvent(ctx.Saga.CorrelationId, ctx.Saga.PlayerId, ctx.Message.Reason, ctx.Message.Balance))
                .Finalize());

        During(AwaitingResults,
            When(RoundPlayed)
                .Unschedule(RoundTimeout)
                .Then(ctx => { ctx.Saga.Payout = ctx.Message.Payout; ctx.Saga.Outcome = ctx.Message.Outcome; })
                .IfElse(ctx => ctx.Saga.Payout > 0,
                    win => win
                        //payout must consist of previous bet returned + actual win
                        .Send(c => new SettleBetCommand(c.Saga.CorrelationId, c.Saga.PlayerId, c.Saga.Payout))
                        .TransitionTo(Settling),
                    loss => loss
                        .Publish(c => new BetSettledEvent(c.Saga.CorrelationId, c.Saga.PlayerId, "loss", c.Saga.Outcome, c.Saga.Payout, c.Saga.Balance))
                        .Finalize()),
            When(RoundTimeout.Received)
                .Send(ctx => new RefundBetCommand(ctx.Saga.CorrelationId, ctx.Saga.PlayerId, ctx.Saga.Stake))
                .TransitionTo(Refunding));

        During(Settling,
            When(PayoutCredited)
                .Then(ctx => ctx.Saga.Balance = ctx.Message.Balance)
                .Publish(c => new BetSettledEvent(c.Saga.CorrelationId, c.Saga.PlayerId, "won", c.Saga.Outcome, c.Saga.Payout, c.Saga.Balance))
                .Finalize());

        During(Refunding,
            When(BetRefunded)
                .Publish(c => new BetFailedEvent(c.Saga.CorrelationId, c.Saga.PlayerId, "Game unavailable", c.Message.Balance))
                .Finalize());

        SetCompletedWhenFinalized();
    }
}
