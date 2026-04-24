using LotofacilMcp.Application.Mapping;
using LotofacilMcp.Application.Validation;
using LotofacilMcp.Domain.Generation;
using LotofacilMcp.Domain.Metrics;
using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;
using LotofacilMcp.Infrastructure.DatasetVersioning;
using LotofacilMcp.Infrastructure.Providers;

namespace LotofacilMcp.Application.UseCases;

public sealed record GenerateCandidateCriteriaInput(
    string Name,
    double? Value,
    GenerateRangeSpecInput? Range,
    GenerateAllowedValuesSpecInput? AllowedValues,
    GenerateTypicalRangeSpecInput? TypicalRange,
    string? Mode);

public sealed record GenerateRangeSpecInput(
    double Min,
    double Max,
    bool? Inclusive);

public sealed record GenerateAllowedValuesSpecInput(
    IReadOnlyList<double>? Values);

public sealed record GenerateTypicalRangeParamsInput(
    double? PLow,
    double? PHigh);

public sealed record GenerateTypicalRangeSpecInput(
    string MetricName,
    string Method,
    double Coverage,
    GenerateTypicalRangeParamsInput? Params,
    string? WindowRef,
    bool? Inclusive);

public sealed record GenerateCandidateWeightInput(
    string Name,
    double Weight);

public sealed record GenerateCandidateFilterInput(
    string Name,
    double? Value,
    double? Min,
    double? Max,
    GenerateRangeSpecInput? Range,
    GenerateAllowedValuesSpecInput? AllowedValues,
    GenerateTypicalRangeSpecInput? TypicalRange,
    string? Mode,
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

public sealed record GenerateGenerationBudgetInput(
    int? MaxAttempts,
    double? PoolMultiplier);

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
    GenerateGenerationBudgetInput? GenerationBudget,
    string FixturePath = "");

