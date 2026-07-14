/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeTestMap.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository (Steps 7-7a)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New IEntityTypeConfiguration<DepartmentIncomeTest> created for keyless entity
 *   - HasNoKey() reflects LINQ-projection origin (qryDeptIncomeTests SELECT query)
 *   - ToView() placeholder — repository uses LINQ joins; view can be materialised later
 *   - All HasColumnName() values are lowercase per project convention
 *   - Money column totalcost given HasColumnType("money")
 *
 * PRESERVED:
 *   - All 14 properties from DepartmentIncomeTest entity
 *   - Column ordering: OPC appears before OCC (matches qryDeptIncomeTests SELECT list)
 *   - Nullable semantics: oracleprojectcode, subaccountcode, opc, occ, spc, scc, testcode nullable
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: create fps.vw_dept_income_test PostgreSQL view if direct view access required
 */

using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    // TRANSFORMENGINE: Keyless map — mirrors qryDeptIncomeTests; repository uses LINQ joins
    public class DepartmentIncomeTestMap : IEntityTypeConfiguration<DepartmentIncomeTest>
    {
        public void Configure(EntityTypeBuilder<DepartmentIncomeTest> builder)
        {
            // TRANSFORMENGINE: HasNoKey — LINQ projection entity
            builder.HasNoKey();
            // TRANSFORMENGINE: ToView placeholder — repository builds via LINQ
            builder.ToView("vw_dept_income_test", "fps");

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

            // TRANSFORMENGINE: OPC before OCC — matches qryDeptIncomeTests SELECT column order
            builder.Property(e => e.OPC)
                .HasMaxLength(50)
                .HasColumnName("opc");

            builder.Property(e => e.OCC)
                .HasMaxLength(50)
                .HasColumnName("occ");

            builder.Property(e => e.Month)
                .HasColumnName("month");

            builder.Property(e => e.SPC)
                .HasMaxLength(50)
                .HasColumnName("spc");

            builder.Property(e => e.WorkGroup)
                .HasMaxLength(50)
                .HasColumnName("workgroup");

            builder.Property(e => e.SCC)
                .HasMaxLength(50)
                .HasColumnName("scc");

            builder.Property(e => e.TestCode)
                .HasMaxLength(20)
                .HasColumnName("testcode");

            builder.Property(e => e.Volume)
                .HasColumnName("volume");

            builder.Property(e => e.TestPrice)
                .HasColumnType("money")
                .HasColumnName("testprice");

            // TRANSFORMENGINE: [TestPrice]*[Volume] AS TotalCost — computed by original query
            builder.Property(e => e.TotalCost)
                .HasColumnType("money")
                .HasColumnName("totalcost");
        }
    }
}
