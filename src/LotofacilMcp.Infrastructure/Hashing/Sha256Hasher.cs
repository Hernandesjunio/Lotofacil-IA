using System.Security.Cryptography;
using System.Text;

namespace LotofacilMcp.Infrastructure.Hashing;

public sealed class Sha256Hasher
{
    public string ComputeHex(string content)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        var bytes = Encoding.UTF8.GetBytes(content);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
