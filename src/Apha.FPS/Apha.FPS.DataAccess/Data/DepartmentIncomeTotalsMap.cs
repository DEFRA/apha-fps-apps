/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeTotalsMap.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository (Steps 7-7a)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New IEntityTypeConfiguration<DepartmentIncomeTotals> created for keyless entity
 *   - HasNoKey() reflects LINQ aggregation/PIVOT origin (qryDeptIncomeTotals TRANSFORM/PIVOT query)
 *   - ToView() placeholder — repository uses LINQ conditional sums (PIVOT emulation)
 *   - All HasColumnName() values are lowercase per project convention
 *   - Money columns totalcosts, timecost, testscost, animalscost, projectspecificscost given HasColumnType("money")
 *
 * PRESERVED:
 *   - All 7 properties from DepartmentIncomeTotals entity
 *   - Nullable pivot cost columns: individual area totals nullable when no data exists for that area
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: create fps.vw_dept_income_totals PostgreSQL view with CASE-based pivot
 *     if direct view access is required in future
 */

using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    // TRANSFORMENGINE: Keyless pivot/aggregation map — mirrors qryDeptIncomeTotals TRANSFORM; repository uses LINQ conditional sums
    public class DepartmentIncomeTotalsMap : IEntityTypeConfiguration<DepartmentIncomeTotals>
    {
        public void Configure(EntityTypeBuilder<DepartmentIncomeTotals> builder)
        {
            // TRANSFORMENGINE: HasNoKey — PIVOT aggregation projection entity
            builder.HasNoKey();
            // TRANSFORMENGINE: ToView placeholder — repository emulates PIVOT via LINQ GroupBy + conditional Sum
            builder.ToView("vw_dept_income_totals", "fps");

            builder.Property(e => e.Project)
                .HasMaxLength(20)
                .HasColumnName("project");

            builder.Property(e => e.OracleProjectCode)
                .HasMaxLength(50)
                .HasColumnName("oracleprojectcode");

            // TRANSFORMENGINE: Sum(TotalCost) grand total across all areas
            builder.Property(e => e.TotalCosts)
                .HasColumnType("money")
                .HasColumnName("totalcosts");

            // TRANSFORMENGINE: PIVOT "Time" — nullable when no time data for project
            builder.Property(e => e.TimeCost)
                .HasColumnType("money")
                .HasColumnName("timecost");

            // TRANSFORMENGINE: PIVOT "Tests" — nullable when no test data for project
            builder.Property(e => e.TestsCost)
                .HasColumnType("money")
                .HasColumnName("testscost");

            // TRANSFORMENGINE: PIVOT "Animals" — nullable when no animal data for project
            builder.Property(e => e.AnimalsCost)
                .HasColumnType("money")
                .HasColumnName("animalscost");

            // TRANSFORMENGINE: PIVOT "Project-specifics" — nullable when no additional/exceptional data for project
            builder.Property(e => e.ProjectSpecificsCost)
                .HasColumnType("money")
                .HasColumnName("projectspecificscost");
        }
    }
}
