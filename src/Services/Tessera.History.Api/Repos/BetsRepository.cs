using Microsoft.EntityFrameworkCore;

using Tessera.History.Api.Db;
using Tessera.History.Api.DTOs;

namespace Tessera.History.Api.Repositories;

public interface IBetsRepository
{
    public Task<BetDetailsResponse?> GetAsync(Guid roundId, Guid playerId);
    public Task<List<BetDetailsResponse>> GetAsync(BetsRequest request, Guid playerId);
}

public class BetsRepository : IBetsRepository
{
    private readonly HistoryApiDbContext _db;

    public BetsRepository(HistoryApiDbContext db)
    {
        _db = db;
    }
    public async Task<BetDetailsResponse?> GetAsync(Guid roundId, Guid playerId)
    {
        var bet = await _db.BetDetails
            .Where(b => b.PlayerId == playerId && b.RoundId == roundId)
            .Select(BetDetailsResponseHelpers.ToResponse)
            .FirstOrDefaultAsync();

        return bet;
    }

    public async Task<List<BetDetailsResponse>> GetAsync(BetsRequest request, Guid playerId)
    {
        throw new NotImplementedException();
    }
}
