using LotofacilMcp.Infrastructure.Providers;
using LotofacilMcp.Infrastructure.Hashing;

namespace LotofacilMcp.Infrastructure.DatasetVersioning;

public sealed class DatasetVersionService
{
    private readonly Sha256Hasher _hasher = new();

    public string CreateFromSnapshot(FixtureSnapshot snapshot, string sourcePrefix = "cef")
    {
        if (snapshot is null)
        {
            throw new ArgumentNullException(nameof(snapshot));
        }

        if (string.IsNullOrWhiteSpace(sourcePrefix))
        {
            throw new ArgumentException("source prefix cannot be null or empty.", nameof(sourcePrefix));
        }

        var latestDate = snapshot.Draws
            .Select(draw => DateOnly.Parse(draw.DrawDate))
            .Max();

        var contentHash = _hasher.ComputeHex(snapshot.RawJson);
        var shortHash = contentHash[..8];

        return $"{sourcePrefix}-{latestDate:yyyy-MM-dd}-sha{shortHash}";
    }
}
