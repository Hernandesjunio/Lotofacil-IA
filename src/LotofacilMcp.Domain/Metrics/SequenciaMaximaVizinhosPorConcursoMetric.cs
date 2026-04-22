using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Metrics;

public sealed class SequenciaMaximaVizinhosPorConcursoMetric
{
    public WindowMetricValue Compute(DrawWindow window)
    {
        if (window is null)
        {
            throw new DomainInvariantViolationException("window cannot be null.");
        }

        if (window.Draws.Count != window.Size)
        {
            throw new DomainInvariantViolationException(
                "sequencia_maxima_vizinhos_por_concurso: draw count must match the resolved window size.");
        }

        var series = new int[window.Size];
        for (var i = 0; i < window.Size; i++)
        {
            series[i] = VizinhosConsecutivos.MaxConsecutiveAdjacencyRunLength(window.Draws[i].Numbers);
        }

        return new WindowMetricValue(
            MetricName: "sequencia_maxima_vizinhos_por_concurso",
            Scope: "series",
            Shape: "series",
            Unit: "count",
            Version: "1.0.0",
            Value: Array.ConvertAll(series, static x => (double)x));
    }
}
