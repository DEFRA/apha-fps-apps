/*
 * TRANSFORMENGINE MIGRATION — YearlyFinancialDataDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-09
 *
 * CHANGED:
 *   - New file: no prior C# DTO existed for this entity
 *   - Application-layer DTO mirroring YearlyFinancialData entity
 *     (mabarchive.my_tlkpprojectradtrackdata) for service-layer contracts
 *   - Composite key fields (Year, Project) preserved from entity
 *   - money entity columns kept as decimal? in DTO
 *   - double precision entity columns kept as double? in DTO
 *   - smallint DEFAULT 0 "changed" audit flag columns preserved as short
 *   - TotalCosts: computed property aggregating cost columns for grid/modal display
 *     (replaces MS Access form footer column totals)
 *
 * PRESERVED:
 *   - All field names and types aligned with YearlyFinancialData entity and YearlyFinancialDataRes contract
 *   - Display label mapping comments carried forward from entity definition
 *   - All nullable constraints mirrored from entity
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify TotalCosts computation formula matches MS Access form footer totals
 *     (assumed: PayCosts + NonPayOhCosts + TestCosts + AnimalCosts + NonAnimalCosts + Adjustment)
 */

namespace Apha.PIMS.Application.Dtos
{
    /// <summary>
    /// Application-layer DTO for a per-year financial data record.
    /// Maps to/from <see cref="Apha.PIMS.Core.Entities.YearlyFinancialData"/>.
    /// Composite key: (<see cref="Year"/>, <see cref="Project"/>).
    /// </summary>
    public class YearlyFinancialDataDto
    {
        // TRANSFORMENGINE: Composite PK (year, project) — carried from entity
        /// <summary>Financial year (smallint NOT NULL — part of composite PK).</summary>
        public short Year { get; set; }

        /// <summary>Project code (varchar(20) NOT NULL — part of composite PK).</summary>
        public string? Project { get; set; }

        // TRANSFORMENGINE: money columns → decimal? — budget/cost fields from my_tlkpprojectradtrackdata
        /// <summary>PP/Acc budget — DB column: bfbudget. Display label: "PP/Acc".</summary>
        public decimal? BfBudget { get; set; }

        /// <summary>Customer income budget — DB column: pybudget. Display label: "Customer Income".</summary>
        public decimal? PyBudget { get; set; }

        /// <summary>VLA budget — DB column: vla_budget.</summary>
        public decimal? VlaBudget { get; set; }

        /// <summary>Seedcorn allocation — DB column: seedcorn.</summary>
        public decimal? Seedcorn { get; set; }

        /// <summary>Pay costs — DB column: paycosts.</summary>
        public decimal? PayCosts { get; set; }

        /// <summary>Non-pay overhead costs — DB column: nonpayohcosts.</summary>
        public decimal? NonPayOhCosts { get; set; }

        /// <summary>Test costs — DB column: testcosts.</summary>
        public decimal? TestCosts { get; set; }

        /// <summary>Animal costs — DB column: animalcosts.</summary>
        public decimal? AnimalCosts { get; set; }

        /// <summary>Non-animal (project-specific) costs — DB column: nonanimalcosts. Display label: "Project-Specific Costs".</summary>
        public decimal? NonAnimalCosts { get; set; }

        /// <summary>Cost adjustment — DB column: adjustment.</summary>
        public decimal? Adjustment { get; set; }

        /// <summary>Actual expenditure — DB column: actualexpenditure.</summary>
        public decimal? ActualExpenditure { get; set; }

        // TRANSFORMENGINE: double precision columns → double? — man-effort fields
        /// <summary>Man-hours — DB column: manhours.</summary>
        public double? ManHours { get; set; }

        /// <summary>Man-days — DB column: mandays.</summary>
        public double? ManDays { get; set; }

        /// <summary>Man-years — DB column: manyears.</summary>
        public double? ManYears { get; set; }

        /// <summary>Actual man-years — DB column: actualmanyears.</summary>
        public double? ActualManYears { get; set; }

        // TRANSFORMENGINE: smallint DEFAULT 0 "changed" audit flags — system fields, read-only in service layer
        /// <summary>Flag: man-hours value was manually changed — DB column: manhourschanged.</summary>
        public short ManHoursChanged { get; set; }

        /// <summary>Flag: pay costs value was manually changed — DB column: paycostschanged.</summary>
        public short PayCostsChanged { get; set; }

        /// <summary>Flag: non-pay OH costs value was manually changed — DB column: nonpayohcostschanged.</summary>
        public short NonPayOhCostsChanged { get; set; }

        /// <summary>Flag: test costs value was manually changed — DB column: testcostschanged.</summary>
        public short TestCostsChanged { get; set; }

        /// <summary>Flag: animal costs value was manually changed — DB column: animalcostschanged.</summary>
        public short AnimalCostsChanged { get; set; }

        /// <summary>Flag: non-animal costs value was manually changed — DB column: nonanimalcostschanged.</summary>
        public short NonAnimalCostsChanged { get; set; }

        // TRANSFORMENGINE: remaining fields from entity
        /// <summary>Free-text adjustment comment — DB column: adjustmentcomment (varchar(250)).</summary>
        public string? AdjustmentComment { get; set; }

        /// <summary>Lock/fixed flag (0=unlocked, 1=locked/fixed) — DB column: locked. Display label: "Fixed".</summary>
        public short Locked { get; set; }

        /// <summary>Date the record was costed/fixed — DB column: datecosted. Display label: "Date Fixed".</summary>
        public DateTime? DateCosted { get; set; }

        /// <summary>Username who costed/fixed the record — DB column: costedby (varchar(20)). Display label: "Fixed By".</summary>
        public string? CostedBy { get; set; }

        // TRANSFORMENGINE: Computed property — aggregates cost columns to replace MS Access form footer totals
        //   Formula: PayCosts + NonPayOhCosts + TestCosts + AnimalCosts + NonAnimalCosts + Adjustment
        //   TRANSFORMENGINE TODO: Verify this formula against the legacy Access form footer calculation
        /// <summary>
        /// Computed total of all cost columns (PayCosts + NonPayOhCosts + TestCosts + AnimalCosts + NonAnimalCosts + Adjustment).
        /// Returns null only when all component costs are null; otherwise returns the numeric sum (may be zero).
        /// </summary>
        public decimal? TotalCosts
        {
            get
            {
                if (PayCosts is null && NonPayOhCosts is null && TestCosts is null
                    && AnimalCosts is null && NonAnimalCosts is null && Adjustment is null)
                    return null;

                return (PayCosts ?? 0m)
                    + (NonPayOhCosts ?? 0m)
                    + (TestCosts ?? 0m)
                    + (AnimalCosts ?? 0m)
                    + (NonAnimalCosts ?? 0m)
                    + (Adjustment ?? 0m);
            }
        }
    }
}
