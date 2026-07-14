/*
 * TRANSFORMENGINE MIGRATION — PeriodLookupDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New frontend lookup DTO mirroring backend Apha.Common.Contracts.FPS.PeriodLookupRes
 *   - Placed in Apha.FPSApps.Application.Dtos.DepartmentIncome namespace (dedicated type, not reusing CRUD DTO)
 *   - All 3 properties match backend PeriodLookupRes exactly (case-sensitive)
 *
 * PRESERVED:
 *   - Property names: AccntsPeriod, MonthName, MonthNumber
 *   - AccntsPeriod: fiscal/accounts period number (1–12)
 *   - MonthName: display name for period dropdown (e.g. "April")
 *   - MonthNumber: calendar month number corresponding to AccntsPeriod
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: ApiDtoMapper (FpsDepartmentIncomeApiDtoMapper) must map PeriodLookupRes → this DTO
 *   - TRANSFORMENGINE TODO: confirm AccntsPeriod vs MonthNumber semantics — they may differ when fiscal year
 *     starts in a month other than January
 */

namespace Apha.FPSApps.Application.Dtos.DepartmentIncome
{
    // TRANSFORMENGINE: Frontend lookup DTO — mirrors backend PeriodLookupRes for GET /api/v1/department-income/periods
    // Dedicated lookup type — never reuse for CRUD operations
    public class PeriodLookupDto
    {
        // TRANSFORMENGINE: AccntsPeriod — fiscal/accounts period number (1–12)
        public int AccntsPeriod { get; set; }

        // TRANSFORMENGINE: MonthName — display name for the period dropdown (e.g. "April")
        public string MonthName { get; set; } = null!;

        // TRANSFORMENGINE: MonthNumber — calendar month number corresponding to AccntsPeriod
        public int MonthNumber { get; set; }
    }
}
