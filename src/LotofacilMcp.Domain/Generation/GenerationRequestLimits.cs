namespace LotofacilMcp.Domain.Generation;

/// <summary>
/// Limites normativos de pedido em <c>generate_candidate_games</c> (soma de <c>plan[].count</c>, ADR 0020 D6, <c>mcp-tool-contract.md</c> 26.1).
/// </summary>
public static class GenerationRequestLimits
{
    public const int MaxSumPlanCountPerRequest = 1000;
}
