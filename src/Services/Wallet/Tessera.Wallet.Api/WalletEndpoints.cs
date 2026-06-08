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
            if (!Guid.TryParse(user.FindFirst("sub")?.Value, out var playerId))
                return Results.Unauthorized();

            var balance = await mediator.Send(new GetBalanceQuery(playerId));
            return Results.Ok(balance);
        }).RequireAuthorization();


        app.MapGet("/wallets/me/transactions", async (ClaimsPrincipal user, ISender mediator) =>
        {
            if (!Guid.TryParse(user.FindFirst("sub")?.Value, out var playerId))
                return Results.Unauthorized();

            var transactions = await mediator.Send(new GetTransactionsQuery(playerId));
            return Results.Ok(transactions);
        }).RequireAuthorization();
    }
}
