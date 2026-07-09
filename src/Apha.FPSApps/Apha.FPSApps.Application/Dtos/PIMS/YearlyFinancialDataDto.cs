/*
 * TRANSFORMENGINE MIGRATION — YearlyFinancialDataDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-09
 *
 * CHANGED:
 *   - New file: frontend DTO mirroring Apha.Common.Contracts.PIMS.YearlyFinancialDataRes
 *   - All properties copied with identical names, types, and nullability from backend Res contract
 *   - Placed in Apha.FPSApps.Application.Dtos.PIMS namespace for use in frontend
 *     application and infrastructure layers
 *
 * PRESERVED:
 *   - All property names exactly match YearlyFinancialDataRes (case-sensitive)
 *   - All type definitions: decimal? for money columns, double? for effort columns,
 *     short for audit flags and Locked, DateTime? for date columns, string? for text columns
 *   - TotalCosts included as a settable property (mapped from Res at infrastructure layer)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify TotalCosts computation formula matches MS Access form
 *     footer totals when set by PimsApiDtoMapper (inherited from backend Res)
 */

using System;

namespace Apha.FPSApps.Application.Dtos.PIMS
{
    /// <summary>
    /// Frontend application-layer DTO for a per-year financial data record.
    /// Mirrors <c>Apha.Common.Contracts.PIMS.YearlyFinancialDataRes</c>.
    /// Composite key: (<see cref="Year"/>, <see cref="Project"/>).
    /// </summary>
    public class YearlyFinancialDataDto
    {
        // TRANSFORMENGINE: Composite PK — mirrors YearlyFinancialDataRes
        /// <summary>Financial year (smallint NOT NULL — part of composite PK).</summary>
        public short Year { get; set; }

        /// <summary>Project code (varchar(20) NOT NULL — part of composite PK).</summary>
        public string? Project { get; set; }

        // TRANSFORMENGINE: money columns → decimal? — mirrors backend Res contract
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

        // TRANSFORMENGINE: remaining fields from Res contract
        /// <summary>Free-text adjustment comment — DB column: adjustmentcomment (varchar(250)).</summary>
        public string? AdjustmentComment { get; set; }

        /// <summary>Lock/fixed flag (0=unlocked, 1=locked/fixed) — DB column: locked. Display label: "Fixed".</summary>
        public short Locked { get; set; }

        /// <summary>Date the record was costed/fixed — DB column: datecosted. Display label: "Date Fixed".</summary>
        public DateTime? DateCosted { get; set; }

        /// <summary>Username who costed/fixed the record — DB column: costedby (varchar(20)). Display label: "Fixed By".</summary>
        public string? CostedBy { get; set; }

        // TRANSFORMENGINE: audit "changed" flag columns — read-only; included for frontend display
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

        // TRANSFORMENGINE: computed aggregation field — populated by PimsApiDtoMapper from YearlyFinancialDataRes.TotalCosts
        /// <summary>
        /// Total of all cost columns (PayCosts + NonPayOhCosts + TestCosts + AnimalCosts + NonAnimalCosts + Adjustment).
        /// Populated from YearlyFinancialDataRes.TotalCosts via PimsApiDtoMapper.
        /// </summary>
        public decimal? TotalCosts { get; set; }
    }
}
