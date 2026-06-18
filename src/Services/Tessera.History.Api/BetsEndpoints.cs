using System.Security.Claims;
using MediatR;
using Tessera.History.Api.DTOs;
using Tessera.History.Api.Handlers;

namespace Tessera.History.Api;

public static class BetsEndpointsExtension
{
    public static void MapApiEndpoints(this WebApplication app)
    {
        app.MapGet("/history/bets", async (
            Guid roundId, 
            ClaimsPrincipal user, 
            ISender mediator,
            CancellationToken ct) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userId, out var playerId))
                return Results.Unauthorized();

            var balance = await mediator.Send(new GetBetDetailsQuery(roundId, playerId), ct);
            return Results.Ok(balance);
        }).RequireAuthorization();

        app.MapGet("/history/bets/bulk", async (
            [AsParameters] BetsRequest query, 
            ClaimsPrincipal user,
            ISender mediator,
            CancellationToken ct) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userId, out var playerId))
                return Results.Unauthorized();

            var balance = await mediator.Send(new GetBetsHandlerQuery(query, playerId), ct);
            return Results.Ok(balance);
        }).RequireAuthorization();
    }
}
