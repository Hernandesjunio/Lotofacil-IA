using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace LotofacilMcp.ContractTests;

public sealed class McpTransportParityIntegrationTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _httpFactory = new();
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    private HttpClient _httpClient = default!;
    private McpClient _mcpClient = default!;

    public async Task InitializeAsync()
    {
        _httpClient = _httpFactory.CreateClient();
        _mcpClient = await McpClient.CreateAsync(new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "LotofacilMcp.Server",
            Command = "dotnet",
            Arguments =
            [
                "run",
                "--no-build",
                "--project",
                GetServerProjectPath(),
                "--",
                "--mcp-stdio"
            ]
        }));
    }

    public async Task DisposeAsync()
    {
        _httpClient.Dispose();
        await _mcpClient.DisposeAsync();
        await _httpFactory.DisposeAsync();
    }

    [Fact]
    public async Task McpDiscoveryAndSuccessfulCalls_MatchHttpJsonSemantics()
    {
        var listedTools = await _mcpClient.ListToolsAsync();
        Assert.Contains(listedTools, tool => tool.Name is "get_draw_window");
        Assert.Contains(listedTools, tool => tool.Name is "compute_window_metrics");
        Assert.Contains(listedTools, tool => tool.Name is "analyze_indicator_stability");

        var getDrawWindowRequest = new
        {
            window_size = 3,
            end_contest_id = 1003
        };

        var httpWindowResponse = await _httpClient.PostAsJsonAsync("/tools/get_draw_window", getDrawWindowRequest);
        Assert.Equal(HttpStatusCode.OK, httpWindowResponse.StatusCode);

        var mcpWindowResponse = await _mcpClient.CallToolAsync("get_draw_window", new Dictionary<string, object?>
        {
            ["window_size"] = 3,
            ["end_contest_id"] = 1003
        });

        var httpWindowPayload = await ReadHttpJsonAsync(httpWindowResponse);
        var mcpWindowPayload = ReadMcpStructuredJson(mcpWindowResponse);
        Assert.True(JsonElement.DeepEquals(httpWindowPayload, mcpWindowPayload));

        var computeRequest = new
        {
            window_size = 3,
            end_contest_id = 1003,
            metrics = new object[]
            {
                new { name = "frequencia_por_dezena" }
            }
        };

        var httpComputeResponse = await _httpClient.PostAsJsonAsync("/tools/compute_window_metrics", computeRequest);
        Assert.Equal(HttpStatusCode.OK, httpComputeResponse.StatusCode);

        var mcpComputeResponse = await _mcpClient.CallToolAsync("compute_window_metrics", new Dictionary<string, object?>
        {
            ["window_size"] = 3,
            ["end_contest_id"] = 1003,
            ["metrics"] = new object[]
            {
                new Dictionary<string, object?>
                {
                    ["name"] = "frequencia_por_dezena"
                }
            }
        });

        var httpComputePayload = await ReadHttpJsonAsync(httpComputeResponse);
        var mcpComputePayload = ReadMcpStructuredJson(mcpComputeResponse);
        Assert.True(JsonElement.DeepEquals(httpComputePayload, mcpComputePayload));

        var analyzeRequest = new
        {
            window_size = 5,
            end_contest_id = 1005,
            indicators = new object[]
            {
                new { name = "repeticao_concurso_anterior" },
                new { name = "distribuicao_linha_por_concurso", aggregation = "per_component" }
            },
            top_k = 3,
            min_history = 3
        };

        var httpAnalyzeResponse = await _httpClient.PostAsJsonAsync("/tools/analyze_indicator_stability", analyzeRequest);
        Assert.Equal(HttpStatusCode.OK, httpAnalyzeResponse.StatusCode);

        var mcpAnalyzeResponse = await _mcpClient.CallToolAsync("analyze_indicator_stability", new Dictionary<string, object?>
        {
            ["window_size"] = 5,
            ["end_contest_id"] = 1005,
            ["indicators"] = new object[]
            {
                new Dictionary<string, object?>
                {
                    ["name"] = "repeticao_concurso_anterior"
                },
                new Dictionary<string, object?>
                {
                    ["name"] = "distribuicao_linha_por_concurso",
                    ["aggregation"] = "per_component"
                }
            },
            ["top_k"] = 3,
            ["min_history"] = 3
        });

        var httpAnalyzePayload = await ReadHttpJsonAsync(httpAnalyzeResponse);
        var mcpAnalyzePayload = ReadMcpStructuredJson(mcpAnalyzeResponse);
        Assert.True(JsonElement.DeepEquals(httpAnalyzePayload, mcpAnalyzePayload));
    }

    [Fact]
    public async Task McpAndHttp_ContractErrorParity_IsConsistent()
    {
        var invalidRequest = new
        {
            window_size = 3,
            end_contest_id = 1003
        };

        var httpResponse = await _httpClient.PostAsJsonAsync("/tools/compute_window_metrics", invalidRequest);
        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);

        var mcpResponse = await _mcpClient.CallToolAsync("compute_window_metrics", new Dictionary<string, object?>
        {
            ["window_size"] = 3,
            ["end_contest_id"] = 1003
        });

        Assert.True(mcpResponse.IsError);

        var httpPayload = await ReadHttpJsonAsync(httpResponse);
        var mcpPayload = ReadMcpStructuredJson(mcpResponse);
        Assert.True(JsonElement.DeepEquals(httpPayload, mcpPayload));

        var invalidAnalyzeRequest = new
        {
            window_size = 5,
            end_contest_id = 1005,
            indicators = new object[]
            {
                new { name = "distribuicao_linha_por_concurso" }
            },
            top_k = 3,
            min_history = 3
        };

        var httpAnalyzeResponse = await _httpClient.PostAsJsonAsync("/tools/analyze_indicator_stability", invalidAnalyzeRequest);
        Assert.Equal(HttpStatusCode.BadRequest, httpAnalyzeResponse.StatusCode);

        var mcpAnalyzeResponse = await _mcpClient.CallToolAsync("analyze_indicator_stability", new Dictionary<string, object?>
        {
            ["window_size"] = 5,
            ["end_contest_id"] = 1005,
            ["indicators"] = new object[]
            {
                new Dictionary<string, object?>
                {
                    ["name"] = "distribuicao_linha_por_concurso"
                }
            },
            ["top_k"] = 3,
            ["min_history"] = 3
        });

        Assert.True(mcpAnalyzeResponse.IsError);

        var httpAnalyzePayload = await ReadHttpJsonAsync(httpAnalyzeResponse);
        var mcpAnalyzePayload = ReadMcpStructuredJson(mcpAnalyzeResponse);
        Assert.True(JsonElement.DeepEquals(httpAnalyzePayload, mcpAnalyzePayload));
    }

    private async Task<JsonElement> ReadHttpJsonAsync(HttpResponseMessage response)
    {
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        return payload;
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

    private static string GetServerProjectPath()
    {
        var repositoryRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

        return Path.Combine(repositoryRoot, "src", "LotofacilMcp.Server", "LotofacilMcp.Server.csproj");
    }
}
