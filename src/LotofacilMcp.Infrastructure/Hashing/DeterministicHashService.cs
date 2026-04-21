using LotofacilMcp.Infrastructure.CanonicalJson;

namespace LotofacilMcp.Infrastructure.Hashing;

public sealed class DeterministicHashService
{
    private readonly CanonicalJsonSerializer _canonicalJsonSerializer;
    private readonly Sha256Hasher _sha256Hasher;

    public DeterministicHashService(
        CanonicalJsonSerializer canonicalJsonSerializer,
        Sha256Hasher sha256Hasher)
    {
        _canonicalJsonSerializer = canonicalJsonSerializer;
        _sha256Hasher = sha256Hasher;
    }

    public string Compute(
        object input,
        string datasetVersion,
        string toolVersion)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (string.IsNullOrWhiteSpace(datasetVersion))
        {
            throw new ArgumentException("dataset_version cannot be null or empty.", nameof(datasetVersion));
        }

        if (string.IsNullOrWhiteSpace(toolVersion))
        {
            throw new ArgumentException("tool_version cannot be null or empty.", nameof(toolVersion));
        }

        var canonicalPayload = _canonicalJsonSerializer.Serialize(new
        {
            input,
            dataset_version = datasetVersion,
            tool_version = toolVersion
        });

        return _sha256Hasher.ComputeHex(canonicalPayload);
    }
}
