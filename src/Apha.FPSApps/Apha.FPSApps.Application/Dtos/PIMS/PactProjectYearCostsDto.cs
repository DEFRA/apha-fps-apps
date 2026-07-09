/*
 * TRANSFORMENGINE MIGRATION — PactProjectYearCostsDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-09
 *
 * CHANGED:
 *   - New file: frontend DTO mirroring Apha.Common.Contracts.PIMS.PactProjectYearCostsRes
 *   - All properties copied with identical names, types, and nullability from backend Res contract
 *   - Placed in Apha.FPSApps.Application.Dtos.PIMS namespace for use in frontend
 *     application and infrastructure layers
 *   - Year mapped to short to match PactProjectYearCostsRes (cast from backend
 *     DTO's double at the PimsApiDtoMapper layer)
 *
 * PRESERVED:
 *   - All property names exactly match PactProjectYearCostsRes (case-sensitive)
 *   - All type definitions: decimal? for cost columns, double? for Hours,
 *     short for Year, string? for Project
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm Year type (short vs double) aligns with PimsApiDtoMapper
 *     cast from backend PactProjectYearCostsDto.Year (double) to PactProjectYearCostsRes.Year (short)
 */

namespace Apha.FPSApps.Application.Dtos.PIMS
{
    /// <summary>
    /// Frontend application-layer DTO for PACT actuals aggregation.
    /// Mirrors <c>Apha.Common.Contracts.PIMS.PactProjectYearCostsRes</c>.
    /// Used by the "Update Costing" button flow to populate cost fields in the modal before saving.
    /// </summary>
    public class PactProjectYearCostsDto
    {
        // TRANSFORMENGINE: Grouping key fields — mirrors PactProjectYearCostsRes
        /// <summary>Project code — DB column: project (varchar(20)).</summary>
        public string? Project { get; set; }

        /// <summary>Financial year — cast to short at API/frontend layer from backend double precision value.</summary>
        public short Year { get; set; }

        // TRANSFORMENGINE: Aggregated cost columns — mirrors PactProjectYearCostsRes (sum() results)
        /// <summary>Total subcontract costs — sum(subcontracts).</summary>
        public decimal? SubContracts { get; set; }

        /// <summary>Total animal costs — sum(animals). Maps to AnimalCosts in main yearly record.</summary>
        public decimal? Animals { get; set; }

        /// <summary>Total test/transfer costs — sum(transfercosts). Maps to TestCosts in main yearly record.</summary>
        public decimal? Tests { get; set; }

        /// <summary>Total pay costs — sum(vtcc_summary.pay). Maps to PayCosts in main yearly record.</summary>
        public decimal? Pay { get; set; }

        /// <summary>Total non-pay + overhead costs — sum(nonpay + overhead). Maps to NonPayOhCosts in main yearly record.</summary>
        public decimal? NonPayOH { get; set; }

        /// <summary>Total all-in costs — sum(totalcost). Used by btnFixCosting to set ActualExpenditure.</summary>
        public decimal? TotalCosts { get; set; }

        /// <summary>Total time costs — sum(timecosts). Display only.</summary>
        public decimal? TimeCost { get; set; }

        // TRANSFORMENGINE: Hours (double precision in source view)
        /// <summary>Total hours — sum(totalhours). Maps to ManHours in main yearly record; ManDays/ManYears derived.</summary>
        public double? Hours { get; set; }

        // TRANSFORMENGINE: Optional joined lookup fields from MY_tlkpProject via qryProjectYearTotals_frm
        /// <summary>Customer income from MY_tlkpProject. Display label: "Customer Income".</summary>
        public decimal? CustIncome { get; set; }

        /// <summary>VLA budget from MY_tlkpProject.Budget_CVL. Display label: "VLA Budget".</summary>
        public decimal? BudgetCvl { get; set; }
    }
}
