using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace LotofacilMcp.ContractTests;

/// <summary>
/// Hotfix 23.6.x — evidências e testes de contrato para travar eficiência (ADR 0023 + mcp-tool-contract).
/// Critério: regressões de duplicação, utilidade mínima ou knobs/hashing quebram testes.
/// Massa/requests/expectativas explicitadas em fixtures/goldens em tests/fixtures/golden/phase23/.
/// </summary>
public sealed class Hotfix236EfficiencyEvidenceContractTests : IAsyncLifetime
{
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
    private HttpClient _httpClient = default!;
    private McpClient _mcp = default!;

    // Default for most hotfix scenarios.
    private readonly string _defaultFixturePath = ContractTestFixturePaths.SyntheticMinWindowJson();

    public async Task InitializeAsync()
    {
        (_httpClient, _mcp) = await CreateHttpMcpClientAsync(_defaultFixturePath);
    }

    public async Task DisposeAsync()
    {
        if (_mcp is not null)
        {
            await _mcp.DisposeAsync();
        }

        _httpClient?.Dispose();
    }

    // Hotfix 23.6.1 — utilidade factual (get_draw_window)
    [Fact]
    public async Task HF23_M01_GetDrawWindow_FactualShort_Standard_ContentHasContestDateAndNumbers_AndNoJsonDuplication()
    {
        var golden = LoadGolden("get-draw-window.window1-end3.standard.golden.json");
        var request = golden.GetProperty("request");

        var response = await _mcp.CallToolAsync("get_draw_window", ToArgs(request));
        Assert.False(response.IsError);

        var structured = ReadStructured(response);
        AssertWindow(structured, size: 1, start: 3, end: 3);
        AssertDraw(structured.GetProperty("draws")[0], contestId: 3, drawDate: "2003-10-13",
            numbers: [1, 4, 6, 7, 8, 9, 10, 11, 12, 14, 16, 17, 20, 23, 24]);

        var text = ReadText(response);
        AssertContentUtilityAndNoDup(text, structured, mustContainTokens: golden.GetProperty("required_content").GetProperty("must_include"));
        AssertDoesNotContainPhrases(text, golden.GetProperty("required_content").GetProperty("must_not_include_phrases"));
    }

    [Fact]
    public async Task HF23_M02_GetDrawWindow_ShortWindow_Standard_ContentMentionsSpanAndLastDraw_AndNoAdminOnlySummary()
    {
        var golden = LoadGolden("get-draw-window.window3-end3.standard.golden.json");
        var request = golden.GetProperty("request");

        var response = await _mcp.CallToolAsync("get_draw_window", ToArgs(request));
        Assert.False(response.IsError);

        var structured = ReadStructured(response);
        AssertWindow(structured, size: 3, start: 1, end: 3);
        Assert.Equal(3, structured.GetProperty("draws").GetArrayLength());

        var last = structured.GetProperty("draws")[2];
        AssertDraw(last, contestId: 3, drawDate: "2003-10-13",
            numbers: [1, 4, 6, 7, 8, 9, 10, 11, 12, 14, 16, 17, 20, 23, 24]);

        var text = ReadText(response);
        AssertContentUtilityAndNoDup(text, structured, mustContainTokens: golden.GetProperty("required_content").GetProperty("must_include"));
        AssertDoesNotContainPhrases(text, golden.GetProperty("required_content").GetProperty("must_not_include_phrases"));

        // Hotfix 23.6.3: rejeita resumo administrativo sem expor fatos principais.
        Assert.Contains("3", text, StringComparison.Ordinal);
        Assert.Contains("2003-10-13", text, StringComparison.Ordinal);
    }

