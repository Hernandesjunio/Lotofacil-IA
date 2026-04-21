using LotofacilMcp.Application.Mapping;
using LotofacilMcp.Application.UseCases;
using LotofacilMcp.Application.Validation;
using LotofacilMcp.Domain.Metrics;
using LotofacilMcp.Domain.Normalization;
using LotofacilMcp.Domain.Windows;
using LotofacilMcp.Infrastructure.DatasetVersioning;
using LotofacilMcp.Infrastructure.Providers;

namespace LotofacilMcp.Infrastructure.Tests;

public sealed class Phase5ApplicationUseCasesTests
{
    [Fact]
    public void GetDrawWindowUseCase_ResolvesExpectedWindow()
    {
        var sut = BuildGetDrawWindowUseCase();
        var input = new GetDrawWindowInput(
            WindowSize: 3,
            EndContestId: 1004,
            FixturePath: GetFixturePath());

        var result = sut.Execute(input);

        Assert.Equal("1.0.0", result.ToolVersion);
        Assert.StartsWith("cef-", result.DatasetVersion);
        Assert.Equal(3, result.Window.Size);
        Assert.Equal(1002, result.Window.StartContestId);
        Assert.Equal(1004, result.Window.EndContestId);
        Assert.Equal([1002, 1003, 1004], result.Draws.Select(draw => draw.ContestId).ToArray());
        Assert.Equal(3, result.DeterministicHashInput.WindowSize);
        Assert.Equal(1004, result.DeterministicHashInput.EndContestId);
    }

    [Fact]
    public void ComputeWindowMetricsUseCase_ReturnsTypedMetricWithoutServerLogic()
    {
        var sut = BuildComputeWindowMetricsUseCase();
        var input = new ComputeWindowMetricsInput(
            WindowSize: 3,
            EndContestId: 1003,
            Metrics: [new MetricRequestInput("frequencia_por_dezena")],
            FixturePath: GetFixturePath());

        var result = sut.Execute(input);

        var metric = Assert.Single(result.Metrics);
        Assert.Equal("1.0.0", result.ToolVersion);
        Assert.StartsWith("cef-", result.DatasetVersion);
        Assert.Equal(3, result.Window.Size);
        Assert.Equal("frequencia_por_dezena", metric.MetricName);
        Assert.Equal("window", metric.Scope);
        Assert.Equal("vector_by_dezena", metric.Shape);
        Assert.Equal("count", metric.Unit);
        Assert.Equal("1.0.0", metric.Version);
        Assert.Equal(result.Window, metric.Window);
        Assert.Equal(
            [2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0],
            metric.Value.ToArray());
        Assert.Equal(3, result.DeterministicHashInput.WindowSize);
        Assert.Equal(1003, result.DeterministicHashInput.EndContestId);
        Assert.False(result.DeterministicHashInput.AllowPending);
        Assert.Equal("frequencia_por_dezena", Assert.Single(result.DeterministicHashInput.Metrics).Name);
    }

    [Fact]
    public void ComputeWindowMetricsUseCase_WithoutMetrics_ThrowsInvalidRequest()
    {
        var sut = BuildComputeWindowMetricsUseCase();
        var input = new ComputeWindowMetricsInput(
            WindowSize: 3,
            EndContestId: 1003,
            Metrics: null,
            FixturePath: GetFixturePath());

        var error = Assert.Throws<ApplicationValidationException>(() => sut.Execute(input));

        Assert.Equal("INVALID_REQUEST", error.Code);
        Assert.Equal("metrics", error.Details["missing_field"]);
    }

    [Fact]
    public void ComputeWindowMetricsUseCase_WithUnknownMetric_ThrowsUnknownMetric()
    {
        var sut = BuildComputeWindowMetricsUseCase();
        var input = new ComputeWindowMetricsInput(
            WindowSize: 3,
            EndContestId: 1003,
            Metrics: [new MetricRequestInput("metrica_inexistente")],
            FixturePath: GetFixturePath());

        var error = Assert.Throws<ApplicationValidationException>(() => sut.Execute(input));

        Assert.Equal("UNKNOWN_METRIC", error.Code);
        Assert.Equal("metrica_inexistente", error.Details["metric_name"]);
    }

    private static GetDrawWindowUseCase BuildGetDrawWindowUseCase()
    {
        return new GetDrawWindowUseCase(
            new SyntheticFixtureProvider(),
            new DatasetVersionService(),
            new WindowResolver(),
            new V0CrossFieldValidator(),
            new V0RequestMapper(new DrawNormalizer()));
    }

    private static ComputeWindowMetricsUseCase BuildComputeWindowMetricsUseCase()
    {
        return new ComputeWindowMetricsUseCase(
            new SyntheticFixtureProvider(),
            new DatasetVersionService(),
            new WindowResolver(),
            new FrequencyByDezenaMetric(),
            new V0CrossFieldValidator(),
            new V0RequestMapper(new DrawNormalizer()));
    }

    private static string GetFixturePath()
    {
        return Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "fixtures", "synthetic_min_window.json"));
    }
}
