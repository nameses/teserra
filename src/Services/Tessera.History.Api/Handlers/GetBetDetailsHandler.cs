using MediatR;

using Tessera.History.Api.DTOs;
using Tessera.History.Api.Repositories;

namespace Tessera.History.Api.Handlers;

public record GetBetDetailsQuery(Guid RoundId, Guid PlayerId) : IRequest<BetDetailsResponse?>;

public class GetBetDetailsHandler : IRequestHandler<GetBetDetailsQuery, BetDetailsResponse?>
{
    private readonly IBetsRepository _repo;

    public GetBetDetailsHandler(IBetsRepository repo) => _repo = repo;

    public async Task<BetDetailsResponse?> Handle(GetBetDetailsQuery request, CancellationToken cancellationToken)
    {
        var betDetails = await _repo.GetAsync(request.RoundId, request.PlayerId, cancellationToken);

        return betDetails;
    }
}
