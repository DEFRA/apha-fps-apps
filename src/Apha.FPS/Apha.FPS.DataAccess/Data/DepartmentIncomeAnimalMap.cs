/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeAnimalMap.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository (Steps 7-7a)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New IEntityTypeConfiguration<DepartmentIncomeAnimal> created for keyless entity
 *   - HasNoKey() reflects LINQ-projection origin (qryDeptIncomeAnimals SELECT query)
 *   - ToView() placeholder — repository uses LINQ joins; view can be materialised later
 *   - All HasColumnName() values are lowercase per project convention
 *   - Money columns (rate, totalcost) given HasColumnType("money")
 *
 * PRESERVED:
 *   - All 13 properties from DepartmentIncomeAnimal entity
 *   - Nullable semantics: occ, opc nullable from RIGHT JOIN on costcentre
 *   - SPC literal "SSSD" and SCC literal "35227" semantics documented
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: create fps.vw_dept_income_animal PostgreSQL view if direct view access required
 */

using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    // TRANSFORMENGINE: Keyless map — mirrors qryDeptIncomeAnimals; repository uses LINQ joins and VBA helper ports
    public class DepartmentIncomeAnimalMap : IEntityTypeConfiguration<DepartmentIncomeAnimal>
    {
        public void Configure(EntityTypeBuilder<DepartmentIncomeAnimal> builder)
        {
            // TRANSFORMENGINE: HasNoKey — LINQ projection entity
            builder.HasNoKey();
            // TRANSFORMENGINE: ToView placeholder — repository builds via LINQ
            builder.ToView("vw_dept_income_animal", "fps");

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

            // TRANSFORMENGINE: SPC = literal "SSSD" from Access query
            builder.Property(e => e.SPC)
                .HasMaxLength(50)
                .HasColumnName("spc");

            // TRANSFORMENGINE: SCC = literal 35227 from Access query — stored as string
            builder.Property(e => e.SCC)
                .HasMaxLength(10)
                .HasColumnName("scc");

            // TRANSFORMENGINE: fnAnimalDesc([description]) result — VBA helper ported to repository
            builder.Property(e => e.AnimalType)
                .HasMaxLength(50)
                .HasColumnName("animaltype");

            // TRANSFORMENGINE: fnAnimalDays([description]) result — VBA helper ported to repository
            builder.Property(e => e.AnimalDays)
                .HasColumnName("animaldays");

            // TRANSFORMENGINE: DLookUp DailyRate via EF join on Animals entity in repository
            builder.Property(e => e.Rate)
                .HasColumnType("money")
                .HasColumnName("rate");

            builder.Property(e => e.TotalCost)
                .HasColumnType("money")
                .HasColumnName("totalcost");
        }
    }
}
