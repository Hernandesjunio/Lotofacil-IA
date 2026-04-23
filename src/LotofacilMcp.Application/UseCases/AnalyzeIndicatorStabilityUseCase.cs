using LotofacilMcp.Application.Mapping;
using LotofacilMcp.Application.Validation;
using LotofacilMcp.Domain.Analytics;
using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;
using LotofacilMcp.Infrastructure.DatasetVersioning;
using LotofacilMcp.Infrastructure.Providers;

namespace LotofacilMcp.Application.UseCases;

public sealed record AnalyzeIndicatorStabilityInput(
    int WindowSize,
    int? EndContestId,
    IReadOnlyList<StabilityIndicatorRequestInput>? Indicators,
    string? NormalizationMethod,
    int TopK,
    int MinHistory,
    string FixturePath = "");

public sealed record AnalyzeIndicatorStabilityDeterministicHashInput(
    int WindowSize,
    int? EndContestId,
    string NormalizationMethod,
    int TopK,
    int MinHistory,
    IReadOnlyList<StabilityIndicatorRequestInput> Indicators);

public sealed record AnalyzeIndicatorStabilityResult(
    string DatasetVersion,
    string ToolVersion,
    AnalyzeIndicatorStabilityDeterministicHashInput DeterministicHashInput,
    WindowDescriptor Window,
    string NormalizationMethod,
    IReadOnlyList<StabilityRankingEntryView> Ranking);

public sealed class AnalyzeIndicatorStabilityUseCase
{
    public const string ToolVersion = "1.0.0";

    private readonly SyntheticFixtureProvider _fixtureProvider;
    private readonly DatasetVersionService _datasetVersionService;
    private readonly WindowResolver _windowResolver;
    private readonly V0CrossFieldValidator _validator;
    private readonly V0RequestMapper _mapper;
    private readonly IndicatorStabilityAnalyzer _analyzer;

    public AnalyzeIndicatorStabilityUseCase(
        SyntheticFixtureProvider fixtureProvider,
        DatasetVersionService datasetVersionService,
        WindowResolver windowResolver,
        V0CrossFieldValidator validator,
        V0RequestMapper mapper,
        IndicatorStabilityAnalyzer analyzer)
    {
        _fixtureProvider = fixtureProvider;
        _datasetVersionService = datasetVersionService;
        _windowResolver = windowResolver;
        _validator = validator;
        _mapper = mapper;
        _analyzer = analyzer;
    }

    public AnalyzeIndicatorStabilityResult Execute(AnalyzeIndicatorStabilityInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        _validator.ValidateAnalyzeIndicatorStability(input);

        var normalizationMethod = string.IsNullOrWhiteSpace(input.NormalizationMethod)
            ? "madn"
            : input.NormalizationMethod!;

        var snapshot = _fixtureProvider.LoadSnapshot(input.FixturePath);
        var normalizedDraws = _mapper.MapSnapshotToDomainDraws(snapshot);

        try
        {
            var window = _windowResolver.Resolve(normalizedDraws, input.WindowSize, input.EndContestId);
            ValidateMinHistoryAgainstResolvedWindow(input.MinHistory, window.Draws.Count);
            var analysis = _analyzer.Analyze(
                window,
                input.Indicators!
                    .Select(indicator => new StabilityIndicatorRequest(indicator.Name, indicator.Aggregation))
                    .ToArray(),
                normalizationMethod,
                input.TopK,
                input.MinHistory);
            var windowView = _mapper.MapWindow(window);

            return new AnalyzeIndicatorStabilityResult(
                DatasetVersion: _datasetVersionService.CreateFromSnapshot(snapshot),
                ToolVersion: ToolVersion,
                DeterministicHashInput: new AnalyzeIndicatorStabilityDeterministicHashInput(
                    input.WindowSize,
                    input.EndContestId,
                    normalizationMethod,
                    input.TopK,
                    input.MinHistory,
                    input.Indicators!.ToArray()),
                Window: windowView,
                NormalizationMethod: analysis.NormalizationMethod,
                Ranking: analysis.Ranking
                    .Select(entry => new StabilityRankingEntryView(
                        entry.IndicatorName,
                        entry.Aggregation,
                        entry.ComponentIndex,
                        entry.Shape,
                        entry.Dispersion,
                        entry.StabilityScore,
                        entry.Explanation))
                    .ToArray());
        }
        catch (DomainInvariantViolationException ex)
        {
            throw MapDomainError(ex);
        }
    }

    private static ApplicationValidationException MapDomainError(DomainInvariantViolationException ex)
    {
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

        if (ex.Message.Contains("unknown indicator", StringComparison.Ordinal))
        {
            return new ApplicationValidationException(
                code: "UNKNOWN_METRIC",
                message: ex.Message,
                details: new Dictionary<string, object?>());
        }

        if (ex.Message.Contains("unsupported aggregation", StringComparison.Ordinal) ||
            ex.Message.Contains("aggregation is required", StringComparison.Ordinal))
        {
            return new ApplicationValidationException(
                code: "UNSUPPORTED_AGGREGATION",
                message: ex.Message,
                details: new Dictionary<string, object?>());
        }

        if (ex.Message.Contains("unsupported normalization_method", StringComparison.Ordinal) ||
            ex.Message.Contains("coefficient_of_variation", StringComparison.Ordinal))
        {
            return new ApplicationValidationException(
                code: "UNSUPPORTED_NORMALIZATION_METHOD",
                message: ex.Message,
                details: new Dictionary<string, object?>());
        }

        return new ApplicationValidationException(
            code: "INCOMPATIBLE_INDICATOR_FOR_STABILITY",
            message: ex.Message,
            details: new Dictionary<string, object?>());
    }

    private static void ValidateMinHistoryAgainstResolvedWindow(int minHistory, int effectiveWindowSize)
    {
        if (minHistory <= effectiveWindowSize)
        {
            return;
        }

        throw new ApplicationValidationException(
            code: "INSUFFICIENT_HISTORY",
            message: "min_history exceeds the effective resolved window size.",
            details: new Dictionary<string, object?>
            {
                ["min_history"] = minHistory,
                ["effective_window_size"] = effectiveWindowSize
            });
    }
}
