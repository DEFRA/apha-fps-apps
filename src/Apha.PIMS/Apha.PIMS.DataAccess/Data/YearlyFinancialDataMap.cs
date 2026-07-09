/*
 * TRANSFORMENGINE MIGRATION — YearlyFinancialDataMap.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository (Steps 7-7a)
 * Migrated : 2026-07-09
 *
 * CHANGED:
 *   - New file: EF Core IEntityTypeConfiguration<YearlyFinancialData> for mabarchive.my_tlkpprojectradtrackdata
 *   - Composite primary key (year, project) mapped via HasKey with constraint name pk_my_tlkpprojectradtrackdata
 *   - All money columns mapped with HasColumnType("money") per PostgreSQL DDL
 *   - All double precision columns mapped with HasColumnType("double precision")
 *   - Timestamp column (datecosted) mapped with HasColumnType("timestamp without time zone")
 *   - smallint DEFAULT 0 "changed" flag columns mapped with HasDefaultValue((short)0)
 *   - locked column mapped with HasDefaultValue((short)0)
 *   - vla_budget uses HasColumnName("vla_budget") (underscore in DDL preserved lowercase)
 *   - FK to g_tlkpproject_radtrackdata(parentproject) noted; no navigation property configured
 *   - ToTable and all HasColumnName values lowercase to match PostgreSQL DDL
 *
 * PRESERVED:
 *   - Column names verbatim from mabarchive.my_tlkpprojectradtrackdata DDL
 *   - All NOT NULL / nullable constraints from DDL
 *   - All DEFAULT values from DDL
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify EF translates money columns correctly for Npgsql — if
 *     decimal? mapping causes cast errors at runtime, change HasColumnType to "numeric" and
 *     add a Value Converter for the money type
 */

using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    /// <summary>
    /// EF Core configuration for <see cref="YearlyFinancialData"/>.
    /// Maps to the <c>mabarchive.my_tlkpprojectradtrackdata</c> PostgreSQL table.
    /// Composite primary key: (<c>year</c>, <c>project</c>).
    /// </summary>
    public class YearlyFinancialDataMap : IEntityTypeConfiguration<YearlyFinancialData>
    {
        private const string ColumnTypeTimestamp = "timestamp without time zone";
        private const string ColumnTypeMoney = "money";
        private const string ColumnTypeDouble = "double precision";

        public void Configure(EntityTypeBuilder<YearlyFinancialData> entity)
        {
            // TRANSFORMENGINE: Composite PK — CONSTRAINT pk_my_tlkpprojectradtrackdata PRIMARY KEY (year, project)
            entity.HasKey(e => new { e.Year, e.Project })
                  .HasName("pk_my_tlkpprojectradtrackdata");

            entity.ToTable("my_tlkpprojectradtrackdata", "mabarchive");

            // TRANSFORMENGINE: year — smallint NOT NULL (part of composite PK)
            entity.Property(e => e.Year)
                  .HasColumnName("year");

            // TRANSFORMENGINE: project — character varying(20) NOT NULL (part of composite PK, FK to g_tlkpproject_radtrackdata)
            entity.Property(e => e.Project)
                  .HasMaxLength(20)
                  .HasColumnName("project");

            // TRANSFORMENGINE: money columns → decimal? with HasColumnType("money")
            entity.Property(e => e.BfBudget)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("bfbudget");

            entity.Property(e => e.PyBudget)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("pybudget");

            entity.Property(e => e.Seedcorn)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("seedcorn");

            entity.Property(e => e.PayCosts)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("paycosts");

            entity.Property(e => e.NonPayOhCosts)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("nonpayohcosts");

            entity.Property(e => e.TestCosts)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("testcosts");

            entity.Property(e => e.AnimalCosts)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("animalcosts");

            entity.Property(e => e.NonAnimalCosts)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("nonanimalcosts");

            entity.Property(e => e.Adjustment)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("adjustment");

            entity.Property(e => e.ActualExpenditure)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("actualexpenditure");

            // TRANSFORMENGINE: vla_budget has underscore in DDL — must match exactly
            entity.Property(e => e.VlaBudget)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("vla_budget");

            // TRANSFORMENGINE: double precision columns → double?
            entity.Property(e => e.ManHours)
                  .HasColumnType(ColumnTypeDouble)
                  .HasColumnName("manhours");

            entity.Property(e => e.ManDays)
                  .HasColumnType(ColumnTypeDouble)
                  .HasColumnName("mandays");

            entity.Property(e => e.ManYears)
                  .HasColumnType(ColumnTypeDouble)
                  .HasColumnName("manyears");

            entity.Property(e => e.ActualManYears)
                  .HasColumnType(ColumnTypeDouble)
                  .HasColumnName("actualmanyears");

            // TRANSFORMENGINE: smallint DEFAULT 0 "changed" audit flags
            entity.Property(e => e.ManHoursChanged)
                  .HasDefaultValue((short)0)
                  .HasColumnName("manhourschanged");

            entity.Property(e => e.PayCostsChanged)
                  .HasDefaultValue((short)0)
                  .HasColumnName("paycostschanged");

            entity.Property(e => e.NonPayOhCostsChanged)
                  .HasDefaultValue((short)0)
                  .HasColumnName("nonpayohcostschanged");

            entity.Property(e => e.TestCostsChanged)
                  .HasDefaultValue((short)0)
                  .HasColumnName("testcostschanged");

            entity.Property(e => e.AnimalCostsChanged)
                  .HasDefaultValue((short)0)
                  .HasColumnName("animalcostschanged");

            entity.Property(e => e.NonAnimalCostsChanged)
                  .HasDefaultValue((short)0)
                  .HasColumnName("nonanimalcostschanged");

            // TRANSFORMENGINE: remaining scalar columns
            entity.Property(e => e.AdjustmentComment)
                  .HasMaxLength(250)
                  .HasColumnName("adjustmentcomment");

            entity.Property(e => e.Locked)
                  .HasDefaultValue((short)0)
                  .HasColumnName("locked");

            entity.Property(e => e.DateCosted)
                  .HasColumnType(ColumnTypeTimestamp)
                  .HasColumnName("datecosted");

            entity.Property(e => e.CostedBy)
                  .HasMaxLength(20)
                  .HasColumnName("costedby");
        }
    }
}
