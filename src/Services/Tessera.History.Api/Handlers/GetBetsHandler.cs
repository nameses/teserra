using MediatR;
using Tessera.History.Api.DTOs;
using Tessera.History.Api.Repositories;

namespace Tessera.History.Api.Handlers;

public record GetBetsHandlerQuery(BetsRequest Request, Guid PlayerId) : IRequest<BetsResponse>;

public class GetBetsHandler : IRequestHandler<GetBetsHandlerQuery, BetsResponse>
{
    private readonly IBetsRepository _repo;

    public GetBetsHandler(IBetsRepository repo) => _repo = repo;

    public async Task<BetsResponse> Handle(GetBetsHandlerQuery query, CancellationToken cancellationToken)
    {
        var betDetails = await _repo.GetBulkAsync(query.Request, query.PlayerId, cancellationToken);

        return betDetails;
    }
}
