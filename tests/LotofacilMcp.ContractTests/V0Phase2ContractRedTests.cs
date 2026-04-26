using System.Text.Json;
using LotofacilMcp.Server.Tools;

namespace LotofacilMcp.ContractTests;

public sealed class V0Phase6ContractTests
{
    [Fact]
    public void ComputeWindowMetrics_ResponseContainsMinimalEnvelopeAndMetricValueContract()
    {
        var sut = new V0Tools(ContractTestFixturePaths.SyntheticMinWindowJson());
        var request = new ComputeWindowMetricsRequest(
            WindowSize: 3,
            EndContestId: 1003,
            Metrics: [new MetricRequest("frequencia_por_dezena")]);

        var response = sut.ComputeWindowMetrics(request);

        var payload = Assert.IsType<ComputeWindowMetricsResponse>(response);
        Assert.False(string.IsNullOrWhiteSpace(payload.DatasetVersion));
        Assert.False(string.IsNullOrWhiteSpace(payload.ToolVersion));
        Assert.False(string.IsNullOrWhiteSpace(payload.DeterministicHash));

        var metric = Assert.Single(payload.Metrics);
        Assert.Equal("frequencia_por_dezena", metric.MetricName);
        Assert.Equal("window", metric.Scope);
        Assert.Equal("vector_by_dezena", metric.Shape);
        Assert.Equal("count", metric.Unit);
        Assert.Equal("1.0.0", metric.Version);
        Assert.NotNull(metric.Window);
        Assert.Equal(3, metric.Window.Size);
        Assert.Equal(1001, metric.Window.StartContestId);
        Assert.Equal(1003, metric.Window.EndContestId);
        Assert.Equal(25, metric.Value.Count);
        Assert.False(string.IsNullOrWhiteSpace(metric.Explanation));

        using var json = JsonSerializer.SerializeToDocument(payload);
        var root = json.RootElement;
        Assert.True(root.TryGetProperty("dataset_version", out _));
        Assert.True(root.TryGetProperty("tool_version", out _));
        Assert.True(root.TryGetProperty("deterministic_hash", out _));

        var firstMetricJson = root.GetProperty("metrics")[0];
        Assert.True(firstMetricJson.TryGetProperty("metric_name", out _));
        Assert.True(firstMetricJson.TryGetProperty("scope", out _));
        Assert.True(firstMetricJson.TryGetProperty("shape", out _));
        Assert.True(firstMetricJson.TryGetProperty("unit", out _));
        Assert.True(firstMetricJson.TryGetProperty("version", out _));
        Assert.True(firstMetricJson.TryGetProperty("window", out _));
        Assert.True(firstMetricJson.TryGetProperty("value", out _));
        Assert.True(firstMetricJson.TryGetProperty("explanation", out _));
    }

    [Fact]
    public void ComputeWindowMetrics_WithUnknownMetric_ReturnsUnknownMetricErrorShape()
    {
        var sut = new V0Tools(ContractTestFixturePaths.SyntheticMinWindowJson());
        var request = new ComputeWindowMetricsRequest(
            WindowSize: 3,
            EndContestId: 1003,
            Metrics: [new MetricRequest("metrica_inexistente")]);

        var response = sut.ComputeWindowMetrics(request);

        var error = Assert.IsType<ContractErrorEnvelope>(response).Error;
        Assert.Equal("UNKNOWN_METRIC", error.Code);
        Assert.Equal("metrica_inexistente", error.Details["metric_name"]);
        Assert.False(error.Details.ContainsKey("allowed_metrics"));
    }

    [Fact]
    public void ComputeWindowMetrics_WithCatalogMetricOutsideRouteAllowlist_ReturnsScenarioAErrorPayload()
    {
        var sut = new V0Tools(ContractTestFixturePaths.SyntheticMinWindowJson());
        var request = new ComputeWindowMetricsRequest(
            WindowSize: 3,
            EndContestId: 1003,
            Metrics: [new MetricRequest("repeticao_concurso_anterior")]);

        var response = sut.ComputeWindowMetrics(request);

        var error = Assert.IsType<ContractErrorEnvelope>(response).Error;
        Assert.Equal("UNKNOWN_METRIC", error.Code);
        Assert.Equal("repeticao_concurso_anterior", error.Details["metric_name"]);
        Assert.Equal("pending_requires_opt_in", error.Details["reason"]);

        var allowedMetrics = Assert.IsType<string[]>(error.Details["allowed_metrics"]);
        Assert.Contains("frequencia_por_dezena", allowedMetrics);
        Assert.DoesNotContain("repeticao_concurso_anterior", allowedMetrics);
    }

    [Fact]
    public void ComputeWindowMetrics_WithPendingMetricAndAllowPendingTrue_ReturnsMetricValue()
    {
        var sut = new V0Tools(ContractTestFixturePaths.SyntheticMinWindowJson());
        var request = new ComputeWindowMetricsRequest(
            WindowSize: 3,
            EndContestId: 1003,
            Metrics: [new MetricRequest("repeticao_concurso_anterior")],
            AllowPending: true);

        var response = sut.ComputeWindowMetrics(request);

        var payload = Assert.IsType<ComputeWindowMetricsResponse>(response);
        var metric = Assert.Single(payload.Metrics);
        Assert.Equal("repeticao_concurso_anterior", metric.MetricName);
        Assert.Equal("series", metric.Scope);
        Assert.Equal("series", metric.Shape);
    }

    [Fact]
    public void ComputeWindowMetrics_WithoutMetrics_ReturnsInvalidRequestErrorShape()
    {
        var sut = new V0Tools(ContractTestFixturePaths.SyntheticMinWindowJson());
        var request = new ComputeWindowMetricsRequest(
            WindowSize: 3,
            EndContestId: 1003,
            Metrics: null);

        var response = sut.ComputeWindowMetrics(request);

        var error = Assert.IsType<ContractErrorEnvelope>(response).Error;
        Assert.Equal("INVALID_REQUEST", error.Code);
        Assert.Equal("metrics", error.Details["missing_field"]);
    }

    [Fact]
    public void ComputeWindowMetrics_PreservesRequestCardinalityForDuplicateMetrics()
    {
        var sut = new V0Tools(ContractTestFixturePaths.SyntheticMinWindowJson());
        var request = new ComputeWindowMetricsRequest(
            WindowSize: 3,
            EndContestId: 1003,
            Metrics:
            [
                new MetricRequest("frequencia_por_dezena"),
                new MetricRequest("frequencia_por_dezena"),
                new MetricRequest("frequencia_por_dezena")
            ]);

        var response = sut.ComputeWindowMetrics(request);

        var payload = Assert.IsType<ComputeWindowMetricsResponse>(response);
        Assert.Equal(3, payload.Metrics.Count);
        Assert.Equal(
            request.Metrics!.Select(metric => metric.Name).ToArray(),
            payload.Metrics.Select(metric => metric.MetricName).ToArray());
    }
}