public sealed record GenerateCandidateGamesDeterministicHashInput(
    int WindowSize,
    int? EndContestId,
    ulong? Seed,
    IReadOnlyList<GenerateCandidatePlanItemInput> Plan,
    GenerateGlobalConstraintsInput GlobalConstraints,
    GenerateStructuralExclusionsInput StructuralExclusions,
    GenerateGenerationBudgetInput GenerationBudget);

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
    private readonly TypicalRangeResolver _typicalRangeResolver = new();
    private readonly GenerationBudgetResolver _generationBudgetResolver = new();
    private readonly SoftConstraintPenaltyResolver _softPenaltyResolver = new();

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
            var resolvedGenerationBudget = ResolveGenerationBudget(input.GenerationBudget);
            var candidates = BuildCandidates(
                input,
                window,
                rankedNumbers,
                context,
                resolvedGlobalConstraints,
                resolvedStructuralExclusions,
                resolvedGenerationBudget);

            return new GenerateCandidateGamesResult(
                DatasetVersion: _datasetVersionService.CreateFromSnapshot(snapshot),
                ToolVersion: ToolVersion,
                DeterministicHashInput: new GenerateCandidateGamesDeterministicHashInput(
                    input.WindowSize,
                    input.EndContestId,
                    input.Seed,
                    input.Plan.ToArray(),
                    resolvedGlobalConstraints,
                    resolvedStructuralExclusions,
                    resolvedGenerationBudget),
                Window: windowView,
                CandidateGames: candidates);
        }
        catch (DomainInvariantViolationException ex)
        {
            throw MapDomainError(ex);
        }
    }

    private CandidateGameView[] BuildCandidates(
        GenerateCandidateGamesInput input,
        DrawWindow window,
        IReadOnlyList<int> rankedNumbers,
        GenerationContext context,
        GenerateGlobalConstraintsInput globalConstraints,
        GenerateStructuralExclusionsInput structuralExclusions,
        GenerateGenerationBudgetInput generationBudget)
    {
        var candidates = new List<CandidateGameView>();
        var usedGameKeys = new HashSet<string>(StringComparer.Ordinal);
        var sequenceSeed = 0;

        for (var planIndex = 0; planIndex < input.Plan.Count; planIndex++)
        {
            var planItem = input.Plan[planIndex];
            var execution = ResolveExecutionPlan(planItem, planIndex, input.Seed, structuralExclusions, window);
            var poolSize = ResolvePoolSize(execution.SearchMethod);
            var resolvedBudget = _generationBudgetResolver.Resolve(
                new GenerationBudgetSpec(generationBudget.MaxAttempts, generationBudget.PoolMultiplier),
                poolSize);
            var selected = new List<CandidateSelection>();
            var selectedKeys = globalConstraints.UniqueGames is true
                ? new HashSet<string>(StringComparer.Ordinal)
                : null;
            var rejectedCountByReason = new SortedDictionary<string, int>(StringComparer.Ordinal);
            var softPenaltyAppliedByConstraint = new SortedDictionary<string, int>(StringComparer.Ordinal);
            var softPenaltySumByConstraint = new SortedDictionary<string, double>(StringComparer.Ordinal);
            var attemptsUsed = 0;

            for (var poolIndex = 0; poolIndex < resolvedBudget.MaxAttempts; poolIndex++)
            {
                attemptsUsed++;
                var effectiveSeed = execution.SeedUsed ?? 0UL;
                var numbers = BuildCandidateNumbers(rankedNumbers, effectiveSeed, sequenceSeed + poolIndex);
                var key = string.Join(",", numbers);
                if (globalConstraints.UniqueGames is true &&
                    (usedGameKeys.Contains(key) || (selectedKeys is not null && selectedKeys.Contains(key))))
                {
                    IncrementCounter(rejectedCountByReason, "duplicate_game");
                    continue;
                }

                var profile = BuildProfile(numbers, context);
                var strategyScore = ComputeScore(execution.StrategyName, profile, execution.Weights);
                if (!MatchesCriteria(execution.StrategyName, profile, strategyScore, execution.Criteria, out var criteriaRejectionReason))
                {
                    IncrementCounter(rejectedCountByReason, criteriaRejectionReason ?? "criteria_mismatch");
                    continue;
                }

                if (!PassesFilters(
                        profile,
                        execution.Filters,
                        out var filterRejectionReason,
                        out var totalSoftPenalty,
                        out var softPenalties))
                {
                    IncrementCounter(rejectedCountByReason, filterRejectionReason ?? "filter_mismatch");
                    continue;
                }

                foreach (var penalty in softPenalties)
                {
                    IncrementCounter(softPenaltyAppliedByConstraint, penalty.ConstraintName);
                    IncrementCounter(softPenaltySumByConstraint, penalty.ConstraintName, penalty.Penalty);
                }

                var finalScore = Clamp01(strategyScore - totalSoftPenalty);
                selectedKeys?.Add(key);
                selected.Add(new CandidateSelection(numbers, profile, finalScore, strategyScore, totalSoftPenalty));
            }

            sequenceSeed += resolvedBudget.MaxAttempts;
            selected.Sort((left, right) => CompareSelections(left, right, execution.TieBreakRule));
            execution.ResolvedDefaults[$"plan[{planIndex}].generation_budget.max_attempts"] = resolvedBudget.MaxAttempts;
            execution.ResolvedDefaults[$"plan[{planIndex}].generation_budget.pool_multiplier"] = resolvedBudget.PoolMultiplier;
            execution.ResolvedDefaults[$"plan[{planIndex}].attempts_used"] = attemptsUsed;
            execution.ResolvedDefaults[$"plan[{planIndex}].accepted_count"] = selected.Count;
            execution.ResolvedDefaults[$"plan[{planIndex}].rejected_count_by_reason"] = rejectedCountByReason
                .ToDictionary(entry => entry.Key, entry => (object?)entry.Value, StringComparer.Ordinal);
            execution.ResolvedDefaults[$"plan[{planIndex}].soft_penalty.version"] = SoftConstraintPenaltyResolver.PenaltyVersion;
            execution.ResolvedDefaults[$"plan[{planIndex}].soft_penalty.applied_count_by_constraint"] = softPenaltyAppliedByConstraint
                .ToDictionary(entry => entry.Key, entry => (object?)entry.Value, StringComparer.Ordinal);
            execution.ResolvedDefaults[$"plan[{planIndex}].soft_penalty.sum_by_constraint"] = softPenaltySumByConstraint
                .ToDictionary(entry => entry.Key, entry => (object?)entry.Value, StringComparer.Ordinal);

            if (selected.Count < planItem.Count)
            {
                var collapseHint = rejectedCountByReason.Count == 0
                    ? "budget_exhausted_without_classified_rejections"
                    : rejectedCountByReason
                        .OrderByDescending(static pair => pair.Value)
                        .ThenBy(static pair => pair.Key, StringComparer.Ordinal)
                        .First()
                        .Key;
                throw new ApplicationValidationException(
                    code: "STRUCTURAL_EXCLUSION_CONFLICT",
                    message: "configured criteria/filters make the requested plan infeasible.",
                    details: new Dictionary<string, object?>
                    {
                        ["strategy_name"] = planItem.StrategyName,
                        ["requested_count"] = planItem.Count,
                        ["available_count"] = selected.Count,
                        ["attempts_used"] = attemptsUsed,
                        ["accepted_count"] = selected.Count,
                        ["rejected_count_by_reason"] = rejectedCountByReason.ToDictionary(entry => entry.Key, entry => (object?)entry.Value, StringComparer.Ordinal),
                        ["collapse_hint"] = collapseHint
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

    private ExecutionPlan ResolveExecutionPlan(
        GenerateCandidatePlanItemInput item,
        int planIndex,
        ulong? inputSeed,
        GenerateStructuralExclusionsInput structuralExclusions,
        DrawWindow window)
    {
        var resolvedDefaults = new Dictionary<string, object?>(StringComparer.Ordinal);
        var strategyVersion = ResolveStrategyVersion(item, planIndex, resolvedDefaults);
        var searchMethod = ResolveSearchMethod(item, planIndex, resolvedDefaults);
        var tieBreakRule = ResolveTieBreakRule(item, planIndex, resolvedDefaults);
        var criteria = ResolveCriteria(item, planIndex, resolvedDefaults, window);
        var weights = ResolveWeights(item, planIndex, resolvedDefaults);
        var filters = ResolveFilters(item, planIndex, structuralExclusions, resolvedDefaults, window);

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

    private IReadOnlyList<GenerateCandidateCriteriaInput> ResolveCriteria(
        GenerateCandidatePlanItemInput item,
        int planIndex,
        IDictionary<string, object?> defaults,
        DrawWindow window)
    {
        if (item.Criteria is { Count: > 0 })
        {
            var resolved = new List<GenerateCandidateCriteriaInput>(item.Criteria.Count);
            for (var criterionIndex = 0; criterionIndex < item.Criteria.Count; criterionIndex++)
            {
                var criterion = item.Criteria[criterionIndex];
                var mode = ResolveMode(
                    criterion.Mode,
                    $"plan[{planIndex}].criteria[{criterionIndex}]",
                    defaults);
                var range = NormalizeRange(
                    criterion.Range,
                    $"plan[{planIndex}].criteria[{criterionIndex}]",
                    defaults);
                var typicalRange = NormalizeTypicalRange(
                    criterion.TypicalRange,
                    $"plan[{planIndex}].criteria[{criterionIndex}]",
                    defaults,
                    window);
                var allowedValues = NormalizeAllowedValues(
                    criterion.AllowedValues,
                    $"plan[{planIndex}].criteria[{criterionIndex}]",
                    defaults);
                var effectiveRange = range ?? (typicalRange is null
                    ? null
                    : new GenerateRangeSpecInput(
                        Min: typicalRange.ResolvedRange.Min,
                        Max: typicalRange.ResolvedRange.Max,
                        Inclusive: typicalRange.ResolvedRange.Inclusive));
                resolved.Add(criterion with
                {
                    Mode = mode,
                    Range = effectiveRange,
                    AllowedValues = allowedValues
                });
            }

            return resolved;
        }

        GenerateCandidateCriteriaInput[] criteria = string.Equals(item.StrategyName, DeclaredCompositeProfileStrategy, StringComparison.Ordinal)
            ?
            [
                new GenerateCandidateCriteriaInput("min_composite_score", 0.55d, null, null, null, "hard")
            ]
            :
            [
                new GenerateCandidateCriteriaInput("min_frequency_alignment", 0.55d, null, null, null, "hard"),
                new GenerateCandidateCriteriaInput("min_repeat_alignment", 0.5d, null, null, null, "hard"),
                new GenerateCandidateCriteriaInput("min_top10_overlap", 6d, null, null, null, "hard")
            ];
        defaults[$"plan[{planIndex}].criteria"] = criteria.Select(static c => new { name = c.Name, value = c.Value, mode = c.Mode }).ToArray();
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

    private IReadOnlyList<GenerateCandidateFilterInput> ResolveFilters(
        GenerateCandidatePlanItemInput item,
        int planIndex,
        GenerateStructuralExclusionsInput structuralExclusions,
        IDictionary<string, object?> defaults,
        DrawWindow window)
    {
        var fromStructural = new Dictionary<string, GenerateCandidateFilterInput>(StringComparer.Ordinal)
        {
            ["max_consecutive_run"] = new GenerateCandidateFilterInput("max_consecutive_run", structuralExclusions.MaxConsecutiveRun, null, null, null, null, null, "hard", "1.0.0"),
            ["max_neighbor_count"] = new GenerateCandidateFilterInput("max_neighbor_count", structuralExclusions.MaxNeighborCount, null, null, null, null, null, "hard", "1.0.0"),
            ["min_row_entropy_norm"] = new GenerateCandidateFilterInput("min_row_entropy_norm", structuralExclusions.MinRowEntropyNorm, null, null, null, null, null, "hard", "1.0.0"),
            ["max_hhi_linha"] = new GenerateCandidateFilterInput("max_hhi_linha", structuralExclusions.MaxHhiLinha, null, null, null, null, null, "hard", "1.0.0"),
            ["min_slot_alignment"] = new GenerateCandidateFilterInput("min_slot_alignment", structuralExclusions.MinSlotAlignment, null, null, null, null, null, "hard", "1.0.0"),
            ["max_outlier_score"] = new GenerateCandidateFilterInput("max_outlier_score", structuralExclusions.MaxOutlierScore, null, null, null, null, null, "hard", "1.0.0")
        };

        if (structuralExclusions.RepeatRange is not null)
        {
            fromStructural["repeat_range"] = new GenerateCandidateFilterInput(
                "repeat_range",
                null,
                structuralExclusions.RepeatRange.Min,
                structuralExclusions.RepeatRange.Max,
                null,
                null,
                null,
                "hard",
                "1.0.0");
        }

        if (item.Filters is { Count: > 0 })
        {
            for (var filterIndex = 0; filterIndex < item.Filters.Count; filterIndex++)
            {
                var filter = item.Filters[filterIndex];
                var mode = ResolveMode(
                    filter.Mode,
                    $"plan[{planIndex}].filters[{filterIndex}]",
                    defaults);
                if (string.Equals(mode, "soft", StringComparison.Ordinal))
                {
                    defaults[$"plan[{planIndex}].filters[{filterIndex}].soft_penalty.version"] = SoftConstraintPenaltyResolver.PenaltyVersion;
                    defaults[$"plan[{planIndex}].filters[{filterIndex}].soft_penalty.scale"] = _softPenaltyResolver.ResolveScale(filter.Name);
                    defaults[$"plan[{planIndex}].filters[{filterIndex}].soft_penalty.formula"] = "linear_normalized_violation";
                }
                var range = NormalizeRange(
                    filter.Range,
                    $"plan[{planIndex}].filters[{filterIndex}]",
                    defaults);
                var typicalRange = NormalizeTypicalRange(
                    filter.TypicalRange,
                    $"plan[{planIndex}].filters[{filterIndex}]",
                    defaults,
                    window);
                var allowedValues = NormalizeAllowedValues(
                    filter.AllowedValues,
                    $"plan[{planIndex}].filters[{filterIndex}]",
                    defaults);
                var effectiveRange = range ?? (typicalRange is null
                    ? null
                    : new GenerateRangeSpecInput(
                        Min: typicalRange.ResolvedRange.Min,
                        Max: typicalRange.ResolvedRange.Max,
                        Inclusive: typicalRange.ResolvedRange.Inclusive));
                fromStructural[filter.Name] = filter with
                {
                    Mode = mode,
                    Range = effectiveRange,
                    AllowedValues = allowedValues,
                    Version = string.IsNullOrWhiteSpace(filter.Version) ? "1.0.0" : filter.Version
                };
            }

            defaults[$"plan[{planIndex}].filters_source"] = "merged_structural_exclusions_plus_plan_filters";
            return fromStructural.Values.ToArray();
        }

        defaults[$"plan[{planIndex}].filters"] = fromStructural.Values
            .Select(static f => new
            {
                name = f.Name,
                value = f.Value,
                min = f.Min,
                max = f.Max,
                range = f.Range is null ? null : new { min = f.Range.Min, max = f.Range.Max, inclusive = f.Range.Inclusive },
                allowed_values = f.AllowedValues?.Values?.ToArray(),
                typical_range = f.TypicalRange is null
                    ? null
                    : new
                    {
                        metric_name = f.TypicalRange.MetricName,
                        method = f.TypicalRange.Method,
                        coverage = f.TypicalRange.Coverage,
                        @params = f.TypicalRange.Params is null
                            ? null
                            : new { p_low = f.TypicalRange.Params.PLow, p_high = f.TypicalRange.Params.PHigh },
                        window_ref = f.TypicalRange.WindowRef,
                        inclusive = f.TypicalRange.Inclusive
                    },
                mode = f.Mode,
                version = f.Version
            })
            .ToArray();
        return fromStructural.Values.ToArray();
    }

    private static string ResolveMode(
        string? mode,
        string pathPrefix,
        IDictionary<string, object?> defaults)
    {
        if (!string.IsNullOrWhiteSpace(mode))
        {
            return mode;
        }

        defaults[$"{pathPrefix}.mode"] = "hard";
        return "hard";
    }

    private static GenerateRangeSpecInput? NormalizeRange(
        GenerateRangeSpecInput? range,
        string pathPrefix,
        IDictionary<string, object?> defaults)
    {
        if (range is null)
        {
            return null;
        }

        if (range.Inclusive.HasValue)
        {
            return range;
        }

        defaults[$"{pathPrefix}.range.inclusive"] = true;
        return range with { Inclusive = true };
    }

    private static GenerateAllowedValuesSpecInput? NormalizeAllowedValues(
        GenerateAllowedValuesSpecInput? allowedValues,
        string pathPrefix,
        IDictionary<string, object?> defaults)
    {
        if (allowedValues?.Values is not { Count: > 0 })
        {
            return allowedValues;
        }

        var normalized = allowedValues.Values
            .Distinct()
            .OrderBy(static value => value)
            .ToArray();
        defaults[$"{pathPrefix}.allowed_values.values"] = normalized;
        return allowedValues with
        {
            Values = normalized
        };
    }

    private TypicalRangeResolution? NormalizeTypicalRange(
        GenerateTypicalRangeSpecInput? typicalRange,
        string pathPrefix,
        IDictionary<string, object?> defaults,
        DrawWindow window)
    {
        if (typicalRange is null)
        {
            return null;
        }

        var metric = _windowMetricDispatcher.Dispatch(typicalRange.MetricName, window);
        var resolution = _typicalRangeResolver.Resolve(
            new TypicalRangeSpec(
                MetricName: typicalRange.MetricName,
                Method: typicalRange.Method,
                Coverage: typicalRange.Coverage,
                Params: typicalRange.Params is null
                    ? null
                    : new TypicalRangePercentileParams(typicalRange.Params.PLow, typicalRange.Params.PHigh),
                WindowRef: typicalRange.WindowRef,
                Inclusive: typicalRange.Inclusive),
            metric.Value);

        defaults[$"{pathPrefix}.typical_range.resolved_range"] = new
        {
            min = resolution.ResolvedRange.Min,
            max = resolution.ResolvedRange.Max,
            inclusive = resolution.ResolvedRange.Inclusive
        };
        defaults[$"{pathPrefix}.typical_range.coverage_observed"] = resolution.CoverageObserved;
        defaults[$"{pathPrefix}.typical_range.method_version"] = resolution.MethodVersion;
        if (!typicalRange.Inclusive.HasValue)
        {
            defaults[$"{pathPrefix}.typical_range.inclusive"] = true;
        }

        return resolution;
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

    private static GenerateGenerationBudgetInput ResolveGenerationBudget(GenerateGenerationBudgetInput? input)
    {
        return new GenerateGenerationBudgetInput(
            MaxAttempts: input?.MaxAttempts,
            PoolMultiplier: input?.PoolMultiplier ?? 1d);
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
        IReadOnlyList<GenerateCandidateCriteriaInput> criteria,
        out string? rejectionReason)
    {
        rejectionReason = null;
        foreach (var criterion in criteria)
        {
            if (!TryResolveObservedValue(criterion.Name, profile, compositeScore, out var observedValue))
            {
                continue;
            }

            var evaluation = EvaluateConstraint(criterion.Name, observedValue, criterion.Value, criterion.Range, null, criterion.AllowedValues);
            if (evaluation.Matches)
            {
                continue;
            }

            rejectionReason = $"criteria:{criterion.Name}";
            return false;
        }

        if (string.Equals(strategyName, CommonRepetitionFrequencyStrategy, StringComparison.Ordinal))
        {
            return profile.FrequencyAlignment >= 0d &&
                   profile.RepeatAlignment >= 0d &&
                   profile.Top10OverlapCount >= 0d;
        }

        return compositeScore >= 0d;
    }

    private bool PassesFilters(
        CandidateProfile profile,
        IReadOnlyList<GenerateCandidateFilterInput> filters,
        out string? rejectionReason,
        out double totalSoftPenalty,
        out List<SoftConstraintPenalty> softPenalties)
    {
        rejectionReason = null;
        totalSoftPenalty = 0d;
        softPenalties = new List<SoftConstraintPenalty>();
        foreach (var filter in filters)
        {
            if (!TryResolveObservedValue(filter.Name, profile, profile.OutlierScore, out var observedValue))
            {
                continue;
            }

            var evaluation = EvaluateConstraint(filter.Name, observedValue, filter.Value, filter.Range, (filter.Min, filter.Max), filter.AllowedValues);
            if (evaluation.Matches)
            {
                continue;
            }

            if (string.Equals(filter.Mode, "soft", StringComparison.Ordinal))
            {
                var penalty = _softPenaltyResolver.Resolve(filter.Name, evaluation.ViolationDistance);
                softPenalties.Add(penalty);
                totalSoftPenalty += penalty.Penalty;
                continue;
            }

            rejectionReason = $"filter:{filter.Name}";
            return false;
        }

        return true;
    }

    private static bool TryResolveObservedValue(
        string constraintName,
        CandidateProfile profile,
        double compositeScore,
        out double observedValue)
    {
        observedValue = constraintName switch
        {
            "min_frequency_alignment" or "frequency_alignment" or "freq_alignment" => profile.FrequencyAlignment,
            "min_repeat_alignment" or "repeat_alignment" => profile.RepeatAlignment,
            "min_top10_overlap" or "top10_overlap_count" => profile.Top10OverlapCount,
            "min_composite_score" or "composite_score" => compositeScore,
            "max_consecutive_run" => profile.MaxConsecutiveRun,
            "max_neighbor_count" or "neighbor_count" or "neighbors_count" => profile.NeighborCount,
            "min_row_entropy_norm" or "row_entropy_norm" => profile.RowEntropyNorm,
            "max_hhi_linha" or "hhi_linha" => profile.HhiLinha,
            "min_slot_alignment" or "slot_alignment" => profile.SlotAlignment,
            "max_outlier_score" or "outlier_score" => profile.OutlierScore,
            "repeat_range" or "repeat_count" => profile.RepeatCount,
            _ => double.NaN
        };

        return !double.IsNaN(observedValue);
    }

    private static ConstraintEvaluation EvaluateConstraint(
        string name,
        double observedValue,
        double? scalarValue,
        GenerateRangeSpecInput? range,
        (double? Min, double? Max)? legacyMinMax,
        GenerateAllowedValuesSpecInput? allowedValues)
    {
        if (allowedValues?.Values is { Count: > 0 })
        {
            var minDistance = double.PositiveInfinity;
            foreach (var allowed in allowedValues.Values)
            {
                var distance = Math.Abs(observedValue - allowed);
                if (distance <= 1e-9d)
                {
                    return new ConstraintEvaluation(true, 0d);
                }

                minDistance = Math.Min(minDistance, distance);
            }

            return new ConstraintEvaluation(false, minDistance);
        }

        if (range is not null)
        {
            var inclusive = range.Inclusive ?? true;
            var withinRange = inclusive
                ? observedValue >= range.Min && observedValue <= range.Max
                : observedValue > range.Min && observedValue < range.Max;
            if (withinRange)
            {
                return new ConstraintEvaluation(true, 0d);
            }

            var lowerDistance = observedValue < range.Min ? range.Min - observedValue : 0d;
            var upperDistance = observedValue > range.Max ? observedValue - range.Max : 0d;
            return new ConstraintEvaluation(false, Math.Max(lowerDistance, upperDistance));
        }

        if (legacyMinMax.HasValue && (legacyMinMax.Value.Min.HasValue || legacyMinMax.Value.Max.HasValue))
        {
            var min = legacyMinMax.Value.Min ?? double.NegativeInfinity;
            var max = legacyMinMax.Value.Max ?? double.PositiveInfinity;
            if (observedValue >= min && observedValue <= max)
            {
                return new ConstraintEvaluation(true, 0d);
            }

            var lowerDistance = observedValue < min ? min - observedValue : 0d;
            var upperDistance = observedValue > max ? observedValue - max : 0d;
            return new ConstraintEvaluation(false, Math.Max(lowerDistance, upperDistance));
        }

        if (!scalarValue.HasValue)
        {
            return new ConstraintEvaluation(true, 0d);
        }

        if (name.StartsWith("min_", StringComparison.Ordinal))
        {
            return observedValue >= scalarValue.Value
                ? new ConstraintEvaluation(true, 0d)
                : new ConstraintEvaluation(false, scalarValue.Value - observedValue);
        }

        if (name.StartsWith("max_", StringComparison.Ordinal))
        {
            return observedValue <= scalarValue.Value
                ? new ConstraintEvaluation(true, 0d)
                : new ConstraintEvaluation(false, observedValue - scalarValue.Value);
        }

        var exactDistance = Math.Abs(observedValue - scalarValue.Value);
        return exactDistance <= 1e-9d
            ? new ConstraintEvaluation(true, 0d)
            : new ConstraintEvaluation(false, exactDistance);
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

    private static void IncrementCounter(IDictionary<string, int> counters, string reason)
    {
        if (counters.TryGetValue(reason, out var current))
        {
            counters[reason] = current + 1;
            return;
        }

        counters[reason] = 1;
    }

    private static void IncrementCounter(IDictionary<string, double> counters, string reason, double increment)
    {
        if (counters.TryGetValue(reason, out var current))
        {
            counters[reason] = current + increment;
            return;
        }

        counters[reason] = increment;
    }

    private static ApplicationValidationException MapDomainError(DomainInvariantViolationException ex)
    {
        const string unknownMetricPrefix = "UNKNOWN_METRIC:";
        if (ex.Message.StartsWith(unknownMetricPrefix, StringComparison.Ordinal))
        {
            var metricName = ex.Message[unknownMetricPrefix.Length..].Trim();
            return new ApplicationValidationException(
                code: "UNKNOWN_METRIC",
                message: "metric name is not listed in the metric catalog.",
                details: new Dictionary<string, object?>
                {
                    ["metric_name"] = metricName
                });
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
        double Score,
        double BaseScore,
        double SoftPenaltyTotal);

    private sealed record ConstraintEvaluation(
        bool Matches,
        double ViolationDistance);

    private sealed record ExecutionPlan(
        string StrategyName,
        string StrategyVersion,
        string SearchMethod,
        string TieBreakRule,
        ulong? SeedUsed,
        IReadOnlyList<GenerateCandidateCriteriaInput> Criteria,
        IReadOnlyList<GenerateCandidateWeightInput> Weights,
        IReadOnlyList<GenerateCandidateFilterInput> Filters,
        Dictionary<string, object?> ResolvedDefaults);
}
