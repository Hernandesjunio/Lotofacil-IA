using LotofacilMcp.Application.Mapping;
using LotofacilMcp.Application.Validation;
using LotofacilMcp.Domain.Generation;
using LotofacilMcp.Domain.Metrics;
using LotofacilMcp.Domain.Models;
using LotofacilMcp.Domain.Windows;
using LotofacilMcp.Infrastructure.DatasetVersioning;
using LotofacilMcp.Infrastructure.Providers;

namespace LotofacilMcp.Application.UseCases;

public sealed record ExplainCandidateGamesInput(
    int WindowSize,
    int? EndContestId,
    IReadOnlyList<IReadOnlyList<int>> Games,
    bool IncludeMetricBreakdown,
    bool IncludeExclusionBreakdown,
    /// <summary>Echo opcional alinhado a <c>generate_candidate_games</c> (ADR 0020).</summary>
    string? GenerationMode = null,
    /// <summary>Echo opcional da semente quando se pretende auditar replay.</summary>
    ulong? Seed = null,
    /// <summary>Echo opcional de <c>replay_guaranteed</c> na resposta de geracao.</summary>
    bool? ReplayGuaranteed = null,
    string FixturePath = "");

public sealed record ExplainCandidateGamesDeterministicHashInput(
    int WindowSize,
    int? EndContestId,
    IReadOnlyList<IReadOnlyList<int>> Games,
    bool IncludeMetricBreakdown,
    bool IncludeExclusionBreakdown,
    string? GenerationMode,
    ulong? Seed,
    bool? ReplayGuaranteed);

public sealed record MetricBreakdownEntryView(
    string MetricName,
    string MetricVersion,
    double Value,
    double Contribution,
    string Explanation);

public sealed record ExclusionBreakdownEntryView(
    string ExclusionName,
    string ExclusionVersion,
    bool Passed,
    double ObservedValue,
    double Threshold,
    string Explanation);

public sealed record ConstraintRangeView(
    double Min,
    double Max,
    bool Inclusive);

public sealed record ConstraintAllowedValuesView(IReadOnlyList<double> Values);

public sealed record ConstraintTypicalRangeView(
    string MetricName,
    string Method,
    double Coverage,
    ConstraintRangeView ResolvedRange,
    double CoverageObserved,
    string MethodVersion);

public sealed record ConstraintSpecView(
    double? Value,
    ConstraintRangeView? Range,
    ConstraintAllowedValuesView? AllowedValues,
    ConstraintTypicalRangeView? TypicalRange);

public sealed record ConstraintResultView(
    bool Passed,
    double Penalty);

public sealed record ConstraintBreakdownEntryView(
    string Kind,
    string Name,
    string Mode,
    double ObservedValue,
    ConstraintSpecView Applied,
    ConstraintResultView Result,
    string Explanation);

public sealed record CandidateStrategyExplanationView(
    string StrategyName,
    string StrategyVersion,
    string SearchMethod,
    string TieBreakRule,
    double Score,
    IReadOnlyList<MetricBreakdownEntryView> MetricBreakdown,
    IReadOnlyList<ExclusionBreakdownEntryView> ExclusionBreakdown,
    IReadOnlyList<ConstraintBreakdownEntryView> ConstraintBreakdown);

public sealed record GameExplanationView(
    IReadOnlyList<int> Game,
    IReadOnlyList<CandidateStrategyExplanationView> CandidateStrategies);

public sealed record CandidateGenerationAuditView(
    string? RequestedGenerationMode,
    string EffectiveGenerationMode,
    bool ContextSupplied,
    bool? SeedDeclared,
    bool? ReplayGuaranteed,
    string IntersectionAndRestrictions,
    string ReplayAndSeedPolicy);

public sealed record ExplainCandidateGamesResult(
    string DatasetVersion,
    string ToolVersion,
    ExplainCandidateGamesDeterministicHashInput DeterministicHashInput,
    WindowDescriptor Window,
    CandidateGenerationAuditView GenerationAudit,
    IReadOnlyList<GameExplanationView> Explanations);

public sealed class ExplainCandidateGamesUseCase
{
    public const string ToolVersion = "1.2.0";
    private const string StrategyVersion = "1.0.0";
    private const string ExclusionVersion = "1.0.0";
    private const string TypicalRangeMethodVersion = "1.0.0";

