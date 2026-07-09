/*
 * TRANSFORMENGINE MIGRATION — YearlyFinancialDataReq.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-09
 *
 * CHANGED:
 *   - New file: no prior C# equivalent existed
 *   - Request contract derived from writable ControlSource-bound fields of MS Access form
 *     frmProjectRadTrackData_Update (RecordSource: MY_tlkpProjectRadTrackData)
 *   - Source table: my_tlkpprojectradtrackdata (composite PK: year + project)
 *   - money DB columns mapped to decimal?; float/double precision mapped to double?
 *   - smallint DEFAULT 0 flag (locked) kept as short to match DB type
 *   - "changed" flag columns (manhourschanged, paycostschanged, etc.) excluded from Req
 *     — they are system/audit fields not edited directly via the form
 *
 * PRESERVED:
 *   - All field names aligned with source DB column semantics and MS Access ControlSource bindings
 *   - Display label mapping noted in comments (e.g. BfBudget = "PP/Acc", PyBudget = "Customer Income")
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether Year/Project composite key should be required
 *     (non-nullable) on update vs. optional for partial-update scenarios
 */

using System;

namespace Apha.Common.Contracts.PIMS
{
    public class YearlyFinancialDataReq
    {
        // TRANSFORMENGINE: Composite PK fields — required for create/update route binding
        public short Year { get; set; }
        public string? Project { get; set; }

        // TRANSFORMENGINE: money columns from my_tlkpprojectradtrackdata
        public decimal? BfBudget { get; set; }          // Display label: "PP/Acc"
        public decimal? PyBudget { get; set; }          // Display label: "Customer Income"
        public decimal? VlaBudget { get; set; }         // DB column: vla_budget
        public decimal? Seedcorn { get; set; }
        public decimal? PayCosts { get; set; }
        public decimal? NonPayOhCosts { get; set; }
        public decimal? TestCosts { get; set; }
        public decimal? AnimalCosts { get; set; }
        public decimal? NonAnimalCosts { get; set; }    // Display label: "Project-Specific Costs"
        public decimal? Adjustment { get; set; }
        public decimal? ActualExpenditure { get; set; }

        // TRANSFORMENGINE: float/double precision columns from my_tlkpprojectradtrackdata
        public double? ManHours { get; set; }
        public double? ManDays { get; set; }
        public double? ManYears { get; set; }
        public double? ActualManYears { get; set; }

        // TRANSFORMENGINE: remaining writable fields
        public string? AdjustmentComment { get; set; } // varchar(250)
        public short Locked { get; set; }               // Display label: "Fixed" checkbox; smallint DEFAULT 0
        public DateTime? DateCosted { get; set; }       // Display label: "Date Fixed"
        public string? CostedBy { get; set; }           // Display label: "Fixed By"; varchar(20)
    }
}
