/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeTimeMap.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository (Steps 7-7a)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New IEntityTypeConfiguration<DepartmentIncomeTime> created for keyless entity
 *   - HasNoKey() reflects LINQ-projection origin (qryDeptIncomeTime SELECT query)
 *   - ToView() name follows fps.vw_dept_income_time convention (view can be created later
 *     to back the DbSet if needed; repository uses LINQ joins against real tables)
 *   - All HasColumnName() values are lowercase per project convention
 *   - Money columns (chargerate, pay, nonpay, overhead, totalcost) given HasColumnType("money")
 *
 * PRESERVED:
 *   - All 18 properties from DepartmentIncomeTime entity
 *   - Nullable semantics: occ, opc, spc, scc, name, gradecode, spnumber nullable
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: create fps.vw_dept_income_time PostgreSQL view to back
 *     this DbSet if direct view-based queries are required in future
 */

using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    // TRANSFORMENGINE: Keyless map — mirrors qryDeptIncomeTime; repository uses LINQ joins not this view DbSet directly
    public class DepartmentIncomeTimeMap : IEntityTypeConfiguration<DepartmentIncomeTime>
    {
        public void Configure(EntityTypeBuilder<DepartmentIncomeTime> builder)
        {
            // TRANSFORMENGINE: HasNoKey — LINQ projection entity, no primary key in source query
            builder.HasNoKey();
            // TRANSFORMENGINE: ToView placeholder — view does not exist yet; repository builds via LINQ
            builder.ToView("vw_dept_income_time", "fps");

            builder.Property(e => e.Project)
                .HasMaxLength(20)
                .HasColumnName("project");

            builder.Property(e => e.OracleProjectCode)
                .HasMaxLength(50)
                .HasColumnName("oracleprojectcode");

            builder.Property(e => e.SubAccountCode)
                .HasMaxLength(50)
                .HasColumnName("subaccountcode");

            builder.Property(e => e.Month)
                .HasColumnName("month");

            builder.Property(e => e.DefraProject)
                .HasMaxLength(3)
                .HasColumnName("defraproject");

            // TRANSFORMENGINE: OCC = CostCentre.CostCentre AS OCC — owning cost centre code
            builder.Property(e => e.OCC)
                .HasMaxLength(50)
                .HasColumnName("occ");

            // TRANSFORMENGINE: OPC = CostCentre.ProfitCentre AS OPC — owning profit centre code
            builder.Property(e => e.OPC)
                .HasMaxLength(50)
                .HasColumnName("opc");

            // TRANSFORMENGINE: SPC = WorkGroup_MAP.ProfitCentre AS SPC
            builder.Property(e => e.SPC)
                .HasMaxLength(50)
                .HasColumnName("spc");

            // TRANSFORMENGINE: SCC = WorkGroup_MAP.CostCentre AS SCC
            builder.Property(e => e.SCC)
                .HasMaxLength(50)
                .HasColumnName("scc");

            builder.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");

            builder.Property(e => e.GradeCode)
                .HasMaxLength(10)
                .HasColumnName("gradecode");

            builder.Property(e => e.SpNumber)
                .HasMaxLength(10)
                .HasColumnName("spnumber");

            builder.Property(e => e.ChargeRate)
                .HasColumnType("money")
                .HasColumnName("chargerate");

            builder.Property(e => e.Pay)
                .HasColumnType("money")
                .HasColumnName("pay");

            builder.Property(e => e.NonPay)
                .HasColumnType("money")
                .HasColumnName("nonpay");

            builder.Property(e => e.Overhead)
                .HasColumnType("money")
                .HasColumnName("overhead");

            builder.Property(e => e.Time)
                .HasColumnName("time");

            builder.Property(e => e.TotalCost)
                .HasColumnType("money")
                .HasColumnName("totalcost");
        }
    }
}
