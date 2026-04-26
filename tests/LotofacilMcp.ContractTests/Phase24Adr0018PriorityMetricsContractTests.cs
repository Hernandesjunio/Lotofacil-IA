using LotofacilMcp.Server.Tools;

namespace LotofacilMcp.ContractTests;

public sealed class Phase24Adr0018PriorityMetricsContractTests
{
    [Fact]
    public void FamilyA_ParesImpares_IsImplementedInDiscovery_AndNotExposedInComputeWindowMetrics()
    {
        var sut = new V0Tools(ContractTestFixturePaths.SyntheticMinWindowJson());
        var discover = Assert.IsType<DiscoverCapabilitiesResponse>(sut.DiscoverCapabilities(new DiscoverCapabilitiesRequest()));

        Assert.Contains("pares_impares", discover.Metrics.ImplementedMetricNames);
        Assert.DoesNotContain("pares_impares", discover.Metrics.ComputeWindowMetricsAllowed);
    }

    [Fact]
    public void FamilyB_MatrizNumeroSlot_IsExposedInCompute_AndCandidateSlotMetricsStayOutOfCompute()
    {
        var sut = new V0Tools(ContractTestFixturePaths.SyntheticMinWindowJson());
        var response = sut.ComputeWindowMetrics(new ComputeWindowMetricsRequest(
            WindowSize: 5,
            EndContestId: 1005,
            Metrics: [new MetricRequest("matriz_numero_slot")]));

        var payload = Assert.IsType<ComputeWindowMetricsResponse>(response);
        var metric = Assert.Single(payload.Metrics);
        Assert.Equal("matriz_numero_slot", metric.MetricName);
        Assert.Equal("window", metric.Scope);
        Assert.Equal("count_matrix[25x15]", metric.Shape);
        Assert.Equal("count", metric.Unit);
        Assert.Equal(25 * 15, metric.Value.Count);

        var discover = Assert.IsType<DiscoverCapabilitiesResponse>(sut.DiscoverCapabilities(new DiscoverCapabilitiesRequest()));
        Assert.Contains("analise_slot", discover.Metrics.ImplementedMetricNames);
        Assert.Contains("surpresa_slot", discover.Metrics.ImplementedMetricNames);
        Assert.Contains("matriz_numero_slot", discover.Metrics.ComputeWindowMetricsAllowed);
        Assert.DoesNotContain("analise_slot", discover.Metrics.ComputeWindowMetricsAllowed);
        Assert.DoesNotContain("surpresa_slot", discover.Metrics.ComputeWindowMetricsAllowed);
    }

    [Fact]
    public void FamilyC_BlocosMetrics_AreExposedInComputeAndReturnExpectedContracts()
    {
        var sut = new V0Tools(ContractTestFixturePaths.SyntheticMinWindowJson());
        var response = sut.ComputeWindowMetrics(new ComputeWindowMetricsRequest(
            WindowSize: 5,
            EndContestId: 1005,
            Metrics:
            [
                new MetricRequest("frequencia_blocos"),
                new MetricRequest("ausencia_blocos"),
                new MetricRequest("estado_atual_dezena")
            ]));

        var payload = Assert.IsType<ComputeWindowMetricsResponse>(response);
        Assert.Equal(3, payload.Metrics.Count);

        var frequenciaBlocos = payload.Metrics[0];
        Assert.Equal("frequencia_blocos", frequenciaBlocos.MetricName);
        Assert.Equal("count_list_by_dezena", frequenciaBlocos.Shape);
        Assert.Equal("window", frequenciaBlocos.Scope);
        Assert.NotEmpty(frequenciaBlocos.Value);

        var ausenciaBlocos = payload.Metrics[1];
        Assert.Equal("ausencia_blocos", ausenciaBlocos.MetricName);
        Assert.Equal("count_list_by_dezena", ausenciaBlocos.Shape);
        Assert.Equal("window", ausenciaBlocos.Scope);
        Assert.NotEmpty(ausenciaBlocos.Value);

        var estadoAtual = payload.Metrics[2];
        Assert.Equal("estado_atual_dezena", estadoAtual.MetricName);
        Assert.Equal("vector_by_dezena", estadoAtual.Shape);
        Assert.Equal("window", estadoAtual.Scope);
        Assert.Equal(25, estadoAtual.Value.Count);
    }

    [Fact]
    public void FamilyD_EstabilidadeRanking_IsExposedInCompute_WhileKlAndPersistenciaStayDiscoveryOnly()
    {
        var sut = new V0Tools(ContractTestFixturePaths.SyntheticMinWindowJson());
        var response = sut.ComputeWindowMetrics(new ComputeWindowMetricsRequest(
            WindowSize: 5,
            EndContestId: 1005,
            Metrics: [new MetricRequest("estabilidade_ranking")]));

        var payload = Assert.IsType<ComputeWindowMetricsResponse>(response);
        var metric = Assert.Single(payload.Metrics);
        Assert.Equal("estabilidade_ranking", metric.MetricName);
        Assert.Equal("scalar", metric.Shape);
        Assert.Equal("window", metric.Scope);
        Assert.Single(metric.Value);
        Assert.InRange(metric.Value[0], 0d, 1d);

        var discover = Assert.IsType<DiscoverCapabilitiesResponse>(sut.DiscoverCapabilities(new DiscoverCapabilitiesRequest()));
        Assert.Contains("divergencia_kl", discover.Metrics.ImplementedMetricNames);
        Assert.Contains("persistencia_atraso_extremo", discover.Metrics.ImplementedMetricNames);
        Assert.DoesNotContain("divergencia_kl", discover.Metrics.ComputeWindowMetricsAllowed);
        Assert.DoesNotContain("persistencia_atraso_extremo", discover.Metrics.ComputeWindowMetricsAllowed);
    }

    [Fact]
    public void FamilyE_RunsAndOutlier_AppearAsImplemented_InDiscovery_NotInCompute()
    {
        var sut = new V0Tools(ContractTestFixturePaths.SyntheticMinWindowJson());
        var discover = Assert.IsType<DiscoverCapabilitiesResponse>(sut.DiscoverCapabilities(new DiscoverCapabilitiesRequest()));

        Assert.Contains("estatistica_runs", discover.Metrics.ImplementedMetricNames);
        Assert.Contains("outlier_score", discover.Metrics.ImplementedMetricNames);
        Assert.DoesNotContain("estatistica_runs", discover.Metrics.ComputeWindowMetricsAllowed);
        Assert.DoesNotContain("outlier_score", discover.Metrics.ComputeWindowMetricsAllowed);
    }
}
