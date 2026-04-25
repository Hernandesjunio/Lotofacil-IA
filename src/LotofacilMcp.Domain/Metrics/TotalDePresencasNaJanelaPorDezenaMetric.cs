using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Metrics;

public sealed class TotalDePresencasNaJanelaPorDezenaMetric
{
    private readonly FrequencyByDezenaMetric _frequencyByDezena;

    public TotalDePresencasNaJanelaPorDezenaMetric(FrequencyByDezenaMetric frequencyByDezena)
    {
        _frequencyByDezena = frequencyByDezena ?? throw new ArgumentNullException(nameof(frequencyByDezena));
    }

    public WindowMetricValue Compute(DrawWindow window)
    {
        if (window is null)
        {
            throw new DomainInvariantViolationException("window cannot be null.");
        }

        var freq = _frequencyByDezena.Compute(window);

        return freq with
        {
            MetricName = "total_de_presencas_na_janela_por_dezena"
        };
    }
}
