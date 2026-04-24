using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace LotofacilMcp.Server.Tools;

[McpServerToolType]
public sealed class V0McpTools
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [McpServerTool(Name = "get_draw_window"), Description("Retorna um recorte canônico de concursos da Lotofácil.")]
    public CallToolResult GetDrawWindow(
        V0Tools tools,
        [Description("Quantidade de concursos consecutivos na janela.")] int? window_size = null,
        [Description("Concurso inicial inclusivo (alternativa a window_size+end, ADR 0008 D2).")]
        int? start_contest_id = null,
        [Description("Concurso final inclusivo da janela.")] int? end_contest_id = null)
    {
        var payload = tools.GetDrawWindow(new GetDrawWindowRequest(
            WindowSize: window_size,
            StartContestId: start_contest_id,
            EndContestId: end_contest_id));

        return ToToolResult(payload, payload is ContractErrorEnvelope);
    }

    [McpServerTool(Name = "compute_window_metrics"), Description("Calcula métricas canônicas para uma janela de concursos.")]
    public CallToolResult ComputeWindowMetrics(
        V0Tools tools,
        [Description("Quantidade de concursos consecutivos na janela.")] int? window_size = null,
        [Description("Concurso inicial inclusivo (alternativa a window_size+end, ADR 0008 D2).")]
        int? start_contest_id = null,
        [Description("Concurso final inclusivo da janela.")] int? end_contest_id = null,
        [Description("Lista de métricas canônicas a calcular.")] IReadOnlyList<MetricRequest>? metrics = null,
        [Description("Permite opt-in para métricas pendentes de detalhamento.")] bool allow_pending = false)
    {
        var payload = tools.ComputeWindowMetrics(new ComputeWindowMetricsRequest(
            WindowSize: window_size,
            StartContestId: start_contest_id,
            EndContestId: end_contest_id,
            Metrics: metrics,
            AllowPending: allow_pending));

        return ToToolResult(payload, payload is ContractErrorEnvelope);
    }

    [McpServerTool(Name = "analyze_indicator_stability"), Description("Compara indicadores na janela e ranqueia estabilidade relativa.")]
    public CallToolResult AnalyzeIndicatorStability(
        V0Tools tools,
        [Description("Quantidade de concursos consecutivos na janela.")] int window_size = 0,
        [Description("Concurso final inclusivo da janela.")] int? end_contest_id = null,
        [Description("Indicadores canônicos para analise de estabilidade.")] IReadOnlyList<StabilityIndicatorRequestDto>? indicators = null,
        [Description("Metodo de normalizacao de volatilidade.")] string? normalization_method = null,
        [Description("Quantidade maxima de itens no ranking final.")] int top_k = 5,
        [Description("Historico minimo necessario para calcular estabilidade.")] int min_history = 20)
    {
        var payload = tools.AnalyzeIndicatorStability(new AnalyzeIndicatorStabilityRequest(
            WindowSize: window_size,
            EndContestId: end_contest_id,
            Indicators: indicators,
            NormalizationMethod: normalization_method,
            TopK: top_k,
            MinHistory: min_history));

        return ToToolResult(payload, payload is ContractErrorEnvelope);
    }

    [McpServerTool(Name = "compose_indicator_analysis"), Description("Composição declarativa de indicadores (recorte: target dezena, weighted_rank).")]
    public CallToolResult ComposeIndicatorAnalysis(
        V0Tools tools,
        [Description("Tamanho da janela.")] int window_size = 0,
        [Description("Concurso final inclusivo.")] int? end_contest_id = null,
        [Description("Unidade alvo (recorte: dezena).")] string? target = null,
        [Description("Operador (recorte: weighted_rank).")] string? @operator = null,
        [Description("Componentes com métrica, transform e peso.")] IReadOnlyList<ComposeIndicatorComponentRequest>? components = null,
        [Description("Limite do ranking.")] int top_k = 10)
    {
        var payload = tools.ComposeIndicatorAnalysis(new ComposeIndicatorAnalysisRequest(
            WindowSize: window_size,
            EndContestId: end_contest_id,
            Target: target ?? string.Empty,
            Operator: @operator ?? string.Empty,
            Components: components ?? Array.Empty<ComposeIndicatorComponentRequest>(),
            TopK: top_k));

        return ToToolResult(payload, payload is ContractErrorEnvelope);
    }

    [McpServerTool(Name = "analyze_indicator_associations"), Description("Mede associacoes Spearman entre series escalares alinhadas na janela (recorte minimo).")]
    public CallToolResult AnalyzeIndicatorAssociations(
        V0Tools tools,
        [Description("Tamanho da janela.")] int window_size = 0,
        [Description("Concurso final inclusivo.")] int? end_contest_id = null,
        [Description("Itens (metrica e agregacao opcional para series vetoriais).")] IReadOnlyList<AssociationItemRequest>? items = null,
        [Description("Metodo (recorte: spearman).")] string? method = null,
        [Description("Top pares por magnitude.")] int top_k = 5,
        [Description("Estabilidade em subjanelas (ainda nao suportado neste recorte).")] object? stability_check = null)
    {
        var payload = tools.AnalyzeIndicatorAssociations(new AnalyzeIndicatorAssociationsRequest(
            WindowSize: window_size,
            EndContestId: end_contest_id,
            Items: items,
            Method: method ?? string.Empty,
            TopK: top_k,
            StabilityCheck: stability_check));

        return ToToolResult(payload, payload is ContractErrorEnvelope);
    }

    [McpServerTool(Name = "summarize_window_patterns"), Description("Resume padroes de janela via IQR (recorte minimo com uma feature escalar).")]
    public CallToolResult SummarizeWindowPatterns(
        V0Tools tools,
        [Description("Tamanho da janela.")] int window_size = 0,
        [Description("Concurso final inclusivo.")] int? end_contest_id = null,
        [Description("Features por concurso para resumir (recorte atual: pares_no_concurso).")] IReadOnlyList<WindowPatternFeatureRequest>? features = null,
        [Description("Limiar de cobertura no intervalo [0,1].")] double coverage_threshold = 0.8,
        [Description("Metodo de faixa tipica (recorte: iqr).")] string? range_method = null)
    {
        var payload = tools.SummarizeWindowPatterns(new SummarizeWindowPatternsRequest(
            WindowSize: window_size,
            EndContestId: end_contest_id,
            Features: features,
            CoverageThreshold: coverage_threshold,
            RangeMethod: range_method ?? string.Empty));

        return ToToolResult(payload, payload is ContractErrorEnvelope);
    }

    [McpServerTool(Name = "summarize_window_aggregates"), Description("Produz agregados canonicos de janela (histograma escalar, top-k de padroes e matriz por posicao).")]
    public CallToolResult SummarizeWindowAggregates(
        V0Tools tools,
        [Description("Tamanho da janela.")] int window_size = 0,
        [Description("Concurso final inclusivo.")] int? end_contest_id = null,
        [Description("Lista de agregados canonicos com metrica fonte, tipo e params explicitos.")] IReadOnlyList<WindowAggregateRequestDto>? aggregates = null)
    {
        var payload = tools.SummarizeWindowAggregates(new SummarizeWindowAggregatesRequest(
            WindowSize: window_size,
            EndContestId: end_contest_id,
            Aggregates: aggregates));

        return ToToolResult(payload, payload is ContractErrorEnvelope);
    }

    [McpServerTool(Name = "generate_candidate_games"), Description("Gera jogos candidatos por estrategia nominal simples com orcamento e determinismo.")]
    public CallToolResult GenerateCandidateGames(
        V0Tools tools,
        [Description("Tamanho da janela.")] int window_size = 0,
        [Description("Concurso final inclusivo.")] int? end_contest_id = null,
        [Description("Seed obrigatoria para metodos sampled ou greedy_topk.")] ulong? seed = null,
        [Description("Plano de geracao por estrategia nominal.")] IReadOnlyList<GenerateCandidatePlanItemRequest>? plan = null)
    {
        var payload = tools.GenerateCandidateGames(new GenerateCandidateGamesRequest(
            WindowSize: window_size,
            EndContestId: end_contest_id,
            Seed: seed,
            Plan: plan));

        return ToToolResult(payload, payload is ContractErrorEnvelope);
    }

    [McpServerTool(Name = "explain_candidate_games"), Description("Explica jogos candidatos com ranking deterministico de estrategias e breakdown rastreavel.")]
    public CallToolResult ExplainCandidateGames(
        V0Tools tools,
        [Description("Tamanho da janela.")] int window_size = 0,
        [Description("Concurso final inclusivo.")] int? end_contest_id = null,
        [Description("Lista de jogos candidatos para explicacao.")] IReadOnlyList<IReadOnlyList<int>>? games = null,
        [Description("Inclui detalhamento por metrica e contribuicao.")] bool include_metric_breakdown = true,
        [Description("Inclui detalhamento de exclusoes estruturais avaliadas.")] bool include_exclusion_breakdown = true)
    {
        var payload = tools.ExplainCandidateGames(new ExplainCandidateGamesRequest(
            WindowSize: window_size,
            EndContestId: end_contest_id,
            Games: games,
            IncludeMetricBreakdown: include_metric_breakdown,
            IncludeExclusionBreakdown: include_exclusion_breakdown));

        return ToToolResult(payload, payload is ContractErrorEnvelope);
    }

    private static CallToolResult ToToolResult(object payload, bool isError)
    {
        var jsonPayload = JsonSerializer.SerializeToElement(payload, JsonOptions);
        return new CallToolResult
        {
            IsError = isError,
            StructuredContent = jsonPayload,
            Content =
            [
                new TextContentBlock
                {
                    Text = JsonSerializer.Serialize(payload, JsonOptions)
                }
            ]
        };
    }
}
