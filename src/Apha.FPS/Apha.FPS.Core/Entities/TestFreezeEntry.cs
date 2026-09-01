namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// One row's reviewed classification for the Test rates workflow (FEC/AGRUP),
    /// frozen onto the staging tables' calculated_action/effective_new_rate/source_current_rate
    /// columns at release time, so the worker's revalidation can compare its re-derived
    /// result against exactly what the approver saw. Buyer is null for FEC rows.
    /// </summary>
    public sealed record TestFreezeEntry(
        string TestCode,
        string? Buyer,
        string CalculatedAction,
        decimal? EffectiveNewRate,
        decimal? SourceCurrentRate);
}
