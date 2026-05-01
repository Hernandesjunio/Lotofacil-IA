using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace LotofacilMcp.Server.Tools;

[McpServerToolType]
public sealed class V0McpTools
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private enum VerbosityLevel
    {
        Minimal,
        Standard,
        Full
    }

    private static VerbosityLevel ParseVerbosity(string? verbosity)
    {
        return verbosity?.Trim().ToLowerInvariant() switch
        {
            "minimal" => VerbosityLevel.Minimal,
            "full" => VerbosityLevel.Full,
            _ => VerbosityLevel.Standard
        };
    }

    [McpServerTool(Name = "discover_capabilities"), Description("Publica metadados determinísticos da superfície de capacidades desta build.")]
    public CallToolResult DiscoverCapabilities(
        V0Tools tools,
        [Description("Controle de verbosidade do resumo humano no canal Content: minimal | standard | full.")] string? verbosity = null)
    {
        var payload = tools.DiscoverCapabilities(new DiscoverCapabilitiesRequest(Verbosity: verbosity));
        return ToToolResult(payload, payload is ContractErrorEnvelope, verbosity);
    }

    [McpServerTool(Name = "help"), Description("Retorna ajuda básica e o índice de templates (resources) disponíveis.")]
    public CallToolResult Help(
        V0Tools tools,
        [Description("Controle de verbosidade do resumo humano no canal Content: minimal | standard | full.")] string? verbosity = null)
    {
        var payload = tools.Help();
        return ToToolResult(payload, payload is ContractErrorEnvelope, verbosity);
    }

    [McpServerTool(Name = "get_draw_window"), Description("Retorna um recorte canônico de concursos da Lotofácil.")]
    public CallToolResult GetDrawWindow(
        V0Tools tools,
        [Description("Quantidade de concursos consecutivos na janela.")] int? window_size = null,
        [Description("Concurso inicial inclusivo (alternativa a window_size+end, ADR 0008 D2).")]
        int? start_contest_id = null,
        [Description("Concurso final inclusivo da janela.")] int? end_contest_id = null,
        [Description("Página 1-based para paginação determinística do payload (apenas em verbosity=full).")] int? page = null,
        [Description("Tamanho da página (1..500). Default quando paginando: 200 (apenas em verbosity=full).")] int? page_size = null,
        [Description("Projeção server-side: lista de campos top-level a incluir no StructuredContent. Campos inválidos => INVALID_REQUEST com allowed_fields.")] IReadOnlyList<string>? fields = null,
        [Description("Controle de verbosidade do resumo humano no canal Content: minimal | standard | full.")] string? verbosity = null)
    {
        if (ParseVerbosity(verbosity) != VerbosityLevel.Full && (page is not null || page_size is not null))
        {
            var err = new ContractErrorEnvelope(new ContractError(
                "INVALID_REQUEST",
                "Pagination requires verbosity=full.",
                new Dictionary<string, object?> { ["constraint"] = "page/page_size are only allowed when verbosity=full" }));
            return ToToolResult(err, isError: true, verbosity);
        }

        var payload = tools.GetDrawWindow(new GetDrawWindowRequest(
            WindowSize: window_size,
            StartContestId: start_contest_id,
            EndContestId: end_contest_id,
            Page: page,
            PageSize: page_size,
            Fields: fields,
            Verbosity: verbosity));

        return ToToolResult(payload, payload is ContractErrorEnvelope, verbosity);
    }

    [McpServerTool(Name = "compute_window_metrics"), Description("Calcula métricas canônicas para uma janela de concursos.")]
    public CallToolResult ComputeWindowMetrics(
        V0Tools tools,
        [Description("Quantidade de concursos consecutivos na janela.")] int? window_size = null,
        [Description("Concurso inicial inclusivo (alternativa a window_size+end, ADR 0008 D2).")]
        int? start_contest_id = null,
        [Description("Concurso final inclusivo da janela.")] int? end_contest_id = null,
        [Description("Lista de métricas canônicas a calcular.")] IReadOnlyList<MetricRequest>? metrics = null,
        [Description("Permite opt-in para métricas pendentes de detalhamento.")] bool allow_pending = false,
        [Description("Página 1-based para paginação determinística do payload (apenas em verbosity=full).")] int? page = null,
        [Description("Tamanho da página (1..500). Default quando paginando: 200 (apenas em verbosity=full).")] int? page_size = null,
        [Description("Inclui campos explicativos (`explanation`) quando true. Default: true.")] bool include_explanations = true,
        [Description("Projeção server-side: lista de campos top-level a incluir no StructuredContent.")] IReadOnlyList<string>? fields = null,
        [Description("Controle de verbosidade do resumo humano no canal Content: minimal | standard | full.")] string? verbosity = null)
    {
        if (ParseVerbosity(verbosity) != VerbosityLevel.Full && (page is not null || page_size is not null))
        {
            var err = new ContractErrorEnvelope(new ContractError(
                "INVALID_REQUEST",
                "Pagination requires verbosity=full.",
                new Dictionary<string, object?> { ["constraint"] = "page/page_size are only allowed when verbosity=full" }));
            return ToToolResult(err, isError: true, verbosity);
        }

        var payload = tools.ComputeWindowMetrics(new ComputeWindowMetricsRequest(
            WindowSize: window_size,
            StartContestId: start_contest_id,
            EndContestId: end_contest_id,
            Metrics: metrics,
            AllowPending: allow_pending,
            Page: page,
            PageSize: page_size,
            Fields: fields,
            IncludeExplanations: include_explanations,
            Verbosity: verbosity));

        return ToToolResult(payload, payload is ContractErrorEnvelope, verbosity);
    }

    [McpServerTool(Name = "analyze_indicator_stability"), Description("Compara indicadores na janela e ranqueia estabilidade relativa.")]
    public CallToolResult AnalyzeIndicatorStability(
        V0Tools tools,
        [Description("Quantidade de concursos consecutivos na janela.")] int? window_size = null,
        [Description("Concurso inicial inclusivo (alternativa a window_size+end, ADR 0008 D2).")]
        int? start_contest_id = null,
        [Description("Concurso final inclusivo da janela.")] int? end_contest_id = null,
        [Description("Indicadores canônicos para analise de estabilidade.")] IReadOnlyList<StabilityIndicatorRequestDto>? indicators = null,
        [Description("Metodo de normalizacao de volatilidade.")] string? normalization_method = null,
        [Description("Quantidade maxima de itens no ranking final.")] int top_k = 5,
        [Description("Historico minimo necessario para calcular estabilidade.")] int min_history = 20,
        [Description("Página 1-based para paginação determinística do payload (apenas em verbosity=full).")] int? page = null,
        [Description("Tamanho da página (1..500). Default quando paginando: 200 (apenas em verbosity=full).")] int? page_size = null,
        [Description("Inclui campos explicativos (`explanation`) quando true. Default: true.")] bool include_explanations = true,
        [Description("Projeção server-side: lista de campos top-level a incluir no StructuredContent.")] IReadOnlyList<string>? fields = null,
        [Description("Controle de verbosidade do resumo humano no canal Content: minimal | standard | full.")] string? verbosity = null)
    {
        if (ParseVerbosity(verbosity) != VerbosityLevel.Full && (page is not null || page_size is not null))
        {
            var err = new ContractErrorEnvelope(new ContractError(
                "INVALID_REQUEST",
                "Pagination requires verbosity=full.",
                new Dictionary<string, object?> { ["constraint"] = "page/page_size are only allowed when verbosity=full" }));
            return ToToolResult(err, isError: true, verbosity);
        }

        var payload = tools.AnalyzeIndicatorStability(new AnalyzeIndicatorStabilityRequest(
            WindowSize: window_size,
            StartContestId: start_contest_id,
            EndContestId: end_contest_id,
            Indicators: indicators,
            NormalizationMethod: normalization_method,
            TopK: top_k,
            MinHistory: min_history,
            Page: page,
            PageSize: page_size,
            Fields: fields,
            IncludeExplanations: include_explanations,
            Verbosity: verbosity));

        return ToToolResult(payload, payload is ContractErrorEnvelope, verbosity);
    }

    [McpServerTool(Name = "compose_indicator_analysis"), Description("Composição declarativa de indicadores (recorte: target dezena, weighted_rank).")]
    public CallToolResult ComposeIndicatorAnalysis(
        V0Tools tools,
        [Description("Tamanho da janela.")] int? window_size = null,
        [Description("Concurso inicial inclusivo (alternativa a window_size+end, ADR 0008 D2).")]
        int? start_contest_id = null,
        [Description("Concurso final inclusivo.")] int? end_contest_id = null,
        [Description("Unidade alvo (recorte: dezena).")] string? target = null,
        [Description("Operador (recorte: weighted_rank).")] string? @operator = null,
        [Description("Componentes com métrica, transform e peso.")] IReadOnlyList<ComposeIndicatorComponentRequest>? components = null,
        [Description("Limite do ranking.")] int top_k = 10,
        [Description("Página 1-based para paginação determinística do payload (apenas em verbosity=full).")] int? page = null,
        [Description("Tamanho da página (1..500). Default quando paginando: 200 (apenas em verbosity=full).")] int? page_size = null,
        [Description("Inclui campos explicativos (`explanation`) quando true. Default: true.")] bool include_explanations = true,
        [Description("Projeção server-side: lista de campos top-level a incluir no StructuredContent.")] IReadOnlyList<string>? fields = null,
        [Description("Controle de verbosidade do resumo humano no canal Content: minimal | standard | full.")] string? verbosity = null)
    {
        if (ParseVerbosity(verbosity) != VerbosityLevel.Full && (page is not null || page_size is not null))
        {
            var err = new ContractErrorEnvelope(new ContractError(
                "INVALID_REQUEST",
                "Pagination requires verbosity=full.",
                new Dictionary<string, object?> { ["constraint"] = "page/page_size are only allowed when verbosity=full" }));
            return ToToolResult(err, isError: true, verbosity);
        }

        var payload = tools.ComposeIndicatorAnalysis(new ComposeIndicatorAnalysisRequest(
            WindowSize: window_size,
            StartContestId: start_contest_id,
            EndContestId: end_contest_id,
            Target: target ?? string.Empty,
            Operator: @operator ?? string.Empty,
            Components: components ?? Array.Empty<ComposeIndicatorComponentRequest>(),
            TopK: top_k,
            Page: page,
            PageSize: page_size,
            Fields: fields,
            IncludeExplanations: include_explanations));

        return ToToolResult(payload, payload is ContractErrorEnvelope, verbosity);
    }

    [McpServerTool(Name = "analyze_indicator_associations"), Description("Mede associacoes Spearman entre series escalares alinhadas e, opcionalmente, estabilidade em subjanelas deterministicas.")]
    public CallToolResult AnalyzeIndicatorAssociations(
        V0Tools tools,
        [Description("Tamanho da janela.")] int? window_size = null,
        [Description("Concurso inicial inclusivo (alternativa a window_size+end, ADR 0008 D2).")]
        int? start_contest_id = null,
        [Description("Concurso final inclusivo.")] int? end_contest_id = null,
        [Description("Itens (metrica e agregacao opcional para series vetoriais).")] IReadOnlyList<AssociationItemRequest>? items = null,
        [Description("Metodo (recorte: spearman).")] string? method = null,
        [Description("Top pares por magnitude.")] int top_k = 5,
        [Description("Estabilidade em subjanelas; requer method=rolling_window, subwindow_size, stride e min_subwindows explicitos.")] AssociationStabilityCheckRequest? stability_check = null,
        [Description("Inclui campos explicativos (`explanation`) quando true. Default: true.")] bool include_explanations = true,
        [Description("Projeção server-side: lista de campos top-level a incluir no StructuredContent.")] IReadOnlyList<string>? fields = null,
        [Description("Controle de verbosidade do resumo humano no canal Content: minimal | standard | full.")] string? verbosity = null)
    {
        var payload = tools.AnalyzeIndicatorAssociations(new AnalyzeIndicatorAssociationsRequest(
            WindowSize: window_size,
            StartContestId: start_contest_id,
            EndContestId: end_contest_id,
            Items: items,
            Method: method ?? string.Empty,
            TopK: top_k,
            StabilityCheck: stability_check,
            Fields: fields,
            IncludeExplanations: include_explanations));

        return ToToolResult(payload, payload is ContractErrorEnvelope, verbosity);
    }

    [McpServerTool(Name = "summarize_window_patterns"), Description("Resume padroes de janela via IQR (recorte minimo com uma feature escalar).")]
    public CallToolResult SummarizeWindowPatterns(
        V0Tools tools,
        [Description("Tamanho da janela.")] int? window_size = null,
        [Description("Concurso inicial inclusivo (alternativa a window_size+end, ADR 0008 D2).")]
        int? start_contest_id = null,
        [Description("Concurso final inclusivo.")] int? end_contest_id = null,
        [Description("Features por concurso para resumir (recorte atual: pares_no_concurso).")] IReadOnlyList<WindowPatternFeatureRequest>? features = null,
        [Description("Limiar de cobertura no intervalo [0,1].")] double coverage_threshold = 0.8,
        [Description("Metodo de faixa tipica (recorte: iqr).")] string? range_method = null,
        [Description("Inclui campos explicativos (`explanation`) quando true. Default: true.")] bool include_explanations = true,
        [Description("Projeção server-side: lista de campos top-level a incluir no StructuredContent.")] IReadOnlyList<string>? fields = null,
        [Description("Controle de verbosidade do resumo humano no canal Content: minimal | standard | full.")] string? verbosity = null)
    {
        var payload = tools.SummarizeWindowPatterns(new SummarizeWindowPatternsRequest(
            WindowSize: window_size,
            StartContestId: start_contest_id,
            EndContestId: end_contest_id,
            Features: features,
            CoverageThreshold: coverage_threshold,
            RangeMethod: range_method ?? string.Empty,
            Fields: fields,
            IncludeExplanations: include_explanations));

        return ToToolResult(payload, payload is ContractErrorEnvelope, verbosity);
    }

    [McpServerTool(Name = "summarize_window_aggregates"), Description("Produz agregados canonicos de janela (histograma escalar, top-k de padroes e matriz por posicao).")]
    public CallToolResult SummarizeWindowAggregates(
        V0Tools tools,
        [Description("Tamanho da janela.")] int? window_size = null,
        [Description("Concurso inicial inclusivo (alternativa a window_size+end, ADR 0008 D2).")]
        int? start_contest_id = null,
        [Description("Concurso final inclusivo.")] int? end_contest_id = null,
        [Description("Lista de agregados canonicos com metrica fonte, tipo e params explicitos.")] IReadOnlyList<WindowAggregateRequestDto>? aggregates = null,
        [Description("Projeção server-side: lista de campos top-level a incluir no StructuredContent.")] IReadOnlyList<string>? fields = null,
        [Description("Controle de verbosidade do resumo humano no canal Content: minimal | standard | full.")] string? verbosity = null)
    {
        var payload = tools.SummarizeWindowAggregates(new SummarizeWindowAggregatesRequest(
            WindowSize: window_size,
            StartContestId: start_contest_id,
            EndContestId: end_contest_id,
            Aggregates: aggregates,
            Fields: fields));

        return ToToolResult(payload, payload is ContractErrorEnvelope, verbosity);
    }

    [McpServerTool(Name = "generate_candidate_games"), Description("Gera candidatos de forma declarativa com criterios/pesos/filtros auditaveis e estrategias publicas.")]
    public CallToolResult GenerateCandidateGames(
        V0Tools tools,
        [Description("Tamanho da janela.")] int? window_size = null,
        [Description("Concurso inicial inclusivo (alternativa a window_size+end, ADR 0008 D2).")]
        int? start_contest_id = null,
        [Description("Concurso final inclusivo.")] int? end_contest_id = null,
        [Description("Semente para replay canónico; omitir em estratégia estocástica => replay_guaranteed false.")] ulong? seed = null,
        [Description("Plano declarativo por estrategia com criteria/weights/filters.")] IReadOnlyList<GenerateCandidatePlanItemRequest>? plan = null,
        [Description("Restricoes globais de unicidade e ordenacao.")] GenerateGlobalConstraintsRequest? global_constraints = null,
        [Description("Exclusoes estruturais globais para filtragem dos candidatos.")] GenerateStructuralExclusionsRequest? structural_exclusions = null,
        [Description("Modo normativo: random_unrestricted | behavior_filtered (omitir = legado com defaults conservadores).")] string? generation_mode = null,
        [Description("Página 1-based para paginação determinística do payload (apenas em verbosity=full).")] int? page = null,
        [Description("Tamanho da página (1..500). Default quando paginando: 200 (apenas em verbosity=full).")] int? page_size = null,
        [Description("Projeção server-side: lista de campos top-level a incluir no StructuredContent.")] IReadOnlyList<string>? fields = null,
        [Description("Controle de verbosidade do resumo humano no canal Content: minimal | standard | full.")] string? verbosity = null)
    {
        if (ParseVerbosity(verbosity) != VerbosityLevel.Full && (page is not null || page_size is not null))
        {
            var err = new ContractErrorEnvelope(new ContractError(
                "INVALID_REQUEST",
                "Pagination requires verbosity=full.",
                new Dictionary<string, object?> { ["constraint"] = "page/page_size are only allowed when verbosity=full" }));
            return ToToolResult(err, isError: true, verbosity);
        }

        var payload = tools.GenerateCandidateGames(new GenerateCandidateGamesRequest(
            WindowSize: window_size,
            StartContestId: start_contest_id,
            EndContestId: end_contest_id,
            Seed: seed,
            Plan: plan,
            GlobalConstraints: global_constraints,
            StructuralExclusions: structural_exclusions,
            GenerationMode: generation_mode,
            Page: page,
            PageSize: page_size,
            Fields: fields));

        return ToToolResult(payload, payload is ContractErrorEnvelope, verbosity);
    }

    [McpServerTool(Name = "explain_candidate_games"), Description("Explica jogos candidatos com ranking deterministico de estrategias, breakdown rastreavel e auditoria opcional de modo/seed/restricoes (ADR 0020).")]
    public CallToolResult ExplainCandidateGames(
        V0Tools tools,
        [Description("Tamanho da janela.")] int? window_size = null,
        [Description("Concurso inicial inclusivo (alternativa a window_size+end, ADR 0008 D2).")]
        int? start_contest_id = null,
        [Description("Concurso final inclusivo.")] int? end_contest_id = null,
        [Description("Lista de jogos candidatos para explicacao.")] IReadOnlyList<IReadOnlyList<int>>? games = null,
        [Description("Inclui detalhamento por metrica e contribuicao.")] bool include_metric_breakdown = true,
        [Description("Inclui detalhamento de exclusoes estruturais avaliadas.")] bool include_exclusion_breakdown = true,
        [Description("Echo opcional: generation_mode alinhado a generate_candidate_games.")] string? generation_mode = null,
        [Description("Echo opcional: seed usada em geracao para auditar replay.")] ulong? seed = null,
        [Description("Echo opcional: replay_guaranteed devolvido na ultima geracao.")] bool? replay_guaranteed = null,
        [Description("Página 1-based para paginação determinística do payload (apenas em verbosity=full).")] int? page = null,
        [Description("Tamanho da página (1..500). Default quando paginando: 200 (apenas em verbosity=full).")] int? page_size = null,
        [Description("Inclui campos explicativos (`explanation`) quando true. Default: true.")] bool include_explanations = true,
        [Description("Projeção server-side: lista de campos top-level a incluir no StructuredContent.")] IReadOnlyList<string>? fields = null,
        [Description("Controle de verbosidade do resumo humano no canal Content: minimal | standard | full.")] string? verbosity = null)
    {
        if (ParseVerbosity(verbosity) != VerbosityLevel.Full && (page is not null || page_size is not null))
        {
            var err = new ContractErrorEnvelope(new ContractError(
                "INVALID_REQUEST",
                "Pagination requires verbosity=full.",
                new Dictionary<string, object?> { ["constraint"] = "page/page_size are only allowed when verbosity=full" }));
            return ToToolResult(err, isError: true, verbosity);
        }

        var payload = tools.ExplainCandidateGames(new ExplainCandidateGamesRequest(
            WindowSize: window_size,
            StartContestId: start_contest_id,
            EndContestId: end_contest_id,
            Games: games,
            IncludeMetricBreakdown: include_metric_breakdown,
            IncludeExclusionBreakdown: include_exclusion_breakdown,
            GenerationMode: generation_mode,
            Seed: seed,
            ReplayGuaranteed: replay_guaranteed,
            Page: page,
            PageSize: page_size,
            Fields: fields,
            IncludeExplanations: include_explanations));

        return ToToolResult(payload, payload is ContractErrorEnvelope, verbosity);
    }

    private static CallToolResult ToToolResult(object payload, bool isError, string? verbosity)
    {
        var jsonPayload = JsonSerializer.SerializeToElement(payload, JsonOptions);
        var summary = BuildHumanSummary(payload, ParseVerbosity(verbosity));
        return new CallToolResult
        {
            IsError = isError,
            StructuredContent = jsonPayload,
            Content =
            [
                new TextContentBlock
                {
                    Text = summary
                }
            ]
        };
    }

    private static string BuildHumanSummary(object payload, VerbosityLevel verbosity)
    {
        // ADR 0023 (D2): Content é texto humano curto e não duplica o JSON canônico do StructuredContent.
        return payload switch
        {
            ContractErrorEnvelope e => verbosity switch
            {
                VerbosityLevel.Minimal => $"Error {e.Error.Code}: {e.Error.Message}",
                VerbosityLevel.Full => $"Error {e.Error.Code}: {e.Error.Message} (see structured payload for details).",
                _ => $"Error {e.Error.Code}: {e.Error.Message}"
            },
            DiscoverCapabilitiesResponse r => verbosity switch
            {
                VerbosityLevel.Minimal => $"Capabilities: build={r.BuildProfile}, tools={r.Tools.Count}, metrics={r.Metrics.ImplementedMetricNames.Count}.",
                VerbosityLevel.Full => $"Capabilities: build={r.BuildProfile}, tool_version={r.ToolVersion}, tools={r.Tools.Count}, implemented_metrics={r.Metrics.ImplementedMetricNames.Count}. See structured payload for full allowlists.",
                _ => $"Capabilities: build={r.BuildProfile}, tools={r.Tools.Count}, implemented_metrics={r.Metrics.ImplementedMetricNames.Count}."
            },
            HelpResponse r => verbosity switch
            {
                VerbosityLevel.Minimal => $"Help: templates={r.Templates.Count}, index={r.IndexResourceUri}.",
                VerbosityLevel.Full => $"Help: templates={r.Templates.Count}, index={r.IndexResourceUri} (tool_version={r.ToolVersion}). See structured payload for markdown and resources.",
                _ => $"Help: templates={r.Templates.Count}, index={r.IndexResourceUri}."
            },
            GetDrawWindowResponse r => verbosity switch
            {
                VerbosityLevel.Minimal => $"Window {r.Window.Size} ({r.Window.StartContestId}..{r.Window.EndContestId}), draws={r.Draws.Count}.",
                VerbosityLevel.Full => $"Window {r.Window.Size} ({r.Window.StartContestId}..{r.Window.EndContestId}), draws={r.Draws.Count}, dataset={r.DatasetVersion}, tool={r.ToolVersion}. See structured payload for draws.",
                _ => $"Window {r.Window.Size} ({r.Window.StartContestId}..{r.Window.EndContestId}), draws={r.Draws.Count}."
            },
            ComputeWindowMetricsResponse r => verbosity switch
            {
                VerbosityLevel.Minimal => $"Computed {r.Metrics.Count} metric(s) for window {r.Window.Size} ({r.Window.StartContestId}..{r.Window.EndContestId}).",
                VerbosityLevel.Full => $"Computed {r.Metrics.Count} metric(s) for window {r.Window.Size} ({r.Window.StartContestId}..{r.Window.EndContestId}), dataset={r.DatasetVersion}, tool={r.ToolVersion}. See structured payload for values.",
                _ => $"Computed {r.Metrics.Count} metric(s) for window {r.Window.Size} ({r.Window.StartContestId}..{r.Window.EndContestId})."
            },
            AnalyzeIndicatorStabilityResponse r => verbosity switch
            {
                VerbosityLevel.Minimal => $"Stability ranking: {r.Ranking.Count} item(s), normalization={r.NormalizationMethod}.",
                VerbosityLevel.Full => $"Stability ranking: {r.Ranking.Count} item(s) for window {r.Window.Size} ({r.Window.StartContestId}..{r.Window.EndContestId}), normalization={r.NormalizationMethod}. See structured payload for scores.",
                _ => $"Stability ranking: {r.Ranking.Count} item(s), normalization={r.NormalizationMethod}."
            },
            ComposeIndicatorAnalysisResponse r => verbosity switch
            {
                VerbosityLevel.Minimal => $"Composition ranking: target={r.Target}, operator={r.Operator}, items={r.Ranking.Count}.",
                VerbosityLevel.Full => $"Composition ranking: target={r.Target}, operator={r.Operator}, items={r.Ranking.Count}, window {r.Window.Size} ({r.Window.StartContestId}..{r.Window.EndContestId}). See structured payload for ranking.",
                _ => $"Composition ranking: target={r.Target}, operator={r.Operator}, items={r.Ranking.Count}."
            },
            AnalyzeIndicatorAssociationsResponse r => verbosity switch
            {
                VerbosityLevel.Minimal => $"Associations: top_pairs={r.AssociationMagnitude.TopPairs.Count}, method={r.Method}.",
                VerbosityLevel.Full => $"Associations: top_pairs={r.AssociationMagnitude.TopPairs.Count}, method={r.Method}, window {r.Window.Size} ({r.Window.StartContestId}..{r.Window.EndContestId}). See structured payload for magnitude/stability.",
                _ => $"Associations: top_pairs={r.AssociationMagnitude.TopPairs.Count}, method={r.Method}."
            },
            SummarizeWindowPatternsResponse r => verbosity switch
            {
                VerbosityLevel.Minimal => $"Pattern summary: features={r.Summaries.Count}, range_method={r.RangeMethod}.",
                VerbosityLevel.Full => $"Pattern summary: features={r.Summaries.Count}, range_method={r.RangeMethod}, window {r.Window.Size} ({r.Window.StartContestId}..{r.Window.EndContestId}). See structured payload for stats.",
                _ => $"Pattern summary: features={r.Summaries.Count}, range_method={r.RangeMethod}."
            },
            SummarizeWindowAggregatesResponse r => verbosity switch
            {
                VerbosityLevel.Minimal => $"Aggregates: {r.Aggregates.Count} item(s).",
                VerbosityLevel.Full => $"Aggregates: {r.Aggregates.Count} item(s), window {r.Window.Size} ({r.Window.StartContestId}..{r.Window.EndContestId}). See structured payload for aggregates.",
                _ => $"Aggregates: {r.Aggregates.Count} item(s)."
            },
            GenerateCandidateGamesResponse r => verbosity switch
            {
                VerbosityLevel.Minimal => $"Generated {r.CandidateGames.Count} candidate game(s), replay_guaranteed={r.ReplayGuaranteed}.",
                VerbosityLevel.Full => $"Generated {r.CandidateGames.Count} candidate game(s), replay_guaranteed={r.ReplayGuaranteed}, window {r.Window.Size} ({r.Window.StartContestId}..{r.Window.EndContestId}). See structured payload for candidates.",
                _ => $"Generated {r.CandidateGames.Count} candidate game(s), replay_guaranteed={r.ReplayGuaranteed}."
            },
            ExplainCandidateGamesResponse r => verbosity switch
            {
                VerbosityLevel.Minimal => $"Explained {r.Explanations.Count} game(s).",
                VerbosityLevel.Full => $"Explained {r.Explanations.Count} game(s), window {r.Window.Size} ({r.Window.StartContestId}..{r.Window.EndContestId}). See structured payload for breakdowns.",
                _ => $"Explained {r.Explanations.Count} game(s)."
            },
            _ => verbosity switch
            {
                VerbosityLevel.Minimal => "OK (see structured payload).",
                VerbosityLevel.Full => $"OK: {payload.GetType().Name} (see structured payload).",
                _ => "OK (see structured payload)."
            }
        };
    }
}
