using LotofacilMcp.Application.Composition;
using LotofacilMcp.Application.Mapping;
using LotofacilMcp.Application.Validation;
using LotofacilMcp.Domain.Metrics;
using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;
using LotofacilMcp.Infrastructure.DatasetVersioning;
using LotofacilMcp.Infrastructure.Providers;

namespace LotofacilMcp.Application.UseCases;

public sealed class ComposeIndicatorAnalysisUseCase
{
    public const string ToolVersion = "1.0.0";

    private const int DezenaVectorLength = 25;

    private readonly SyntheticFixtureProvider _fixtureProvider;
    private readonly DatasetVersionService _datasetVersionService;
    private readonly WindowResolver _windowResolver;
    private readonly WindowMetricDispatcher _windowMetricDispatcher;
    private readonly V0CrossFieldValidator _validator;
    private readonly V0RequestMapper _mapper;

    public ComposeIndicatorAnalysisUseCase(
        SyntheticFixtureProvider fixtureProvider,
        DatasetVersionService datasetVersionService,
        WindowResolver windowResolver,
        WindowMetricDispatcher windowMetricDispatcher,
        V0CrossFieldValidator validator,
        V0RequestMapper mapper)
    {
        _fixtureProvider = fixtureProvider;
        _datasetVersionService = datasetVersionService;
        _windowResolver = windowResolver;
        _windowMetricDispatcher = windowMetricDispatcher;
        _validator = validator;
        _mapper = mapper;
    }

    public ComposeIndicatorAnalysisResult Execute(ComposeIndicatorAnalysisInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        _validator.ValidateComposeIndicatorAnalysis(input);

        var snapshot = _fixtureProvider.LoadSnapshot(input.FixturePath);
        var normalizedDraws = _mapper.MapSnapshotToDomainDraws(snapshot);

        try
        {
            var window = _windowResolver.Resolve(normalizedDraws, input.WindowSize, input.EndContestId);
            var windowView = _mapper.MapWindow(window);
            var scores = new double[DezenaVectorLength];
            foreach (var component in input.Components!)
            {
                var metric = _windowMetricDispatcher.Dispatch(component.MetricName, window);
                if (metric.Value.Count != DezenaVectorLength)
                {
                    throw new ApplicationValidationException(
                        code: "INCOMPATIBLE_COMPOSITION",
                        message: "metric is not a dezena vector compatible with the requested target.",
                        details: new Dictionary<string, object?>
                        {
                            ["metric_name"] = component.MetricName,
                            ["value_length"] = metric.Value.Count
                        });
                }

                var transformed = IndicatorTransformFunctions.Apply(component.Transform, metric.Value);
                for (var d = 0; d < DezenaVectorLength; d++)
                {
                    scores[d] += component.Weight * transformed[d];
                }
            }

            var order = new int[DezenaVectorLength];
            for (var i = 0; i < DezenaVectorLength; i++)
            {
                order[i] = i;
            }

            Array.Sort(order, (a, b) =>
            {
                var sa = scores[a];
                var sb = scores[b];
                var c = sb.CompareTo(sa);
                if (c != 0)
                {
                    return c;
                }

                return a.CompareTo(b);
            });

            var topK = Math.Min(input.TopK, DezenaVectorLength);
            var ranking = new List<WeightedDezenaRankingEntryView>(topK);
            for (var r = 0; r < topK; r++)
            {
                var dezena = order[r] + 1;
                var idx = order[r];
                var explanation = $"weighted sum of transformed components; position {r + 1} after score desc and dezena asc tie-break";
                ranking.Add(new WeightedDezenaRankingEntryView(
                    Dezena: dezena,
                    Rank: r + 1,
                    Score: scores[idx],
                    Explanation: explanation));
            }

            return new ComposeIndicatorAnalysisResult(
                DatasetVersion: _datasetVersionService.CreateFromSnapshot(snapshot),
                ToolVersion: ToolVersion,
                DeterministicHashInput: new ComposeIndicatorAnalysisDeterministicHashInput(
                    input.WindowSize,
                    input.EndContestId,
                    input.Target!,
                    input.Operator!,
                    input.TopK,
                    input.Components!.ToArray()),
                Window: windowView,
                Target: "dezena",
                Operator: "weighted_rank",
                Ranking: ranking);
        }
        catch (DomainInvariantViolationException ex)
        {
            throw MapDomainError(ex);
        }
    }

    private static ApplicationValidationException MapDomainError(DomainInvariantViolationException ex)
    {
        if (ex.Message.StartsWith("UNKNOWN_METRIC:", StringComparison.Ordinal))
        {
            var metricName = ex.Message["UNKNOWN_METRIC:".Length..].Trim();
            var details = new Dictionary<string, object?>
            {
                ["metric_name"] = metricName
            };

            if (MetricAvailabilityCatalog.IsKnownMetric(metricName))
            {
                details["allowed_metrics"] = MetricAvailabilityCatalog.GetComposeIndicatorAnalysisAllowedComponents().ToArray();
            }

            return new ApplicationValidationException(
                code: "UNKNOWN_METRIC",
                message: "requested metric is not available in V0.",
                details: details);
        }

        if (ex.Message.Contains("requested end_contest_id", StringComparison.Ordinal))
        {
            return new ApplicationValidationException(
                code: "INVALID_CONTEST_ID",
                message: ex.Message,
                details: new Dictionary<string, object?>());
        }

        if (ex.Message.Contains("insufficient history", StringComparison.Ordinal))
        {
            return new ApplicationValidationException(
                code: "INSUFFICIENT_HISTORY",
                message: ex.Message,
                details: new Dictionary<string, object?>());
        }

        return new ApplicationValidationException(
            code: "INVALID_REQUEST",
            message: ex.Message,
            details: new Dictionary<string, object?>());
    }
}