    // Hotfix 23.6.2 — utilidade analítica (métricas/rankings)
    [Fact]
    public async Task HF23_M03_ComputeWindowMetrics_Top10TieHeavy_Standard_ContentHasMetricNameAndTop10()
    {
        var golden = LoadGolden("compute-window-metrics.top10-mais-sorteados.tie-heavy.standard.golden.json");
        var request = golden.GetProperty("request");

        await using var tieHeavy = await CreateStdioMcpClientAsync(GetRepoRelativePath(golden.GetProperty("fixture").GetString()!));
        var response = await tieHeavy.CallToolAsync("compute_window_metrics", ToArgs(request));
        Assert.False(response.IsError);

        var structured = ReadStructured(response);
        AssertWindow(structured, size: 5, start: 5001, end: 5005);

        var metrics = structured.GetProperty("metrics");
        Assert.Equal(1, metrics.GetArrayLength());
        Assert.Equal("top10_mais_sorteados", metrics[0].GetProperty("metric_name").GetString());

        // Shape for this metric is a 10-dezena list; we assert exact top10 list.
        var top10 = metrics[0].GetProperty("value").EnumerateArray().Select(x => x.GetInt32()).ToArray();
        Assert.Equal(new[] { 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, top10);

        var text = ReadText(response);
        AssertContentUtilityAndNoDup(text, structured, mustContainTokens: golden.GetProperty("required_content").GetProperty("must_include"));
        AssertDoesNotContainPhrases(text, golden.GetProperty("required_content").GetProperty("must_not_include_phrases"));
    }

    [Fact]
    public async Task HF23_M04_SummarizeWindowPatterns_ParesIqr_Standard_ContentHasFeatureAndStats()
    {
        var golden = LoadGolden("summarize-window-patterns.pares-iqr.standard.golden.json");
        var request = golden.GetProperty("request");

        var response = await _mcp.CallToolAsync("summarize_window_patterns", ToArgs(request));
        Assert.False(response.IsError);

        var structured = ReadStructured(response);
        AssertWindow(structured, size: 5, start: 1001, end: 1005);

        var summaries = structured.GetProperty("summaries");
        Assert.Equal(1, summaries.GetArrayLength());
        var s = summaries[0];
        Assert.Equal("pares_no_concurso", s.GetProperty("metric_name").GetString());
        Assert.Equal(8.0, s.GetProperty("mode").GetDouble(), precision: 12);
        Assert.Equal(8.0, s.GetProperty("median").GetDouble(), precision: 12);
        Assert.Equal(1.0, s.GetProperty("iqr").GetDouble(), precision: 12);
        Assert.Equal(0.6, s.GetProperty("coverage_observed").GetDouble(), precision: 12);

        var text = ReadText(response);
        AssertContentUtilityAndNoDup(text, structured, mustContainTokens: golden.GetProperty("required_content").GetProperty("must_include"));
        AssertDoesNotContainPhrases(text, golden.GetProperty("required_content").GetProperty("must_not_include_phrases"));

        // Hotfix 23.6.3: proíbe resumo genérico sem nenhum valor estatístico.
        Assert.Contains("8", text, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HF23_M05_SummarizeWindowAggregates_CanonicalSmallWindow_Standard_ContentMentionsSalientAggregateIds()
    {
        // Uses the same canonical fixture + request from phase22; hotfix asserts content utility.
        var golden = LoadGolden("summarize-window-aggregates.canonical-small-window.phase22-ref.standard.golden.json");

        var fixturePath = GetRepoRelativePath(golden.GetProperty("fixture").GetString()!);
        await using var client = await CreateStdioMcpClientAsync(fixturePath);

        var response = await client.CallToolAsync("summarize_window_aggregates", BuildCanonicalAggregatesRequest(verbosity: "standard"));
        Assert.False(response.IsError);

        var structured = ReadStructured(response);
        var aggregates = structured.GetProperty("aggregates");
        Assert.Equal(3, aggregates.GetArrayLength());
        Assert.Equal("z_hist_pairs", aggregates[0].GetProperty("id").GetString());
        Assert.Equal("a_topk_rows", aggregates[1].GetProperty("id").GetString());
        Assert.Equal("m_matrix_rows", aggregates[2].GetProperty("id").GetString());

        var text = ReadText(response);
        AssertContentUtilityAndNoDup(text, structured, mustContainTokens: null);
        AssertDoesNotContainPhrases(text, golden.GetProperty("required_content").GetProperty("must_not_include_phrases"));

        // Hotfix 23.6.3: não aceitar "admin-only" (somente contagem); exige ao menos um identificador.
        Assert.True(
            text.Contains("z_hist_pairs", StringComparison.Ordinal)
            || text.Contains("a_topk_rows", StringComparison.Ordinal)
            || text.Contains("m_matrix_rows", StringComparison.Ordinal),
            "Content must mention at least one salient aggregate id.");
    }

    // Hotfix 23.6.6 / HF23-M06 — anti-esvaziamento em full
    [Fact]
    public async Task HF23_M06_FullVerbosity_DoesNotDegenerateToSeeStructuredPayload_ForFactualAndAnalyticalTools()
    {
        // get_draw_window (factual)
        var g1 = await _mcp.CallToolAsync("get_draw_window", new Dictionary<string, object?>
        {
            ["window_size"] = 1,
            ["end_contest_id"] = 3,
            ["verbosity"] = "full"
        });
        Assert.False(g1.IsError);
        var t1 = ReadText(g1);
        AssertContentNotGenericSeeStructured(t1);
        Assert.Contains("3", t1, StringComparison.Ordinal);
        Assert.Contains("2003-10-13", t1, StringComparison.Ordinal);

        // summarize_window_patterns (analytical)
        var g2 = await _mcp.CallToolAsync("summarize_window_patterns", new Dictionary<string, object?>
        {
            ["window_size"] = 5,
            ["end_contest_id"] = 1005,
            ["features"] = new object[] { new Dictionary<string, object?> { ["name"] = "pares_no_concurso" } },
            ["coverage_threshold"] = 0.8,
            ["range_method"] = "iqr",
            ["verbosity"] = "full"
        });
        Assert.False(g2.IsError);
        var t2 = ReadText(g2);
        AssertContentNotGenericSeeStructured(t2);
        Assert.Contains("pares_no_concurso", t2, StringComparison.Ordinal);
        Assert.Contains("8", t2, StringComparison.Ordinal);

        // compute_window_metrics (analytical) — use tie_heavy to guarantee top10 determinístico.
        await using var tieHeavy = await CreateStdioMcpClientAsync(GetRepoRelativePath("tests/fixtures/tie_heavy.json"));
        var g3 = await tieHeavy.CallToolAsync("compute_window_metrics", new Dictionary<string, object?>
        {
            ["window_size"] = 5,
            ["end_contest_id"] = 5005,
            ["metrics"] = new object[] { new Dictionary<string, object?> { ["name"] = "top10_mais_sorteados" } },
            ["verbosity"] = "full"
        });
        Assert.False(g3.IsError);
        var t3 = ReadText(g3);
        AssertContentNotGenericSeeStructured(t3);
        Assert.Contains("top10_mais_sorteados", t3, StringComparison.Ordinal);
        Assert.Contains("11", t3, StringComparison.Ordinal);
    }

    // Hotfix 23.6.7 — meta-tools com onboarding verificável (help)
    [Fact]
    public async Task HF23_M07_Help_QuickstartOperational_ContentIncludesAuditableQuickstart_NotAdministrativeOnly()
    {
        var golden = LoadGolden("help.quickstart-operational.standard.golden.json");
        var request = golden.GetProperty("request");

        var response = await _mcp.CallToolAsync("help", ToArgs(request));
        Assert.False(response.IsError);

        var structured = ReadStructured(response);
        Assert.True(structured.TryGetProperty("getting_started_resource_uri", out _));
        Assert.True(structured.TryGetProperty("index_resource_uri", out _));
        Assert.True(structured.TryGetProperty("quick_start_markdown", out _));
        Assert.True(structured.TryGetProperty("templates", out _));

        var text = ReadText(response);
        AssertContentUtilityAndNoDup(text, structured, mustContainTokens: golden.GetProperty("required_content").GetProperty("must_include"));
        AssertDoesNotContainPhrases(text, golden.GetProperty("required_content").GetProperty("must_not_include_phrases"));
    }

    // Hotfix 23.6.8 — discover_capabilities com constraints verificáveis de janela
    [Fact]
    public async Task HF23_M08_DiscoverCapabilities_WindowConstraints_AreExplicitInStructuredAndContent()
    {
        var golden = LoadGolden("discover-capabilities.window-constraints.standard.golden.json");
        var request = golden.GetProperty("request");

        var response = await _mcp.CallToolAsync("discover_capabilities", ToArgs(request));
        Assert.False(response.IsError);

        var structured = ReadStructured(response);
        var tools = structured.GetProperty("tools");
        var getDrawWindow = tools.EnumerateArray()
            .First(t => string.Equals(t.GetProperty("name").GetString(), "get_draw_window", StringComparison.Ordinal));
        var supported = getDrawWindow.GetProperty("supported_parameters");

        AssertContainsOperationalConstraint(
            supported,
            "window_size.constraint",
            "window_size > 0");
        AssertContainsOperationalConstraint(
            supported,
            "window_size.quickstart",
            "window_size=1 anchors the latest available contest when end_contest_id is omitted");
        AssertContainsOperationalConstraint(
            supported,
            "start_contest_id.constraint",
            "start_contest_id requires end_contest_id");
        AssertContainsOperationalConstraint(
            supported,
            "window_size_start_end.coherence",
            "if start_contest_id/end_contest_id are provided, window_size must be omitted/0 or equal to (end-start+1)");

        var text = ReadText(response);
        AssertContentUtilityAndNoDup(text, structured, mustContainTokens: golden.GetProperty("required_content").GetProperty("must_include"));
        AssertDoesNotContainPhrases(text, golden.GetProperty("required_content").GetProperty("must_not_include_phrases"));
    }

    // Contract coverage: at least minimal and full for target tools.
    [Theory]
    [InlineData("get_draw_window", "tests/fixtures/synthetic_min_window.json")]
    [InlineData("compute_window_metrics", "tests/fixtures/synthetic_min_window.json")]
    [InlineData("summarize_window_patterns", "tests/fixtures/synthetic_min_window.json")]
    [InlineData("summarize_window_aggregates", "tests/fixtures/aggregates_canonical_small_window.json")]
    public async Task Tools_HaveContractCoverage_ForMinimalAndFull(string toolName, string fixture)
    {
        await using var client = await CreateStdioMcpClientAsync(GetRepoRelativePath(fixture));

        var minimal = await client.CallToolAsync(toolName, BuildMinimalRequest(toolName));
        var full = await client.CallToolAsync(toolName, BuildFullRequest(toolName));

        Assert.False(minimal.IsError);
        Assert.False(full.IsError);

        // Efficiency gate: Content exists and is not a JSON dump.
        AssertContentUtilityAndNoDup(ReadText(minimal), ReadStructured(minimal), mustContainTokens: null);
        AssertContentUtilityAndNoDup(ReadText(full), ReadStructured(full), mustContainTokens: null);

        // Hash gate: presentation knobs must affect deterministic_hash (mcp-tool-contract invariant 7 + ADR 0023).
        var hMin = ReadStructured(minimal).GetProperty("deterministic_hash").GetString();
        var hFull = ReadStructured(full).GetProperty("deterministic_hash").GetString();
        Assert.False(string.IsNullOrWhiteSpace(hMin));
        Assert.False(string.IsNullOrWhiteSpace(hFull));
        Assert.NotEqual(hMin, hFull);
    }

    // --- Helpers (goldens / assertions / MCP wiring) ---

    private static JsonElement LoadGolden(string fileName)
    {
        var repoRoot = GetRepositoryRoot();
        var path = Path.Combine(repoRoot, "tests", "fixtures", "golden", "phase23", fileName);
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        return doc.RootElement.Clone();
    }

    private static string GetRepositoryRoot() =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string GetRepoRelativePath(string repoRelative)
    {
        var repoRoot = GetRepositoryRoot();
        return Path.Combine(repoRoot, repoRelative.Replace('/', Path.DirectorySeparatorChar));
    }

    private static async Task<(HttpClient httpClient, McpClient mcp)> CreateHttpMcpClientAsync(string datasetFixturePath)
    {
        // We use HTTP MCP transport here to test Content/StructuredContent split (ADR 0023) via MCP result.
        // WebApplicationFactory is already used across contract tests; we keep it simple by hosting per suite.
        var factory = new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Dataset:DrawsSourceUri"] = datasetFixturePath
                    });
                });
            });

        var http = factory.CreateClient();
        var mcpEndpoint = new Uri(http.BaseAddress!, "mcp");
        var mcp = await McpClient.CreateAsync(new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Name = "LotofacilMcp.Server.Http",
                Endpoint = mcpEndpoint,
                TransportMode = HttpTransportMode.StreamableHttp
            },
            http,
            NullLoggerFactory.Instance,
            ownsHttpClient: false));

        // Ensure factory lives as long as http client; attach to handler lifetime via Dispose of http.
        // (WebApplicationFactory is disposed implicitly when process ends; for hotfix suite this is acceptable.)
        return (http, mcp);
    }

    private static async Task<McpClient> CreateStdioMcpClientAsync(string datasetFixturePath)
    {
        return await McpClient.CreateAsync(new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "LotofacilMcp.Server.Stdio.HF23",
            Command = "cmd",
            Arguments =
            [
                "/c",
                BuildStdioLaunchCommand(datasetFixturePath)
            ]
        }));
    }

    private static string BuildStdioLaunchCommand(string fixturePath)
    {
#if DEBUG
        var configuration = "Debug";
#else
        var configuration = "Release";
#endif
        var serverProjectPath = Path.Combine(GetRepositoryRoot(), "src", "LotofacilMcp.Server", "LotofacilMcp.Server.csproj");
        return
            $"set Dataset__DrawsSourceUri={fixturePath} && " +
            $"dotnet run -c {configuration} --no-build --project {serverProjectPath} -- --mcp-stdio";
    }

    private static Dictionary<string, object?> ToArgs(JsonElement request)
    {
        var dict = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var prop in request.EnumerateObject())
        {
            dict[prop.Name] = ToObject(prop.Value);
        }
        return dict;
    }

    private static object? ToObject(JsonElement el)
    {
        return el.ValueKind switch
        {
            JsonValueKind.String => el.GetString(),
            JsonValueKind.Number => el.TryGetInt32(out var i) ? i : el.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Array => el.EnumerateArray().Select(ToObject).ToArray(),
            JsonValueKind.Object => el.EnumerateObject().ToDictionary(p => p.Name, p => ToObject(p.Value), StringComparer.Ordinal),
            JsonValueKind.Null => null,
            _ => el.ToString()
        };
    }

    private static JsonElement ReadStructured(CallToolResult response)
    {
        if (response.StructuredContent is JsonElement structuredContent)
        {
            return structuredContent;
        }

        throw new InvalidOperationException("Expected StructuredContent JsonElement for MCP call result.");
    }

    private static string ReadText(CallToolResult response) =>
        response.Content.OfType<TextContentBlock>().FirstOrDefault()?.Text ?? string.Empty;

    private void AssertContentUtilityAndNoDup(string content, JsonElement structured, JsonElement? mustContainTokens)
    {
        Assert.False(string.IsNullOrWhiteSpace(content));

        // ADR 0023 D2: Content must not be a JSON dump of StructuredContent.
        Assert.False(content.TrimStart().StartsWith('{'));
        Assert.DoesNotContain("\"dataset_version\"", content, StringComparison.Ordinal);
        Assert.DoesNotContain("\"deterministic_hash\"", content, StringComparison.Ordinal);

        // Strong signal: Content should not parse as JSON.
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions));

        // Strong signal: it should not contain a prefix of the serialized StructuredContent.
        var structuredText = JsonSerializer.Serialize(structured, _jsonOptions);
        var prefix = structuredText.Length <= 80 ? structuredText : structuredText[..80];
        Assert.DoesNotContain(prefix, content, StringComparison.Ordinal);

        if (mustContainTokens is not null)
        {
            foreach (var token in mustContainTokens.Value.EnumerateArray())
            {
                var s = token.GetString();
                if (string.IsNullOrWhiteSpace(s))
                {
                    continue;
                }

                Assert.Matches(@"\b" + System.Text.RegularExpressions.Regex.Escape(s) + @"\b", content);
            }
        }
    }

    private static void AssertDoesNotContainPhrases(string content, JsonElement phrases)
    {
        foreach (var p in phrases.EnumerateArray())
        {
            var s = p.GetString();
            if (string.IsNullOrWhiteSpace(s))
            {
                continue;
            }

            Assert.DoesNotContain(s, content, StringComparison.Ordinal);
        }
    }

    private static void AssertContentNotGenericSeeStructured(string content)
    {
        Assert.DoesNotContain("see structured", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("structured payload", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("structuredcontent", content, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertContainsOperationalConstraint(JsonElement supportedParameters, string key, string expectedValue)
    {
        var values = supportedParameters.GetProperty(key).EnumerateArray().Select(x => x.GetString()).Where(x => x is not null).ToArray();
        Assert.Contains(expectedValue, values, StringComparer.Ordinal);
    }

    private static void AssertWindow(JsonElement structured, int size, int start, int end)
    {
        var window = structured.GetProperty("window");
        Assert.Equal(size, window.GetProperty("size").GetInt32());
        Assert.Equal(start, window.GetProperty("start_contest_id").GetInt32());
        Assert.Equal(end, window.GetProperty("end_contest_id").GetInt32());
    }

    private static void AssertDraw(JsonElement draw, int contestId, string drawDate, int[] numbers)
    {
        Assert.Equal(contestId, draw.GetProperty("contest_id").GetInt32());
        Assert.Equal(drawDate, draw.GetProperty("draw_date").GetString());
        Assert.Equal(numbers, draw.GetProperty("numbers").EnumerateArray().Select(x => x.GetInt32()).ToArray());
    }

    private static Dictionary<string, object?> BuildCanonicalAggregatesRequest(string verbosity)
    {
        return new Dictionary<string, object?>
        {
            ["window_size"] = 6,
            ["end_contest_id"] = 1006,
            ["aggregates"] = new object[]
            {
                new Dictionary<string, object?>
                {
                    ["id"] = "z_hist_pairs",
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
                    ["id"] = "a_topk_rows",
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
                    ["id"] = "m_matrix_rows",
                    ["source_metric_name"] = "distribuicao_linha_por_concurso",
                    ["aggregate_type"] = "histogram_count_vector5_series_per_position_matrix",
                    ["params"] = new Dictionary<string, object?>
                    {
                        ["value_min"] = 0,
                        ["value_max"] = 5
                    }
                }
            },
            ["verbosity"] = verbosity
        };
    }

    private static Dictionary<string, object?> BuildMinimalRequest(string toolName) => toolName switch
    {
        "get_draw_window" => new Dictionary<string, object?>
        {
            ["window_size"] = 1,
            ["end_contest_id"] = 3,
            ["verbosity"] = "minimal"
        },
        "compute_window_metrics" => new Dictionary<string, object?>
        {
            ["window_size"] = 3,
            ["end_contest_id"] = 1003,
            ["metrics"] = new object[] { new Dictionary<string, object?> { ["name"] = "frequencia_por_dezena" } },
            ["verbosity"] = "minimal"
        },
        "summarize_window_patterns" => new Dictionary<string, object?>
        {
            ["window_size"] = 5,
            ["end_contest_id"] = 1005,
            ["features"] = new object[] { new Dictionary<string, object?> { ["name"] = "pares_no_concurso" } },
            ["coverage_threshold"] = 0.8,
            ["range_method"] = "iqr",
            ["verbosity"] = "minimal"
        },
        "summarize_window_aggregates" => BuildCanonicalAggregatesRequest(verbosity: "minimal"),
        _ => throw new ArgumentOutOfRangeException(nameof(toolName), toolName, "Unknown tool for minimal request.")
    };

    private static Dictionary<string, object?> BuildFullRequest(string toolName) => toolName switch
    {
        "get_draw_window" => new Dictionary<string, object?>
        {
            ["window_size"] = 1,
            ["end_contest_id"] = 3,
            ["verbosity"] = "full"
        },
        "compute_window_metrics" => new Dictionary<string, object?>
        {
            ["window_size"] = 3,
            ["end_contest_id"] = 1003,
            ["metrics"] = new object[] { new Dictionary<string, object?> { ["name"] = "frequencia_por_dezena" } },
            ["verbosity"] = "full"
        },
        "summarize_window_patterns" => new Dictionary<string, object?>
        {
            ["window_size"] = 5,
            ["end_contest_id"] = 1005,
            ["features"] = new object[] { new Dictionary<string, object?> { ["name"] = "pares_no_concurso" } },
            ["coverage_threshold"] = 0.8,
            ["range_method"] = "iqr",
            ["verbosity"] = "full"
        },
        "summarize_window_aggregates" => BuildCanonicalAggregatesRequest(verbosity: "full"),
        _ => throw new ArgumentOutOfRangeException(nameof(toolName), toolName, "Unknown tool for full request.")
    };
}

