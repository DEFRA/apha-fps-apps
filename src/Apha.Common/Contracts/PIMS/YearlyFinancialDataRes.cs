/*
 * TRANSFORMENGINE MIGRATION — YearlyFinancialDataRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-09
 *
 * CHANGED:
 *   - New file: no prior C# equivalent existed
 *   - Response contract mirrors the full RecordSource surface of MY_tlkpProjectRadTrackData
 *     plus system/audit "changed" flag columns and a computed TotalCosts field
 *   - Source table: my_tlkpprojectradtrackdata (composite PK: year + project)
 *   - money DB columns mapped to decimal?; float/double precision mapped to double?
 *   - smallint DEFAULT 0 audit flag columns included (manhourschanged etc.) — read-only
 *     outputs needed by the frontend update-costing workflow
 *   - TotalCosts is a computed helper field aggregating money columns for display
 *
 * PRESERVED:
 *   - All field names aligned with source DB column semantics
 *   - Display label mapping noted in comments
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm TotalCosts computation formula matches
 *     MS Access form footer totals (PayCosts + NonPayOhCosts + TestCosts + AnimalCosts +
 *     NonAnimalCosts + Adjustment is the assumed formula — verify against legacy form)
 */

using System;

namespace Apha.Common.Contracts.PIMS
{
    public class YearlyFinancialDataRes
    {
        // TRANSFORMENGINE: Composite PK — always populated in responses
        public short Year { get; set; }
        public string? Project { get; set; }

        // TRANSFORMENGINE: money columns from my_tlkpprojectradtrackdata
        public decimal? BfBudget { get; set; }              // Display label: "PP/Acc"
        public decimal? PyBudget { get; set; }              // Display label: "Customer Income"
        public decimal? VlaBudget { get; set; }             // DB column: vla_budget
        public decimal? Seedcorn { get; set; }
        public decimal? PayCosts { get; set; }
        public decimal? NonPayOhCosts { get; set; }
        public decimal? TestCosts { get; set; }
        public decimal? AnimalCosts { get; set; }
        public decimal? NonAnimalCosts { get; set; }        // Display label: "Project-Specific Costs"
        public decimal? Adjustment { get; set; }
        public decimal? ActualExpenditure { get; set; }

        // TRANSFORMENGINE: float/double precision columns from my_tlkpprojectradtrackdata
        public double? ManHours { get; set; }
        public double? ManDays { get; set; }
        public double? ManYears { get; set; }
        public double? ActualManYears { get; set; }

        // TRANSFORMENGINE: remaining output fields from the full RecordSource
        public string? AdjustmentComment { get; set; }     // varchar(250)
        public short Locked { get; set; }                  // Display label: "Fixed"; smallint DEFAULT 0
        public DateTime? DateCosted { get; set; }           // Display label: "Date Fixed"
        public string? CostedBy { get; set; }               // Display label: "Fixed By"; varchar(20)

        // TRANSFORMENGINE: audit "changed" flag columns — read-only, included for frontend display
        public short ManHoursChanged { get; set; }
        public short PayCostsChanged { get; set; }
        public short NonPayOhCostsChanged { get; set; }
        public short TestCostsChanged { get; set; }
        public short AnimalCostsChanged { get; set; }
        public short NonAnimalCostsChanged { get; set; }

        // TRANSFORMENGINE: computed aggregation field — replaces MS Access form footer column total
        public decimal? TotalCosts { get; set; }
    }
}
