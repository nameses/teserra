using Tessera.History.Api.DTOs.Common;

namespace Tessera.History.Api.DTOs;

public record BetsResponse
(
    IEnumerable<BetDetailsResponse> Items,
    string? NextCursor
);
