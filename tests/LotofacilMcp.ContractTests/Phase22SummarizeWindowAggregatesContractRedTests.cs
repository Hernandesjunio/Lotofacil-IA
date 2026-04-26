using System.Text.Json;
using LotofacilMcp.Server.Tools;

namespace LotofacilMcp.ContractTests;

public sealed class Phase22SummarizeWindowAggregatesContractRedTests
{
    [Fact]
    public void SummarizeWindowAggregates_WithoutAggregates_ReturnsInvalidRequest()
    {
        var response = InvokeSummarizeWindowAggregates(new Dictionary<string, object?>
        {
            ["window_size"] = 6,
            ["end_contest_id"] = 1006
        });

        var error = Assert.IsType<ContractErrorEnvelope>(response).Error;
        Assert.Equal("INVALID_REQUEST", error.Code);
        Assert.Equal("aggregates", Assert.IsType<string>(error.Details["missing_field"]));
    }

    [Fact]
    public void SummarizeWindowAggregates_WithInvalidAggregateType_ReturnsUnsupportedAggregateType()
    {
        var response = InvokeSummarizeWindowAggregates(new Dictionary<string, object?>
        {
            ["window_size"] = 6,
            ["end_contest_id"] = 1006,
            ["aggregates"] = new object[]
            {
                new Dictionary<string, object?>
                {
                    ["id"] = "invalid_aggregate_type",
                    ["source_metric_name"] = "pares_no_concurso",
                    ["aggregate_type"] = "histogram_not_supported",
                    ["params"] = new Dictionary<string, object?>
                    {
                        ["bucket_spec"] = new Dictionary<string, object?>
                        {
                            ["bucket_values"] = new[] { 0, 1, 2, 3, 4, 5 }
                        }
                    }
                }
            }
        });

        var error = Assert.IsType<ContractErrorEnvelope>(response).Error;
        Assert.Equal("UNSUPPORTED_AGGREGATE_TYPE", error.Code);
    }

    [Fact]
    public void SummarizeWindowAggregates_WithAmbiguousBucketSpec_ReturnsInvalidRequest()
    {
        var response = InvokeSummarizeWindowAggregates(new Dictionary<string, object?>
        {
            ["window_size"] = 6,
            ["end_contest_id"] = 1006,
            ["aggregates"] = new object[]
            {
                new Dictionary<string, object?>
                {
                    ["id"] = "invalid_bucket_spec",
                    ["source_metric_name"] = "pares_no_concurso",
                    ["aggregate_type"] = "histogram_scalar_series",
                    ["params"] = new Dictionary<string, object?>
                    {
                        ["bucket_spec"] = new Dictionary<string, object?>
                        {
                            ["bucket_values"] = new[] { 0, 1, 2, 3, 4, 5 },
                            ["min"] = 0,
                            ["max"] = 15,
                            ["width"] = 1
                        }
                    }
                }
            }
        });

        var error = Assert.IsType<ContractErrorEnvelope>(response).Error;
        Assert.Equal("INVALID_REQUEST", error.Code);
    }

    [Fact]
    public void SummarizeWindowAggregates_WithInvalidMatrixBounds_ReturnsInvalidRequest()
    {
        var response = InvokeSummarizeWindowAggregates(new Dictionary<string, object?>
        {
            ["window_size"] = 6,
            ["end_contest_id"] = 1006,
            ["aggregates"] = new object[]
            {
                new Dictionary<string, object?>
                {
                    ["id"] = "invalid_matrix_bounds",
                    ["source_metric_name"] = "distribuicao_linha_por_concurso",
                    ["aggregate_type"] = "histogram_count_vector5_series_per_position_matrix",
                    ["params"] = new Dictionary<string, object?>
                    {
                        ["value_min"] = 5,
                        ["value_max"] = 1
                    }
                }
            }
        });

        var error = Assert.IsType<ContractErrorEnvelope>(response).Error;
        Assert.Equal("INVALID_REQUEST", error.Code);
    }

