/*
 * TRANSFORMENGINE MIGRATION — PeriodLookup.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination (Steps 2-3)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New keyless entity created for the period/month dropdown lookup endpoint
 *   - Source: sf_Period.frm subform (do_not_modify) and PeriodLookupRes Phase 1 contract
 *   - AccntsPeriod: accounts period identifier (1–12 representing fiscal months)
 *   - MonthName: display name for the period (e.g. "April", "May")
 *   - MonthNumber: calendar month number corresponding to AccntsPeriod
 *   - Marked for HasNoKey EF Core mapping (lookup projection — no meaningful PK for EF)
 *
 * PRESERVED:
 *   - Three fields as specified in transform-plan and PeriodLookupRes: AccntsPeriod, MonthName, MonthNumber
 *   - AccntsPeriod and MonthNumber kept separate to support fiscal-year offset scenarios
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: fPeriodTotals stored proc or equivalent period table must be identified;
 *     repository must resolve from fiscal calendar table via EF or raw SQL
 *   - TRANSFORMENGINE TODO: confirm AccntsPeriod vs MonthNumber semantics — may differ when FY starts in April
 *   - TRANSFORMENGINE TODO: PeriodLookupMap.cs must call .HasNoKey() — confirmed keyless in Phase 4
 */

namespace Apha.FPS.Core.Entities
{
    // TRANSFORMENGINE: Keyless lookup entity — period dropdown data for GET /api/v1/department-income/periods
    public class PeriodLookup
    {
        // TRANSFORMENGINE: AccntsPeriod — fiscal/accounts period number (1–12)
        public int AccntsPeriod { get; set; }

        // TRANSFORMENGINE: MonthName — display name for the period (e.g. "April")
        public string MonthName { get; set; } = null!;

        // TRANSFORMENGINE: MonthNumber — calendar month number corresponding to AccntsPeriod
        public int MonthNumber { get; set; }
    }
}
