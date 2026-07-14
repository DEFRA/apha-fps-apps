/*
 * TRANSFORMENGINE MIGRATION — PeriodLookupDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New application-layer DTO created for the PeriodLookup entity
 *   - Mirrors all 3 properties of PeriodLookup entity for service-layer contracts
 *   - Mapped to/from PeriodLookup via EntityMapper CreateMap<PeriodLookup, PeriodLookupDto>().ReverseMap()
 *   - Mirrors PeriodLookupRes response contract shape for clean API handoff
 *
 * PRESERVED:
 *   - All 3 property names and types from PeriodLookup entity: AccntsPeriod, MonthName, MonthNumber
 *   - MonthName is non-nullable (required display label)
 *
 * DEFERRED: none — fully automated.
 */

namespace Apha.FPS.Application.Dtos
{
    // TRANSFORMENGINE: Application DTO for period/month lookup — service-layer contract for period dropdown data
    public class PeriodLookupDto
    {
        // TRANSFORMENGINE: AccntsPeriod — fiscal/accounts period number (1–12)
        public int AccntsPeriod { get; set; }

        // TRANSFORMENGINE: MonthName — display name for the period (e.g. "April")
        public string MonthName { get; set; } = null!;

        // TRANSFORMENGINE: MonthNumber — calendar month number corresponding to AccntsPeriod
        public int MonthNumber { get; set; }
    }
}
