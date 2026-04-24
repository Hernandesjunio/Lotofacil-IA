using LotofacilMcp.Application.Mapping;
using LotofacilMcp.Application.Validation;
using LotofacilMcp.Domain.Metrics;
using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;
using LotofacilMcp.Infrastructure.DatasetVersioning;
using LotofacilMcp.Infrastructure.Providers;

namespace LotofacilMcp.Application.UseCases;

public sealed record GenerateCandidateCriteriaInput(
    string Name,
    double Value);

public sealed record GenerateCandidateWeightInput(
    string Name,
    double Weight);

public sealed record GenerateCandidateFilterInput(
    string Name,
    double? Value,
    double? Min,
    double? Max,
    string? Version);

public sealed record GenerateGlobalConstraintsInput(
    bool? UniqueGames,
    bool? SortedNumbers);

public sealed record GenerateRepeatRangeInput(
    int? Min,
    int? Max);

public sealed record GenerateStructuralExclusionsInput(
    double? MaxConsecutiveRun,
    double? MaxNeighborCount,
    double? MinRowEntropyNorm,
    double? MinColumnEntropyNorm,
    double? MaxHhiLinha,
    double? MaxHhiColuna,
    GenerateRepeatRangeInput? RepeatRange,
    double? MinSlotAlignment,
    double? MaxOutlierScore);

public sealed record GenerateCandidatePlanItemInput(
    string StrategyName,
    int Count,
    string? StrategyVersion,
    string? SearchMethod,
    string? TieBreakRule,
    IReadOnlyList<GenerateCandidateCriteriaInput>? Criteria,
    IReadOnlyList<GenerateCandidateWeightInput>? Weights,
    IReadOnlyList<GenerateCandidateFilterInput>? Filters);

public sealed record GenerateCandidateGamesInput(
    int WindowSize,
    int? EndContestId,
    ulong? Seed,
    IReadOnlyList<GenerateCandidatePlanItemInput> Plan,
    GenerateGlobalConstraintsInput? GlobalConstraints,
    GenerateStructuralExclusionsInput? StructuralExclusions,
    string FixturePath = "");

public sealed record GenerateCandidateGamesDeterministicHashInput(
    int WindowSize,
    int? EndContestId,
    ulong? Seed,
    IReadOnlyList<GenerateCandidatePlanItemInput> Plan,
    GenerateGlobalConstraintsInput GlobalConstraints,
    GenerateStructuralExclusionsInput StructuralExclusions);

public sealed record AppliedConfigurationView(
    IReadOnlyList<GenerateCandidateCriteriaInput> Criteria,
    IReadOnlyList<GenerateCandidateWeightInput> Weights,
    IReadOnlyList<GenerateCandidateFilterInput> Filters,
    IReadOnlyDictionary<string, object?> ResolvedDefaults);

