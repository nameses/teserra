using System.Security.Claims;
using MediatR;
using Tessera.History.Api.Handlers;

namespace Tessera.History.Api;

public static class BetsEndpointsExtension
{
    public static void MapApiEndpoints(this WebApplication app)
    {
        app.MapGet("/history/bets", async (ClaimsPrincipal user, Guid roundId, ISender mediator) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userId, out var playerId))
                return Results.Unauthorized();

            var balance = await mediator.Send(new GetBetDetailsQuery(roundId, playerId));
            return Results.Ok(balance);
        }).RequireAuthorization();
    }
}

