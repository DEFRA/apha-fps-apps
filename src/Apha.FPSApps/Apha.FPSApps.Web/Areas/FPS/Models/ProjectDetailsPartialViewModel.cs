using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class ProjectDetailsPartialViewModel
    {
        // Project details header
        public string SelectedProjectCode { get; set; } = string.Empty;
        public string Program { get; set; } = string.Empty;

        // Full Project Details tab
        public ProjectDetailsViewModel ProjectDetails { get; set; } = new ProjectDetailsViewModel();

        // Plan Summary tab - grids and totals
        public DataGridConfig<StaffJobItemViewModel> PlanSummaryStaffGrid { get; set; } = new DataGridConfig<StaffJobItemViewModel>();
        public DataGridConfig<TestPlanActualItem> PlanSummaryTestGrid { get; set; } = new DataGridConfig<TestPlanActualItem>();
        public DataGridConfig<AnimalPlanItem> PlanSummaryAnimalGrid { get; set; } = new DataGridConfig<AnimalPlanItem>();
        public DataGridConfig<AdditionalCostItemViewModel> PlanSummaryAdditionalGrid { get; set; } = new DataGridConfig<AdditionalCostItemViewModel>();

        [DataType(DataType.Currency)]
        public decimal TotalStaffPlanCost { get; set; }

        [DataType(DataType.Currency)]
        public decimal TotalTestPlanCost { get; set; }

        [DataType(DataType.Currency)]
        public decimal TotalAnimalPlanCost { get; set; }

        [DataType(DataType.Currency)]
        public decimal TotalAdditionalPlanCost { get; set; }

        // Staff Plan vs Actuals tab
        public DataGridConfig<StaffJobItemViewModel> StaffPlanGrid { get; set; } = new DataGridConfig<StaffJobItemViewModel>();
        public DataGridConfig<CompareStaff2Item> StaffActualGrid { get; set; } = new DataGridConfig<CompareStaff2Item>();

        [DataType(DataType.Currency)]
        public decimal StaffTotalPlannedCost { get; set; }

        public double StaffTotalActualHrs { get; set; }
        public double StaffTotalActualCost { get; set; }
        public double StaffPercentOfPlan { get; set; }

        // Test Plan vs Actuals tab
        public DataGridConfig<TestPlanActualItem> TestPlanGrid { get; set; } = new DataGridConfig<TestPlanActualItem>();
        public DataGridConfig<ActualTestOutputItem> TestActualGrid { get; set; } = new DataGridConfig<ActualTestOutputItem>();

        [DataType(DataType.Currency)]
        public decimal TestTotalPlannedCost { get; set; }

        public double TestTotalActualCost { get; set; }
        public double TestPercentOfPlan { get; set; }

        // Animal Plan vs Actuals tab
        public DataGridConfig<AnimalPlanItem> AnimalPlanGrid { get; set; } = new DataGridConfig<AnimalPlanItem>();
        public DataGridConfig<ActualProjectCostItem> AnimalActualGrid { get; set; } = new DataGridConfig<ActualProjectCostItem>();

        [DataType(DataType.Currency)]
        public decimal AnimalTotalPlannedCost { get; set; }

        [DataType(DataType.Currency)]
        public decimal AnimalTotalActualCost { get; set; }

        public double AnimalPercentOfPlan { get; set; }

        // Additional Plan vs Actuals tab
        public DataGridConfig<AdditionalCostItemViewModel> AdditionalPlanGrid { get; set; } = new DataGridConfig<AdditionalCostItemViewModel>();
        public DataGridConfig<ActualProjectCostItem> AdditionalActualGrid { get; set; } = new DataGridConfig<ActualProjectCostItem>();

        [DataType(DataType.Currency)]
        public decimal AdditionalTotalPlannedCost { get; set; }

        [DataType(DataType.Currency)]
        public decimal AdditionalTotalActualCost { get; set; }

        public double AdditionalPercentOfPlan { get; set; }
    }
}