    [Fact]
    public void SummarizeWindowAggregates_WithUnknownMetric_ReturnsUnknownMetric()
    {
        var response = InvokeSummarizeWindowAggregates(new Dictionary<string, object?>
        {
            ["window_size"] = 6,
            ["end_contest_id"] = 1006,
            ["aggregates"] = new object[]
            {
                new Dictionary<string, object?>
                {
                    ["id"] = "unknown_metric",
                    ["source_metric_name"] = "metrica_inexistente",
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
        });

        var error = Assert.IsType<ContractErrorEnvelope>(response).Error;
        Assert.Equal("UNKNOWN_METRIC", error.Code);
        Assert.Equal("metrica_inexistente", Assert.IsType<string>(error.Details["metric_name"]));
    }

    [Fact]
    public void SummarizeWindowAggregates_WithIncompatibleShape_ReturnsUnsupportedShape()
    {
        var response = InvokeSummarizeWindowAggregates(new Dictionary<string, object?>
        {
            ["window_size"] = 6,
            ["end_contest_id"] = 1006,
            ["aggregates"] = new object[]
            {
                new Dictionary<string, object?>
                {
                    ["id"] = "incompatible_shape",
                    ["source_metric_name"] = "pares_no_concurso",
                    ["aggregate_type"] = "topk_patterns_count_vector5_series",
                    ["params"] = new Dictionary<string, object?>
                    {
                        ["top_k"] = 3
                    }
                }
            }
        });

        var error = Assert.IsType<ContractErrorEnvelope>(response).Error;
        Assert.Equal("UNSUPPORTED_SHAPE", error.Code);
    }

    [Fact]
    public void SummarizeWindowAggregates_RepeatedRequest_IsDeterministicForPayloadAndHash()
    {
        var request = BuildCanonicalRequestForFixture();

        var first = InvokeSummarizeWindowAggregates(request, GetCanonicalFixturePath());
        var second = InvokeSummarizeWindowAggregates(request, GetCanonicalFixturePath());

        using var jsonA = JsonSerializer.SerializeToDocument(first);
        using var jsonB = JsonSerializer.SerializeToDocument(second);
        var rootA = jsonA.RootElement;
        var rootB = jsonB.RootElement;

        Assert.True(JsonElement.DeepEquals(rootA, rootB));
        Assert.Equal(
            rootA.GetProperty("deterministic_hash").GetString(),
            rootB.GetProperty("deterministic_hash").GetString());
    }

    [Fact]
    public void SummarizeWindowAggregates_UsesCanonicalOrderingAndDeterministicTieBreaks()
    {
        var response = InvokeSummarizeWindowAggregates(BuildCanonicalRequestForFixture(), GetCanonicalFixturePath());
        using var json = JsonSerializer.SerializeToDocument(response);
        var root = json.RootElement;

        Assert.True(root.TryGetProperty("aggregates", out var aggregates));
        Assert.Equal(JsonValueKind.Array, aggregates.ValueKind);
        Assert.Equal(3, aggregates.GetArrayLength());

        // Ordem de itens deve preservar exatamente a ordem do request.
        Assert.Equal("z_hist_pairs", aggregates[0].GetProperty("id").GetString());
        Assert.Equal("a_topk_rows", aggregates[1].GetProperty("id").GetString());
        Assert.Equal("m_matrix_rows", aggregates[2].GetProperty("id").GetString());

        // Histograma: buckets ordenados por x asc.
        var buckets = aggregates[0].GetProperty("buckets");
        for (var i = 1; i < buckets.GetArrayLength(); i++)
        {
            var previousX = buckets[i - 1].GetProperty("x").GetDouble();
            var currentX = buckets[i].GetProperty("x").GetDouble();
            Assert.True(previousX <= currentX, $"Bucket fora de ordem canônica em x: {previousX} > {currentX}.");
        }

        // Top-k: count desc e desempate lexicográfico asc de pattern.
        var items = aggregates[1].GetProperty("items");
        Assert.Equal(3, items.GetArrayLength());

        Assert.Equal(
            new[] { 2, 4, 4, 2, 3 },
            items[0].GetProperty("pattern").EnumerateArray().Select(x => x.GetInt32()).ToArray());
        Assert.Equal(
            new[] { 3, 3, 3, 3, 3 },
            items[1].GetProperty("pattern").EnumerateArray().Select(x => x.GetInt32()).ToArray());
        Assert.Equal(
            new[] { 4, 2, 3, 4, 2 },
            items[2].GetProperty("pattern").EnumerateArray().Select(x => x.GetInt32()).ToArray());

        for (var i = 1; i < items.GetArrayLength(); i++)
        {
            var previousCount = items[i - 1].GetProperty("count").GetInt32();
            var currentCount = items[i].GetProperty("count").GetInt32();
            Assert.True(previousCount >= currentCount, $"Top-k fora de ordem por count: {previousCount} < {currentCount}.");
        }

        // Matriz: shape cheia 5xK com K derivado de value_min/value_max (0..5 => K=6).
        var matrix = aggregates[2].GetProperty("matrix");
        Assert.Equal(5, matrix.GetArrayLength());
        foreach (var row in matrix.EnumerateArray())
        {
            Assert.Equal(6, row.GetArrayLength());
        }
    }

    [Fact]
    public void SummarizeWindowAggregates_CanonicalFixture_MatchesGoldenEvidence()
    {
        var response = InvokeSummarizeWindowAggregates(BuildCanonicalRequestForFixture(), GetCanonicalFixturePath());
        using var json = JsonSerializer.SerializeToDocument(response);
        var root = json.RootElement;
        var golden = LoadCanonicalAggregatesGolden();

        Assert.Equal(golden.WindowSize, root.GetProperty("window").GetProperty("size").GetInt32());
        Assert.Equal(golden.WindowStartContestId, root.GetProperty("window").GetProperty("start_contest_id").GetInt32());
        Assert.Equal(golden.WindowEndContestId, root.GetProperty("window").GetProperty("end_contest_id").GetInt32());

        var aggregates = root.GetProperty("aggregates");
        Assert.Equal(golden.Aggregates.GetArrayLength(), aggregates.GetArrayLength());
        Assert.True(JsonElement.DeepEquals(golden.Aggregates, aggregates));
    }

    private static Dictionary<string, object?> BuildCanonicalRequestForFixture()
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
            }
        };
    }

    private static object InvokeSummarizeWindowAggregates(Dictionary<string, object?> request, string? fixturePath = null)
    {
        var tools = new V0Tools(fixturePath ?? ContractTestFixturePaths.SyntheticMinWindowJson());
        var method = typeof(V0Tools).GetMethod("SummarizeWindowAggregates");
        Assert.True(
            method is not null,
            "Vermelho esperado: V0Tools ainda nao expoe SummarizeWindowAggregates. Implemente a tool apenas apos estes testes estarem falhando.");

        var parameterType = method!.GetParameters().Single().ParameterType;
        var requestJson = JsonSerializer.Serialize(request);
        var typedRequest = JsonSerializer.Deserialize(requestJson, parameterType);
        Assert.NotNull(typedRequest);

        return method.Invoke(tools, [typedRequest!])!;
    }

    private static string GetCanonicalFixturePath()
    {
        var repositoryRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        return Path.Combine(
            repositoryRoot,
            "tests",
            "fixtures",
            "aggregates_canonical_small_window.json");
    }

    private static CanonicalAggregatesGolden LoadCanonicalAggregatesGolden()
    {
        var repositoryRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var goldenPath = Path.Combine(
            repositoryRoot,
            "tests",
            "fixtures",
            "golden",
            "phase22",
            "summarize-window-aggregates.canonical-small-window.golden.json");

        using var json = JsonDocument.Parse(File.ReadAllText(goldenPath));
        var root = json.RootElement;
        var window = root.GetProperty("window");
        var aggregates = root.GetProperty("aggregates").Clone();

        return new CanonicalAggregatesGolden(
            WindowSize: window.GetProperty("size").GetInt32(),
            WindowStartContestId: window.GetProperty("start_contest_id").GetInt32(),
            WindowEndContestId: window.GetProperty("end_contest_id").GetInt32(),
            Aggregates: aggregates);
    }

    private sealed record CanonicalAggregatesGolden(
        int WindowSize,
        int WindowStartContestId,
        int WindowEndContestId,
        JsonElement Aggregates);
}
