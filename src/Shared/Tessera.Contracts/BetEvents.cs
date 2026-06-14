namespace Tessera.Contracts.Betting;

//commands
public record ReserveFundsCommand(Guid RoundId, Guid PlayerId, decimal Amount);
public record PlayRoundCommand(Guid RoundId, string GameType, decimal Stake, string GameDetails);
public record SettleBetCommand(Guid RoundId, Guid PlayerId, decimal Payout);
public record RefundBetCommand(Guid RoundId, Guid PlayerId, decimal Amount);

//events
public record BetPlacedEvent(Guid RoundId, Guid PlayerId, string GameType, decimal Stake, string GameDetails);
public abstract record FundsReservationEvent;
public record FundsReservedEvent(Guid RoundId, Guid PlayerId, decimal Amount, decimal Balance) : FundsReservationEvent;
public record FundsReservationRejectedEvent(Guid RoundId, Guid PlayerId, string Reason, decimal Balance) : FundsReservationEvent;
public record RoundPlayedEvent(Guid RoundId, string Outcome, decimal Payout);
public record BetFailedEvent(Guid RoundId, Guid PlayerId, string Reason, decimal Balance);
public record BetSettledEvent(Guid RoundId, Guid PlayerId, string Status, string Outcome, decimal Payout, decimal Balance);
public record PayoutCreditedEvent(Guid RoundId, Guid PlayerId, decimal Amount, decimal Balance);
public record BetRefundedEvent(Guid RoundId, Guid PlayerId, decimal Amount, decimal Balance);

//timeout
public record RoundTimeout(Guid RoundId);