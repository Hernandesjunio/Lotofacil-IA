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
        [Description("Quantidade de concursos consecutivos na janela.")] int window_size = 0,
        [Description("Concurso final inclusivo da janela.")] int? end_contest_id = null)
    {
        var payload = tools.GetDrawWindow(new GetDrawWindowRequest(
            WindowSize: window_size,
            EndContestId: end_contest_id));

        return ToToolResult(payload, payload is ContractErrorEnvelope);
    }

    [McpServerTool(Name = "compute_window_metrics"), Description("Calcula métricas canônicas para uma janela de concursos.")]
    public CallToolResult ComputeWindowMetrics(
        V0Tools tools,
        [Description("Quantidade de concursos consecutivos na janela.")] int window_size = 0,
        [Description("Concurso final inclusivo da janela.")] int? end_contest_id = null,
        [Description("Lista de métricas canônicas a calcular.")] IReadOnlyList<MetricRequest>? metrics = null,
        [Description("Permite opt-in para métricas pendentes de detalhamento.")] bool allow_pending = false)
    {
        var payload = tools.ComputeWindowMetrics(new ComputeWindowMetricsRequest(
            WindowSize: window_size,
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
