using System.Security.Claims;
using MediatR;
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

        app.MapPost("/wallets/withdraw", async (DebitBalanceRequest body, ClaimsPrincipal user, ISender mediator, HttpContext http) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userId, out var playerId))
                return Results.Unauthorized();

            if (!http.Request.Headers.TryGetValue("Idempotency-Key", out var hdr)
                || !Guid.TryParse(hdr, out var idempotencyKey))
                return Results.BadRequest(new { error = "missing_idempotency_key" });

            var result = await mediator.Send(new DebitBalanceCommand(playerId, idempotencyKey, body.Amount));
            return result switch
            {
                DebitBalanceResponse.Ok r => Results.Ok(new { r.Balance }),
                DebitBalanceResponse.AlreadyApplied r => Results.Ok(new { r.Balance }),
                DebitBalanceResponse.InsufficientFunds r => Results.BadRequest(new { error = "insufficient_funds", balance = r.Balance, need = r.Need }),
                DebitBalanceResponse.WalletNotFound => Results.NotFound(),
                _ => Results.Problem()
            };
        }).RequireAuthorization();

        app.MapPost("/wallets/deposit", async (CreditBalanceRequest body, ClaimsPrincipal user, ISender mediator, HttpContext http) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userId, out var playerId))
                return Results.Unauthorized();

            if (!http.Request.Headers.TryGetValue("Idempotency-Key", out var hdr)
                || !Guid.TryParse(hdr, out var idempotencyKey))
                return Results.BadRequest(new { error = "missing_idempotency_key" });

            var result = await mediator.Send(new CreditBalanceCommand(playerId, idempotencyKey, body.Amount));
            return result switch
            {
                CreditBalanceResponse.Ok r => Results.Ok(new { r.Balance }),
                CreditBalanceResponse.AlreadyApplied r => Results.Ok(new { r.Balance }),
                CreditBalanceResponse.WalletNotFound => Results.NotFound(),
                _ => Results.Problem()
            };
        }).RequireAuthorization();
    }
}
