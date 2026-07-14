/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeAdditionalMap.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository (Steps 7-7a)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New IEntityTypeConfiguration<DepartmentIncomeAdditional> created for keyless entity
 *   - HasNoKey() reflects LINQ GROUP BY aggregation origin (qryDeptIncomeExceptional SELECT/GROUP BY query)
 *   - ToView() placeholder — repository uses LINQ joins and GroupBy; view can be materialised later
 *   - All HasColumnName() values are lowercase per project convention
 *   - Money column totalcost given HasColumnType("money")
 *
 * PRESERVED:
 *   - All 8 properties from DepartmentIncomeAdditional entity
 *   - Aggregated TotalCost semantics — Sum(Proj_SubContract.Amount) per GROUP BY
 *   - Nullable semantics: occ, opc nullable from RIGHT JOIN
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: create fps.vw_dept_income_additional PostgreSQL view if direct view access required
 */

using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    // TRANSFORMENGINE: Keyless aggregation map — mirrors qryDeptIncomeExceptional GROUP BY; repository uses LINQ
    public class DepartmentIncomeAdditionalMap : IEntityTypeConfiguration<DepartmentIncomeAdditional>
    {
        public void Configure(EntityTypeBuilder<DepartmentIncomeAdditional> builder)
        {
            // TRANSFORMENGINE: HasNoKey — aggregated LINQ projection entity
            builder.HasNoKey();
            // TRANSFORMENGINE: ToView placeholder — repository builds via LINQ GroupBy
            builder.ToView("vw_dept_income_additional", "fps");

            builder.Property(e => e.Project)
                .HasMaxLength(20)
                .HasColumnName("project");

            builder.Property(e => e.OracleProjectCode)
                .HasMaxLength(50)
                .HasColumnName("oracleprojectcode");

            builder.Property(e => e.SubAccountCode)
                .HasMaxLength(50)
                .HasColumnName("subaccountcode");

            builder.Property(e => e.DefraProject)
                .HasMaxLength(3)
                .HasColumnName("defraproject");

            // TRANSFORMENGINE: OPC from CostCentre RIGHT JOIN — nullable
            builder.Property(e => e.OPC)
                .HasMaxLength(50)
                .HasColumnName("opc");

            builder.Property(e => e.OCC)
                .HasMaxLength(50)
                .HasColumnName("occ");

            builder.Property(e => e.Month)
                .HasColumnName("month");

            // TRANSFORMENGINE: Sum(Proj_SubContract.Amount) — aggregated exceptional/project-specific costs
            builder.Property(e => e.TotalCost)
                .HasColumnType("money")
                .HasColumnName("totalcost");
        }
    }
}
