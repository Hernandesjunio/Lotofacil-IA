using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;

namespace LotofacilMcp.ContractTests;

/// <summary>
/// ADR 0022 D1.1/D1.2 + spec-driven guide Fase 28B:
/// HTTP/HTTPS Dataset:DrawsSourceUri treated as JSON snapshot versioned by content (cache by hash).
/// </summary>
public sealed class Phase28BDatasetDrawsSourceUriHttpContractTests : IAsyncLifetime
{
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
    private TestHttpsDatasetServer _datasetServer = default!;

    public async Task InitializeAsync()
    {
        _datasetServer = await TestHttpsDatasetServer.StartAsync();
        _datasetServer.SetJson(InitialDatasetJson());
    }

    public async Task DisposeAsync()
    {
        await _datasetServer.DisposeAsync();
    }

    [Fact]
    public async Task HttpDataset_Success_RepeatedCallsSameContent_YieldSameDatasetVersion()
    {
        await using var factory = CreateServerFactory(_datasetServer.DatasetUrl);
        using var client = factory.CreateClient();

        var request = new
        {
            window_size = 3,
            end_contest_id = 1003
        };

        var first = await client.PostAsJsonAsync("/tools/get_draw_window", request);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var firstJson = await first.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        var datasetVersionA = firstJson.GetProperty("dataset_version").GetString();

        var second = await client.PostAsJsonAsync("/tools/get_draw_window", request);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        var secondJson = await second.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        var datasetVersionB = secondJson.GetProperty("dataset_version").GetString();

        Assert.Equal(datasetVersionA, datasetVersionB);
    }

    [Fact]
    public async Task HttpDataset_ChangingRemoteContent_ChangesDatasetVersion()
    {
        await using var factory = CreateServerFactory(_datasetServer.DatasetUrl);
        using var client = factory.CreateClient();

        var request = new
        {
            window_size = 3,
            end_contest_id = 1003
        };

        var before = await client.PostAsJsonAsync("/tools/get_draw_window", request);
        Assert.Equal(HttpStatusCode.OK, before.StatusCode);
        var beforeJson = await before.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        var datasetVersionA = beforeJson.GetProperty("dataset_version").GetString();

        _datasetServer.SetJson(UpdatedDatasetJson());

        var after = await client.PostAsJsonAsync("/tools/get_draw_window", request);
        Assert.Equal(HttpStatusCode.OK, after.StatusCode);
        var afterJson = await after.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        var datasetVersionB = afterJson.GetProperty("dataset_version").GetString();

        Assert.NotEqual(datasetVersionA, datasetVersionB);
    }

