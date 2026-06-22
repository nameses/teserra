using System.Security.Claims;
using MediatR;

using Tessera.Wallet.Api.Db;
using Tessera.Wallet.Api.Handlers;
using Tessera.Wallet.Api.Handlers.Models;

namespace Tessera.Wallet.Api;

public static class WalletEndpointsExtension
{
    public static void MapApiEndpoints(this WebApplication app)
    {
        app.MapGet("/wallets/me/balance", async (ClaimsPrincipal user, ISender mediator) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userId, out var playerId))
                return Results.Unauthorized();

            var balance = await mediator.Send(new GetBalanceQuery(playerId));
            return Results.Ok(balance);
        }).RequireAuthorization();



        app.MapGet("/wallets/me/transactions", async (ClaimsPrincipal user, ISender mediator) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userId, out var playerId))
                return Results.Unauthorized();

            var transactions = await mediator.Send(new GetTransactionsQuery(playerId));
            return Results.Ok(transactions);
        }).RequireAuthorization();

        app.MapPost(
            "/wallets/withdraw",
            async (WalletOperationRequest body,
            ClaimsPrincipal user,
            ISender mediator,
            HttpContext http) => await makeWalletOperation(body, user, mediator, http, OperationType.Withdrawal)
        ).RequireAuthorization();

        app.MapPost(
            "/wallets/deposit",
            async (WalletOperationRequest body,
            ClaimsPrincipal user,
            ISender mediator,
            HttpContext http) => await makeWalletOperation(body, user, mediator, http, OperationType.Deposit)
        ).RequireAuthorization();
    }

    private static async Task<IResult> makeWalletOperation(
        WalletOperationRequest body, 
        ClaimsPrincipal user, 
        ISender mediator, 
        HttpContext http,
        OperationType operationType)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userId, out var playerId))
            return Results.Unauthorized();

        if (!http.Request.Headers.TryGetValue("Idempotency-Key", out var hdr)
            || !Guid.TryParse(hdr, out var idempotencyKey))
            return Results.BadRequest(new { error = "missing_idempotency_key" });

        if(body.Amount <= 0)
        {
            return Results.BadRequest(new { error = "negative_amount" });
        }

        var result = await mediator.Send(new WalletOperationCommand(playerId, idempotencyKey, body.Amount, operationType));
        return result switch
        {
            WalletOperationResponse.Ok r => Results.Ok(new { r.Balance }),
            WalletOperationResponse.AlreadyApplied r => Results.Ok(new { r.Balance }),
            WalletOperationResponse.InsufficientFunds r => Results.BadRequest(new { error = "insufficient_funds", balance = r.Balance, need = r.Need }),
            WalletOperationResponse.WalletNotFound => Results.NotFound(),
            _ => Results.Problem()
        };
    }
}
