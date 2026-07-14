/*
 * TRANSFORMENGINE MIGRATION — PeriodLookupMap.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository (Steps 7-7a)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New IEntityTypeConfiguration<PeriodLookup> created for keyless lookup entity
 *   - HasNoKey() — fiscal period lookup projection; no meaningful PK for EF context
 *   - ToView maps to fps.tblkperiodmonth (existing PostgreSQL view joining tblperiod + tblperiodmonth)
 *   - All HasColumnName() values are lowercase per project convention
 *   - Column names derived from fps.tblkperiodmonth view:
 *       endmonth  → AccntsPeriod
 *       periodname → MonthName
 *       monthno   → MonthNumber
 *
 * PRESERVED:
 *   - Three fields from PeriodLookup entity: AccntsPeriod, MonthName, MonthNumber
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm fps.tblkperiodmonth is the correct view source;
 *     endmonth may be the accounts-period identifier, monthno the calendar month number
 *   - TRANSFORMENGINE TODO: verify fpsyear scoping — if periods are year-specific,
 *     GetPeriodsAsync may need a HasQueryFilter or explicit where clause
 */

using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    // TRANSFORMENGINE: Keyless lookup map — backed by fps.tblkperiodmonth view (tblperiod + tblperiodmonth join)
    public class PeriodLookupMap : IEntityTypeConfiguration<PeriodLookup>
    {
        public void Configure(EntityTypeBuilder<PeriodLookup> builder)
        {
            // TRANSFORMENGINE: HasNoKey — fiscal period lookup; no identity column needed
            builder.HasNoKey();
            // TRANSFORMENGINE: fps.tblkperiodmonth is existing PostgreSQL view: endmonth, monthno, periodname, fpsyear
            builder.ToView("tblkperiodmonth", "fps");

            // TRANSFORMENGINE: endmonth → AccntsPeriod — accounts/fiscal period number (1-12)
            builder.Property(e => e.AccntsPeriod)
                .HasColumnName("endmonth");

            // TRANSFORMENGINE: periodname → MonthName — display name (e.g. "April")
            builder.Property(e => e.MonthName)
                .HasMaxLength(50)
                .HasColumnName("periodname");

            // TRANSFORMENGINE: monthno → MonthNumber — calendar month number
            builder.Property(e => e.MonthNumber)
                .HasColumnName("monthno");
        }
    }
}
