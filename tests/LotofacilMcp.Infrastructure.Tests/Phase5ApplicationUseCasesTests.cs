using LotofacilMcp.Application.Mapping;
using LotofacilMcp.Application.UseCases;
using LotofacilMcp.Application.Validation;
using LotofacilMcp.Domain.Analytics;
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
            EndContestId: 4,
            FixturePath: GetFixturePath());

        var result = sut.Execute(input);

        Assert.Equal("1.0.0", result.ToolVersion);
        Assert.StartsWith("cef-", result.DatasetVersion);
        Assert.Equal(3, result.Window.Size);
        Assert.Equal(2, result.Window.StartContestId);
        Assert.Equal(4, result.Window.EndContestId);
        Assert.Equal([2, 3, 4], result.Draws.Select(draw => draw.ContestId).ToArray());
        Assert.Equal(3, result.DeterministicHashInput.WindowSize);
        Assert.Equal(4, result.DeterministicHashInput.EndContestId);
    }

    [Fact]
    public void ComputeWindowMetricsUseCase_ReturnsTypedMetricWithoutServerLogic()
    {
        var sut = BuildComputeWindowMetricsUseCase();
        var input = new ComputeWindowMetricsInput(
            WindowSize: 3,
            EndContestId: 3,
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
            [2, 1, 1, 2, 2, 3, 2, 1, 3, 2, 3, 2, 2, 2, 1, 3, 1, 1, 1, 3, 0, 0, 3, 3, 1],
            metric.Value.ToArray());
        Assert.Equal(3, result.DeterministicHashInput.WindowSize);
        Assert.Equal(3, result.DeterministicHashInput.EndContestId);
        Assert.False(result.DeterministicHashInput.AllowPending);
        Assert.Equal("frequencia_por_dezena", Assert.Single(result.DeterministicHashInput.Metrics).Name);
    }

    [Fact]
    public void ComputeWindowMetricsUseCase_WithoutMetrics_ThrowsInvalidRequest()
    {
        var sut = BuildComputeWindowMetricsUseCase();
        var input = new ComputeWindowMetricsInput(
            WindowSize: 3,
            EndContestId: 3,
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
            EndContestId: 3,
            Metrics: [new MetricRequestInput("metrica_inexistente")],
            FixturePath: GetFixturePath());

        var error = Assert.Throws<ApplicationValidationException>(() => sut.Execute(input));

        Assert.Equal("UNKNOWN_METRIC", error.Code);
        Assert.Equal("metrica_inexistente", error.Details["metric_name"]);
    }

    [Fact]
    public void ComputeWindowMetricsUseCase_ReturnsTop10MaisSorteadosPerCatalog()
    {
        var sut = BuildComputeWindowMetricsUseCase();
        var input = new ComputeWindowMetricsInput(
            WindowSize: 3,
            EndContestId: 3,
            Metrics: [new MetricRequestInput("top10_mais_sorteados")],
            FixturePath: GetFixturePath());

        var result = sut.Execute(input);

        var metric = Assert.Single(result.Metrics);
        Assert.Equal("top10_mais_sorteados", metric.MetricName);
        Assert.Equal("window", metric.Scope);
        Assert.Equal("dezena_list[10]", metric.Shape);
        Assert.Equal("dimensionless", metric.Unit);
        Assert.Equal("1.0.0", metric.Version);
        Assert.Equal([6, 9, 11, 16, 20, 23, 24, 1, 4, 5], metric.Value.ToArray());
    }

    [Fact]
    public void AnalyzeIndicatorStabilityUseCase_ReturnsRankingWithDefaultMadn()
    {
        var sut = BuildAnalyzeIndicatorStabilityUseCase();
        var input = new AnalyzeIndicatorStabilityInput(
            WindowSize: 5,
            EndContestId: 5,
            Indicators:
            [
                new StabilityIndicatorRequestInput("repeticao_concurso_anterior", null),
                new StabilityIndicatorRequestInput("distribuicao_linha_por_concurso", "per_component")
            ],
            NormalizationMethod: null,
            TopK: 3,
            MinHistory: 3,
            FixturePath: GetFixturePath());

        var result = sut.Execute(input);

        Assert.Equal("1.0.0", result.ToolVersion);
        Assert.StartsWith("cef-", result.DatasetVersion);
        Assert.Equal("madn", result.NormalizationMethod);
        Assert.Equal(5, result.Window.Size);
        Assert.Equal(3, result.Ranking.Count);
        Assert.All(result.Ranking, entry =>
        {
            Assert.InRange(entry.StabilityScore, 0d, 1d);
            Assert.True(entry.Dispersion >= 0d);
            Assert.False(string.IsNullOrWhiteSpace(entry.IndicatorName));
            Assert.False(string.IsNullOrWhiteSpace(entry.Explanation));
        });
    }

    [Fact]
    public void AnalyzeIndicatorStabilityUseCase_WithoutVectorAggregation_ThrowsUnsupportedAggregation()
    {
        var sut = BuildAnalyzeIndicatorStabilityUseCase();
        var input = new AnalyzeIndicatorStabilityInput(
            WindowSize: 5,
            EndContestId: 5,
            Indicators:
            [
                new StabilityIndicatorRequestInput("distribuicao_linha_por_concurso", null)
            ],
            NormalizationMethod: "madn",
            TopK: 3,
            MinHistory: 3,
            FixturePath: GetFixturePath());

        var error = Assert.Throws<ApplicationValidationException>(() => sut.Execute(input));

        Assert.Equal("UNSUPPORTED_AGGREGATION", error.Code);
    }

    [Fact]
    public void AnalyzeIndicatorStabilityUseCase_WithWindowBelowMinHistory_ThrowsInsufficientHistory()
    {
        var sut = BuildAnalyzeIndicatorStabilityUseCase();
        var input = new AnalyzeIndicatorStabilityInput(
            WindowSize: 3,
            EndContestId: 3,
            Indicators:
            [
                new StabilityIndicatorRequestInput("repeticao_concurso_anterior", null)
            ],
            NormalizationMethod: "madn",
            TopK: 3,
            MinHistory: 5,
            FixturePath: GetFixturePath());

        var error = Assert.Throws<ApplicationValidationException>(() => sut.Execute(input));

        Assert.Equal("INSUFFICIENT_HISTORY", error.Code);
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
            BuildWindowMetricDispatcher(),
            new V0CrossFieldValidator(),
            new V0RequestMapper(new DrawNormalizer()));
    }

    private static WindowMetricDispatcher BuildWindowMetricDispatcher()
    {
        var frequency = new FrequencyByDezenaMetric();
        return new WindowMetricDispatcher(frequency, new Top10MaisSorteadosMetric(frequency));
    }

    private static AnalyzeIndicatorStabilityUseCase BuildAnalyzeIndicatorStabilityUseCase()
    {
        return new AnalyzeIndicatorStabilityUseCase(
            new SyntheticFixtureProvider(),
            new DatasetVersionService(),
            new WindowResolver(),
            new V0CrossFieldValidator(),
            new V0RequestMapper(new DrawNormalizer()),
            new IndicatorStabilityAnalyzer());
    }

    private static string GetFixturePath()
    {
        return Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "fixtures", "synthetic_min_window.json"));
    }
}
