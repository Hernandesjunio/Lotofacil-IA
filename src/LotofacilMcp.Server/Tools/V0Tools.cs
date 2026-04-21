namespace LotofacilMcp.Server.Tools;

public sealed record MetricRequest(string Name);

public sealed record ComputeWindowMetricsRequest(
    int WindowSize,
    int? EndContestId,
    IReadOnlyList<MetricRequest>? Metrics);

public sealed record GetDrawWindowRequest(
    int WindowSize,
    int? EndContestId);

public sealed record ContractError(string Code, string Message, IReadOnlyDictionary<string, object?> Details);

public sealed record ContractErrorEnvelope(ContractError Error);

public sealed record DrawDto(int ContestId, string DrawDate, IReadOnlyList<int> Numbers);

public sealed record GetDrawWindowResponse(IReadOnlyList<DrawDto> Draws);

public sealed class V0Tools
{
    public object ComputeWindowMetrics(ComputeWindowMetricsRequest request)
    {
        throw new NotImplementedException("compute_window_metrics is implemented in Phase 5/7.");
    }

    public object GetDrawWindow(GetDrawWindowRequest request)
    {
        throw new NotImplementedException("get_draw_window is implemented in Phase 5/7.");
    }
}
