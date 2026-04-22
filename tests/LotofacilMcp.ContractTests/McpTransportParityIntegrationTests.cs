using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace LotofacilMcp.ContractTests;

public sealed class McpTransportParityIntegrationTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _httpFactory = new();
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    private HttpClient _httpClient = default!;
    private McpClient _stdioMcpClient = default!;
    private McpClient _httpMcpClient = default!;

    public async Task InitializeAsync()
    {
        _httpClient = _httpFactory.CreateClient();
        _stdioMcpClient = await McpClient.CreateAsync(new StdioClientTransport(new StdioClientTransportOptions
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
    public async Task McpDiscoveryAndSuccessfulCalls_MatchHttpJsonSemantics()
    {
        await AssertToolDiscoveryAsync(_stdioMcpClient);
        await AssertToolDiscoveryAsync(_httpMcpClient);

        var getDrawWindowRequest = new
        {
            window_size = 3,
            end_contest_id = 1003
        };

        var httpWindowResponse = await _httpClient.PostAsJsonAsync("/tools/get_draw_window", getDrawWindowRequest);
        Assert.Equal(HttpStatusCode.OK, httpWindowResponse.StatusCode);

        var stdioWindowResponse = await _stdioMcpClient.CallToolAsync("get_draw_window", new Dictionary<string, object?>
        {
            ["window_size"] = 3,
            ["end_contest_id"] = 1003
        });
        var httpMcpWindowResponse = await _httpMcpClient.CallToolAsync("get_draw_window", new Dictionary<string, object?>
        {
            ["window_size"] = 3,
            ["end_contest_id"] = 1003
        });

        var httpWindowPayload = await ReadHttpJsonAsync(httpWindowResponse);
        Assert.True(JsonElement.DeepEquals(httpWindowPayload, ReadMcpStructuredJson(stdioWindowResponse)));
        Assert.True(JsonElement.DeepEquals(httpWindowPayload, ReadMcpStructuredJson(httpMcpWindowResponse)));

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

        var stdioComputeResponse = await _stdioMcpClient.CallToolAsync("compute_window_metrics", new Dictionary<string, object?>
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
        var httpMcpComputeResponse = await _httpMcpClient.CallToolAsync("compute_window_metrics", new Dictionary<string, object?>
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
        Assert.True(JsonElement.DeepEquals(httpComputePayload, ReadMcpStructuredJson(stdioComputeResponse)));
        Assert.True(JsonElement.DeepEquals(httpComputePayload, ReadMcpStructuredJson(httpMcpComputeResponse)));

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

        var stdioAnalyzeResponse = await _stdioMcpClient.CallToolAsync("analyze_indicator_stability", new Dictionary<string, object?>
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
        var httpMcpAnalyzeResponse = await _httpMcpClient.CallToolAsync("analyze_indicator_stability", new Dictionary<string, object?>
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
        Assert.True(JsonElement.DeepEquals(httpAnalyzePayload, ReadMcpStructuredJson(stdioAnalyzeResponse)));
        Assert.True(JsonElement.DeepEquals(httpAnalyzePayload, ReadMcpStructuredJson(httpMcpAnalyzeResponse)));

        var composeRequest = new Dictionary<string, object?>
        {
            ["window_size"] = 5,
            ["end_contest_id"] = 1005,
            ["target"] = "dezena",
            ["operator"] = "weighted_rank",
            ["top_k"] = 10,
            ["components"] = new object[]
            {
                new Dictionary<string, object?>
                {
                    ["metric_name"] = "frequencia_por_dezena",
                    ["transform"] = "normalize_max",
                    ["weight"] = 0.4
                },
                new Dictionary<string, object?>
                {
                    ["metric_name"] = "atraso_por_dezena",
                    ["transform"] = "invert_normalize_max",
                    ["weight"] = 0.3
                },
                new Dictionary<string, object?>
                {
                    ["metric_name"] = "assimetria_blocos",
                    ["transform"] = "shift_scale_unit_interval",
                    ["weight"] = 0.3
                }
            }
        };

        var httpComposeResponse = await _httpClient.PostAsJsonAsync("/tools/compose_indicator_analysis", composeRequest);
        Assert.Equal(HttpStatusCode.OK, httpComposeResponse.StatusCode);

        var stdioComposeResponse = await _stdioMcpClient.CallToolAsync("compose_indicator_analysis", composeRequest);
        var httpMcpComposeResponse = await _httpMcpClient.CallToolAsync("compose_indicator_analysis", composeRequest);

        var httpComposePayload = await ReadHttpJsonAsync(httpComposeResponse);
        Assert.True(JsonElement.DeepEquals(httpComposePayload, ReadMcpStructuredJson(stdioComposeResponse)));
        Assert.True(JsonElement.DeepEquals(httpComposePayload, ReadMcpStructuredJson(httpMcpComposeResponse)));

        var associationRequest = new Dictionary<string, object?>
        {
            ["window_size"] = 5,
            ["end_contest_id"] = 1005,
            ["method"] = "spearman",
            ["top_k"] = 6,
            ["items"] = new object[]
            {
                new Dictionary<string, object?> { ["name"] = "repeticao_concurso_anterior" },
                new Dictionary<string, object?> { ["name"] = "pares_no_concurso" },
                new Dictionary<string, object?> { ["name"] = "quantidade_vizinhos_por_concurso" },
                new Dictionary<string, object?> { ["name"] = "sequencia_maxima_vizinhos_por_concurso" }
            }
        };

        var httpAssociationResponse = await _httpClient.PostAsJsonAsync("/tools/analyze_indicator_associations", associationRequest);
        Assert.Equal(HttpStatusCode.OK, httpAssociationResponse.StatusCode);

        var stdioAssociationResponse = await _stdioMcpClient.CallToolAsync("analyze_indicator_associations", associationRequest);
        var httpMcpAssociationResponse = await _httpMcpClient.CallToolAsync("analyze_indicator_associations", associationRequest);

        var httpAssociationPayload = await ReadHttpJsonAsync(httpAssociationResponse);
        Assert.True(JsonElement.DeepEquals(httpAssociationPayload, ReadMcpStructuredJson(stdioAssociationResponse)));
        Assert.True(JsonElement.DeepEquals(httpAssociationPayload, ReadMcpStructuredJson(httpMcpAssociationResponse)));
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

        var stdioResponse = await _stdioMcpClient.CallToolAsync("compute_window_metrics", new Dictionary<string, object?>
        {
            ["window_size"] = 3,
            ["end_contest_id"] = 1003
        });
        var httpMcpResponse = await _httpMcpClient.CallToolAsync("compute_window_metrics", new Dictionary<string, object?>
        {
            ["window_size"] = 3,
            ["end_contest_id"] = 1003
        });

        Assert.True(stdioResponse.IsError);
        Assert.True(httpMcpResponse.IsError);

        var httpPayload = await ReadHttpJsonAsync(httpResponse);
        Assert.True(JsonElement.DeepEquals(httpPayload, ReadMcpStructuredJson(stdioResponse)));
        Assert.True(JsonElement.DeepEquals(httpPayload, ReadMcpStructuredJson(httpMcpResponse)));

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

        var stdioAnalyzeResponse = await _stdioMcpClient.CallToolAsync("analyze_indicator_stability", new Dictionary<string, object?>
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
        var httpMcpAnalyzeResponse = await _httpMcpClient.CallToolAsync("analyze_indicator_stability", new Dictionary<string, object?>
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

        Assert.True(stdioAnalyzeResponse.IsError);
        Assert.True(httpMcpAnalyzeResponse.IsError);

        var httpAnalyzePayload = await ReadHttpJsonAsync(httpAnalyzeResponse);
        Assert.True(JsonElement.DeepEquals(httpAnalyzePayload, ReadMcpStructuredJson(stdioAnalyzeResponse)));
        Assert.True(JsonElement.DeepEquals(httpAnalyzePayload, ReadMcpStructuredJson(httpMcpAnalyzeResponse)));

        var invalidAssociationRequest = new Dictionary<string, object?>
        {
            ["window_size"] = 5,
            ["end_contest_id"] = 1005,
            ["method"] = "spearman",
            ["top_k"] = 3,
            ["items"] = new object[]
            {
                new Dictionary<string, object?> { ["name"] = "repeticao_concurso_anterior" },
                new Dictionary<string, object?> { ["name"] = "distribuicao_linha_por_concurso" }
            }
        };

        var httpAssocErr = await _httpClient.PostAsJsonAsync("/tools/analyze_indicator_associations", invalidAssociationRequest);
        Assert.Equal(HttpStatusCode.BadRequest, httpAssocErr.StatusCode);

        var stdioAssocErr = await _stdioMcpClient.CallToolAsync("analyze_indicator_associations", invalidAssociationRequest);
        var httpMcpAssocErr = await _httpMcpClient.CallToolAsync("analyze_indicator_associations", invalidAssociationRequest);

        Assert.True(stdioAssocErr.IsError);
        Assert.True(httpMcpAssocErr.IsError);

        var httpAssocPayload = await ReadHttpJsonAsync(httpAssocErr);
        Assert.True(JsonElement.DeepEquals(httpAssocPayload, ReadMcpStructuredJson(stdioAssocErr)));
        Assert.True(JsonElement.DeepEquals(httpAssocPayload, ReadMcpStructuredJson(httpMcpAssocErr)));
    }

    private static async Task AssertToolDiscoveryAsync(McpClient client)
    {
        var listedTools = await client.ListToolsAsync();
        Assert.Contains(listedTools, tool => tool.Name is "get_draw_window");
        Assert.Contains(listedTools, tool => tool.Name is "compute_window_metrics");
        Assert.Contains(listedTools, tool => tool.Name is "analyze_indicator_stability");
        Assert.Contains(listedTools, tool => tool.Name is "compose_indicator_analysis");
        Assert.Contains(listedTools, tool => tool.Name is "analyze_indicator_associations");
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