    [Fact]
    public async Task HttpDataset_InvalidJson_ReturnsDatasetUnavailableInvalidFormat()
    {
        _datasetServer.SetRaw("not-json");

        await using var factory = CreateServerFactory(_datasetServer.DatasetUrl);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/tools/get_draw_window", new
        {
            window_size = 3,
            end_contest_id = 1003
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        var error = payload.GetProperty("error");
        Assert.Equal("DATASET_UNAVAILABLE", error.GetProperty("code").GetString());
        Assert.Equal("invalid_format", error.GetProperty("details").GetProperty("reason").GetString());
        Assert.Equal(_datasetServer.DatasetUrl.ToString(), error.GetProperty("details").GetProperty("source").GetString());
    }

    [Fact]
    public async Task HttpDataset_NetworkOr404_ReturnsDatasetUnavailableUnreachable()
    {
        var missingUrl = new Uri(_datasetServer.BaseUri, "/missing");
        await using var factory = CreateServerFactory(missingUrl);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/tools/get_draw_window", new
        {
            window_size = 3,
            end_contest_id = 1003
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        var error = payload.GetProperty("error");
        Assert.Equal("DATASET_UNAVAILABLE", error.GetProperty("code").GetString());
        Assert.Equal("unreachable", error.GetProperty("details").GetProperty("reason").GetString());
        Assert.Equal(missingUrl.ToString(), error.GetProperty("details").GetProperty("source").GetString());
    }

    private static WebApplicationFactory<Program> CreateServerFactory(Uri datasetUrl)
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Dataset:DrawsSourceUri"] = datasetUrl.ToString()
                });
            });
        });
    }

    private static string InitialDatasetJson() =>
        """
        {
          "draws": [
            { "contest_id": 1001, "draw_date": "2024-01-01", "numbers": [1,2,3,4,5,6,7,8,9,10,11,12,13,14,15] },
            { "contest_id": 1002, "draw_date": "2024-01-02", "numbers": [1,2,3,4,5,6,7,8,9,10,11,12,13,14,16] },
            { "contest_id": 1003, "draw_date": "2024-01-03", "numbers": [1,2,3,4,5,6,7,8,9,10,11,12,13,14,17] }
          ]
        }
        """;

    private static string UpdatedDatasetJson() =>
        """
        {
          "draws": [
            { "contest_id": 1001, "draw_date": "2024-01-01", "numbers": [1,2,3,4,5,6,7,8,9,10,11,12,13,14,15] },
            { "contest_id": 1002, "draw_date": "2024-01-02", "numbers": [1,2,3,4,5,6,7,8,9,10,11,12,13,14,16] },
            { "contest_id": 1003, "draw_date": "2024-01-03", "numbers": [1,2,3,4,5,6,7,8,9,10,11,12,13,14,18] }
          ]
        }
        """;

    private sealed class TestHttpsDatasetServer : IAsyncDisposable
    {
        private readonly CancellationTokenSource _cts = new();
        private readonly object _lock = new();
        private string _payload = "{}";

        private WebApplication _app = default!;

        public Uri BaseUri { get; private set; } = default!;
        public Uri DatasetUrl => new(BaseUri, "/draws");

        public static async Task<TestHttpsDatasetServer> StartAsync()
        {
            var cert = CreateSelfSignedLocalhostCert();

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                EnvironmentName = "Development"
            });

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Listen(System.Net.IPAddress.Loopback, 0, listen =>
                {
                    listen.Protocols = HttpProtocols.Http1;
                    listen.UseHttps(cert);
                });
            });

            var app = builder.Build();
            var server = new TestHttpsDatasetServer { _app = app };

            app.MapGet("/draws", () =>
            {
                string payload;
                lock (server._lock)
                {
                    payload = server._payload;
                }

                return Results.Text(payload, "application/json", Encoding.UTF8);
            });

            await app.StartAsync(server._cts.Token);
            var url = app.Urls.Single(u => u.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
            server.BaseUri = new Uri(url);
            return server;
        }

        public void SetJson(string json)
        {
            // Keep server "dumb"; JSON validity is validated by the main server snapshotter per ADR 0022 D1.1.
            lock (_lock)
            {
                _payload = json;
            }
        }

        public void SetRaw(string raw)
        {
            lock (_lock)
            {
                _payload = raw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            try
            {
                await _app.StopAsync();
            }
            catch
            {
                // ignore stop errors in test cleanup
            }
            finally
            {
                await _app.DisposeAsync();
                _cts.Dispose();
            }
        }

        private static X509Certificate2 CreateSelfSignedLocalhostCert()
        {
            using var rsa = RSA.Create(2048);
            var req = new CertificateRequest(
                "CN=localhost",
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            req.CertificateExtensions.Add(
                new X509BasicConstraintsExtension(false, false, 0, false));
            req.CertificateExtensions.Add(
                new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, false));
            req.CertificateExtensions.Add(
                new X509SubjectKeyIdentifierExtension(req.PublicKey, false));

            // Subject Alternative Names: localhost + loopback IPs
            var sanBuilder = new SubjectAlternativeNameBuilder();
            sanBuilder.AddDnsName("localhost");
            sanBuilder.AddIpAddress(System.Net.IPAddress.Loopback);
            sanBuilder.AddIpAddress(System.Net.IPAddress.IPv6Loopback);
            req.CertificateExtensions.Add(sanBuilder.Build());

            var cert = req.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(30));
            return new X509Certificate2(cert.Export(X509ContentType.Pfx));
        }
    }
}

