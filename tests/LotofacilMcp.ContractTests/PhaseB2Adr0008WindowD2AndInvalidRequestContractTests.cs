using System.Net;
using System.Text;
using System.Text.Json;
using LotofacilMcp.Server.Tools;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace LotofacilMcp.ContractTests;

/// <summary>
/// Fase B.2 / ADR 0008 critério 3: equivalência de recorte D2, recusa de combinação ambígua,
/// paridade HTTP (REST) e MCP (stdio e HTTP /mcp) conforme mcp-tool-contract (Window, erros).
/// </summary>
public sealed class PhaseB2Adr0008WindowD2AndInvalidRequestContractTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _httpFactory = new();
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    private HttpClient _httpClient = null!;
    private McpClient _stdioMcpClient = null!;
    private McpClient _httpMcpClient = null!;

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
                "-c",
#if DEBUG
                "Debug",
#else
                "Release",
#endif
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
    public async Task D2_StartEndInclusives_EquivalentToWindowSizeAndEnd_ForFrequenciaAndTop10_OnSyntheticFixture()
    {
        const int startContestId = 1001;
        const int endContestId = 1003;
        const int windowSize = endContestId - startContestId + 1;

        const string metrics = "[{\"name\":\"frequencia_por_dezena\"},{\"name\":\"top10_mais_sorteados\"}]";
        var byWindowAndEnd = $"{{\"window_size\":{windowSize},\"end_contest_id\":{endContestId},\"metrics\":{metrics}}}";
        var byStartAndEnd =
            $"{{\"start_contest_id\":{startContestId},\"end_contest_id\":{endContestId},\"metrics\":{metrics}}}";

        var httpBaseline = await _httpClient.PostAsync(
            "/tools/compute_window_metrics",
            new StringContent(byWindowAndEnd, Encoding.UTF8, "application/json"));
        var httpD2 = await _httpClient.PostAsync(
            "/tools/compute_window_metrics",
            new StringContent(byStartAndEnd, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, httpBaseline.StatusCode);
        Assert.Equal(HttpStatusCode.OK, httpD2.StatusCode);

        var stdioBase = await _stdioMcpClient.CallToolAsync(
            "compute_window_metrics",
            McpArgsWindowSizeAndEnd(windowSize, endContestId));
        var stdioD2 = await _stdioMcpClient.CallToolAsync("compute_window_metrics", McpArgsStartAndEndOnly(startContestId, endContestId));
        var httpMcpBase = await _httpMcpClient.CallToolAsync(
            "compute_window_metrics",
            McpArgsWindowSizeAndEnd(windowSize, endContestId));
        var httpMcpD2 = await _httpMcpClient.CallToolAsync("compute_window_metrics", McpArgsStartAndEndOnly(startContestId, endContestId));

        var baselineElement = await ReadHttpJsonElementAsync(httpBaseline);
        var d2Element = await ReadHttpJsonElementAsync(httpD2);
        Assert.True(
            JsonElement.DeepEquals(baselineElement, d2Element),
            "D2: compute_window_metrics (synthetic) — baseline e forma start+end+inclusivos idênticos (HTTP).");

        Assert.True(JsonElement.DeepEquals(baselineElement, ReadMcpStructuredJson(stdioBase)));
        Assert.True(JsonElement.DeepEquals(baselineElement, ReadMcpStructuredJson(httpMcpBase)));
        Assert.True(
            JsonElement.DeepEquals(baselineElement, ReadMcpStructuredJson(stdioD2)),
            "D2: MCP stdio (start+end) alinhado ao HTTP baseline.");
        Assert.True(
            JsonElement.DeepEquals(baselineElement, ReadMcpStructuredJson(httpMcpD2)),
            "D2: MCP HTTP (start+end) alinhado ao HTTP baseline.");
    }

    [Fact]
    public async Task D2_EquivalentForms_GetDrawWindow_ProduceIdenticalJson()
    {
        const int startContestId = 1001;
        const int endContestId = 1003;
        const int windowSize = endContestId - startContestId + 1;

        var byWindowAndEnd = $"{{\"window_size\":{windowSize},\"end_contest_id\":{endContestId}}}";
        var byStartAndEnd = $"{{\"start_contest_id\":{startContestId},\"end_contest_id\":{endContestId}}}";

        var httpBaseline = await _httpClient.PostAsync(
            "/tools/get_draw_window",
            new StringContent(byWindowAndEnd, Encoding.UTF8, "application/json"));
        var httpD2 = await _httpClient.PostAsync(
            "/tools/get_draw_window",
            new StringContent(byStartAndEnd, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, httpBaseline.StatusCode);
        Assert.Equal(HttpStatusCode.OK, httpD2.StatusCode);

        var baseEl = await ReadHttpJsonElementAsync(httpBaseline);
        var d2El = await ReadHttpJsonElementAsync(httpD2);
        Assert.True(JsonElement.DeepEquals(baseEl, d2El), "D2: get_draw_window — duas formas (HTTP).");

        var stdioBase = await _stdioMcpClient.CallToolAsync(
            "get_draw_window",
            new Dictionary<string, object?> { ["window_size"] = windowSize, ["end_contest_id"] = endContestId });
        var stdioD2 = await _stdioMcpClient.CallToolAsync(
            "get_draw_window",
            new Dictionary<string, object?>
            {
                ["start_contest_id"] = startContestId,
                ["end_contest_id"] = endContestId
            });
        var httpMcpBase = await _httpMcpClient.CallToolAsync(
            "get_draw_window",
            new Dictionary<string, object?> { ["window_size"] = windowSize, ["end_contest_id"] = endContestId });
        var httpMcpD2 = await _httpMcpClient.CallToolAsync(
            "get_draw_window",
            new Dictionary<string, object?>
            {
                ["start_contest_id"] = startContestId,
                ["end_contest_id"] = endContestId
            });

        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(stdioBase)));
        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(httpMcpBase)));
        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(stdioD2)));
        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(httpMcpD2)));
    }

    [Fact]
    public async Task D2_EquivalentForms_AnalyzeIndicatorStability_ProduceIdenticalJson()
    {
        const int startContestId = 1001;
        const int endContestId = 1003;
        const int windowSize = endContestId - startContestId + 1;

        const string indicators = "[{\"name\":\"pares_no_concurso\",\"aggregation\":\"identity\"}]";
        var byWindowAndEnd = $"{{\"window_size\":{windowSize},\"end_contest_id\":{endContestId},\"min_history\":{windowSize},\"indicators\":{indicators}}}";
        var byStartAndEnd = $"{{\"start_contest_id\":{startContestId},\"end_contest_id\":{endContestId},\"min_history\":{windowSize},\"indicators\":{indicators}}}";

        var httpBaseline = await _httpClient.PostAsync(
            "/tools/analyze_indicator_stability",
            new StringContent(byWindowAndEnd, Encoding.UTF8, "application/json"));
        var httpD2 = await _httpClient.PostAsync(
            "/tools/analyze_indicator_stability",
            new StringContent(byStartAndEnd, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, httpBaseline.StatusCode);
        Assert.Equal(HttpStatusCode.OK, httpD2.StatusCode);

        var baseEl = await ReadHttpJsonElementAsync(httpBaseline);
        var d2El = await ReadHttpJsonElementAsync(httpD2);
        Assert.True(JsonElement.DeepEquals(baseEl, d2El));

        var argsBase = new Dictionary<string, object?>
        {
            ["window_size"] = windowSize,
            ["end_contest_id"] = endContestId,
            ["min_history"] = windowSize,
            ["indicators"] = new object[]
            {
                new Dictionary<string, object?> { ["name"] = "pares_no_concurso", ["aggregation"] = "identity" }
            }
        };
        var argsD2 = new Dictionary<string, object?>
        {
            ["start_contest_id"] = startContestId,
            ["end_contest_id"] = endContestId,
            ["min_history"] = windowSize,
            ["indicators"] = new object[]
            {
                new Dictionary<string, object?> { ["name"] = "pares_no_concurso", ["aggregation"] = "identity" }
            }
        };
        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(await _stdioMcpClient.CallToolAsync("analyze_indicator_stability", argsBase))));
        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(await _httpMcpClient.CallToolAsync("analyze_indicator_stability", argsBase))));
        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(await _stdioMcpClient.CallToolAsync("analyze_indicator_stability", argsD2))));
        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(await _httpMcpClient.CallToolAsync("analyze_indicator_stability", argsD2))));
    }

    [Fact]
    public async Task D2_EquivalentForms_ComposeIndicatorAnalysis_ProduceIdenticalJson()
    {
        const int startContestId = 1001;
        const int endContestId = 1003;
        const int windowSize = endContestId - startContestId + 1;

        const string components = "[{\"metric_name\":\"frequencia_por_dezena\",\"transform\":\"normalize_max\",\"weight\":1.0}]";
        var byWindowAndEnd =
            $"{{\"window_size\":{windowSize},\"end_contest_id\":{endContestId},\"target\":\"dezena\",\"operator\":\"weighted_rank\",\"components\":{components},\"top_k\":10}}";
        var byStartAndEnd =
            $"{{\"start_contest_id\":{startContestId},\"end_contest_id\":{endContestId},\"target\":\"dezena\",\"operator\":\"weighted_rank\",\"components\":{components},\"top_k\":10}}";

        var httpBaseline = await _httpClient.PostAsync(
            "/tools/compose_indicator_analysis",
            new StringContent(byWindowAndEnd, Encoding.UTF8, "application/json"));
        var httpD2 = await _httpClient.PostAsync(
            "/tools/compose_indicator_analysis",
            new StringContent(byStartAndEnd, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, httpBaseline.StatusCode);
        Assert.Equal(HttpStatusCode.OK, httpD2.StatusCode);

        var baseEl = await ReadHttpJsonElementAsync(httpBaseline);
        var d2El = await ReadHttpJsonElementAsync(httpD2);
        Assert.True(JsonElement.DeepEquals(baseEl, d2El));

        var baseArgs = new Dictionary<string, object?>
        {
            ["window_size"] = windowSize,
            ["end_contest_id"] = endContestId,
            ["target"] = "dezena",
            ["operator"] = "weighted_rank",
            ["top_k"] = 10,
            ["components"] = new object[]
            {
                new Dictionary<string, object?>
                {
                    ["metric_name"] = "frequencia_por_dezena",
                    ["transform"] = "normalize_max",
                    ["weight"] = 1.0
                }
            }
        };
        var d2Args = new Dictionary<string, object?>(baseArgs)
        {
            ["window_size"] = null,
            ["start_contest_id"] = startContestId
        };
        d2Args.Remove("window_size");

        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(await _stdioMcpClient.CallToolAsync("compose_indicator_analysis", baseArgs))));
        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(await _httpMcpClient.CallToolAsync("compose_indicator_analysis", baseArgs))));
        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(await _stdioMcpClient.CallToolAsync("compose_indicator_analysis", d2Args))));
        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(await _httpMcpClient.CallToolAsync("compose_indicator_analysis", d2Args))));
    }

    [Fact]
    public async Task D2_EquivalentForms_AnalyzeIndicatorAssociations_ProduceIdenticalJson()
    {
        const int startContestId = 1001;
        const int endContestId = 1003;
        const int windowSize = endContestId - startContestId + 1;

        const string items = "[{\"name\":\"pares_no_concurso\",\"aggregation\":\"identity\"},{\"name\":\"repeticao_concurso_anterior\",\"aggregation\":\"identity\"}]";
        var byWindowAndEnd =
            $"{{\"window_size\":{windowSize},\"end_contest_id\":{endContestId},\"method\":\"spearman\",\"top_k\":5,\"items\":{items}}}";
        var byStartAndEnd =
            $"{{\"start_contest_id\":{startContestId},\"end_contest_id\":{endContestId},\"method\":\"spearman\",\"top_k\":5,\"items\":{items}}}";

        var httpBaseline = await _httpClient.PostAsync(
            "/tools/analyze_indicator_associations",
            new StringContent(byWindowAndEnd, Encoding.UTF8, "application/json"));
        var httpD2 = await _httpClient.PostAsync(
            "/tools/analyze_indicator_associations",
            new StringContent(byStartAndEnd, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, httpBaseline.StatusCode);
        Assert.Equal(HttpStatusCode.OK, httpD2.StatusCode);

        var baseEl = await ReadHttpJsonElementAsync(httpBaseline);
        var d2El = await ReadHttpJsonElementAsync(httpD2);
        Assert.True(JsonElement.DeepEquals(baseEl, d2El));

        var baseArgs = new Dictionary<string, object?>
        {
            ["window_size"] = windowSize,
            ["end_contest_id"] = endContestId,
            ["method"] = "spearman",
            ["top_k"] = 5,
            ["items"] = new object[]
            {
                new Dictionary<string, object?> { ["name"] = "pares_no_concurso", ["aggregation"] = "identity" },
                new Dictionary<string, object?> { ["name"] = "repeticao_concurso_anterior", ["aggregation"] = "identity" }
            }
        };
        var d2Args = new Dictionary<string, object?>
        {
            ["start_contest_id"] = startContestId,
            ["end_contest_id"] = endContestId,
            ["method"] = "spearman",
            ["top_k"] = 5,
            ["items"] = new object[]
            {
                new Dictionary<string, object?> { ["name"] = "pares_no_concurso", ["aggregation"] = "identity" },
                new Dictionary<string, object?> { ["name"] = "repeticao_concurso_anterior", ["aggregation"] = "identity" }
            }
        };
        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(await _stdioMcpClient.CallToolAsync("analyze_indicator_associations", baseArgs))));
        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(await _httpMcpClient.CallToolAsync("analyze_indicator_associations", baseArgs))));
        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(await _stdioMcpClient.CallToolAsync("analyze_indicator_associations", d2Args))));
        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(await _httpMcpClient.CallToolAsync("analyze_indicator_associations", d2Args))));
    }

    [Fact]
    public async Task D2_EquivalentForms_SummarizeWindowPatterns_ProduceIdenticalJson()
    {
        const int startContestId = 1001;
        const int endContestId = 1003;
        const int windowSize = endContestId - startContestId + 1;

        const string features = "[{\"metric_name\":\"pares_no_concurso\",\"aggregation\":\"identity\"}]";
        var byWindowAndEnd =
            $"{{\"window_size\":{windowSize},\"end_contest_id\":{endContestId},\"coverage_threshold\":0.8,\"range_method\":\"iqr\",\"features\":{features}}}";
        var byStartAndEnd =
            $"{{\"start_contest_id\":{startContestId},\"end_contest_id\":{endContestId},\"coverage_threshold\":0.8,\"range_method\":\"iqr\",\"features\":{features}}}";

        var httpBaseline = await _httpClient.PostAsync(
            "/tools/summarize_window_patterns",
            new StringContent(byWindowAndEnd, Encoding.UTF8, "application/json"));
        var httpD2 = await _httpClient.PostAsync(
            "/tools/summarize_window_patterns",
            new StringContent(byStartAndEnd, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, httpBaseline.StatusCode);
        Assert.Equal(HttpStatusCode.OK, httpD2.StatusCode);

        var baseEl = await ReadHttpJsonElementAsync(httpBaseline);
        var d2El = await ReadHttpJsonElementAsync(httpD2);
        Assert.True(JsonElement.DeepEquals(baseEl, d2El));

        var baseArgs = new Dictionary<string, object?>
        {
            ["window_size"] = windowSize,
            ["end_contest_id"] = endContestId,
            ["coverage_threshold"] = 0.8,
            ["range_method"] = "iqr",
            ["features"] = new object[]
            {
                new Dictionary<string, object?> { ["metric_name"] = "pares_no_concurso", ["aggregation"] = "identity" }
            }
        };
        var d2Args = new Dictionary<string, object?>
        {
            ["start_contest_id"] = startContestId,
            ["end_contest_id"] = endContestId,
            ["coverage_threshold"] = 0.8,
            ["range_method"] = "iqr",
            ["features"] = new object[]
            {
                new Dictionary<string, object?> { ["metric_name"] = "pares_no_concurso", ["aggregation"] = "identity" }
            }
        };
        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(await _stdioMcpClient.CallToolAsync("summarize_window_patterns", baseArgs))));
        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(await _httpMcpClient.CallToolAsync("summarize_window_patterns", baseArgs))));
        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(await _stdioMcpClient.CallToolAsync("summarize_window_patterns", d2Args))));
        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(await _httpMcpClient.CallToolAsync("summarize_window_patterns", d2Args))));
    }

    [Fact]
    public async Task D2_EquivalentForms_SummarizeWindowAggregates_ProduceIdenticalJson()
    {
        const int startContestId = 1001;
        const int endContestId = 1003;
        const int windowSize = endContestId - startContestId + 1;

        const string aggregates =
            """
            [
              {
                "id": "pairs_hist",
                "source_metric_name": "pares_no_concurso",
                "aggregate_type": "histogram_scalar_series",
                "params": { "bucket_spec": { "bucket_values": [0,1,2,3,4,5] } }
              }
            ]
            """;
        var byWindowAndEnd = $"{{\"window_size\":{windowSize},\"end_contest_id\":{endContestId},\"aggregates\":{aggregates}}}";
        var byStartAndEnd = $"{{\"start_contest_id\":{startContestId},\"end_contest_id\":{endContestId},\"aggregates\":{aggregates}}}";

        var httpBaseline = await _httpClient.PostAsync(
            "/tools/summarize_window_aggregates",
            new StringContent(byWindowAndEnd, Encoding.UTF8, "application/json"));
        var httpD2 = await _httpClient.PostAsync(
            "/tools/summarize_window_aggregates",
            new StringContent(byStartAndEnd, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, httpBaseline.StatusCode);
        Assert.Equal(HttpStatusCode.OK, httpD2.StatusCode);

        var baseEl = await ReadHttpJsonElementAsync(httpBaseline);
        var d2El = await ReadHttpJsonElementAsync(httpD2);
        Assert.True(JsonElement.DeepEquals(baseEl, d2El));

        var baseArgs = new Dictionary<string, object?>
        {
            ["window_size"] = windowSize,
            ["end_contest_id"] = endContestId,
            ["aggregates"] = new object[]
            {
                new Dictionary<string, object?>
                {
                    ["id"] = "pairs_hist",
                    ["source_metric_name"] = "pares_no_concurso",
                    ["aggregate_type"] = "histogram_scalar_series",
                    ["params"] = new Dictionary<string, object?>
                    {
                        ["bucket_spec"] = new Dictionary<string, object?>
                        {
                            ["bucket_values"] = new[] { 0, 1, 2, 3, 4, 5 }
                        }
                    }
                }
            }
        };
        var d2Args = new Dictionary<string, object?>
        {
            ["start_contest_id"] = startContestId,
            ["end_contest_id"] = endContestId,
            ["aggregates"] = baseArgs["aggregates"]
        };
        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(await _stdioMcpClient.CallToolAsync("summarize_window_aggregates", baseArgs))));
        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(await _httpMcpClient.CallToolAsync("summarize_window_aggregates", baseArgs))));
        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(await _stdioMcpClient.CallToolAsync("summarize_window_aggregates", d2Args))));
        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(await _httpMcpClient.CallToolAsync("summarize_window_aggregates", d2Args))));
    }

    [Fact]
    public async Task D2_EquivalentForms_GenerateCandidateGames_ProduceIdenticalJson()
    {
        const int startContestId = 1001;
        const int endContestId = 1003;
        const int windowSize = endContestId - startContestId + 1;

        const string plan = "[{\"strategy_name\":\"common_repetition_frequency\",\"count\":1,\"search_method\":\"greedy_topk\"}]";
        var byWindowAndEnd = $"{{\"window_size\":{windowSize},\"end_contest_id\":{endContestId},\"seed\":424242,\"plan\":{plan}}}";
        var byStartAndEnd = $"{{\"start_contest_id\":{startContestId},\"end_contest_id\":{endContestId},\"seed\":424242,\"plan\":{plan}}}";

        var httpBaseline = await _httpClient.PostAsync(
            "/tools/generate_candidate_games",
            new StringContent(byWindowAndEnd, Encoding.UTF8, "application/json"));
        var httpD2 = await _httpClient.PostAsync(
            "/tools/generate_candidate_games",
            new StringContent(byStartAndEnd, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, httpBaseline.StatusCode);
        Assert.Equal(HttpStatusCode.OK, httpD2.StatusCode);

        var baseEl = await ReadHttpJsonElementAsync(httpBaseline);
        var d2El = await ReadHttpJsonElementAsync(httpD2);
        Assert.True(JsonElement.DeepEquals(baseEl, d2El));

        var baseArgs = new Dictionary<string, object?>
        {
            ["window_size"] = windowSize,
            ["end_contest_id"] = endContestId,
            ["seed"] = 424242UL,
            ["plan"] = new object[]
            {
                new Dictionary<string, object?>
                {
                    ["strategy_name"] = "common_repetition_frequency",
                    ["count"] = 1,
                    ["search_method"] = "greedy_topk"
                }
            }
        };
        var d2Args = new Dictionary<string, object?>
        {
            ["start_contest_id"] = startContestId,
            ["end_contest_id"] = endContestId,
            ["seed"] = 424242UL,
            ["plan"] = baseArgs["plan"]
        };
        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(await _stdioMcpClient.CallToolAsync("generate_candidate_games", baseArgs))));
        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(await _httpMcpClient.CallToolAsync("generate_candidate_games", baseArgs))));
        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(await _stdioMcpClient.CallToolAsync("generate_candidate_games", d2Args))));
        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(await _httpMcpClient.CallToolAsync("generate_candidate_games", d2Args))));
    }

    [Fact]
    public async Task D2_EquivalentForms_ExplainCandidateGames_ProduceIdenticalJson()
    {
        const int startContestId = 1001;
        const int endContestId = 1003;
        const int windowSize = endContestId - startContestId + 1;

        const string games = "[[1,2,3,4,5,6,7,8,9,10,11,12,13,14,15]]";
        var byWindowAndEnd = $"{{\"window_size\":{windowSize},\"end_contest_id\":{endContestId},\"games\":{games}}}";
        var byStartAndEnd = $"{{\"start_contest_id\":{startContestId},\"end_contest_id\":{endContestId},\"games\":{games}}}";

        var httpBaseline = await _httpClient.PostAsync(
            "/tools/explain_candidate_games",
            new StringContent(byWindowAndEnd, Encoding.UTF8, "application/json"));
        var httpD2 = await _httpClient.PostAsync(
            "/tools/explain_candidate_games",
            new StringContent(byStartAndEnd, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, httpBaseline.StatusCode);
        Assert.Equal(HttpStatusCode.OK, httpD2.StatusCode);

        var baseEl = await ReadHttpJsonElementAsync(httpBaseline);
        var d2El = await ReadHttpJsonElementAsync(httpD2);
        Assert.True(JsonElement.DeepEquals(baseEl, d2El));

        var baseArgs = new Dictionary<string, object?>
        {
            ["window_size"] = windowSize,
            ["end_contest_id"] = endContestId,
            ["games"] = new object[]
            {
                new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }
            }
        };
        var d2Args = new Dictionary<string, object?>
        {
            ["start_contest_id"] = startContestId,
            ["end_contest_id"] = endContestId,
            ["games"] = baseArgs["games"]
        };
        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(await _stdioMcpClient.CallToolAsync("explain_candidate_games", baseArgs))));
        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(await _httpMcpClient.CallToolAsync("explain_candidate_games", baseArgs))));
        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(await _stdioMcpClient.CallToolAsync("explain_candidate_games", d2Args))));
        Assert.True(JsonElement.DeepEquals(baseEl, ReadMcpStructuredJson(await _httpMcpClient.CallToolAsync("explain_candidate_games", d2Args))));
    }

    [Fact]
    public async Task AmbiguousWindow_IncompatibleSizeAndStartEnd_RejectedWithInvalidRequest_OnComputeAndGet_AcrossTransports()
    {
        // 1001..1003 = 3 concursos; window_size=2 conflitua.
        const int windowSize = 2;
        const int startContestId = 1001;
        const int endContestId = 1003;

        var shared = $"{{\"window_size\":{windowSize},\"start_contest_id\":{startContestId},\"end_contest_id\":{endContestId}}}";
        var errComputeBody = shared.TrimEnd('}')
            + @", ""metrics"":[{""name"":""frequencia_por_dezena""},{""name"":""top10_mais_sorteados""}]" + "}";

        var httpCompute = await _httpClient.PostAsync(
            "/tools/compute_window_metrics",
            new StringContent(errComputeBody, Encoding.UTF8, "application/json"));
        var httpGet = await _httpClient.PostAsync(
            "/tools/get_draw_window",
            new StringContent(shared, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, httpCompute.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, httpGet.StatusCode);

        var hErrCompute = await ReadHttpJsonElementAsync(httpCompute);
        var hErrGet = await ReadHttpJsonElementAsync(httpGet);
        AssertErrorCodeIsInvalidRequest(hErrCompute);
        AssertErrorCodeIsInvalidRequest(hErrGet);

        var mcpArgsCompute = new Dictionary<string, object?>
        {
            ["window_size"] = windowSize,
            ["start_contest_id"] = startContestId,
            ["end_contest_id"] = endContestId,
            ["metrics"] = new object[]
            {
                new Dictionary<string, object?> { ["name"] = "frequencia_por_dezena" },
                new Dictionary<string, object?> { ["name"] = "top10_mais_sorteados" }
            }
        };
        var mcpArgsGet = new Dictionary<string, object?>
        {
            ["window_size"] = windowSize,
            ["start_contest_id"] = startContestId,
            ["end_contest_id"] = endContestId
        };
        var stdioC = await _stdioMcpClient.CallToolAsync("compute_window_metrics", mcpArgsCompute);
        var httpMcpC = await _httpMcpClient.CallToolAsync("compute_window_metrics", mcpArgsCompute);
        var stdioG = await _stdioMcpClient.CallToolAsync("get_draw_window", mcpArgsGet);
        var httpMcpG = await _httpMcpClient.CallToolAsync("get_draw_window", mcpArgsGet);
        AssertParityError(hErrCompute, stdioC);
        AssertParityError(hErrCompute, httpMcpC);
        AssertParityError(hErrGet, stdioG);
        AssertParityError(hErrGet, httpMcpG);
    }

    [Fact]
    public async Task StartAfterEnd_RejectedOnCompute_AsyncPolicy_InvalidRequestOrInvalidContestId_Parity()
    {
        // start_contest_id &gt; end_contest_id, sem window_size, para forçar rejeição sem a rota a ignorar extremos
        // (com window_size+extremos conflitantes, ver teste de ambiguidade).
        // Pós-ADR 0008: INVALID_REQUEST ou INVALID_CONTEST_ID (mcp-tool-contract, entidade Window).
        // Pré resolução D2 no servidor: corpo sem window_size ainda cai em INVALID_WINDOW_SIZE; não pode ser 200.
        const string body =
            """
            {
              "start_contest_id": 1003,
              "end_contest_id": 1001,
              "metrics": [ { "name": "frequencia_por_dezena" } ]
            }
            """;

        var httpR = await _httpClient.PostAsync(
            "/tools/compute_window_metrics",
            new StringContent(body, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, httpR.StatusCode);
        var httpJson = await ReadHttpJsonElementAsync(httpR);
        AssertErrorCodeInSet(
            httpJson,
            "INVALID_REQUEST",
            "INVALID_CONTEST_ID",
            "INVALID_WINDOW_SIZE");

        var args = new Dictionary<string, object?>
        {
            ["start_contest_id"] = 1003,
            ["end_contest_id"] = 1001,
            ["metrics"] = new object[] { new Dictionary<string, object?> { ["name"] = "frequencia_por_dezena" } }
        };
        var s = await _stdioMcpClient.CallToolAsync("compute_window_metrics", args);
        var h = await _httpMcpClient.CallToolAsync("compute_window_metrics", args);
        AssertParityErrorWithCodeInSet(
            httpJson, s, "INVALID_REQUEST", "INVALID_CONTEST_ID", "INVALID_WINDOW_SIZE");
        AssertParityErrorWithCodeInSet(
            httpJson, h, "INVALID_REQUEST", "INVALID_CONTEST_ID", "INVALID_WINDOW_SIZE");
    }

    private static void AssertErrorCodeIsInvalidRequest(JsonElement errorEnvelope) =>
        Assert.Equal("INVALID_REQUEST", errorEnvelope.GetProperty("error").GetProperty("code").GetString());

    private static void AssertErrorCodeInSet(JsonElement errorEnvelope, params string[] allowed)
    {
        var code = errorEnvelope.GetProperty("error").GetProperty("code").GetString()!;
        Assert.NotNull(allowed);
        Assert.True(
            Array.Exists(allowed, a => string.Equals(a, code, StringComparison.Ordinal)),
            $"Expected error code in [{string.Join(", ", allowed)}], was {code}.");
    }

    private static void AssertParityError(JsonElement httpError, CallToolResult mcpResult)
    {
        Assert.True(mcpResult.IsError);
        Assert.True(JsonElement.DeepEquals(httpError, ReadMcpStructuredJson(mcpResult)));
    }

    private static void AssertParityErrorWithCodeInSet(
        JsonElement httpError,
        CallToolResult mcpResult,
        params string[] allowed)
    {
        Assert.True(mcpResult.IsError);
        var mcpJson = ReadMcpStructuredJson(mcpResult);
        var httpCode = httpError.GetProperty("error").GetProperty("code").GetString()!;
        var mcpCode = mcpJson.GetProperty("error").GetProperty("code").GetString()!;
        Assert.Equal(httpCode, mcpCode);
        Assert.True(
            Array.Exists(allowed, a => string.Equals(a, httpCode, StringComparison.Ordinal)),
            $"code {httpCode} not in set.");
        Assert.True(JsonElement.DeepEquals(httpError, mcpJson));
    }

    private async Task<JsonElement> ReadHttpJsonElementAsync(HttpResponseMessage response) =>
        JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(), _jsonOptions);

    private static Dictionary<string, object?> McpArgsWindowSizeAndEnd(int windowSize, int endContestId) =>
        new()
        {
            ["window_size"] = windowSize,
            ["end_contest_id"] = endContestId,
            ["metrics"] = new object[]
            {
                new Dictionary<string, object?> { ["name"] = "frequencia_por_dezena" },
                new Dictionary<string, object?> { ["name"] = "top10_mais_sorteados" }
            }
        };

    private static Dictionary<string, object?> McpArgsStartAndEndOnly(int startContestId, int endContestId) =>
        new()
        {
            ["start_contest_id"] = startContestId,
            ["end_contest_id"] = endContestId,
            ["metrics"] = new object[]
            {
                new Dictionary<string, object?> { ["name"] = "frequencia_por_dezena" },
                new Dictionary<string, object?> { ["name"] = "top10_mais_sorteados" }
            }
        };

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

/// <summary>
/// top10_mais_sorteados@1.0.0 (Tabela 2 metric-catalog) com fixture de empate;
/// alinhado ao in-process (V0Tools) e, quando configurado, à mesma configuração que o host de integração.
/// </summary>
public sealed class PhaseB2Adr0008TieHeavyTop10ContractTests
{
    [Fact]
    public void Top10MaisSorteados_StableUnderFrequencyTies_MatchesTable2Golden_TieHeavyFixture()
    {
        var tieHeavyPath = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..", "..", "tests", "fixtures", "tie_heavy.json"));
        var sut = new V0Tools(tieHeavyPath);

        var request = new ComputeWindowMetricsRequest(
            WindowSize: 5,
            EndContestId: 5005,
            Metrics: [new MetricRequest("top10_mais_sorteados")],
            AllowPending: false);

        var response = sut.ComputeWindowMetrics(request);
        var payload = Assert.IsType<ComputeWindowMetricsResponse>(response);
        var top10 = Assert.Single(payload.Metrics, m => m.MetricName == "top10_mais_sorteados");
        Assert.Equal("1.0.0", top10.Version);
        Assert.Equal("window", top10.Scope);
        Assert.Equal("dezena_list[10]", top10.Shape);
        Assert.Equal("dimensionless", top10.Unit);

        var golden = LoadTop10Golden();
        var expected = golden.ExpectedDezenaTop10.Select(Convert.ToDouble).ToArray();
        Assert.Equal(expected, top10.Value);
    }

    [Fact]
    public async Task TieHeavy_Top10MaisSorteados_Integration_Parity_Http_AndMcp()
    {
        var repositoryRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var tieHeavy = Path.Combine(repositoryRoot, "tests", "fixtures", "tie_heavy.json");

        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(
                    new Dictionary<string, string?> { ["V0Data:FixturePath"] = tieHeavy });
            });
        });

        using var http = factory.CreateClient();
        var golden = LoadTop10Golden();
        var expectedTop10 = golden.ExpectedDezenaTop10;

        var body =
            """
            { "window_size": 5, "end_contest_id": 5005, "metrics": [ { "name": "top10_mais_sorteados" } ] }
            """;
        var httpR = await http.PostAsync(
            "/tools/compute_window_metrics",
            new StringContent(body, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, httpR.StatusCode);
        var httpJson = await JsonDocument.ParseAsync(await httpR.Content.ReadAsStreamAsync());
        var firstMetric = httpJson.RootElement.GetProperty("metrics")[0];
        var httpValues = firstMetric.GetProperty("value").EnumerateArray().Select(e => (int)e.GetDouble()).ToArray();
        Assert.Equal(expectedTop10, httpValues);
        Assert.Equal("1.0.0", firstMetric.GetProperty("version").GetString());
        Assert.Equal("top10_mais_sorteados", firstMetric.GetProperty("metric_name").GetString());

        var mcp = await McpClient.CreateAsync(new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Name = "LotofacilMcp.Server.TieHeavy",
                Endpoint = new Uri(http.BaseAddress!, "mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            http,
            NullLoggerFactory.Instance,
            ownsHttpClient: false));
        try
        {
            var mcpResult = await mcp.CallToolAsync(
                "compute_window_metrics",
                new Dictionary<string, object?>
                {
                    ["window_size"] = 5,
                    ["end_contest_id"] = 5005,
                    ["metrics"] = new object[] { new Dictionary<string, object?> { ["name"] = "top10_mais_sorteados" } }
                });
            var mcpEl = ReadMcpResultJson(mcpResult);
            var mcpVal = mcpEl
                .GetProperty("metrics")[0]
                .GetProperty("value")
                .EnumerateArray()
                .Select(e => (int)e.GetDouble())
                .ToArray();
            Assert.Equal(expectedTop10, mcpVal);
            Assert.True(
                JsonElement.DeepEquals(
                    httpJson.RootElement,
                    mcpEl),
                "Tie heavy: resposta canónica de compute_window_metrics (HTTP) == MCP (streamable HTTP) para a mesma janela.");
        }
        finally
        {
            await mcp.DisposeAsync();
        }
    }

    private static Top10MaisSorteadosGolden LoadTop10Golden()
    {
        var goldenPath = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..", "..", "tests", "fixtures", "golden", "phaseB2", "tie-heavy-top10-mais-sorteados.golden.json"));
        using var doc = JsonDocument.Parse(File.ReadAllText(goldenPath));
        var r = doc.RootElement;
        var arr = r.GetProperty("expected_dezena_top10");
        return new Top10MaisSorteadosGolden(ExpectedDezenaTop10: arr.EnumerateArray().Select(e => e.GetInt32()).ToArray());
    }

    private static JsonElement ReadMcpResultJson(CallToolResult response)
    {
        if (response.StructuredContent is JsonElement e)
        {
            return e;
        }

        var t = response.Content.OfType<TextContentBlock>().FirstOrDefault()?.Text
            ?? throw new InvalidOperationException("MCP: sem conteúdo JSON.");
        return JsonSerializer.Deserialize<JsonElement>(t)!;
    }

    private sealed record Top10MaisSorteadosGolden(int[] ExpectedDezenaTop10);
}
