/*
 * TRANSFORMENGINE MIGRATION — YearlyFinancialData.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination (Steps 2-3)
 * Migrated : 2026-07-09
 *
 * CHANGED:
 *   - New file: no prior C# entity existed for this table
 *   - Entity derived from PostgreSQL DDL: mabarchive.my_tlkpprojectradtrackdata
 *   - Composite primary key: (Year, Project) — matches CONSTRAINT pk_my_tlkpprojectradtrackdata
 *   - PostgreSQL money columns mapped to decimal?
 *   - PostgreSQL double precision columns mapped to double?
 *   - PostgreSQL smallint columns mapped to short / short?
 *   - PostgreSQL timestamp without time zone mapped to DateTime?
 *   - PostgreSQL character varying mapped to string?
 *   - DB column vla_budget mapped to VlaBudget property (PascalCase normalisation)
 *   - "changed" flag columns (ManHoursChanged, PayCostsChanged, etc.) included as short?
 *     — system/audit fields maintained by DB triggers/app logic
 *   - Foreign key to g_tlkpproject_radtrackdata (parentproject) noted; navigation
 *     property intentionally omitted in Core layer (EF map handles FK configuration)
 *
 * PRESERVED:
 *   - All column names from DDL, normalised to PascalCase
 *   - DEFAULT 0 semantics noted in XML docs; EF map will set HasDefaultValue(0)
 *   - All nullable constraints mirrored exactly from DDL
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify EF map (YearlyFinancialDataMap.cs) sets HasColumnType("money")
 *     for all decimal? properties so PostgreSQL money cast is applied correctly
 */

namespace Apha.PIMS.Core.Entities
{
    /// <summary>
    /// Entity representing a per-year financial data record for a RAD-track project.
    /// Maps to the <c>mabarchive.my_tlkpprojectradtrackdata</c> PostgreSQL table.
    /// Composite primary key: (<see cref="Year"/>, <see cref="Project"/>).
    /// </summary>
    public class YearlyFinancialData
    {
        // TRANSFORMENGINE: Composite PK (year, project) — CONSTRAINT pk_my_tlkpprojectradtrackdata
        /// <summary>Financial year (smallint NOT NULL — part of composite PK).</summary>
        public short Year { get; set; }

        /// <summary>Project code (varchar(20) NOT NULL — part of composite PK, FK to g_tlkpproject_radtrackdata.parentproject).</summary>
        public string Project { get; set; } = null!;

        // TRANSFORMENGINE: money columns → decimal? (nullable money columns from DDL)
        /// <summary>PP/Acc budget — DB column: bfbudget (money).</summary>
        public decimal? BfBudget { get; set; }

        /// <summary>Customer income budget — DB column: pybudget (money).</summary>
        public decimal? PyBudget { get; set; }

        /// <summary>Seedcorn allocation — DB column: seedcorn (money).</summary>
        public decimal? Seedcorn { get; set; }

        /// <summary>Pay costs — DB column: paycosts (money).</summary>
        public decimal? PayCosts { get; set; }

        /// <summary>Non-pay overhead costs — DB column: nonpayohcosts (money).</summary>
        public decimal? NonPayOhCosts { get; set; }

        /// <summary>Test costs — DB column: testcosts (money).</summary>
        public decimal? TestCosts { get; set; }

        /// <summary>Animal costs — DB column: animalcosts (money).</summary>
        public decimal? AnimalCosts { get; set; }

        /// <summary>Non-animal (project-specific) costs — DB column: nonanimalcosts (money).</summary>
        public decimal? NonAnimalCosts { get; set; }

        /// <summary>Cost adjustment — DB column: adjustment (money).</summary>
        public decimal? Adjustment { get; set; }

        /// <summary>Actual expenditure — DB column: actualexpenditure (money).</summary>
        public decimal? ActualExpenditure { get; set; }

        /// <summary>VLA budget — DB column: vla_budget (money).</summary>
        public decimal? VlaBudget { get; set; }

        // TRANSFORMENGINE: double precision columns → double?
        /// <summary>Man-hours — DB column: manhours (double precision).</summary>
        public double? ManHours { get; set; }

        /// <summary>Man-days — DB column: mandays (double precision).</summary>
        public double? ManDays { get; set; }

        /// <summary>Man-years — DB column: manyears (double precision).</summary>
        public double? ManYears { get; set; }

        /// <summary>Actual man-years — DB column: actualmanyears (double precision).</summary>
        public double? ActualManYears { get; set; }

        // TRANSFORMENGINE: smallint DEFAULT 0 "changed" audit flags
        /// <summary>Flag: man-hours value was manually changed — DB column: manhourschanged (smallint DEFAULT 0).</summary>
        public short ManHoursChanged { get; set; }

        /// <summary>Flag: pay costs value was manually changed — DB column: paycostschanged (smallint DEFAULT 0).</summary>
        public short PayCostsChanged { get; set; }

        /// <summary>Flag: non-pay OH costs value was manually changed — DB column: nonpayohcostschanged (smallint DEFAULT 0).</summary>
        public short NonPayOhCostsChanged { get; set; }

        /// <summary>Flag: test costs value was manually changed — DB column: testcostschanged (smallint DEFAULT 0).</summary>
        public short TestCostsChanged { get; set; }

        /// <summary>Flag: animal costs value was manually changed — DB column: animalcostschanged (smallint DEFAULT 0).</summary>
        public short AnimalCostsChanged { get; set; }

        /// <summary>Flag: non-animal costs value was manually changed — DB column: nonanimalcostschanged (smallint DEFAULT 0).</summary>
        public short NonAnimalCostsChanged { get; set; }

        // TRANSFORMENGINE: remaining columns from DDL
        /// <summary>Free-text adjustment comment — DB column: adjustmentcomment (varchar(250)).</summary>
        public string? AdjustmentComment { get; set; }

        /// <summary>Lock flag (0=unlocked, 1=locked/fixed) — DB column: locked (smallint DEFAULT 0). Display label: "Fixed".</summary>
        public short Locked { get; set; }

        /// <summary>Date the record was costed/fixed — DB column: datecosted (timestamp without time zone). Display label: "Date Fixed".</summary>
        public DateTime? DateCosted { get; set; }

        /// <summary>Username who costed/fixed the record — DB column: costedby (varchar(20)). Display label: "Fixed By".</summary>
        public string? CostedBy { get; set; }
    }
}
