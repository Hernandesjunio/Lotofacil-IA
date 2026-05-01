using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace LotofacilMcp.ContractTests;

/// <summary>
/// ADR 0025 acceptance checks for MCP HTTP deploy shape.
/// References:
/// - docs/adrs/0025-deploy-http-docker-iis-cloud-para-mcp-http-v1.md
/// - docs/adrs/0022-fonte-de-dados-e-metadados-de-ganhadores-v1.md
/// </summary>
public sealed class Adr0025HttpAcceptanceContractTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _httpFactory;
    private HttpClient _httpClient = default!;
    private McpClient _httpMcpClient = default!;

    public Adr0025HttpAcceptanceContractTests()
    {
        // No Dataset:DrawsSourceUri configured on purpose.
        _httpFactory = new WebApplicationFactory<Program>();
    }

    public async Task InitializeAsync()
    {
        _httpClient = _httpFactory.CreateClient();

        // ADR 0025 / README: endpoint MCP HTTP mínimo é /mcp (streamable HTTP).
        var mcpEndpoint = new Uri(_httpClient.BaseAddress!, "mcp");
        _httpMcpClient = await McpClient.CreateAsync(new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Name = "LotofacilMcp.Server.Http.NoDataset",
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
        _httpClient.Dispose();
        await _httpFactory.DisposeAsync();
    }

    [Fact]
    public async Task McpHttpEndpoint_AllowsToolsList_AndToolsCall_EvenWithoutDataset()
    {
        // Accept criteria: host can connect and do tools/list and tools/call.
        var tools = await _httpMcpClient.ListToolsAsync();
        Assert.NotEmpty(tools);
        Assert.Contains(tools, t => t.Name is "help");
        Assert.Contains(tools, t => t.Name is "discover_capabilities");

        var help = await _httpMcpClient.CallToolAsync("help", new Dictionary<string, object?>());
        Assert.False(help.IsError);
        var helpJson = ReadMcpStructuredJson(help);
        Assert.True(helpJson.TryGetProperty("getting_started_resource_uri", out _));
        Assert.True(helpJson.TryGetProperty("index_resource_uri", out _));
    }

    [Fact]
    public async Task WithoutDatasetDrawsSourceUri_DatasetDependentTools_ReturnDatasetUnavailable_MissingEnv()
    {
        // ADR 0025 acceptance: without Dataset__DrawsSourceUri, history-dependent tools must return DATASET_UNAVAILABLE.
        // ADR 0022 D4: details.reason == "missing_env", with missing_env == "Dataset__DrawsSourceUri".
        var response = await _httpMcpClient.CallToolAsync("get_draw_window", new Dictionary<string, object?>
        {
            ["window_size"] = 3,
            ["end_contest_id"] = 1003
        });

        Assert.True(response.IsError);
        var json = ReadMcpStructuredJson(response);
        var error = json.GetProperty("error");
        Assert.Equal("DATASET_UNAVAILABLE", error.GetProperty("code").GetString());

        var details = error.GetProperty("details");
        Assert.Equal("missing_env", details.GetProperty("reason").GetString());
        Assert.Equal("Dataset__DrawsSourceUri", details.GetProperty("missing_env").GetString());
    }

    private static JsonElement ReadMcpStructuredJson(CallToolResult response)
    {
        if (response.StructuredContent is JsonElement structuredContent)
        {
            return structuredContent;
        }

        var jsonText = response.Content
            .OfType<TextContentBlock>()
            .FirstOrDefault()
            ?.Text;

        if (string.IsNullOrWhiteSpace(jsonText))
        {
            throw new InvalidOperationException("MCP call result did not contain structured content or text JSON.");
        }

        return JsonSerializer.Deserialize<JsonElement>(jsonText);
    }
}

