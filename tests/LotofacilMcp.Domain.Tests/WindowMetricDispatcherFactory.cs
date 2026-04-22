using LotofacilMcp.Domain.Metrics;

namespace LotofacilMcp.Domain.Tests;

internal static class WindowMetricDispatcherFactory
{
    public static WindowMetricDispatcher Create()
    {
        var frequency = new FrequencyByDezenaMetric();
        return new WindowMetricDispatcher(
            frequency,
            new Top10MaisSorteadosMetric(frequency),
            new Top10MenosSorteadosMetric(frequency),
            new ParesNoConcursoMetric(),
            new RepeticaoConcursoAnteriorMetric(),
            new QuantidadeVizinhosPorConcursoMetric(),
            new SequenciaMaximaVizinhosPorConcursoMetric(),
            new DistribuicaoLinhaPorConcursoMetric(),
            new DistribuicaoColunaPorConcursoMetric(),
            new EntropiaLinhaPorConcursoMetric(),
            new EntropiaColunaPorConcursoMetric(),
            new HhiLinhaPorConcursoMetric(),
            new HhiColunaPorConcursoMetric());
    }
}
