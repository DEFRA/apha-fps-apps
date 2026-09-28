namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// One row's reviewed classification for the Test rates workflow (FEC/AGRUP),
    /// frozen onto the staging tables' calculated_action/effective_new_rate/source_current_rate
    /// columns at release time, so the worker's revalidation can compare its re-derived
    /// result against exactly what the approver saw. Buyer is null for FEC rows.
    /// TestCode/Buyer are the as-staged (as-uploaded) values used to locate the staging row;
    /// ResolvedTestCode/ResolvedBuyer are the live table's actual-cased key for the same
    /// business entity (falling back to the as-staged value when there is no live row, i.e.
    /// Insert) — frozen alongside so the worker's later exact-match UPDATE targets the live
    /// row's real casing/whitespace rather than whatever the uploader happened to type.
    /// </summary>
    public sealed record TestFreezeEntry(
        string TestCode,
        string? Buyer,
        string CalculatedAction,
        decimal? EffectiveNewRate,
        decimal? SourceCurrentRate,
        string ResolvedTestCode,
        string? ResolvedBuyer);
}
