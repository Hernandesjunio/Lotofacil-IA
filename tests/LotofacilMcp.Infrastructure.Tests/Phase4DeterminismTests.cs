using System.Text.Json;
using LotofacilMcp.Infrastructure.CanonicalJson;
using LotofacilMcp.Infrastructure.DatasetVersioning;
using LotofacilMcp.Infrastructure.Hashing;
using LotofacilMcp.Infrastructure.Providers;

namespace LotofacilMcp.Infrastructure.Tests;

public sealed class Phase4DeterminismTests
{
    [Fact]
    public void SameSnapshot_GeneratesStableDatasetVersion()
    {
        var fixturePath = GetFixturePath();
        var provider = new SyntheticFixtureProvider();
        var datasetVersionService = new DatasetVersionService();

        var snapshotA = provider.LoadSnapshot(fixturePath);
        var snapshotB = provider.LoadSnapshot(fixturePath);

        var versionA = datasetVersionService.CreateFromSnapshot(snapshotA);
        var versionB = datasetVersionService.CreateFromSnapshot(snapshotB);

        Assert.Equal(versionA, versionB);
        Assert.StartsWith("cef-", versionA);
    }

    [Fact]
    public void SameCanonicalInput_GeneratesStableDeterministicHash()
    {
        var canonicalSerializer = new CanonicalJsonSerializer();
        var hashService = new DeterministicHashService(canonicalSerializer, new Sha256Hasher());
        const string datasetVersion = "cef-2026-01-05-sha8f65ff3a";
        const string toolVersion = "1.0.0";

        using var jsonA = JsonDocument.Parse("""
            {
              "window_size": 3,
              "metrics": [
                { "name": "frequencia_por_dezena" }
              ],
              "end_contest_id": 1003
            }
            """);

        using var jsonB = JsonDocument.Parse("""
            {
              "metrics": [
                { "name": "frequencia_por_dezena" }
              ],
              "end_contest_id": 1003,
              "window_size": 3
            }
            """);

        var canonicalA = canonicalSerializer.Serialize(jsonA.RootElement);
        var canonicalB = canonicalSerializer.Serialize(jsonB.RootElement);
        var hashA = hashService.Compute(jsonA.RootElement, datasetVersion, toolVersion);
        var hashB = hashService.Compute(jsonB.RootElement, datasetVersion, toolVersion);

        Assert.Equal(canonicalA, canonicalB);
        Assert.Equal(hashA, hashB);
    }

    private static string GetFixturePath()
    {
        return Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "fixtures", "synthetic_min_window.json"));
    }
}
