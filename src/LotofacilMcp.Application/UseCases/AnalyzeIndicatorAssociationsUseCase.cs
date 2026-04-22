using LotofacilMcp.Application.Mapping;
using LotofacilMcp.Application.Validation;
using LotofacilMcp.Domain.Analytics;
using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;
using LotofacilMcp.Infrastructure.DatasetVersioning;
using LotofacilMcp.Infrastructure.Providers;

namespace LotofacilMcp.Application.UseCases;

public sealed record AssociationMagnitudeEntryView(
    string IndicatorA,
    string AggregationA,
    int? ComponentIndexA,
    string IndicatorB,
    string AggregationB,
    int? ComponentIndexB,
    double AssociationStrength,
    string Explanation);

public sealed record AssociationMagnitudeView(
    string Method,
    IReadOnlyList<AssociationMagnitudeEntryView> TopPairs);

public sealed record AnalyzeIndicatorAssociationsInput(
    int WindowSize,
    int? EndContestId,
    IReadOnlyList<StabilityIndicatorRequestInput> Items,
    string Method,
    int TopK,
    string FixturePath = "");

public sealed record AnalyzeIndicatorAssociationsDeterministicHashInput(
    int WindowSize,
    int? EndContestId,
    string Method,
    int TopK,
    IReadOnlyList<StabilityIndicatorRequestInput> Items);

public sealed record AnalyzeIndicatorAssociationsResult(
    string DatasetVersion,
    string ToolVersion,
    AnalyzeIndicatorAssociationsDeterministicHashInput DeterministicHashInput,
    WindowDescriptor Window,
    string Method,
    AssociationMagnitudeView AssociationMagnitude,
    IReadOnlyDictionary<string, object?>? AssociationStability);

public sealed class AnalyzeIndicatorAssociationsUseCase
{
    public const string ToolVersion = "1.0.0";

    private readonly SyntheticFixtureProvider _fixtureProvider;
    private readonly DatasetVersionService _datasetVersionService;
    private readonly WindowResolver _windowResolver;
    private readonly V0CrossFieldValidator _validator;
    private readonly V0RequestMapper _mapper;
    private readonly IndicatorAssociationAnalyzer _associationAnalyzer;

    public AnalyzeIndicatorAssociationsUseCase(
        SyntheticFixtureProvider fixtureProvider,
        DatasetVersionService datasetVersionService,
        WindowResolver windowResolver,
        V0CrossFieldValidator validator,
        V0RequestMapper mapper,
        IndicatorAssociationAnalyzer associationAnalyzer)
    {
        _fixtureProvider = fixtureProvider;
        _datasetVersionService = datasetVersionService;
        _windowResolver = windowResolver;
        _validator = validator;
        _mapper = mapper;
        _associationAnalyzer = associationAnalyzer;
    }

    public AnalyzeIndicatorAssociationsResult Execute(AnalyzeIndicatorAssociationsInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        _validator.ValidateAnalyzeIndicatorAssociations(input);

        var snapshot = _fixtureProvider.LoadSnapshot(input.FixturePath);
        var normalizedDraws = _mapper.MapSnapshotToDomainDraws(snapshot);

        try
        {
            var window = _windowResolver.Resolve(normalizedDraws, input.WindowSize, input.EndContestId);
            var items = input.Items!
                .Select(request => new StabilityIndicatorRequest(request.Name, request.Aggregation))
                .ToArray();

            var magnitude = _associationAnalyzer.ComputeSpearmanMagnitude(
                window,
                items,
                input.TopK);

            var windowView = _mapper.MapWindow(window);
            return new AnalyzeIndicatorAssociationsResult(
                DatasetVersion: _datasetVersionService.CreateFromSnapshot(snapshot),
                ToolVersion: ToolVersion,
                DeterministicHashInput: new AnalyzeIndicatorAssociationsDeterministicHashInput(
                    input.WindowSize,
                    input.EndContestId,
                    "spearman",
                    input.TopK,
                    input.Items!.ToArray()),
                Window: windowView,
                Method: "spearman",
                AssociationMagnitude: new AssociationMagnitudeView(
                    magnitude.Method,
                    magnitude.TopPairs
                        .Select(entry => new AssociationMagnitudeEntryView(
                            entry.IndicatorA,
                            entry.AggregationA,
                            entry.ComponentIndexA,
                            entry.IndicatorB,
                            entry.AggregationB,
                            entry.ComponentIndexB,
                            entry.AssociationStrength,
                            entry.Explanation))
                        .ToArray()),
                AssociationStability: null);
        }
        catch (DomainInvariantViolationException ex)
        {
            throw MapDomainError(ex);
        }
    }

    private static ApplicationValidationException MapDomainError(DomainInvariantViolationException ex)
    {
        if (ex.Message.Contains("aggregation is required for non-scalar", StringComparison.Ordinal))
        {
            return new ApplicationValidationException(
                code: "UNSUPPORTED_AGGREGATION",
                message: "vector or multivalue series requires an explicit aggregation for association.",
                details: new Dictionary<string, object?>());
        }

        if (ex.Message.Contains("unknown indicator requested", StringComparison.Ordinal) ||
            ex.Message.StartsWith("UNKNOWN_METRIC", StringComparison.Ordinal))
        {
            return new ApplicationValidationException(
                code: "UNKNOWN_METRIC",
                message: "requested metric is not available in V0.",
                details: new Dictionary<string, object?>());
        }

        if (ex.Message.Contains("unsupported aggregation for indicator", StringComparison.Ordinal) ||
            ex.Message.Contains("unsupported aggregation for scalar", StringComparison.Ordinal))
        {
            return new ApplicationValidationException(
                code: "UNSUPPORTED_AGGREGATION",
                message: "aggregation is not valid for the requested association.",
                details: new Dictionary<string, object?>());
        }

        if (ex.Message.Contains("at least two resolved scalar series", StringComparison.Ordinal))
        {
            return new ApplicationValidationException(
                code: "INVALID_REQUEST",
                message: "at least two compatible scalar series are required to compute association.",
                details: new Dictionary<string, object?>());
        }

        return new ApplicationValidationException(
            code: "INCOMPATIBLE_INDICATOR_FOR_STABILITY",
            message: ex.Message,
            details: new Dictionary<string, object?>());
    }
}
