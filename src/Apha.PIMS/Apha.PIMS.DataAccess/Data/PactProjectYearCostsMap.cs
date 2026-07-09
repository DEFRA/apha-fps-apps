/*
 * TRANSFORMENGINE MIGRATION — PactProjectYearCostsMap.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository (Steps 7-7a)
 * Migrated : 2026-07-09
 *
 * CHANGED:
 *   - New file: EF Core IEntityTypeConfiguration<PactProjectYearCosts> for mabarchive.vpactprojectyearcosts
 *   - Registered as a keyless entity (HasNoKey) — the view has no primary key
 *   - ToView("vpactprojectyearcosts", "mabarchive") — read-only PostgreSQL view
 *   - All sum() aggregate columns (monetary totals) mapped with HasColumnType("money")
 *   - hours column mapped with HasColumnType("double precision") (sum of totalhours)
 *   - year and monthno columns are CASE-derived doubles in the view; mapped HasColumnType("double precision")
 *   - project column mapped HasMaxLength(20) to match my_projectmonthfinal.project DDL
 *   - All HasColumnName values lowercase to match view column aliases
 *
 * PRESERVED:
 *   - View column aliases from mabarchive.vpactprojectyearcosts DDL:
 *     project, year, monthno, subcontracts, animals, tests, pay, nonpayoh, hours, totalcosts, timecost
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm that Npgsql handles money-column projection from this view
 *     without a cast error — if needed, change affected HasColumnType entries to "numeric"
 */

using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    /// <summary>
    /// EF Core configuration for <see cref="PactProjectYearCosts"/>.
    /// Maps to the read-only <c>mabarchive.vpactprojectyearcosts</c> PostgreSQL view.
    /// Registered with <c>HasNoKey()</c> — no primary key exists on the view.
    /// </summary>
    public class PactProjectYearCostsMap : IEntityTypeConfiguration<PactProjectYearCosts>
    {
        private const string ColumnTypeMoney = "money";
        private const string ColumnTypeDouble = "double precision";

        public void Configure(EntityTypeBuilder<PactProjectYearCosts> entity)
        {
            // TRANSFORMENGINE: Keyless entity — read-only view, no PK
            entity.HasNoKey();

            entity.ToView("vpactprojectyearcosts", "mabarchive");

            // TRANSFORMENGINE: Grouping key columns from view
            entity.Property(e => e.Project)
                  .HasMaxLength(20)
                  .HasColumnName("project");

            // TRANSFORMENGINE: year is CASE-derived double precision in the view
            entity.Property(e => e.Year)
                  .HasColumnType(ColumnTypeDouble)
                  .HasColumnName("year");

            // TRANSFORMENGINE: monthno is double precision in the view (from my_projectmonthfinal.monthno)
            entity.Property(e => e.MonthNo)
                  .HasColumnType(ColumnTypeDouble)
                  .HasColumnName("monthno");

            // TRANSFORMENGINE: sum() monetary aggregate columns → money type
            entity.Property(e => e.SubContracts)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("subcontracts");

            entity.Property(e => e.Animals)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("animals");

            entity.Property(e => e.Tests)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("tests");

            entity.Property(e => e.Pay)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("pay");

            entity.Property(e => e.NonPayOH)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("nonpayoh");

            entity.Property(e => e.TotalCosts)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("totalcosts");

            entity.Property(e => e.TimeCost)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("timecost");

            // TRANSFORMENGINE: hours is sum(totalhours) — double precision aggregate
            entity.Property(e => e.Hours)
                  .HasColumnType(ColumnTypeDouble)
                  .HasColumnName("hours");
        }
    }
}
