/*
 * TRANSFORMENGINE MIGRATION — PactCostsItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-09
 *
 * CHANGED:
 *   - Full Phase 11 implementation: [Display] attributes added to all properties
 *   - Panel display mapping confirmed from frmProjectRadTrackData.html "Actual (Pact) Details" table
 *     in the details/view modal (pactCustomerIncome, pactVLABudget, pactPay, pactNonPayOH,
 *      pactTotalTimeCosts, pactTest, pactAnimal, pactProjectSpecific, pactTotalCosts, pactManHours,
 *      pactManDays, pactManYears)
 *   - NOT a DataGrid item — used as a plain display model in the "Update Costing" modal panel
 *   - CustIncome → Customer Income (pactCustomerIncome)
 *   - BudgetCvl  → VLA Budget (pactVLABudget)
 *   - Pay        → Pay (pactPay)
 *   - NonPayOH   → NonPayOH (pactNonPayOH)
 *   - TimeCost   → Total Time Costs (pactTotalTimeCosts)
 *   - Tests      → Test (pactTest)
 *   - Animals    → Animal (pactAnimal)
 *   - SubContracts → Project Specific (pactProjectSpecific) — closest semantic match
 *   - TotalCosts → Total Costs (pactTotalCosts)
 *   - Hours      → Man Hours (pactManHours); ManDays/ManYears derived in view layer
 *
 * PRESERVED:
 *   - All property names exactly match PactProjectYearCostsDto for convention-based AutoMapper mapping
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm SubContracts maps to "Project Specific" pact panel field —
 *     PactProjectYearCostsDto.SubContracts may be a separate line from NonAnimalCosts;
 *     verify against backend qryProjectYearTotals view
 *   - TRANSFORMENGINE TODO: Confirm ManDays and ManYears are derived from Hours in the view/JS
 *     (pactManDays = Hours/8, pactManYears = Hours/1650) or returned as separate fields
 */

using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    /// <summary>
    /// Display item for the PACT costs panel ("Update Costing" / "Actual (Pact) Details" flow).
    /// Maps to/from <c>Apha.FPSApps.Application.Dtos.PIMS.PactProjectYearCostsDto</c>.
    /// Used in the details view modal to display actuals from the PACT system.
    /// </summary>
    public class PactCostsItem
    {
        // TRANSFORMENGINE: Grouping key — passed as context only; not displayed in panel.
        public string? Project { get; set; }
        public short Year { get; set; }

        // TRANSFORMENGINE: SubContracts — maps to pactProjectSpecific panel field.
        //                  Closest semantic match in PactProjectYearCostsDto to "Project Specific" costs.
        [Display(Name = "Project Specific")]
        public decimal? SubContracts { get; set; }

        // TRANSFORMENGINE: Animals → pactAnimal panel field.
        [Display(Name = "Animal")]
        public decimal? Animals { get; set; }

        // TRANSFORMENGINE: Tests → pactTest panel field.
        [Display(Name = "Test")]
        public decimal? Tests { get; set; }

        // TRANSFORMENGINE: Pay → pactPay panel field.
        [Display(Name = "Pay")]
        public decimal? Pay { get; set; }

        // TRANSFORMENGINE: NonPayOH → pactNonPayOH panel field.
        [Display(Name = "NonPayOH")]
        public decimal? NonPayOH { get; set; }

        // TRANSFORMENGINE: TotalCosts → pactTotalCosts panel field.
        [Display(Name = "Total Costs")]
        public decimal? TotalCosts { get; set; }

        // TRANSFORMENGINE: TimeCost → pactTotalTimeCosts panel field.
        [Display(Name = "Total Time Costs")]
        public decimal? TimeCost { get; set; }

        // TRANSFORMENGINE: Hours → pactManHours panel field.
        //                  pactManDays / pactManYears are derived in view JS (Hours/8, Hours/1650).
        [Display(Name = "Man Hours")]
        public double? Hours { get; set; }

        // TRANSFORMENGINE: CustIncome → pactCustomerIncome panel field.
        [Display(Name = "Customer Income")]
        public decimal? CustIncome { get; set; }

        // TRANSFORMENGINE: BudgetCvl → pactVLABudget panel field.
        [Display(Name = "VLA Budget")]
        public decimal? BudgetCvl { get; set; }
    }
}
