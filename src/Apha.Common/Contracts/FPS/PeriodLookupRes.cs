/*
 * TRANSFORMENGINE MIGRATION — PeriodLookupRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New lookup response contract created for period dropdown endpoint
 *   - Source: qptGetPeriodData pass-through query calling fPeriodTotals stored proc
 *   - AccntsPeriod: accounts period identifier (1–12 typical fiscal months)
 *   - MonthName: display name for the period (e.g. "April", "May")
 *   - MonthNumber: calendar month number corresponding to AccntsPeriod
 *
 * PRESERVED:
 *   - Three fields as specified in transform-plan: AccntsPeriod, MonthName, MonthNumber
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: fPeriodTotals stored proc must be re-implemented in DepartmentIncomeRepository
 *     as a LINQ query or EF raw SQL against the period/fiscal calendar table
 *   - TRANSFORMENGINE TODO: confirm AccntsPeriod vs MonthNumber semantics — they may differ when fiscal year
 *     starts in a month other than January
 */

namespace Apha.Common.Contracts.FPS
{
    // TRANSFORMENGINE: Lookup response for GET /api/v1/department-income/periods — period dropdown data
    public class PeriodLookupRes
    {
        // TRANSFORMENGINE: AccntsPeriod — fiscal/accounts period number (1–12)
        public int AccntsPeriod { get; set; }

        // TRANSFORMENGINE: MonthName — display name for the period (e.g. "April")
        public string MonthName { get; set; } = null!;

        // TRANSFORMENGINE: MonthNumber — calendar month number corresponding to AccntsPeriod
        public int MonthNumber { get; set; }
    }
}
