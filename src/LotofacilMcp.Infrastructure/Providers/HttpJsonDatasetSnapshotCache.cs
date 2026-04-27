using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LotofacilMcp.Infrastructure.Providers;

public sealed record HttpJsonDatasetSnapshotResult(
    bool Success,
    string? SnapshotPath,
    string? ContentSha256Hex,
    string? FailureReason);

/// <summary>
/// Downloads a remote JSON dataset (HTTP/HTTPS) and materializes a local snapshot cached by content hash.
/// Cache is an optimization only; dataset_version must reflect the effective content read (ADR 0022 D1.2).
/// </summary>
public sealed class HttpJsonDatasetSnapshotCache
{
    private readonly HttpClient _httpClient;
    private readonly string _cacheDirectory;

    public HttpJsonDatasetSnapshotCache(HttpClient httpClient, string cacheDirectory)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _cacheDirectory = string.IsNullOrWhiteSpace(cacheDirectory)
            ? throw new ArgumentException("cache directory cannot be null or empty.", nameof(cacheDirectory))
            : cacheDirectory;
    }

    public HttpJsonDatasetSnapshotResult GetOrCreateSnapshot(string sourceUrl)
    {
        if (string.IsNullOrWhiteSpace(sourceUrl))
        {
            return new HttpJsonDatasetSnapshotResult(
                Success: false,
                SnapshotPath: null,
                ContentSha256Hex: null,
                FailureReason: "unreachable");
        }

        if (!Uri.TryCreate(sourceUrl.Trim(), UriKind.Absolute, out var uri) ||
            (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
             !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
        {
            return new HttpJsonDatasetSnapshotResult(
                Success: false,
                SnapshotPath: null,
                ContentSha256Hex: null,
                FailureReason: "invalid_format");
        }

        byte[] payloadBytes;
        try
        {
            using var response = _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead)
                .GetAwaiter()
                .GetResult();

            if (!response.IsSuccessStatusCode)
            {
                return new HttpJsonDatasetSnapshotResult(
                    Success: false,
                    SnapshotPath: null,
                    ContentSha256Hex: null,
                    FailureReason: "unreachable");
            }

            payloadBytes = response.Content.ReadAsByteArrayAsync()
                .GetAwaiter()
                .GetResult();
        }
        catch (HttpRequestException)
        {
            return new HttpJsonDatasetSnapshotResult(
                Success: false,
                SnapshotPath: null,
                ContentSha256Hex: null,
                FailureReason: "unreachable");
        }
        catch (TaskCanceledException)
        {
            return new HttpJsonDatasetSnapshotResult(
                Success: false,
                SnapshotPath: null,
                ContentSha256Hex: null,
                FailureReason: "unreachable");
        }

        var sha256Hex = ComputeSha256Hex(payloadBytes);
        Directory.CreateDirectory(_cacheDirectory);
        var snapshotPath = Path.Combine(_cacheDirectory, $"{sha256Hex}.json");

        // Validate JSON semantics for V0 HTTP profile (ADR 0022 D1.1): always JSON.
        // - invalid JSON => invalid_format
        // - valid JSON but schema missing/empty draws => invalid_data
        var jsonText = DecodeUtf8(payloadBytes);
        if (!TryValidateV0DrawsJson(jsonText, out var validationReason))
        {
            return new HttpJsonDatasetSnapshotResult(
                Success: false,
                SnapshotPath: null,
                ContentSha256Hex: sha256Hex,
                FailureReason: validationReason);
        }

        if (!File.Exists(snapshotPath))
        {
            var tempPath = snapshotPath + ".tmp";
            File.WriteAllText(tempPath, jsonText, Encoding.UTF8);
            File.Move(tempPath, snapshotPath, overwrite: true);
        }

        return new HttpJsonDatasetSnapshotResult(
            Success: true,
            SnapshotPath: snapshotPath,
            ContentSha256Hex: sha256Hex,
            FailureReason: null);
    }

    private static string DecodeUtf8(byte[] bytes)
    {
        return Encoding.UTF8.GetString(bytes);
    }

    private static string ComputeSha256Hex(byte[] bytes)
    {
        var hash = SHA256.HashData(bytes);
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
        {
            sb.Append(b.ToString("x2"));
        }

        return sb.ToString();
    }

    private static bool TryValidateV0DrawsJson(string jsonText, out string failureReason)
    {
        failureReason = "invalid_format";
        try
        {
            using var doc = JsonDocument.Parse(jsonText);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                failureReason = "invalid_data";
                return false;
            }

            if (!root.TryGetProperty("draws", out var draws) || draws.ValueKind != JsonValueKind.Array)
            {
                failureReason = "invalid_data";
                return false;
            }

            if (draws.GetArrayLength() == 0)
            {
                failureReason = "invalid_data";
                return false;
            }

            failureReason = string.Empty;
            return true;
        }
        catch (JsonException)
        {
            failureReason = "invalid_format";
            return false;
        }
    }
}

