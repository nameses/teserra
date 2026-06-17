using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tessera.History.Api.DTOs.Common;

namespace Tessera.History.Api.Services;

internal record BetCursor(long PlacedAtTicks, Guid RoundId, SortDirection Dir)
{
    [JsonIgnore] public DateTime PlacedAt => new(PlacedAtTicks, DateTimeKind.Utc);

    public string Encode() =>
        Base64Url.EncodeToString(JsonSerializer.SerializeToUtf8Bytes(this));

    public static BetCursor? Decode(string? token, SortDirection currentDir)
    {
        if (string.IsNullOrEmpty(token)) return null;
        try
        {
            var c = JsonSerializer.Deserialize<BetCursor>(Base64Url.DecodeFromChars(token));
            return c is null || c.Dir != currentDir ? null : c; // dir changed -> restart
        }
        catch { return null; }
    }
}
