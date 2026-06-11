using Grpc.Core;

using MediatR;

using Microsoft.AspNetCore.Components.Forms;

using Tessera.Contracts.Grpc;
using Tessera.Wallet.Api.Db;
using Tessera.Wallet.Api.Handlers.Models;

namespace Tessera.Wallet.Api.Services;

public class WalletGrpcService : WalletService.WalletServiceBase
{
    private readonly ISender _mediator;
    public WalletGrpcService(ISender mediator) => _mediator = mediator;

    public override async Task<WalletReply> DebitForBet(DebitForBetRequest r, ServerCallContext ctx)
    {
        return await makeWalletOperation(
            Guid.Parse(r.PlayerId), 
            Guid.Parse(r.RoundId), 
            decimal.Parse(r.Amount), 
            OperationType.BetStake);
    }

    public override async Task<WalletReply> CreditPayout(CreditPayoutRequest r, ServerCallContext ctx)
    {
        return await makeWalletOperation(
            Guid.Parse(r.PlayerId), 
            Guid.Parse(r.RoundId), 
            decimal.Parse(r.Amount), 
            OperationType.BetPayout);
    }

    public override async Task<WalletReply> Refund(RefundRequest r, ServerCallContext ctx)
    {
        return await makeWalletOperation(
            Guid.Parse(r.PlayerId), 
            Guid.Parse(r.RoundId), 
            decimal.Parse(r.Amount), 
            OperationType.Refund);
    }
    private async Task<WalletReply> makeWalletOperation(
        Guid playerId, 
        Guid roundId, 
        decimal amount, 
        OperationType type)
    {
        var result = await _mediator.Send(new WalletOperationCommand(playerId, roundId, amount, type));

        return result switch
        {
            WalletOperationResponse.Ok x => Reply(Outcome.Ok, x.Balance),
            WalletOperationResponse.AlreadyApplied x => Reply(Outcome.AlreadyApplied, x.Balance),
            WalletOperationResponse.InsufficientFunds x => Reply(Outcome.InsufficientFunds, x.Balance),
            WalletOperationResponse.WalletNotFound => new WalletReply { Outcome = Outcome.WalletNotFound },
            _ => throw new RpcException(new Status(StatusCode.Internal, "command failed"))
        };
    }
    static WalletReply Reply(Outcome o, decimal b) => new() { Outcome = o, Balance = b.ToString() };
}
