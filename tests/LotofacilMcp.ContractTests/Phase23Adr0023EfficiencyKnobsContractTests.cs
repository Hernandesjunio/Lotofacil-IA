using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace LotofacilMcp.ContractTests;

public sealed class Phase23Adr0023EfficiencyKnobsContractTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _httpFactory;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    private HttpClient _httpClient = default!;
    private McpClient _stdioMcpClient = default!;
    private McpClient _httpMcpClient = default!;

    public Phase23Adr0023EfficiencyKnobsContractTests()
    {
        _httpFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Dataset:DrawsSourceUri"] = ContractTestFixturePaths.SyntheticMinWindowJson()
                });
            });
        });
    }

    public async Task InitializeAsync()
    {
        _httpClient = _httpFactory.CreateClient();

        _stdioMcpClient = await McpClient.CreateAsync(new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "LotofacilMcp.Server",
            Command = "cmd",
            Arguments =
            [
                "/c",
                BuildStdioLaunchCommand()
            ]
        }));

        var mcpEndpoint = new Uri(_httpClient.BaseAddress!, "mcp");
        _httpMcpClient = await McpClient.CreateAsync(new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Name = "LotofacilMcp.Server.Http",
                Endpoint = mcpEndpoint,
                TransportMode = HttpTransportMode.StreamableHttp
            },
            _httpClient,
            NullLoggerFactory.Instance,
            ownsHttpClient: false));
    }

    public async Task DisposeAsync()
    {
        await _httpMcpClient.DisposeAsync();
        await _stdioMcpClient.DisposeAsync();
        _httpClient.Dispose();
        await _httpFactory.DisposeAsync();
    }

    [Fact]
    public async Task DiscoverCapabilities_SupportsMinimalAndFull_AndVerbosityChangesDeterministicHash()
    {
        var minimal = await _stdioMcpClient.CallToolAsync("discover_capabilities", new Dictionary<string, object?>
        {
            ["verbosity"] = "minimal"
        });
        var full = await _stdioMcpClient.CallToolAsync("discover_capabilities", new Dictionary<string, object?>
        {
            ["verbosity"] = "full"
        });

        Assert.False(minimal.IsError);
        Assert.False(full.IsError);

        var hashMinimal = ReadStructured(minimal).GetProperty("deterministic_hash").GetString();
        var hashFull = ReadStructured(full).GetProperty("deterministic_hash").GetString();

        Assert.False(string.IsNullOrWhiteSpace(hashMinimal));
        Assert.False(string.IsNullOrWhiteSpace(hashFull));
        Assert.NotEqual(hashMinimal, hashFull);
    }

    [Fact]
    public async Task ComputeWindowMetrics_SupportsMinimalAndFull_AndVerbosityChangesDeterministicHash()
    {
        var args = new Dictionary<string, object?>
        {
            ["window_size"] = 3,
            ["end_contest_id"] = 1003,
            ["metrics"] = new object[]
            {
                new Dictionary<string, object?> { ["name"] = "frequencia_por_dezena" }
            }
        };

        var minimal = await _httpMcpClient.CallToolAsync("compute_window_metrics", new Dictionary<string, object?>(args)
        {
            ["verbosity"] = "minimal"
        });
        var full = await _httpMcpClient.CallToolAsync("compute_window_metrics", new Dictionary<string, object?>(args)
        {
            ["verbosity"] = "full"
        });

        Assert.False(minimal.IsError);
        Assert.False(full.IsError);

        var hashMinimal = ReadStructured(minimal).GetProperty("deterministic_hash").GetString();
        var hashFull = ReadStructured(full).GetProperty("deterministic_hash").GetString();

        Assert.NotEqual(hashMinimal, hashFull);
    }

    [Fact]
    public async Task Content_DoesNotDuplicateStructuredContentJson()
    {
        var response = await _stdioMcpClient.CallToolAsync("compute_window_metrics", new Dictionary<string, object?>
        {
            ["window_size"] = 3,
            ["end_contest_id"] = 1003,
            ["metrics"] = new object[]
            {
                new Dictionary<string, object?> { ["name"] = "frequencia_por_dezena" }
            },
            ["verbosity"] = "full"
        });

        Assert.False(response.IsError);

        var structured = ReadStructured(response);
        var summaryText = response.Content.OfType<TextContentBlock>().FirstOrDefault()?.Text ?? string.Empty;

        // ADR 0023 (D2): Content é um resumo humano curto, não um dump do JSON.
        Assert.False(string.IsNullOrWhiteSpace(summaryText));
        Assert.False(summaryText.TrimStart().StartsWith('{'));
        Assert.DoesNotContain("\"dataset_version\"", summaryText, StringComparison.Ordinal);
        Assert.DoesNotContain("\"deterministic_hash\"", summaryText, StringComparison.Ordinal);

        // Garante que Content não é um JSON equivalente ao StructuredContent.
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<JsonElement>(summaryText, _jsonOptions));

        // "Evidência forte": não deve conter um prefixo do JSON estruturado serializado.
        var structuredText = JsonSerializer.Serialize(structured, _jsonOptions);
        var prefix = structuredText.Length <= 80 ? structuredText : structuredText[..80];
        Assert.DoesNotContain(prefix, summaryText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DeterministicHash_ChangesWhenPresentationKnobsChange()
    {
        var baseArgs = new Dictionary<string, object?>
        {
            ["window_size"] = 3,
            ["end_contest_id"] = 1003,
            ["metrics"] = new object[]
            {
                new Dictionary<string, object?> { ["name"] = "frequencia_por_dezena" }
            }
        };

        var standard = await _httpMcpClient.CallToolAsync("compute_window_metrics", new Dictionary<string, object?>(baseArgs)
        {
            ["verbosity"] = "standard",
            ["include_explanations"] = true
        });
        var noExplanations = await _httpMcpClient.CallToolAsync("compute_window_metrics", new Dictionary<string, object?>(baseArgs)
        {
            ["verbosity"] = "standard",
            ["include_explanations"] = false
        });
        var projected = await _httpMcpClient.CallToolAsync("compute_window_metrics", new Dictionary<string, object?>(baseArgs)
        {
            ["verbosity"] = "standard",
            ["include_explanations"] = true,
            ["fields"] = new[] { "dataset_version", "tool_version", "deterministic_hash", "window" }
        });
        var paged = await _httpMcpClient.CallToolAsync("compute_window_metrics", new Dictionary<string, object?>(baseArgs)
        {
            ["verbosity"] = "full",
            ["include_explanations"] = true,
            ["page"] = 1,
            ["page_size"] = 1
        });

        Assert.False(standard.IsError);
        Assert.False(noExplanations.IsError);
        Assert.False(projected.IsError);
        Assert.False(paged.IsError);

        var h0 = ReadStructured(standard).GetProperty("deterministic_hash").GetString();
        var h1 = ReadStructured(noExplanations).GetProperty("deterministic_hash").GetString();
        var h2 = ReadStructured(projected).GetProperty("deterministic_hash").GetString();
        var h3 = ReadStructured(paged).GetProperty("deterministic_hash").GetString();

        Assert.NotEqual(h0, h1);
        Assert.NotEqual(h0, h2);
        Assert.NotEqual(h0, h3);
    }

    [Fact]
    public async Task Pagination_IsRejected_WhenVerbosityIsNotFull()
    {
        // ADR 0023 / mcp-tool-contract: page/page_size only allowed for responses large in verbosity="full".
        var response = await _httpMcpClient.CallToolAsync("compute_window_metrics", new Dictionary<string, object?>
        {
            ["window_size"] = 3,
            ["end_contest_id"] = 1003,
            ["metrics"] = new object[]
            {
                new Dictionary<string, object?> { ["name"] = "frequencia_por_dezena" }
            },
            ["verbosity"] = "standard",
            ["page"] = 1,
            ["page_size"] = 1
        });

        Assert.True(response.IsError);
        var structured = ReadStructured(response);
        var error = structured.GetProperty("error");
        Assert.Equal("INVALID_REQUEST", error.GetProperty("code").GetString());

        var details = error.GetProperty("details");
        Assert.Equal("page", details.GetProperty("field").GetString());
    }

    private static JsonElement ReadStructured(CallToolResult response)
    {
        if (response.StructuredContent is JsonElement structuredContent)
        {
            return structuredContent;
        }

        throw new InvalidOperationException("Expected StructuredContent JsonElement for MCP call result.");
    }

    private static string BuildStdioLaunchCommand()
    {
        var fixturePath = ContractTestFixturePaths.SyntheticMinWindowJson();
        var serverProjectPath = GetServerProjectPath();
#if DEBUG
        var configuration = "Debug";
#else
        var configuration = "Release";
#endif
        return
            $"set Dataset__DrawsSourceUri={fixturePath} && " +
            $"dotnet run -c {configuration} --no-build --project {serverProjectPath} -- --mcp-stdio";
    }

    private static string GetServerProjectPath()
    {
        var repositoryRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

        return Path.Combine(repositoryRoot, "src", "LotofacilMcp.Server", "LotofacilMcp.Server.csproj");
    }
}

