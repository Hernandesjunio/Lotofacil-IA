using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;

namespace LotofacilMcp.Domain.Metrics;

public sealed class Top10MaioresTotaisDePresencasNaJanelaMetric
{
    private readonly TotalDePresencasNaJanelaPorDezenaMetric _totalByDezena;

    public Top10MaioresTotaisDePresencasNaJanelaMetric(TotalDePresencasNaJanelaPorDezenaMetric totalByDezena)
    {
        _totalByDezena = totalByDezena ?? throw new ArgumentNullException(nameof(totalByDezena));
    }

    public WindowMetricValue Compute(DrawWindow window)
    {
        if (window is null)
        {
            throw new DomainInvariantViolationException("window cannot be null.");
        }

        var totals = _totalByDezena.Compute(window).Value;

        var top10 = Enumerable.Range(1, 25)
            .OrderByDescending(d => totals[d - 1])
            .ThenBy(d => d)
            .Take(10)
            .ToArray();

        return new WindowMetricValue(
            MetricName: "top10_maiores_totais_de_presencas_na_janela",
            Scope: "window",
            Shape: "dezena_list[10]",
            Unit: "dimensionless",
            Version: "1.0.0",
            Value: Array.ConvertAll(top10, static d => (double)d));
    }
}
