namespace LotofacilMcp.Domain.Generation;

/// <summary>
/// Valores canónicos de <c>generation_mode</c> em <c>generate_candidate_games</c> (ADR 0020 D1, <c>generation-strategies.md</c>).
/// </summary>
public static class GenerationModes
{
    public const string RandomUnrestricted = "random_unrestricted";
    public const string BehaviorFiltered = "behavior_filtered";
}
