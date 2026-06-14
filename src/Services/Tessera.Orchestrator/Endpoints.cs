using System.Security.Claims;
using MassTransit;
using Tessera.Contracts.Betting;
using Tessera.Orchestrator.Db;

namespace Tessera.Orchestrator;

public static class WalletEndpointsExtension
{
    public static void MapApiEndpoints(this WebApplication app)
    {
        app.MapPost("/bets", async (PlaceBetRequest body, ClaimsPrincipal user, IPublishEndpoint bus) =>
        {
            var sub = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(sub, out var playerId))
                return Results.Unauthorized();

            if (body.Stake <= 0)
                return Results.BadRequest(new { error = "invalid_stake" });
            if (string.IsNullOrWhiteSpace(body.GameType))
                return Results.BadRequest(new { error = "missing_game" });
            if (string.IsNullOrWhiteSpace(body.GameDetails))
                return Results.BadRequest(new { error = "missing_details" });

            var roundId = Guid.NewGuid();
            await bus.Publish(new BetPlacedEvent(roundId, playerId, body.GameType, body.Stake, body.GameDetails));

            return Results.Accepted($"/bets/{roundId}", new { betId = roundId });
        }).RequireAuthorization();

        app.MapGet("/bets/{id:guid}", async (Guid id, OrchestratorDbContext db) =>
        {
            var saga = await db.Set<BetState>().FindAsync(id);
            return saga is null
                ? Results.NotFound(new { hint = "Сompleted bets live in the history service" })
                : Results.Ok(new { betId = saga.CorrelationId, state = saga.CurrentState });
        }).RequireAuthorization();
    }
}

public record PlaceBetRequest(string GameType, decimal Stake, string GameDetails);
