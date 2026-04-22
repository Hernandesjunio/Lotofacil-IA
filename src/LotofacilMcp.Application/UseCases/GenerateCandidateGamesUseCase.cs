using LotofacilMcp.Application.Mapping;
using LotofacilMcp.Application.Validation;
using LotofacilMcp.Domain.Metrics;
using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;
using LotofacilMcp.Infrastructure.DatasetVersioning;
using LotofacilMcp.Infrastructure.Providers;

namespace LotofacilMcp.Application.UseCases;

public sealed record GenerateCandidatePlanItemInput(
    string StrategyName,
    int Count,
    string? SearchMethod);

public sealed record GenerateCandidateGamesInput(
    int WindowSize,
    int? EndContestId,
    ulong? Seed,
    IReadOnlyList<GenerateCandidatePlanItemInput> Plan,
    string FixturePath = "");

public sealed record GenerateCandidateGamesDeterministicHashInput(
    int WindowSize,
    int? EndContestId,
    ulong? Seed,
    IReadOnlyList<GenerateCandidatePlanItemInput> Plan);

public sealed record CandidateGameView(
    IReadOnlyList<int> Numbers,
    string StrategyName,
    string StrategyVersion,
    string SearchMethod,
    string TieBreakRule,
    ulong? SeedUsed);

public sealed record GenerateCandidateGamesResult(
    string DatasetVersion,
    string ToolVersion,
    GenerateCandidateGamesDeterministicHashInput DeterministicHashInput,
    WindowDescriptor Window,
    IReadOnlyList<CandidateGameView> CandidateGames);

public sealed class GenerateCandidateGamesUseCase
{
    public const string ToolVersion = "1.0.0";

    private const string CommonRepetitionFrequencyStrategy = "common_repetition_frequency";
    private const string CommonRepetitionFrequencyVersion = "1.0.0";
    private const string TieBreakRule = "lexicographic_numbers_asc";
    private const string DefaultCommonRepetitionFrequencySearchMethod = "greedy_topk";

    private readonly SyntheticFixtureProvider _fixtureProvider;
    private readonly DatasetVersionService _datasetVersionService;
    private readonly WindowResolver _windowResolver;
    private readonly WindowMetricDispatcher _windowMetricDispatcher;
    private readonly V0CrossFieldValidator _validator;
    private readonly V0RequestMapper _mapper;

    public GenerateCandidateGamesUseCase(
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

    public GenerateCandidateGamesResult Execute(GenerateCandidateGamesInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        _validator.ValidateGenerateCandidateGames(input);

        var snapshot = _fixtureProvider.LoadSnapshot(input.FixturePath);
        var normalizedDraws = _mapper.MapSnapshotToDomainDraws(snapshot);

        try
        {
            var window = _windowResolver.Resolve(normalizedDraws, input.WindowSize, input.EndContestId);
            var windowView = _mapper.MapWindow(window);
            var frequencyMetric = _windowMetricDispatcher.Dispatch("frequencia_por_dezena", window);
            var rankedNumbers = BuildFrequencyRanking(frequencyMetric.Value);
            var candidates = BuildCandidates(input, rankedNumbers);

            return new GenerateCandidateGamesResult(
                DatasetVersion: _datasetVersionService.CreateFromSnapshot(snapshot),
                ToolVersion: ToolVersion,
                DeterministicHashInput: new GenerateCandidateGamesDeterministicHashInput(
                    input.WindowSize,
                    input.EndContestId,
                    input.Seed,
                    input.Plan.ToArray()),
                Window: windowView,
                CandidateGames: candidates);
        }
        catch (DomainInvariantViolationException ex)
        {
            throw MapDomainError(ex);
        }
    }

    private static CandidateGameView[] BuildCandidates(
        GenerateCandidateGamesInput input,
        IReadOnlyList<int> rankedNumbers)
    {
        var candidates = new List<CandidateGameView>();
        var sequence = 0;

        foreach (var planItem in input.Plan)
        {
            var effectiveSearchMethod = ResolveSearchMethod(planItem);
            for (var i = 0; i < planItem.Count; i++)
            {
                var numbers = BuildCandidateNumbers(rankedNumbers, input.Seed ?? 0UL, sequence);
                sequence++;
                candidates.Add(new CandidateGameView(
                    Numbers: numbers,
                    StrategyName: CommonRepetitionFrequencyStrategy,
                    StrategyVersion: CommonRepetitionFrequencyVersion,
                    SearchMethod: effectiveSearchMethod,
                    TieBreakRule: TieBreakRule,
                    SeedUsed: RequiresSeed(effectiveSearchMethod) ? input.Seed : null));
            }
        }

        return candidates.ToArray();
    }

    private static IReadOnlyList<int> BuildCandidateNumbers(
        IReadOnlyList<int> rankedNumbers,
        ulong seed,
        int sequence)
    {
        var selected = new int[15];
        var start = (int)((seed + (ulong)sequence) % 25UL);
        for (var i = 0; i < selected.Length; i++)
        {
            selected[i] = rankedNumbers[(start + i) % rankedNumbers.Count];
        }

        Array.Sort(selected);
        return selected;
    }

    private static int[] BuildFrequencyRanking(IReadOnlyList<double> frequencyByDezena)
    {
        if (frequencyByDezena.Count != 25)
        {
            throw new ApplicationValidationException(
                code: "INCOMPATIBLE_COMPOSITION",
                message: "frequencia_por_dezena must resolve as vector_by_dezena with length 25.",
                details: new Dictionary<string, object?>
                {
                    ["metric_name"] = "frequencia_por_dezena",
                    ["value_length"] = frequencyByDezena.Count
                });
        }

        var ranking = Enumerable.Range(1, 25).ToArray();
        Array.Sort(ranking, (a, b) =>
        {
            var fa = frequencyByDezena[a - 1];
            var fb = frequencyByDezena[b - 1];
            var byFrequencyDesc = fb.CompareTo(fa);
            if (byFrequencyDesc != 0)
            {
                return byFrequencyDesc;
            }

            return a.CompareTo(b);
        });
        return ranking;
    }

    private static string ResolveSearchMethod(GenerateCandidatePlanItemInput planItem)
    {
        if (!string.Equals(planItem.StrategyName, CommonRepetitionFrequencyStrategy, StringComparison.Ordinal))
        {
            return planItem.SearchMethod ?? string.Empty;
        }

        return string.IsNullOrWhiteSpace(planItem.SearchMethod)
            ? DefaultCommonRepetitionFrequencySearchMethod
            : planItem.SearchMethod;
    }

    private static bool RequiresSeed(string searchMethod)
    {
        return string.Equals(searchMethod, "sampled", StringComparison.Ordinal) ||
               string.Equals(searchMethod, "greedy_topk", StringComparison.Ordinal);
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

        return new ApplicationValidationException(
            code: "INVALID_REQUEST",
            message: ex.Message,
            details: new Dictionary<string, object?>());
    }
}
