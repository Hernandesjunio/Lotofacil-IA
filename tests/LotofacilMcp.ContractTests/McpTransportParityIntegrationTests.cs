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
                "--configuration",
                "Release",
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

        var discoverRequest = new Dictionary<string, object?>();
        var httpDiscoverResponse = await _httpClient.PostAsJsonAsync("/tools/discover_capabilities", discoverRequest);
        Assert.Equal(HttpStatusCode.OK, httpDiscoverResponse.StatusCode);

        var stdioDiscoverResponse = await _stdioMcpClient.CallToolAsync("discover_capabilities", discoverRequest);
        var httpMcpDiscoverResponse = await _httpMcpClient.CallToolAsync("discover_capabilities", discoverRequest);

        var httpDiscoverPayload = await ReadHttpJsonAsync(httpDiscoverResponse);
        Assert.True(JsonElement.DeepEquals(httpDiscoverPayload, ReadMcpStructuredJson(stdioDiscoverResponse)));
        Assert.True(JsonElement.DeepEquals(httpDiscoverPayload, ReadMcpStructuredJson(httpMcpDiscoverResponse)));

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

        var summarizeRequest = new Dictionary<string, object?>
        {
            ["window_size"] = 5,
            ["end_contest_id"] = 1005,
            ["coverage_threshold"] = 0.8,
            ["range_method"] = "iqr",
            ["features"] = new object[]
            {
                new Dictionary<string, object?>
                {
                    ["metric_name"] = "pares_no_concurso"
                }
            }
        };

        var httpSummarizeResponse = await _httpClient.PostAsJsonAsync("/tools/summarize_window_patterns", summarizeRequest);
        Assert.Equal(HttpStatusCode.OK, httpSummarizeResponse.StatusCode);

        var stdioSummarizeResponse = await _stdioMcpClient.CallToolAsync("summarize_window_patterns", summarizeRequest);
        var httpMcpSummarizeResponse = await _httpMcpClient.CallToolAsync("summarize_window_patterns", summarizeRequest);

        var httpSummarizePayload = await ReadHttpJsonAsync(httpSummarizeResponse);
        Assert.True(JsonElement.DeepEquals(httpSummarizePayload, ReadMcpStructuredJson(stdioSummarizeResponse)));
        Assert.True(JsonElement.DeepEquals(httpSummarizePayload, ReadMcpStructuredJson(httpMcpSummarizeResponse)));

        var summarizeAggregatesRequest = new Dictionary<string, object?>
        {
            ["window_size"] = 5,
            ["end_contest_id"] = 1005,
            ["aggregates"] = new object[]
            {
                new Dictionary<string, object?>
                {
                    ["id"] = "pairs_histogram",
                    ["source_metric_name"] = "pares_no_concurso",
                    ["aggregate_type"] = "histogram_scalar_series",
                    ["params"] = new Dictionary<string, object?>
                    {
                        ["bucket_spec"] = new Dictionary<string, object?>
                        {
                            ["bucket_values"] = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }
                        },
                        ["include_ratios"] = true
                    }
                },
                new Dictionary<string, object?>
                {
                    ["id"] = "rows_topk",
                    ["source_metric_name"] = "distribuicao_linha_por_concurso",
                    ["aggregate_type"] = "topk_patterns_count_vector5_series",
                    ["params"] = new Dictionary<string, object?>
                    {
                        ["top_k"] = 3,
                        ["include_ratios"] = true
                    }
                },
                new Dictionary<string, object?>
                {
                    ["id"] = "rows_matrix",
                    ["source_metric_name"] = "distribuicao_linha_por_concurso",
                    ["aggregate_type"] = "histogram_count_vector5_series_per_position_matrix",
                    ["params"] = new Dictionary<string, object?>
                    {
                        ["value_min"] = 0,
                        ["value_max"] = 5
                    }
                }
            }
        };

        var httpSummarizeAggregatesResponse = await _httpClient.PostAsJsonAsync("/tools/summarize_window_aggregates", summarizeAggregatesRequest);
        Assert.Equal(HttpStatusCode.OK, httpSummarizeAggregatesResponse.StatusCode);

        var stdioSummarizeAggregatesResponse = await _stdioMcpClient.CallToolAsync("summarize_window_aggregates", summarizeAggregatesRequest);
        var httpMcpSummarizeAggregatesResponse = await _httpMcpClient.CallToolAsync("summarize_window_aggregates", summarizeAggregatesRequest);

        var httpSummarizeAggregatesPayload = await ReadHttpJsonAsync(httpSummarizeAggregatesResponse);
        Assert.True(JsonElement.DeepEquals(httpSummarizeAggregatesPayload, ReadMcpStructuredJson(stdioSummarizeAggregatesResponse)));
        Assert.True(JsonElement.DeepEquals(httpSummarizeAggregatesPayload, ReadMcpStructuredJson(httpMcpSummarizeAggregatesResponse)));

        var repeatedHttpSummarizeAggregatesResponse = await _httpClient.PostAsJsonAsync("/tools/summarize_window_aggregates", summarizeAggregatesRequest);
        Assert.Equal(HttpStatusCode.OK, repeatedHttpSummarizeAggregatesResponse.StatusCode);
        var repeatedHttpSummarizeAggregatesPayload = await ReadHttpJsonAsync(repeatedHttpSummarizeAggregatesResponse);
        Assert.Equal(
            httpSummarizeAggregatesPayload.GetProperty("deterministic_hash").GetString(),
            repeatedHttpSummarizeAggregatesPayload.GetProperty("deterministic_hash").GetString());

        var generateRequest = new Dictionary<string, object?>
        {
            ["window_size"] = 5,
            ["end_contest_id"] = 1005,
            ["seed"] = 424242UL,
            ["plan"] = new object[]
            {
                new Dictionary<string, object?>
                {
                    ["strategy_name"] = "common_repetition_frequency",
                    ["count"] = 2
                }
            }
        };

        var httpGenerateResponse = await _httpClient.PostAsJsonAsync("/tools/generate_candidate_games", generateRequest);
        Assert.Equal(HttpStatusCode.OK, httpGenerateResponse.StatusCode);

        var stdioGenerateResponse = await _stdioMcpClient.CallToolAsync("generate_candidate_games", generateRequest);
        var httpMcpGenerateResponse = await _httpMcpClient.CallToolAsync("generate_candidate_games", generateRequest);

        var httpGeneratePayload = await ReadHttpJsonAsync(httpGenerateResponse);
        Assert.True(JsonElement.DeepEquals(httpGeneratePayload, ReadMcpStructuredJson(stdioGenerateResponse)));
        Assert.True(JsonElement.DeepEquals(httpGeneratePayload, ReadMcpStructuredJson(httpMcpGenerateResponse)));

        var explainRequest = new Dictionary<string, object?>
        {
            ["window_size"] = 5,
            ["end_contest_id"] = 1005,
            ["include_metric_breakdown"] = true,
            ["include_exclusion_breakdown"] = true,
            ["games"] = new object[]
            {
                new[]
                {
                    1, 3, 4, 5, 7, 8, 10, 11, 13, 15, 17, 18, 20, 22, 24
                }
            }
        };

        var httpExplainResponse = await _httpClient.PostAsJsonAsync("/tools/explain_candidate_games", explainRequest);
        Assert.Equal(HttpStatusCode.OK, httpExplainResponse.StatusCode);

        var stdioExplainResponse = await _stdioMcpClient.CallToolAsync("explain_candidate_games", explainRequest);
        var httpMcpExplainResponse = await _httpMcpClient.CallToolAsync("explain_candidate_games", explainRequest);

        var httpExplainPayload = await ReadHttpJsonAsync(httpExplainResponse);
        Assert.True(JsonElement.DeepEquals(httpExplainPayload, ReadMcpStructuredJson(stdioExplainResponse)));
        Assert.True(JsonElement.DeepEquals(httpExplainPayload, ReadMcpStructuredJson(httpMcpExplainResponse)));
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

        var minHistoryGapRequest = new Dictionary<string, object?>
        {
            ["window_size"] = 3,
            ["end_contest_id"] = 1003,
            ["indicators"] = new object[]
            {
                new Dictionary<string, object?>
                {
                    ["name"] = "repeticao_concurso_anterior"
                }
            },
            ["top_k"] = 3,
            ["min_history"] = 4
        };

        var httpMinHistoryGapResponse = await _httpClient.PostAsJsonAsync("/tools/analyze_indicator_stability", minHistoryGapRequest);
        Assert.Equal(HttpStatusCode.BadRequest, httpMinHistoryGapResponse.StatusCode);

        var stdioMinHistoryGapResponse = await _stdioMcpClient.CallToolAsync("analyze_indicator_stability", minHistoryGapRequest);
        var httpMcpMinHistoryGapResponse = await _httpMcpClient.CallToolAsync("analyze_indicator_stability", minHistoryGapRequest);

        Assert.True(stdioMinHistoryGapResponse.IsError);
        Assert.True(httpMcpMinHistoryGapResponse.IsError);

        var httpMinHistoryGapPayload = await ReadHttpJsonAsync(httpMinHistoryGapResponse);
        Assert.True(JsonElement.DeepEquals(httpMinHistoryGapPayload, ReadMcpStructuredJson(stdioMinHistoryGapResponse)));
        Assert.True(JsonElement.DeepEquals(httpMinHistoryGapPayload, ReadMcpStructuredJson(httpMcpMinHistoryGapResponse)));

        var minHistoryGapError = httpMinHistoryGapPayload.GetProperty("error");
        Assert.Equal("INSUFFICIENT_HISTORY", minHistoryGapError.GetProperty("code").GetString());
        var minHistoryGapDetails = minHistoryGapError.GetProperty("details");
        Assert.Equal(4, minHistoryGapDetails.GetProperty("min_history").GetInt32());
        Assert.Equal(3, minHistoryGapDetails.GetProperty("effective_window_size").GetInt32());

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

        var unsupportedStabilityCheckRequest = new Dictionary<string, object?>
        {
            ["window_size"] = 5,
            ["end_contest_id"] = 1005,
            ["method"] = "spearman",
            ["top_k"] = 3,
            ["items"] = new object[]
            {
                new Dictionary<string, object?> { ["name"] = "repeticao_concurso_anterior" },
                new Dictionary<string, object?> { ["name"] = "pares_no_concurso" }
            },
            ["stability_check"] = new Dictionary<string, object?>
            {
                ["method"] = "rolling_window",
                ["subwindow_size"] = 3
            }
        };

        var httpUnsupportedStabilityCheck = await _httpClient.PostAsJsonAsync("/tools/analyze_indicator_associations", unsupportedStabilityCheckRequest);
        Assert.Equal(HttpStatusCode.BadRequest, httpUnsupportedStabilityCheck.StatusCode);

        var stdioUnsupportedStabilityCheck = await _stdioMcpClient.CallToolAsync("analyze_indicator_associations", unsupportedStabilityCheckRequest);
        var httpMcpUnsupportedStabilityCheck = await _httpMcpClient.CallToolAsync("analyze_indicator_associations", unsupportedStabilityCheckRequest);

        Assert.True(stdioUnsupportedStabilityCheck.IsError);
        Assert.True(httpMcpUnsupportedStabilityCheck.IsError);

        var httpUnsupportedStabilityCheckPayload = await ReadHttpJsonAsync(httpUnsupportedStabilityCheck);
        Assert.True(JsonElement.DeepEquals(httpUnsupportedStabilityCheckPayload, ReadMcpStructuredJson(stdioUnsupportedStabilityCheck)));
        Assert.True(JsonElement.DeepEquals(httpUnsupportedStabilityCheckPayload, ReadMcpStructuredJson(httpMcpUnsupportedStabilityCheck)));
        Assert.Equal(
            "UNSUPPORTED_STABILITY_CHECK",
            httpUnsupportedStabilityCheckPayload.GetProperty("error").GetProperty("code").GetString());

        var invalidSummarizeRequest = new Dictionary<string, object?>
        {
            ["window_size"] = 5,
            ["end_contest_id"] = 1005,
            ["coverage_threshold"] = 0.8,
            ["range_method"] = "mad",
            ["features"] = new object[]
            {
                new Dictionary<string, object?>
                {
                    ["metric_name"] = "pares_no_concurso"
                }
            }
        };

        var httpSummarizeErr = await _httpClient.PostAsJsonAsync("/tools/summarize_window_patterns", invalidSummarizeRequest);
        Assert.Equal(HttpStatusCode.BadRequest, httpSummarizeErr.StatusCode);

        var stdioSummarizeErr = await _stdioMcpClient.CallToolAsync("summarize_window_patterns", invalidSummarizeRequest);
        var httpMcpSummarizeErr = await _httpMcpClient.CallToolAsync("summarize_window_patterns", invalidSummarizeRequest);

        Assert.True(stdioSummarizeErr.IsError);
        Assert.True(httpMcpSummarizeErr.IsError);

        var httpSummarizeErrPayload = await ReadHttpJsonAsync(httpSummarizeErr);
        Assert.True(JsonElement.DeepEquals(httpSummarizeErrPayload, ReadMcpStructuredJson(stdioSummarizeErr)));
        Assert.True(JsonElement.DeepEquals(httpSummarizeErrPayload, ReadMcpStructuredJson(httpMcpSummarizeErr)));

        var invalidSummarizeAggregatesRequest = new Dictionary<string, object?>
        {
            ["window_size"] = 5,
            ["end_contest_id"] = 1005,
            ["aggregates"] = new object[]
            {
                new Dictionary<string, object?>
                {
                    ["id"] = "invalid_aggregate_type",
                    ["source_metric_name"] = "pares_no_concurso",
                    ["aggregate_type"] = "not_supported",
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

        var httpSummarizeAggregatesErr = await _httpClient.PostAsJsonAsync("/tools/summarize_window_aggregates", invalidSummarizeAggregatesRequest);
        Assert.Equal(HttpStatusCode.BadRequest, httpSummarizeAggregatesErr.StatusCode);

        var stdioSummarizeAggregatesErr = await _stdioMcpClient.CallToolAsync("summarize_window_aggregates", invalidSummarizeAggregatesRequest);
        var httpMcpSummarizeAggregatesErr = await _httpMcpClient.CallToolAsync("summarize_window_aggregates", invalidSummarizeAggregatesRequest);

        Assert.True(stdioSummarizeAggregatesErr.IsError);
        Assert.True(httpMcpSummarizeAggregatesErr.IsError);

        var httpSummarizeAggregatesErrPayload = await ReadHttpJsonAsync(httpSummarizeAggregatesErr);
        Assert.True(JsonElement.DeepEquals(httpSummarizeAggregatesErrPayload, ReadMcpStructuredJson(stdioSummarizeAggregatesErr)));
        Assert.True(JsonElement.DeepEquals(httpSummarizeAggregatesErrPayload, ReadMcpStructuredJson(httpMcpSummarizeAggregatesErr)));

        var invalidGenerateRequest = new Dictionary<string, object?>
        {
            ["window_size"] = 5,
            ["end_contest_id"] = 1005,
            ["plan"] = new object[]
            {
                new Dictionary<string, object?>
                {
                    ["strategy_name"] = "common_repetition_frequency",
                    ["count"] = 1
                }
            }
        };

        var httpGenerateErr = await _httpClient.PostAsJsonAsync("/tools/generate_candidate_games", invalidGenerateRequest);
        Assert.Equal(HttpStatusCode.BadRequest, httpGenerateErr.StatusCode);

        var stdioGenerateErr = await _stdioMcpClient.CallToolAsync("generate_candidate_games", invalidGenerateRequest);
        var httpMcpGenerateErr = await _httpMcpClient.CallToolAsync("generate_candidate_games", invalidGenerateRequest);

        Assert.True(stdioGenerateErr.IsError);
        Assert.True(httpMcpGenerateErr.IsError);

        var httpGenerateErrPayload = await ReadHttpJsonAsync(httpGenerateErr);
        Assert.True(JsonElement.DeepEquals(httpGenerateErrPayload, ReadMcpStructuredJson(stdioGenerateErr)));
        Assert.True(JsonElement.DeepEquals(httpGenerateErrPayload, ReadMcpStructuredJson(httpMcpGenerateErr)));

        var invalidExplainRequest = new Dictionary<string, object?>
        {
            ["window_size"] = 5,
            ["end_contest_id"] = 1005,
            ["include_metric_breakdown"] = true,
            ["include_exclusion_breakdown"] = true,
            ["games"] = Array.Empty<object>()
        };

        var httpExplainErr = await _httpClient.PostAsJsonAsync("/tools/explain_candidate_games", invalidExplainRequest);
        Assert.Equal(HttpStatusCode.BadRequest, httpExplainErr.StatusCode);

        var stdioExplainErr = await _stdioMcpClient.CallToolAsync("explain_candidate_games", invalidExplainRequest);
        var httpMcpExplainErr = await _httpMcpClient.CallToolAsync("explain_candidate_games", invalidExplainRequest);

        Assert.True(stdioExplainErr.IsError);
        Assert.True(httpMcpExplainErr.IsError);

        var httpExplainErrPayload = await ReadHttpJsonAsync(httpExplainErr);
        Assert.True(JsonElement.DeepEquals(httpExplainErrPayload, ReadMcpStructuredJson(stdioExplainErr)));
        Assert.True(JsonElement.DeepEquals(httpExplainErrPayload, ReadMcpStructuredJson(httpMcpExplainErr)));
    }

    private static async Task AssertToolDiscoveryAsync(McpClient client)
    {
        var listedTools = await client.ListToolsAsync();
        Assert.Contains(listedTools, tool => tool.Name is "discover_capabilities");
        Assert.Contains(listedTools, tool => tool.Name is "help");
        Assert.Contains(listedTools, tool => tool.Name is "get_draw_window");
        Assert.Contains(listedTools, tool => tool.Name is "compute_window_metrics");
        Assert.Contains(listedTools, tool => tool.Name is "analyze_indicator_stability");
        Assert.Contains(listedTools, tool => tool.Name is "compose_indicator_analysis");
        Assert.Contains(listedTools, tool => tool.Name is "analyze_indicator_associations");
        Assert.Contains(listedTools, tool => tool.Name is "summarize_window_patterns");
        Assert.Contains(listedTools, tool => tool.Name is "summarize_window_aggregates");
        Assert.Contains(listedTools, tool => tool.Name is "generate_candidate_games");
        Assert.Contains(listedTools, tool => tool.Name is "explain_candidate_games");
    }

    [Fact]
    public async Task McpResourceDiscovery_IncludesPromptIndex_AndHelpReturnsIndex()
    {
        var stdioResources = await _stdioMcpClient.ListResourcesAsync();
        var httpResources = await _httpMcpClient.ListResourcesAsync();

        Assert.Contains(stdioResources, r => r.Uri is "lotofacil-ia://prompts/index@1.0.0");
        Assert.Contains(httpResources, r => r.Uri is "lotofacil-ia://prompts/index@1.0.0");
        Assert.Contains(stdioResources, r => r.Uri is "lotofacil-ia://help/getting-started@1.0.0");
        Assert.Contains(httpResources, r => r.Uri is "lotofacil-ia://help/getting-started@1.0.0");

        var stdioIndex = stdioResources.First(r => r.Uri is "lotofacil-ia://prompts/index@1.0.0");
        var httpIndex = httpResources.First(r => r.Uri is "lotofacil-ia://prompts/index@1.0.0");
        var stdioGettingStarted = stdioResources.First(r => r.Uri is "lotofacil-ia://help/getting-started@1.0.0");
        var httpGettingStarted = httpResources.First(r => r.Uri is "lotofacil-ia://help/getting-started@1.0.0");

        var stdioIndexRead = await stdioIndex.ReadAsync();
        var httpIndexRead = await httpIndex.ReadAsync();
        var stdioGettingStartedRead = await stdioGettingStarted.ReadAsync();
        var httpGettingStartedRead = await httpGettingStarted.ReadAsync();

        var stdioText = stdioIndexRead.Contents.OfType<TextResourceContents>().First().Text;
        var httpText = httpIndexRead.Contents.OfType<TextResourceContents>().First().Text;
        var stdioGettingStartedText = stdioGettingStartedRead.Contents.OfType<TextResourceContents>().First().Text;
        var httpGettingStartedText = httpGettingStartedRead.Contents.OfType<TextResourceContents>().First().Text;

        Assert.Contains("Índice de templates", stdioText);
        Assert.Contains("Índice de templates", httpText);
        Assert.Contains("Getting started", stdioGettingStartedText);
        Assert.Contains("Getting started", httpGettingStartedText);

        var stdioHelp = await _stdioMcpClient.CallToolAsync("help", new Dictionary<string, object?>());
        var httpHelp = await _httpMcpClient.CallToolAsync("help", new Dictionary<string, object?>());

        Assert.False(stdioHelp.IsError);
        Assert.False(httpHelp.IsError);

        var stdioHelpJson = ReadMcpStructuredJson(stdioHelp);
        var httpHelpJson = ReadMcpStructuredJson(httpHelp);

        Assert.True(stdioHelpJson.TryGetProperty("getting_started_resource_uri", out var stdioGettingStartedUri));
        Assert.Equal("lotofacil-ia://help/getting-started@1.0.0", stdioGettingStartedUri.GetString());
        Assert.True(stdioHelpJson.TryGetProperty("index_resource_uri", out var stdioIndexUri));
        Assert.Equal("lotofacil-ia://prompts/index@1.0.0", stdioIndexUri.GetString());
        Assert.True(stdioHelpJson.TryGetProperty("index_markdown", out var stdioIndexMarkdown));
        Assert.Contains("Índice de templates", stdioIndexMarkdown.GetString());
        Assert.True(stdioHelpJson.TryGetProperty("quick_start_markdown", out var stdioQuickStartMarkdown));
        Assert.Contains("Comece por aqui", stdioQuickStartMarkdown.GetString());

        Assert.True(httpHelpJson.TryGetProperty("getting_started_resource_uri", out var httpGettingStartedUri));
        Assert.Equal("lotofacil-ia://help/getting-started@1.0.0", httpGettingStartedUri.GetString());
        Assert.True(httpHelpJson.TryGetProperty("index_resource_uri", out var httpIndexUri));
        Assert.Equal("lotofacil-ia://prompts/index@1.0.0", httpIndexUri.GetString());
        Assert.True(httpHelpJson.TryGetProperty("index_markdown", out var httpIndexMarkdown));
        Assert.Contains("Índice de templates", httpIndexMarkdown.GetString());
        Assert.True(httpHelpJson.TryGetProperty("quick_start_markdown", out var httpQuickStartMarkdown));
        Assert.Contains("Comece por aqui", httpQuickStartMarkdown.GetString());
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