    private readonly SyntheticFixtureProvider _fixtureProvider;
    private readonly DatasetVersionService _datasetVersionService;
    private readonly WindowResolver _windowResolver;
    private readonly WindowMetricDispatcher _windowMetricDispatcher;
    private readonly V0CrossFieldValidator _validator;
    private readonly V0RequestMapper _mapper;

    public ExplainCandidateGamesUseCase(
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

    public ExplainCandidateGamesResult Execute(ExplainCandidateGamesInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        _validator.ValidateExplainCandidateGames(input);

        var snapshot = _fixtureProvider.LoadSnapshot(input.FixturePath);
        var normalizedDraws = _mapper.MapSnapshotToDomainDraws(snapshot);

        try
        {
            var window = _windowResolver.Resolve(normalizedDraws, input.WindowSize, input.EndContestId);
            var windowView = _mapper.MapWindow(window);
            var frequencyMetric = _windowMetricDispatcher.Dispatch("frequencia_por_dezena", window);
            var repeatMetric = _windowMetricDispatcher.Dispatch("repeticao_concurso_anterior", window);
            var top10Metric = _windowMetricDispatcher.Dispatch("top10_mais_sorteados", window);
            var top10Set = top10Metric.Value.Select(static value => (int)value).ToHashSet();
            var lastDraw = window.Draws[^1];
            var repeatStats = QuantileStats(repeatMetric.Value);
            var repetitionMedian = repeatStats.Median;

            var generationAudit = BuildGenerationAudit(input);

            var explanations = input.Games
                .Select(game => BuildGameExplanation(
                    game,
                    input.IncludeMetricBreakdown,
                    input.IncludeExclusionBreakdown,
                    frequencyMetric.Version,
                    repeatMetric.Version,
                    top10Metric.Version,
                    top10Set,
                    repetitionMedian,
                    repeatStats,
                    lastDraw.Numbers,
                    frequencyMetric.Value))
                .ToArray();

            return new ExplainCandidateGamesResult(
                DatasetVersion: _datasetVersionService.CreateFromSnapshot(snapshot),
                ToolVersion: ToolVersion,
                DeterministicHashInput: new ExplainCandidateGamesDeterministicHashInput(
                    input.WindowSize,
                    input.EndContestId,
                    input.Games.Select(game => (IReadOnlyList<int>)game.ToArray()).ToArray(),
                    input.IncludeMetricBreakdown,
                    input.IncludeExclusionBreakdown,
                    string.IsNullOrWhiteSpace(input.GenerationMode) ? null : input.GenerationMode.Trim(),
                    input.Seed,
                    input.ReplayGuaranteed),
                Window: windowView,
                GenerationAudit: generationAudit,
                Explanations: explanations);
        }
        catch (DomainInvariantViolationException ex)
        {
            throw MapDomainError(ex);
        }
    }

    private static CandidateGenerationAuditView BuildGenerationAudit(ExplainCandidateGamesInput input)
    {
        var modeTrimmed = string.IsNullOrWhiteSpace(input.GenerationMode) ? null : input.GenerationMode.Trim();
        var contextSupplied = modeTrimmed is not null || input.Seed.HasValue || input.ReplayGuaranteed is not null;
        var effective = modeTrimmed ?? "unspecified";
        if (string.Equals(effective, GenerationModes.RandomUnrestricted, StringComparison.Ordinal))
        {
            effective = GenerationModes.RandomUnrestricted;
        }
        else if (string.Equals(effective, GenerationModes.BehaviorFiltered, StringComparison.Ordinal))
        {
            effective = GenerationModes.BehaviorFiltered;
        }
        // else: remain "unspecified" or invalid (validator should reject)

        var seedDeclared = !contextSupplied && !input.Seed.HasValue
            ? (bool?)null
            : input.Seed.HasValue;

        return new CandidateGenerationAuditView(
            RequestedGenerationMode: modeTrimmed,
            EffectiveGenerationMode: effective,
            ContextSupplied: contextSupplied,
            SeedDeclared: seedDeclared,
            ReplayGuaranteed: input.ReplayGuaranteed,
            IntersectionAndRestrictions: BuildIntersectionText(effective),
            ReplayAndSeedPolicy: BuildReplayText(input, contextSupplied, seedDeclared));
    }

    private static string BuildIntersectionText(string effectiveGenerationMode)
    {
        if (string.Equals(effectiveGenerationMode, GenerationModes.BehaviorFiltered, StringComparison.Ordinal))
        {
            return "Restricoes efetivas: o conjunto admissivel e a interseccao (conjunçao) de cada restricao declarada no pedido " +
                "de geracao (criterios por faixa, filtros, exclusoes estruturais e plano de estrategias), salvo semantica " +
                "alternativa documentada. O jogo e avaliado adiante em relacao a varias perspectivas de aderencia descritiva; " +
                "isso nao implica maior chance no sorteio oficial e continua condicionado a janela.";
        }

        if (string.Equals(effectiveGenerationMode, GenerationModes.RandomUnrestricted, StringComparison.Ordinal))
        {
            return "No modo random_unrestricted, a politica alvo nao aplica por defeito exclusoes estruturais nem criterios de " +
                "aderencia nao declarados. O candidato valido continua a ser 15 dezenas distintas (ordenacao canonica). A " +
                "metrica e o breakdown abaixo caracterizam o jogo perante a janela, sem reintroduzir como filtro obrigatorio " +
                "guardrails nao pedidos nesse modo.";
        }

        return "Sem `generation_mode` alinhado ao `generate_candidate_games`, a geracao nao e auditada aqui. " +
            "Inclua `generation_mode` e, se for util, `seed` e `replay_guaranteed` coerentes com a resposta de geracao. " +
            "Com `behavior_filtered`, o conjunto admissivel e tipicamente a interseccao das restricoes declaradas. " +
            "Com `random_unrestricted`, nao se exigem por defeito filtro de comportamento nao declarado (ADR 0020).";
    }

    private static string BuildReplayText(
        ExplainCandidateGamesInput input,
        bool contextSupplied,
        bool? seedDeclared)
    {
        if (input.ReplayGuaranteed is null)
        {
            if (!contextSupplied)
            {
                return "Episodio: contexto de replay nao fornecido; faltam `seed` e/ou `replay_guaranteed` " +
                    "se quiser justificar a reprodutibilidade de pedidos alinhados a `generate_candidate_games`.";
            }

            return "Episodio: `replay_guaranteed` nao foi reenviado nesta explicacao; a auditoria de " +
                "reprodutibilidade fica parcial (fornecha o echo da resposta de geracao quando possivel).";
        }

        if (input.ReplayGuaranteed is false)
        {
            return "O episodio nao e replayavel quanto a lista concreta de candidatos: com estocastica e sem semente, " +
                "uma nova invocacao com o mesmo JSON (salvo a politica de `deterministic_hash` para inputs nao " +
                "aleatorios) pode devolver outro lote; os indicadores abaixo permanecem condicionados a janela.";
        }

        // replay_guaranteed is true
        if (seedDeclared is not true)
        {
            return "A resposta de geracao indica `replay_guaranteed` verdadeiro; ainda assim, semente nao reenviada " +
                "aqui, pelo que a cadeia canónica nao fica plenamente audivel nesta explicacao. Com `seed` e mesmos " +
                "inputs, a porcao estocastica reproduz a sequencia de candidatos declarada, sujeita a `unique_games` e regras.";
        }

        return "Com a mesma `seed`, o mesmo `dataset_version` e o mesmo request canónico, a porcao estocastica " +
            "e reprodutivel (sequencia de candidatos ordenada, com `unique_games` e restricoes declaradas). " +
            "A ausencia de promessa fora desse vinculo permanece vinculada a planos nao estocasticos e a politica de " +
            "`random_unrestricted` / `behavior_filtered` indicada no modo de geracao (sem implicar resultado futuro do sorteio).";
    }

    private static GameExplanationView BuildGameExplanation(
        IReadOnlyList<int> game,
        bool includeMetricBreakdown,
        bool includeExclusionBreakdown,
        string frequencyVersion,
        string repetitionVersion,
        string top10Version,
        HashSet<int> top10Set,
        double repetitionMedian,
        QuantileStatsResult repeatStats,
        IReadOnlyList<int> lastDrawNumbers,
        IReadOnlyList<double> frequencyByDezena)
    {
        var profile = BuildProfile(game, top10Set, repetitionMedian, lastDrawNumbers, frequencyByDezena);
        var exclusions = BuildExclusions(profile, includeExclusionBreakdown);

        var strategies = new List<CandidateStrategyExplanationView>
        {
            BuildCommonRepetitionFrequency(profile, includeMetricBreakdown, exclusions, frequencyVersion, repetitionVersion, top10Version, repeatStats),
            BuildRowEntropyBalance(profile, includeMetricBreakdown, exclusions, frequencyVersion),
            BuildSlotWeighted(profile, includeMetricBreakdown, exclusions),
            BuildOutlierCandidate(profile, includeMetricBreakdown, exclusions),
            BuildDeclaredCompositeProfile(profile, includeMetricBreakdown, exclusions, frequencyVersion, repetitionVersion)
        };

        var ordered = strategies
            .OrderByDescending(strategy => strategy.Score)
            .ThenBy(strategy => strategy.StrategyName, StringComparer.Ordinal)
            .ToArray();

        return new GameExplanationView(game.ToArray(), ordered);
    }

    private static CandidateStrategyExplanationView BuildCommonRepetitionFrequency(
        GameProfile profile,
        bool includeMetricBreakdown,
        IReadOnlyList<ExclusionBreakdownEntryView> exclusions,
        string frequencyVersion,
        string repetitionVersion,
        string top10Version,
        QuantileStatsResult repeatStats)
    {
        var score = Clamp01(0.6 * profile.FrequencyAlignment + 0.4 * profile.RepeatAlignment);
        var penalty = 0d;
        if (profile.Top10OverlapCount < 6)
        {
            penalty = 0.1;
            score = Clamp01(score - penalty);
        }

        var metricBreakdown = includeMetricBreakdown
            ? new[]
            {
                new MetricBreakdownEntryView(
                    MetricName: "frequencia_por_dezena",
                    MetricVersion: frequencyVersion,
                    Value: profile.FrequencyAlignment,
                    Contribution: 0.6 * profile.FrequencyAlignment,
                    Explanation: "Aderencia media da frequencia das dezenas do jogo na janela."),
                new MetricBreakdownEntryView(
                    MetricName: "repeticao_concurso_anterior",
                    MetricVersion: repetitionVersion,
                    Value: profile.RepeatAlignment,
                    Contribution: 0.4 * profile.RepeatAlignment,
                    Explanation: "Distancia normalizada da repeticao do jogo para a mediana da janela."),
                new MetricBreakdownEntryView(
                    MetricName: "top10_mais_sorteados",
                    MetricVersion: top10Version,
                    Value: profile.Top10OverlapRatio,
                    Contribution: 0d,
                    Explanation: "Razao de dezenas do jogo presentes no top10 historico (filtro estrutural da estrategia).")
            }
            : Array.Empty<MetricBreakdownEntryView>();

        var constraintBreakdown = BuildConstraintBreakdown(
            profile,
            exclusions,
            additional:
            [
                new ConstraintBreakdownEntryView(
                    Kind: "criterion",
                    Name: "top10_overlap_count",
                    Mode: "soft",
                    ObservedValue: profile.Top10OverlapCount,
                    Applied: new ConstraintSpecView(
                        Value: null,
                        Range: new ConstraintRangeView(6, 10, true),
                        AllowedValues: null,
                        TypicalRange: null),
                    Result: new ConstraintResultView(
                        Passed: profile.Top10OverlapCount >= 6,
                        Penalty: penalty),
                    Explanation: "Penaliza candidatos com sobreposicao no top10 historico abaixo do limiar."),
                new ConstraintBreakdownEntryView(
                    Kind: "criterion",
                    Name: "repeat_count",
                    Mode: "hard",
                    ObservedValue: profile.RepeatCount,
                    Applied: new ConstraintSpecView(
                        Value: null,
                        Range: null,
                        AllowedValues: null,
                        TypicalRange: new ConstraintTypicalRangeView(
                            MetricName: "repeticao_concurso_anterior",
                            Method: "iqr",
                            Coverage: 0.8,
                            ResolvedRange: new ConstraintRangeView(repeatStats.Q1, repeatStats.Q3, true),
                            CoverageObserved: repeatStats.CoverageObservedInIqr,
                            MethodVersion: TypicalRangeMethodVersion)),
                    Result: new ConstraintResultView(
                        Passed: IsInRange(profile.RepeatCount, repeatStats.Q1, repeatStats.Q3, inclusive: true),
                        Penalty: 0d),
                    Explanation: "Audita aderencia do repeat_count do jogo a faixa tipica (IQR) da repeticao na janela.")
            ]);

        return new CandidateStrategyExplanationView(
            StrategyName: "common_repetition_frequency",
            StrategyVersion: StrategyVersion,
            SearchMethod: "greedy_topk",
            TieBreakRule: "hhi_linha_asc_then_lexicographic_numbers_asc",
            Score: score,
            MetricBreakdown: metricBreakdown,
            ExclusionBreakdown: exclusions,
            ConstraintBreakdown: constraintBreakdown);
    }

    private static CandidateStrategyExplanationView BuildRowEntropyBalance(
        GameProfile profile,
        bool includeMetricBreakdown,
        IReadOnlyList<ExclusionBreakdownEntryView> exclusions,
        string frequencyVersion)
    {
        var score = profile.FrequencyAlignment;
        var entropyPenalty = 0d;
        if (profile.RowEntropyNorm < 0.95)
        {
            entropyPenalty = 0.15;
            score -= entropyPenalty;
        }

        var hhiPenalty = 0d;
        if (profile.HhiLinha > 0.25)
        {
            hhiPenalty = 0.15;
            score -= hhiPenalty;
        }

        var metricBreakdown = includeMetricBreakdown
            ? new[]
            {
                new MetricBreakdownEntryView(
                    MetricName: "frequencia_por_dezena",
                    MetricVersion: frequencyVersion,
                    Value: profile.FrequencyAlignment,
                    Contribution: Clamp01(score),
                    Explanation: "Score base da estrategia no recorte V1."),
                new MetricBreakdownEntryView(
                    MetricName: "entropia_linha",
                    MetricVersion: "1.0.0",
                    Value: profile.RowEntropyNorm,
                    Contribution: profile.RowEntropyNorm < 0.95 ? -entropyPenalty : 0d,
                    Explanation: "Penaliza jogos abaixo do limiar de entropia de linhas da estrategia."),
                new MetricBreakdownEntryView(
                    MetricName: "hhi_concentracao",
                    MetricVersion: "1.0.0",
                    Value: profile.HhiLinha,
                    Contribution: profile.HhiLinha > 0.25 ? -hhiPenalty : 0d,
                    Explanation: "Penaliza concentracao excessiva em linhas para manter equilibrio.")
            }
            : Array.Empty<MetricBreakdownEntryView>();

        var constraintBreakdown = BuildConstraintBreakdown(
            profile,
            exclusions,
            additional:
            [
                new ConstraintBreakdownEntryView(
                    Kind: "criterion",
                    Name: "row_entropy_norm",
                    Mode: "soft",
                    ObservedValue: profile.RowEntropyNorm,
                    Applied: new ConstraintSpecView(
                        Value: null,
                        Range: new ConstraintRangeView(0.95, 1.0, true),
                        AllowedValues: null,
                        TypicalRange: null),
                    Result: new ConstraintResultView(
                        Passed: profile.RowEntropyNorm >= 0.95,
                        Penalty: entropyPenalty),
                    Explanation: "Penaliza jogos com entropia normalizada abaixo do limiar."),
                new ConstraintBreakdownEntryView(
                    Kind: "criterion",
                    Name: "hhi_linha",
                    Mode: "soft",
                    ObservedValue: profile.HhiLinha,
                    Applied: new ConstraintSpecView(
                        Value: null,
                        Range: new ConstraintRangeView(0.0, 0.25, true),
                        AllowedValues: null,
                        TypicalRange: null),
                    Result: new ConstraintResultView(
                        Passed: profile.HhiLinha <= 0.25,
                        Penalty: hhiPenalty),
                    Explanation: "Penaliza jogos com concentracao (HHI) acima do limiar.")
            ]);

        return new CandidateStrategyExplanationView(
            StrategyName: "row_entropy_balance",
            StrategyVersion: StrategyVersion,
            SearchMethod: "sampled",
            TieBreakRule: "hhi_coluna_asc_then_lexicographic_numbers_asc",
            Score: Clamp01(score),
            MetricBreakdown: metricBreakdown,
            ExclusionBreakdown: exclusions,
            ConstraintBreakdown: constraintBreakdown);
    }

    private static CandidateStrategyExplanationView BuildSlotWeighted(
        GameProfile profile,
        bool includeMetricBreakdown,
        IReadOnlyList<ExclusionBreakdownEntryView> exclusions)
    {
        var metricBreakdown = includeMetricBreakdown
            ? new[]
            {
                new MetricBreakdownEntryView(
                    MetricName: "analise_slot",
                    MetricVersion: "1.0.0",
                    Value: profile.SlotAlignment,
                    Contribution: profile.SlotAlignment,
                    Explanation: "Aderencia deterministica do jogo ao perfil slot x dezena."),
                new MetricBreakdownEntryView(
                    MetricName: "surpresa_slot",
                    MetricVersion: "1.0.0",
                    Value: profile.SlotSurprise,
                    Contribution: 0d,
                    Explanation: "Usado no tie-break da estrategia para desempatar mesma aderencia.")
            }
            : Array.Empty<MetricBreakdownEntryView>();

        var constraintBreakdown = BuildConstraintBreakdown(profile, exclusions, additional: Array.Empty<ConstraintBreakdownEntryView>());

        return new CandidateStrategyExplanationView(
            StrategyName: "slot_weighted",
            StrategyVersion: StrategyVersion,
            SearchMethod: "exhaustive",
            TieBreakRule: "surpresa_slot_asc_then_lexicographic_numbers_asc",
            Score: profile.SlotAlignment,
            MetricBreakdown: metricBreakdown,
            ExclusionBreakdown: exclusions,
            ConstraintBreakdown: constraintBreakdown);
    }

    private static CandidateStrategyExplanationView BuildOutlierCandidate(
        GameProfile profile,
        bool includeMetricBreakdown,
        IReadOnlyList<ExclusionBreakdownEntryView> exclusions)
    {
        var metricBreakdown = includeMetricBreakdown
            ? new[]
            {
                new MetricBreakdownEntryView(
                    MetricName: "outlier_score",
                    MetricVersion: "1.0.0",
                    Value: profile.OutlierScore,
                    Contribution: profile.OutlierScore,
                    Explanation: "Distancia estrutural normalizada em relacao ao centro da janela."),
                new MetricBreakdownEntryView(
                    MetricName: "surpresa_slot",
                    MetricVersion: "1.0.0",
                    Value: profile.SlotSurprise,
                    Contribution: 0d,
                    Explanation: "Desempate prioriza jogos com maior surpresa de slot."),
                new MetricBreakdownEntryView(
                    MetricName: "entropia_linha",
                    MetricVersion: "1.0.0",
                    Value: profile.RowEntropyNorm,
                    Contribution: 0d,
                    Explanation: "Filtro minimo de dispersao por linha para evitar colapso espacial.")
            }
            : Array.Empty<MetricBreakdownEntryView>();

        var constraintBreakdown = BuildConstraintBreakdown(profile, exclusions, additional: Array.Empty<ConstraintBreakdownEntryView>());

        return new CandidateStrategyExplanationView(
            StrategyName: "outlier_candidate",
            StrategyVersion: StrategyVersion,
            SearchMethod: "sampled",
            TieBreakRule: "surpresa_slot_desc_then_lexicographic_numbers_asc",
            Score: profile.OutlierScore,
            MetricBreakdown: metricBreakdown,
            ExclusionBreakdown: exclusions,
            ConstraintBreakdown: constraintBreakdown);
    }

    private static CandidateStrategyExplanationView BuildDeclaredCompositeProfile(
        GameProfile profile,
        bool includeMetricBreakdown,
        IReadOnlyList<ExclusionBreakdownEntryView> exclusions,
        string frequencyVersion,
        string repetitionVersion)
    {
        var score = Clamp01(
            0.3 * profile.FrequencyAlignment +
            0.2 * profile.RepeatAlignment +
            0.2 * profile.SlotAlignment +
            0.2 * profile.RowEntropyNorm +
            0.1 * (1d - profile.HhiLinha));

        var metricBreakdown = includeMetricBreakdown
            ? new[]
            {
                new MetricBreakdownEntryView(
                    MetricName: "freq_alignment",
                    MetricVersion: frequencyVersion,
                    Value: profile.FrequencyAlignment,
                    Contribution: 0.3 * profile.FrequencyAlignment,
                    Explanation: "Componente declarado de aderencia de frequencia."),
                new MetricBreakdownEntryView(
                    MetricName: "repeat_alignment",
                    MetricVersion: repetitionVersion,
                    Value: profile.RepeatAlignment,
                    Contribution: 0.2 * profile.RepeatAlignment,
                    Explanation: "Componente declarado de alinhamento com repeticao historica."),
                new MetricBreakdownEntryView(
                    MetricName: "slot_alignment",
                    MetricVersion: "1.0.0",
                    Value: profile.SlotAlignment,
                    Contribution: 0.2 * profile.SlotAlignment,
                    Explanation: "Componente declarado de aderencia de slots."),
                new MetricBreakdownEntryView(
                    MetricName: "row_entropy_norm",
                    MetricVersion: "1.0.0",
                    Value: profile.RowEntropyNorm,
                    Contribution: 0.2 * profile.RowEntropyNorm,
                    Explanation: "Componente declarado de dispersao por linhas."),
                new MetricBreakdownEntryView(
                    MetricName: "hhi_linha",
                    MetricVersion: "1.0.0",
                    Value: profile.HhiLinha,
                    Contribution: 0.1 * (1d - profile.HhiLinha),
                    Explanation: "Componente declarado de concentracao invertida.")
            }
            : Array.Empty<MetricBreakdownEntryView>();

        var constraintBreakdown = BuildConstraintBreakdown(profile, exclusions, additional: Array.Empty<ConstraintBreakdownEntryView>());

        return new CandidateStrategyExplanationView(
            StrategyName: "declared_composite_profile",
            StrategyVersion: StrategyVersion,
            SearchMethod: "greedy_topk",
            TieBreakRule: "outlier_score_asc_then_hhi_linha_asc_then_lexicographic_numbers_asc",
            Score: score,
            MetricBreakdown: metricBreakdown,
            ExclusionBreakdown: exclusions,
            ConstraintBreakdown: constraintBreakdown);
    }

    private static IReadOnlyList<ConstraintBreakdownEntryView> BuildConstraintBreakdown(
        GameProfile profile,
        IReadOnlyList<ExclusionBreakdownEntryView> exclusions,
        IReadOnlyList<ConstraintBreakdownEntryView> additional)
    {
        var list = new List<ConstraintBreakdownEntryView>(exclusions.Count + additional.Count);

        foreach (var exclusion in exclusions)
        {
            list.Add(new ConstraintBreakdownEntryView(
                Kind: "filter",
                Name: exclusion.ExclusionName,
                Mode: "hard",
                ObservedValue: exclusion.ObservedValue,
                Applied: new ConstraintSpecView(
                    Value: exclusion.Threshold,
                    Range: null,
                    AllowedValues: null,
                    TypicalRange: null),
                Result: new ConstraintResultView(
                    Passed: exclusion.Passed,
                    Penalty: 0d),
                Explanation: exclusion.Explanation));
        }

        foreach (var item in additional)
        {
            list.Add(item);
        }

        return list;
    }

    private static bool IsInRange(double value, double min, double max, bool inclusive)
    {
        if (inclusive)
        {
            return value >= min && value <= max;
        }

        return value > min && value < max;
    }

    private static IReadOnlyList<ExclusionBreakdownEntryView> BuildExclusions(GameProfile profile, bool includeExclusionBreakdown)
    {
        if (!includeExclusionBreakdown)
        {
            return Array.Empty<ExclusionBreakdownEntryView>();
        }

        return
        [
            new ExclusionBreakdownEntryView(
                ExclusionName: "max_consecutive_run",
                ExclusionVersion: ExclusionVersion,
                Passed: profile.MaxConsecutiveRun <= 8d,
                ObservedValue: profile.MaxConsecutiveRun,
                Threshold: 8d,
                Explanation: "Rejeita jogos com sequencia maxima acima do limite canonicamente declarado."),
            new ExclusionBreakdownEntryView(
                ExclusionName: "max_neighbor_count",
                ExclusionVersion: ExclusionVersion,
                Passed: profile.NeighborCount <= 7d,
                ObservedValue: profile.NeighborCount,
                Threshold: 7d,
                Explanation: "Controla excesso de dezenas adjacentes no jogo."),
            new ExclusionBreakdownEntryView(
                ExclusionName: "min_row_entropy_norm",
                ExclusionVersion: ExclusionVersion,
                Passed: profile.RowEntropyNorm >= 0.82d,
                ObservedValue: profile.RowEntropyNorm,
                Threshold: 0.82d,
                Explanation: "Exige dispersao minima por linha no volante."),
            new ExclusionBreakdownEntryView(
                ExclusionName: "max_hhi_linha",
                ExclusionVersion: ExclusionVersion,
                Passed: profile.HhiLinha <= 0.30d,
                ObservedValue: profile.HhiLinha,
                Threshold: 0.30d,
                Explanation: "Limita concentracao excessiva de dezenas em poucas linhas."),
            new ExclusionBreakdownEntryView(
                ExclusionName: "min_slot_alignment",
                ExclusionVersion: ExclusionVersion,
                Passed: profile.SlotAlignment >= 0.08d,
                ObservedValue: profile.SlotAlignment,
                Threshold: 0.08d,
                Explanation: "Exige aderencia minima ao padrao de slots observado.")
        ];
    }

    private static GameProfile BuildProfile(
        IReadOnlyList<int> game,
        HashSet<int> top10Set,
        double repetitionMedian,
        IReadOnlyList<int> lastDrawNumbers,
        IReadOnlyList<double> frequencyByDezena)
    {
        var maxFrequency = frequencyByDezena.Max();
        var frequencyAlignment = game.Average(number => frequencyByDezena[number - 1] / maxFrequency);

        var repetition = game.Count(number => lastDrawNumbers.Contains(number));
        var repeatAlignment = Clamp01(1d - Math.Abs(repetition - repetitionMedian) / 15d);

        Span<int> rowCounts = stackalloc int[5];
        foreach (var number in game)
        {
            rowCounts[(number - 1) / 5]++;
        }

        var rowEntropyNorm = Clamp01(ShannonEntropyBits.FromNonNegativeCounts(rowCounts) / Math.Log2(5d));
        var hhiLinha = Clamp01(HerfindahlHirschmanIndex.FromNonNegativeCounts(rowCounts));
        var neighborCount = CountNeighbors(game);
        var maxConsecutiveRun = VizinhosConsecutivos.MaxConsecutiveAdjacencyRunLength(game);
        var slotAlignment = ComputeSlotAlignment(game);
        var slotSurprise = Clamp01(1d - slotAlignment);
        var top10OverlapCount = game.Count(number => top10Set.Contains(number));
        var top10OverlapRatio = top10OverlapCount / 10d;
        var outlierScore = Clamp01((1d - frequencyAlignment + 1d - repeatAlignment + (1d - rowEntropyNorm) + hhiLinha) / 4d);

        return new GameProfile(
            FrequencyAlignment: frequencyAlignment,
            RepeatAlignment: repeatAlignment,
            SlotAlignment: slotAlignment,
            SlotSurprise: slotSurprise,
            RowEntropyNorm: rowEntropyNorm,
            HhiLinha: hhiLinha,
            NeighborCount: neighborCount,
            MaxConsecutiveRun: maxConsecutiveRun,
            Top10OverlapCount: top10OverlapCount,
            Top10OverlapRatio: top10OverlapRatio,
            OutlierScore: outlierScore)
        {
            RepeatCount = repetition
        };
    }

    private static int CountNeighbors(IReadOnlyList<int> sortedGame)
    {
        var count = 0;
        for (var i = 1; i < sortedGame.Count; i++)
        {
            if (sortedGame[i] - sortedGame[i - 1] == 1)
            {
                count++;
            }
        }

        return count;
    }

    private static double ComputeSlotAlignment(IReadOnlyList<int> sortedGame)
    {
        double distanceSum = 0d;
        for (var i = 0; i < sortedGame.Count; i++)
        {
            var expected = 1d + i * (24d / 14d);
            distanceSum += Math.Abs(sortedGame[i] - expected) / 24d;
        }

        return Clamp01(1d - (distanceSum / sortedGame.Count));
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

    private sealed record GameProfile(
        double FrequencyAlignment,
        double RepeatAlignment,
        double SlotAlignment,
        double SlotSurprise,
        double RowEntropyNorm,
        double HhiLinha,
        int NeighborCount,
        int MaxConsecutiveRun,
        int Top10OverlapCount,
        double Top10OverlapRatio,
        double OutlierScore)
    {
        public int RepeatCount { get; init; }
    }

    private static QuantileStatsResult QuantileStats(IReadOnlyList<double> values)
    {
        if (values.Count == 0)
        {
            return new QuantileStatsResult(0d, 0d, 0d, 0d, 0);
        }

        var ordered = values.OrderBy(static v => v).ToArray();
        var median = Quantile(ordered, 0.5);
        var q1 = Quantile(ordered, 0.25);
        var q3 = Quantile(ordered, 0.75);
        var coverageObserved = ordered.Count(v => v >= q1 && v <= q3) / (double)ordered.Length;
        return new QuantileStatsResult(q1, median, q3, coverageObserved, ordered.Length);
    }

    private static double Quantile(double[] sorted, double p)
    {
        if (sorted.Length == 1)
        {
            return sorted[0];
        }

        var idx = (sorted.Length - 1) * p;
        var lo = (int)Math.Floor(idx);
        var hi = (int)Math.Ceiling(idx);
        if (lo == hi)
        {
            return sorted[lo];
        }

        var t = idx - lo;
        return sorted[lo] * (1d - t) + sorted[hi] * t;
    }

    private sealed record QuantileStatsResult(double Q1, double Median, double Q3, double CoverageObservedInIqr, int N);
}