public sealed record CandidateGameView(
    IReadOnlyList<int> Numbers,
    string StrategyName,
    string StrategyVersion,
    string SearchMethod,
    string TieBreakRule,
    ulong? SeedUsed,
    AppliedConfigurationView AppliedConfiguration);

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
    private const string DeclaredCompositeProfileStrategy = "declared_composite_profile";
    private const string CommonRepetitionFrequencyVersion = "1.0.0";
    private const string DeclaredCompositeProfileVersion = "1.0.0";
    private const string CommonTieBreakRule = "lexicographic_numbers_asc";
    private const string DeclaredTieBreakRule = "outlier_score_asc_then_hhi_linha_asc_then_lexicographic_numbers_asc";
    private const string DefaultCommonRepetitionFrequencySearchMethod = "greedy_topk";
    private const string DefaultDeclaredCompositeSearchMethod = "sampled";

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
            var top10Metric = _windowMetricDispatcher.Dispatch("top10_mais_sorteados", window);
            var repetitionMetric = _windowMetricDispatcher.Dispatch("repeticao_concurso_anterior", window);

            var rankedNumbers = BuildFrequencyRanking(frequencyMetric.Value);
            var context = BuildGenerationContext(window, frequencyMetric.Value, top10Metric.Value, repetitionMetric.Value);
            var resolvedGlobalConstraints = ResolveGlobalConstraints(input.GlobalConstraints);
            var resolvedStructuralExclusions = ResolveStructuralExclusions(input.StructuralExclusions);
            var candidates = BuildCandidates(input, rankedNumbers, context, resolvedGlobalConstraints, resolvedStructuralExclusions);

            return new GenerateCandidateGamesResult(
                DatasetVersion: _datasetVersionService.CreateFromSnapshot(snapshot),
                ToolVersion: ToolVersion,
                DeterministicHashInput: new GenerateCandidateGamesDeterministicHashInput(
                    input.WindowSize,
                    input.EndContestId,
                    input.Seed,
                    input.Plan.ToArray(),
                    resolvedGlobalConstraints,
                    resolvedStructuralExclusions),
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
        IReadOnlyList<int> rankedNumbers,
        GenerationContext context,
        GenerateGlobalConstraintsInput globalConstraints,
        GenerateStructuralExclusionsInput structuralExclusions)
    {
        var candidates = new List<CandidateGameView>();
        var usedGameKeys = new HashSet<string>(StringComparer.Ordinal);
        var sequenceSeed = 0;

        for (var planIndex = 0; planIndex < input.Plan.Count; planIndex++)
        {
            var planItem = input.Plan[planIndex];
            var execution = ResolveExecutionPlan(planItem, planIndex, input.Seed, structuralExclusions);
            var poolSize = ResolvePoolSize(execution.SearchMethod);
            var selected = new List<CandidateSelection>();

            for (var poolIndex = 0; poolIndex < poolSize; poolIndex++)
            {
                var effectiveSeed = execution.SeedUsed ?? 0UL;
                var numbers = BuildCandidateNumbers(rankedNumbers, effectiveSeed, sequenceSeed + poolIndex);
                var key = string.Join(",", numbers);
                if (globalConstraints.UniqueGames is true && usedGameKeys.Contains(key))
                {
                    continue;
                }

                var profile = BuildProfile(numbers, context);
                var strategyScore = ComputeScore(execution.StrategyName, profile, execution.Weights);
                if (!MatchesCriteria(execution.StrategyName, profile, strategyScore, execution.Criteria))
                {
                    continue;
                }

                if (!PassesFilters(profile, execution.Filters))
                {
                    continue;
                }

                selected.Add(new CandidateSelection(numbers, profile, strategyScore));
            }

            sequenceSeed += poolSize;
            selected.Sort((left, right) => CompareSelections(left, right, execution.TieBreakRule));

            if (selected.Count < planItem.Count)
            {
                throw new ApplicationValidationException(
                    code: "STRUCTURAL_EXCLUSION_CONFLICT",
                    message: "configured criteria/filters make the requested plan infeasible.",
                    details: new Dictionary<string, object?>
                    {
                        ["strategy_name"] = planItem.StrategyName,
                        ["requested_count"] = planItem.Count,
                        ["available_count"] = selected.Count
                    });
            }

            foreach (var selection in selected.Take(planItem.Count))
            {
                var key = string.Join(",", selection.Numbers);
                usedGameKeys.Add(key);
                candidates.Add(new CandidateGameView(
                    Numbers: selection.Numbers,
                    StrategyName: execution.StrategyName,
                    StrategyVersion: execution.StrategyVersion,
                    SearchMethod: execution.SearchMethod,
                    TieBreakRule: execution.TieBreakRule,
                    SeedUsed: execution.SeedUsed,
                    AppliedConfiguration: new AppliedConfigurationView(
                        Criteria: execution.Criteria,
                        Weights: execution.Weights,
                        Filters: execution.Filters,
                        ResolvedDefaults: execution.ResolvedDefaults)));
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
        var stepOptions = new[] { 1, 2, 3, 4, 6, 7, 8, 9, 11, 12 };
        var start = (int)((seed + (ulong)(sequence * 7)) % 25UL);
        var step = stepOptions[sequence % stepOptions.Length];

        var picked = 0;
        var walk = 0;
        while (picked < selected.Length)
        {
            var index = (start + (walk * step)) % rankedNumbers.Count;
            var number = rankedNumbers[index];
            if (!selected.Take(picked).Contains(number))
            {
                selected[picked] = number;
                picked++;
            }

            walk++;
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

    private static GenerationContext BuildGenerationContext(
        DrawWindow window,
        IReadOnlyList<double> frequencyByDezena,
        IReadOnlyList<double> top10,
        IReadOnlyList<double> repetitionSeries)
    {
        var top10Set = top10.Select(static x => (int)x).ToHashSet();
        var lastDrawNumbers = window.Draws[^1].Numbers.ToArray();
        var repetitionMedian = Median(repetitionSeries);

        return new GenerationContext(
            FrequencyByDezena: frequencyByDezena,
            Top10Set: top10Set,
            LastDrawNumbers: lastDrawNumbers,
            RepetitionMedian: repetitionMedian);
    }

    private static ExecutionPlan ResolveExecutionPlan(
        GenerateCandidatePlanItemInput item,
        int planIndex,
        ulong? inputSeed,
        GenerateStructuralExclusionsInput structuralExclusions)
    {
        var resolvedDefaults = new Dictionary<string, object?>(StringComparer.Ordinal);
        var strategyVersion = ResolveStrategyVersion(item, planIndex, resolvedDefaults);
        var searchMethod = ResolveSearchMethod(item, planIndex, resolvedDefaults);
        var tieBreakRule = ResolveTieBreakRule(item, planIndex, resolvedDefaults);
        var criteria = ResolveCriteria(item, planIndex, resolvedDefaults);
        var weights = ResolveWeights(item, planIndex, resolvedDefaults);
        var filters = ResolveFilters(item, planIndex, structuralExclusions, resolvedDefaults);

        return new ExecutionPlan(
            StrategyName: item.StrategyName,
            StrategyVersion: strategyVersion,
            SearchMethod: searchMethod,
            TieBreakRule: tieBreakRule,
            SeedUsed: RequiresSeed(searchMethod) ? inputSeed : null,
            Criteria: criteria,
            Weights: weights,
            Filters: filters,
            ResolvedDefaults: resolvedDefaults);
    }

    private static string ResolveStrategyVersion(
        GenerateCandidatePlanItemInput item,
        int planIndex,
        IDictionary<string, object?> defaults)
    {
        if (!string.IsNullOrWhiteSpace(item.StrategyVersion))
        {
            return item.StrategyVersion;
        }

        var value = string.Equals(item.StrategyName, DeclaredCompositeProfileStrategy, StringComparison.Ordinal)
            ? DeclaredCompositeProfileVersion
            : CommonRepetitionFrequencyVersion;
        defaults[$"plan[{planIndex}].strategy_version"] = value;
        return value;
    }

    private static string ResolveSearchMethod(
        GenerateCandidatePlanItemInput item,
        int planIndex,
        IDictionary<string, object?> defaults)
    {
        if (!string.IsNullOrWhiteSpace(item.SearchMethod))
        {
            return item.SearchMethod;
        }

        var value = string.Equals(item.StrategyName, DeclaredCompositeProfileStrategy, StringComparison.Ordinal)
            ? DefaultDeclaredCompositeSearchMethod
            : DefaultCommonRepetitionFrequencySearchMethod;
        defaults[$"plan[{planIndex}].search_method"] = value;
        return value;
    }

    private static string ResolveTieBreakRule(
        GenerateCandidatePlanItemInput item,
        int planIndex,
        IDictionary<string, object?> defaults)
    {
        if (!string.IsNullOrWhiteSpace(item.TieBreakRule))
        {
            return item.TieBreakRule;
        }

        var value = string.Equals(item.StrategyName, DeclaredCompositeProfileStrategy, StringComparison.Ordinal)
            ? DeclaredTieBreakRule
            : CommonTieBreakRule;
        defaults[$"plan[{planIndex}].tie_break_rule"] = value;
        return value;
    }

    private static IReadOnlyList<GenerateCandidateCriteriaInput> ResolveCriteria(
        GenerateCandidatePlanItemInput item,
        int planIndex,
        IDictionary<string, object?> defaults)
    {
        if (item.Criteria is { Count: > 0 })
        {
            return item.Criteria.ToArray();
        }

        GenerateCandidateCriteriaInput[] criteria = string.Equals(item.StrategyName, DeclaredCompositeProfileStrategy, StringComparison.Ordinal)
            ?
            [
                new GenerateCandidateCriteriaInput("min_composite_score", 0.55d)
            ]
            :
            [
                new GenerateCandidateCriteriaInput("min_frequency_alignment", 0.55d),
                new GenerateCandidateCriteriaInput("min_repeat_alignment", 0.5d),
                new GenerateCandidateCriteriaInput("min_top10_overlap", 6d)
            ];
        defaults[$"plan[{planIndex}].criteria"] = criteria.Select(static c => new { name = c.Name, value = c.Value }).ToArray();
        return criteria;
    }

    private static IReadOnlyList<GenerateCandidateWeightInput> ResolveWeights(
        GenerateCandidatePlanItemInput item,
        int planIndex,
        IDictionary<string, object?> defaults)
    {
        if (item.Weights is { Count: > 0 })
        {
            return item.Weights.ToArray();
        }

        GenerateCandidateWeightInput[] weights = string.Equals(item.StrategyName, DeclaredCompositeProfileStrategy, StringComparison.Ordinal)
            ?
            [
                new GenerateCandidateWeightInput("freq_alignment", 0.3d),
                new GenerateCandidateWeightInput("repeat_alignment", 0.2d),
                new GenerateCandidateWeightInput("slot_alignment", 0.2d),
                new GenerateCandidateWeightInput("row_entropy_norm", 0.2d),
                new GenerateCandidateWeightInput("hhi_linha_inverse", 0.1d)
            ]
            :
            [
                new GenerateCandidateWeightInput("freq_alignment", 0.6d),
                new GenerateCandidateWeightInput("repeat_alignment", 0.4d)
            ];
        defaults[$"plan[{planIndex}].weights"] = weights.Select(static w => new { name = w.Name, weight = w.Weight }).ToArray();
        return weights;
    }

    private static IReadOnlyList<GenerateCandidateFilterInput> ResolveFilters(
        GenerateCandidatePlanItemInput item,
        int planIndex,
        GenerateStructuralExclusionsInput structuralExclusions,
        IDictionary<string, object?> defaults)
    {
        var fromStructural = new Dictionary<string, GenerateCandidateFilterInput>(StringComparer.Ordinal)
        {
            ["max_consecutive_run"] = new GenerateCandidateFilterInput("max_consecutive_run", structuralExclusions.MaxConsecutiveRun, null, null, "1.0.0"),
            ["max_neighbor_count"] = new GenerateCandidateFilterInput("max_neighbor_count", structuralExclusions.MaxNeighborCount, null, null, "1.0.0"),
            ["min_row_entropy_norm"] = new GenerateCandidateFilterInput("min_row_entropy_norm", structuralExclusions.MinRowEntropyNorm, null, null, "1.0.0"),
            ["max_hhi_linha"] = new GenerateCandidateFilterInput("max_hhi_linha", structuralExclusions.MaxHhiLinha, null, null, "1.0.0"),
            ["min_slot_alignment"] = new GenerateCandidateFilterInput("min_slot_alignment", structuralExclusions.MinSlotAlignment, null, null, "1.0.0"),
            ["max_outlier_score"] = new GenerateCandidateFilterInput("max_outlier_score", structuralExclusions.MaxOutlierScore, null, null, "1.0.0")
        };

        if (structuralExclusions.RepeatRange is not null)
        {
            fromStructural["repeat_range"] = new GenerateCandidateFilterInput(
                "repeat_range",
                null,
                structuralExclusions.RepeatRange.Min,
                structuralExclusions.RepeatRange.Max,
                "1.0.0");
        }

        if (item.Filters is { Count: > 0 })
        {
            foreach (var filter in item.Filters)
            {
                fromStructural[filter.Name] = filter with
                {
                    Version = string.IsNullOrWhiteSpace(filter.Version) ? "1.0.0" : filter.Version
                };
            }

            defaults[$"plan[{planIndex}].filters_source"] = "merged_structural_exclusions_plus_plan_filters";
            return fromStructural.Values.ToArray();
        }

        defaults[$"plan[{planIndex}].filters"] = fromStructural.Values
            .Select(static f => new { name = f.Name, value = f.Value, min = f.Min, max = f.Max, version = f.Version })
            .ToArray();
        return fromStructural.Values.ToArray();
    }

    private static GenerateGlobalConstraintsInput ResolveGlobalConstraints(GenerateGlobalConstraintsInput? input)
    {
        return new GenerateGlobalConstraintsInput(
            UniqueGames: input?.UniqueGames ?? true,
            SortedNumbers: input?.SortedNumbers ?? true);
    }

    private static GenerateStructuralExclusionsInput ResolveStructuralExclusions(GenerateStructuralExclusionsInput? input)
    {
        return new GenerateStructuralExclusionsInput(
            MaxConsecutiveRun: input?.MaxConsecutiveRun ?? 8d,
            MaxNeighborCount: input?.MaxNeighborCount ?? 7d,
            MinRowEntropyNorm: input?.MinRowEntropyNorm ?? 0.82d,
            MinColumnEntropyNorm: input?.MinColumnEntropyNorm,
            MaxHhiLinha: input?.MaxHhiLinha ?? 0.30d,
            MaxHhiColuna: input?.MaxHhiColuna,
            RepeatRange: input?.RepeatRange ?? new GenerateRepeatRangeInput(0, 15),
            MinSlotAlignment: input?.MinSlotAlignment ?? 0.08d,
            MaxOutlierScore: input?.MaxOutlierScore ?? 1d);
    }

    private static int ResolvePoolSize(string searchMethod)
    {
        return searchMethod switch
        {
            "sampled" => 180,
            "greedy_topk" => 140,
            "exhaustive" => 260,
            _ => 120
        };
    }

    private static CandidateProfile BuildProfile(IReadOnlyList<int> numbers, GenerationContext context)
    {
        var maxFrequency = context.FrequencyByDezena.Max();
        var frequencyAlignment = numbers.Average(n => context.FrequencyByDezena[n - 1] / maxFrequency);
        var repeatCount = numbers.Count(n => context.LastDrawNumbers.Contains(n));
        var repeatAlignment = Clamp01(1d - Math.Abs(repeatCount - context.RepetitionMedian) / 15d);
        var top10OverlapCount = numbers.Count(n => context.Top10Set.Contains(n));

        Span<int> rowCounts = stackalloc int[5];
        foreach (var number in numbers)
        {
            rowCounts[(number - 1) / 5]++;
        }

        var rowEntropyNorm = Clamp01(ShannonEntropyBits.FromNonNegativeCounts(rowCounts) / Math.Log2(5d));
        var hhiLinha = Clamp01(HerfindahlHirschmanIndex.FromNonNegativeCounts(rowCounts));
        var neighborCount = CountNeighbors(numbers);
        var maxConsecutiveRun = VizinhosConsecutivos.MaxConsecutiveAdjacencyRunLength(numbers);
        var slotAlignment = ComputeSlotAlignment(numbers);
        var outlierScore = Clamp01((1d - frequencyAlignment + 1d - repeatAlignment + (1d - rowEntropyNorm) + hhiLinha) / 4d);

        return new CandidateProfile(
            FrequencyAlignment: frequencyAlignment,
            RepeatAlignment: repeatAlignment,
            RepeatCount: repeatCount,
            SlotAlignment: slotAlignment,
            RowEntropyNorm: rowEntropyNorm,
            HhiLinha: hhiLinha,
            NeighborCount: neighborCount,
            MaxConsecutiveRun: maxConsecutiveRun,
            Top10OverlapCount: top10OverlapCount,
            OutlierScore: outlierScore);
    }

    private static double ComputeScore(
        string strategyName,
        CandidateProfile profile,
        IReadOnlyList<GenerateCandidateWeightInput> weights)
    {
        if (string.Equals(strategyName, CommonRepetitionFrequencyStrategy, StringComparison.Ordinal))
        {
            var freqWeight = weights.FirstOrDefault(w => string.Equals(w.Name, "freq_alignment", StringComparison.Ordinal))?.Weight ?? 0.6d;
            var repeatWeight = weights.FirstOrDefault(w => string.Equals(w.Name, "repeat_alignment", StringComparison.Ordinal))?.Weight ?? 0.4d;
            return Clamp01((freqWeight * profile.FrequencyAlignment) + (repeatWeight * profile.RepeatAlignment));
        }

        double sum = 0d;
        foreach (var weight in weights)
        {
            var value = weight.Name switch
            {
                "freq_alignment" => profile.FrequencyAlignment,
                "repeat_alignment" => profile.RepeatAlignment,
                "slot_alignment" => profile.SlotAlignment,
                "row_entropy_norm" => profile.RowEntropyNorm,
                "hhi_linha_inverse" => 1d - profile.HhiLinha,
                "outlier_centrality" => 1d / (1d + profile.OutlierScore),
                _ => 0d
            };
            sum += weight.Weight * Clamp01(value);
        }

        return Clamp01(sum);
    }

    private static bool MatchesCriteria(
        string strategyName,
        CandidateProfile profile,
        double compositeScore,
        IReadOnlyList<GenerateCandidateCriteriaInput> criteria)
    {
        var lookup = criteria.ToDictionary(c => c.Name, c => c.Value, StringComparer.Ordinal);
        if (string.Equals(strategyName, CommonRepetitionFrequencyStrategy, StringComparison.Ordinal))
        {
            var minFrequency = lookup.TryGetValue("min_frequency_alignment", out var valueFrequency) ? valueFrequency : 0d;
            var minRepeat = lookup.TryGetValue("min_repeat_alignment", out var valueRepeat) ? valueRepeat : 0d;
            var minTop10 = lookup.TryGetValue("min_top10_overlap", out var valueTop10) ? valueTop10 : 0d;
            return profile.FrequencyAlignment >= minFrequency &&
                   profile.RepeatAlignment >= minRepeat &&
                   profile.Top10OverlapCount >= minTop10;
        }

        var minCompositeScore = lookup.TryGetValue("min_composite_score", out var compositeValue) ? compositeValue : 0d;
        return compositeScore >= minCompositeScore;
    }

    private static bool PassesFilters(
        CandidateProfile profile,
        IReadOnlyList<GenerateCandidateFilterInput> filters)
    {
        foreach (var filter in filters)
        {
            if (string.Equals(filter.Name, "max_consecutive_run", StringComparison.Ordinal) &&
                filter.Value.HasValue &&
                profile.MaxConsecutiveRun > filter.Value.Value)
            {
                return false;
            }

            if (string.Equals(filter.Name, "max_neighbor_count", StringComparison.Ordinal) &&
                filter.Value.HasValue &&
                profile.NeighborCount > filter.Value.Value)
            {
                return false;
            }

            if (string.Equals(filter.Name, "min_row_entropy_norm", StringComparison.Ordinal) &&
                filter.Value.HasValue &&
                profile.RowEntropyNorm < filter.Value.Value)
            {
                return false;
            }

            if (string.Equals(filter.Name, "max_hhi_linha", StringComparison.Ordinal) &&
                filter.Value.HasValue &&
                profile.HhiLinha > filter.Value.Value)
            {
                return false;
            }

            if (string.Equals(filter.Name, "min_slot_alignment", StringComparison.Ordinal) &&
                filter.Value.HasValue &&
                profile.SlotAlignment < filter.Value.Value)
            {
                return false;
            }

            if (string.Equals(filter.Name, "max_outlier_score", StringComparison.Ordinal) &&
                filter.Value.HasValue &&
                profile.OutlierScore > filter.Value.Value)
            {
                return false;
            }

            if (string.Equals(filter.Name, "repeat_range", StringComparison.Ordinal))
            {
                var min = filter.Min ?? 0d;
                var max = filter.Max ?? 15d;
                if (profile.RepeatCount < min || profile.RepeatCount > max)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static int CompareSelections(
        CandidateSelection left,
        CandidateSelection right,
        string tieBreakRule)
    {
        var byScore = right.Score.CompareTo(left.Score);
        if (byScore != 0)
        {
            return byScore;
        }

        if (string.Equals(tieBreakRule, DeclaredTieBreakRule, StringComparison.Ordinal))
        {
            var byOutlier = left.Profile.OutlierScore.CompareTo(right.Profile.OutlierScore);
            if (byOutlier != 0)
            {
                return byOutlier;
            }

            var byHhi = left.Profile.HhiLinha.CompareTo(right.Profile.HhiLinha);
            if (byHhi != 0)
            {
                return byHhi;
            }
        }

        return CompareNumbersLexicographic(left.Numbers, right.Numbers);
    }

    private static int CompareNumbersLexicographic(IReadOnlyList<int> left, IReadOnlyList<int> right)
    {
        for (var i = 0; i < Math.Min(left.Count, right.Count); i++)
        {
            var cmp = left[i].CompareTo(right[i]);
            if (cmp != 0)
            {
                return cmp;
            }
        }

        return left.Count.CompareTo(right.Count);
    }

    private static int CountNeighbors(IReadOnlyList<int> sortedNumbers)
    {
        var count = 0;
        for (var i = 1; i < sortedNumbers.Count; i++)
        {
            if (sortedNumbers[i] - sortedNumbers[i - 1] == 1)
            {
                count++;
            }
        }

        return count;
    }

    private static double ComputeSlotAlignment(IReadOnlyList<int> sortedNumbers)
    {
        double distanceSum = 0d;
        for (var i = 0; i < sortedNumbers.Count; i++)
        {
            var expected = 1d + i * (24d / 14d);
            distanceSum += Math.Abs(sortedNumbers[i] - expected) / 24d;
        }

        return Clamp01(1d - (distanceSum / sortedNumbers.Count));
    }

    private static bool RequiresSeed(string searchMethod)
    {
        return string.Equals(searchMethod, "sampled", StringComparison.Ordinal) ||
               string.Equals(searchMethod, "greedy_topk", StringComparison.Ordinal);
    }

    private static double Median(IReadOnlyList<double> values)
    {
        if (values.Count == 0)
        {
            return 0d;
        }

        var ordered = values.OrderBy(static value => value).ToArray();
        var mid = ordered.Length / 2;
        return ordered.Length % 2 == 0
            ? (ordered[mid - 1] + ordered[mid]) / 2d
            : ordered[mid];
    }

    private static double Clamp01(double value)
    {
        if (value < 0d)
        {
            return 0d;
        }

        if (value > 1d)
        {
            return 1d;
        }

        return value;
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

    private sealed record GenerationContext(
        IReadOnlyList<double> FrequencyByDezena,
        HashSet<int> Top10Set,
        IReadOnlyList<int> LastDrawNumbers,
        double RepetitionMedian);

    private sealed record CandidateProfile(
        double FrequencyAlignment,
        double RepeatAlignment,
        int RepeatCount,
        double SlotAlignment,
        double RowEntropyNorm,
        double HhiLinha,
        int NeighborCount,
        int MaxConsecutiveRun,
        int Top10OverlapCount,
        double OutlierScore);

    private sealed record CandidateSelection(
        IReadOnlyList<int> Numbers,
        CandidateProfile Profile,
        double Score);

    private sealed record ExecutionPlan(
        string StrategyName,
        string StrategyVersion,
        string SearchMethod,
        string TieBreakRule,
        ulong? SeedUsed,
        IReadOnlyList<GenerateCandidateCriteriaInput> Criteria,
        IReadOnlyList<GenerateCandidateWeightInput> Weights,
        IReadOnlyList<GenerateCandidateFilterInput> Filters,
        IReadOnlyDictionary<string, object?> ResolvedDefaults);
}
