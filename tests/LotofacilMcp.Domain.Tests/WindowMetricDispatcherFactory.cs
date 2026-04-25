using LotofacilMcp.Domain.Metrics;

namespace LotofacilMcp.Domain.Tests;

internal static class WindowMetricDispatcherFactory
{
    public static WindowMetricDispatcher Create()
    {
        var frequency = new FrequencyByDezenaMetric();
        var total = new TotalDePresencasNaJanelaPorDezenaMetric(frequency);
        return new WindowMetricDispatcher(
            frequency,
            total,
            new Top10MaisSorteadosMetric(frequency),
            new Top10MenosSorteadosMetric(frequency),
            new Top10MaioresTotaisDePresencasNaJanelaMetric(total),
            new Top10MenoresTotaisDePresencasNaJanelaMetric(total),
            new SequenciaAtualDePresencasPorDezenaMetric(),
            new ParesNoConcursoMetric(),
            new RepeticaoConcursoAnteriorMetric(),
            new QuantidadeVizinhosPorConcursoMetric(),
            new SequenciaMaximaVizinhosPorConcursoMetric(),
            new DistribuicaoLinhaPorConcursoMetric(),
            new DistribuicaoColunaPorConcursoMetric(),
            new EntropiaLinhaPorConcursoMetric(),
            new EntropiaColunaPorConcursoMetric(),
            new HhiLinhaPorConcursoMetric(),
            new HhiColunaPorConcursoMetric(),
            new AtrasoPorDezenaMetric(),
            new AssimetriaBlocosMetric(),
            new MatrizNumeroSlotMetric(),
            new FrequenciaBlocosMetric(),
            new AusenciaBlocosMetric(),
            new EstadoAtualDezenaMetric(),
            new EstabilidadeRankingMetric());
    }
}
