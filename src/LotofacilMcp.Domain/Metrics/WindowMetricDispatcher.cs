using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Metrics;

public sealed class WindowMetricDispatcher
{
    private readonly IReadOnlyDictionary<string, Func<DrawWindow, WindowMetricValue>> _dispatchByMetricName;

    public WindowMetricDispatcher(
        FrequencyByDezenaMetric frequencyByDezenaMetric,
        Top10MaisSorteadosMetric top10MaisSorteadosMetric,
        Top10MenosSorteadosMetric top10MenosSorteadosMetric,
        ParesNoConcursoMetric paresNoConcursoMetric,
        RepeticaoConcursoAnteriorMetric repeticaoConcursoAnteriorMetric,
        QuantidadeVizinhosPorConcursoMetric quantidadeVizinhosPorConcursoMetric,
        SequenciaMaximaVizinhosPorConcursoMetric sequenciaMaximaVizinhosPorConcursoMetric,
        DistribuicaoLinhaPorConcursoMetric distribuicaoLinhaPorConcursoMetric)
    {
        ArgumentNullException.ThrowIfNull(frequencyByDezenaMetric);
        ArgumentNullException.ThrowIfNull(top10MaisSorteadosMetric);
        ArgumentNullException.ThrowIfNull(top10MenosSorteadosMetric);
        ArgumentNullException.ThrowIfNull(paresNoConcursoMetric);
        ArgumentNullException.ThrowIfNull(repeticaoConcursoAnteriorMetric);
        ArgumentNullException.ThrowIfNull(quantidadeVizinhosPorConcursoMetric);
        ArgumentNullException.ThrowIfNull(sequenciaMaximaVizinhosPorConcursoMetric);
        ArgumentNullException.ThrowIfNull(distribuicaoLinhaPorConcursoMetric);

        _dispatchByMetricName = new Dictionary<string, Func<DrawWindow, WindowMetricValue>>(StringComparer.Ordinal)
        {
            ["frequencia_por_dezena"] = frequencyByDezenaMetric.Compute,
            ["top10_mais_sorteados"] = top10MaisSorteadosMetric.Compute,
            ["top10_menos_sorteados"] = top10MenosSorteadosMetric.Compute,
            ["pares_no_concurso"] = paresNoConcursoMetric.Compute,
            ["repeticao_concurso_anterior"] = repeticaoConcursoAnteriorMetric.Compute,
            ["quantidade_vizinhos_por_concurso"] = quantidadeVizinhosPorConcursoMetric.Compute,
            ["sequencia_maxima_vizinhos_por_concurso"] = sequenciaMaximaVizinhosPorConcursoMetric.Compute,
            ["distribuicao_linha_por_concurso"] = distribuicaoLinhaPorConcursoMetric.Compute
        };
    }

    public WindowMetricValue Dispatch(string metricName, DrawWindow window)
    {
        if (string.IsNullOrWhiteSpace(metricName))
        {
            throw new DomainInvariantViolationException("UNKNOWN_METRIC: ");
        }

        if (_dispatchByMetricName.TryGetValue(metricName, out var computeMetric))
        {
            return computeMetric(window);
        }

        throw new DomainInvariantViolationException($"UNKNOWN_METRIC: {metricName}");
    }
}
