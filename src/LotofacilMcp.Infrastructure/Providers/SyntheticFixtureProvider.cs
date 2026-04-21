using System.Text.Json;
using System.Text.Json.Serialization;
using LotofacilMcp.Domain.Models;

namespace LotofacilMcp.Infrastructure.Providers;

public sealed record FixtureSnapshot(
    string RawJson,
    IReadOnlyList<RawDraw> Draws);

public sealed class SyntheticFixtureProvider
{
    public FixtureSnapshot LoadSnapshot(string fixturePath)
    {
        if (string.IsNullOrWhiteSpace(fixturePath))
        {
            throw new ArgumentException("fixture path cannot be null or empty.", nameof(fixturePath));
        }

        var fullPath = Path.GetFullPath(fixturePath);
        var rawJson = File.ReadAllText(fullPath);
        var root = JsonSerializer.Deserialize<FixtureRoot>(rawJson);

        if (root?.Draws is null || root.Draws.Count == 0)
        {
            throw new InvalidOperationException("fixture must contain at least one draw.");
        }

        var draws = root.Draws
            .Select(draw => new RawDraw(
                draw.ContestId,
                draw.DrawDate,
                draw.Numbers))
            .ToArray();

        return new FixtureSnapshot(rawJson, draws);
    }

    private sealed record FixtureRoot(
        [property: JsonPropertyName("draws")] IReadOnlyList<FixtureDraw> Draws);

    private sealed record FixtureDraw(
        [property: JsonPropertyName("contest_id")] int ContestId,
        [property: JsonPropertyName("draw_date")] string DrawDate,
        [property: JsonPropertyName("numbers")] IReadOnlyList<int> Numbers);
}
