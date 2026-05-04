using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class ProjectPlanningViewModel
    {
        /// <summary>
        /// Gets or sets the project code value (e.g., FZ2000).
        /// This is the unique identifier for the project being planned.
        /// </summary>
        [Required(ErrorMessage = "Project code is required")]
        [Display(Name = "Project Code")]
        [StringLength(50, ErrorMessage = "Project code cannot exceed 50 characters")]
        public string ProjectCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the project description textarea value.
        /// Contains the full description of the project (e.g., "Salmonell Surveillance and control programme").
        /// </summary>
        [Required(ErrorMessage = "Project description is required")]
        [Display(Name = "Project Description")]
        [StringLength(2000, ErrorMessage = "Project description cannot exceed 2000 characters")]
        [DataType(DataType.MultilineText)]
        public string ProjectDescription { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the selected programme name (e.g., Bact, ADMIN, ASU, B&M, etc.).
        /// Represents the programme under which this project is categorized.
        /// </summary>
        [Required(ErrorMessage = "Programme selection is required")]
        [Display(Name = "Selected Programme")]
        [StringLength(100, ErrorMessage = "Programme name cannot exceed 100 characters")]
        public string SelectedProgramme { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the selected year (e.g., 2025-2026).
        /// Represents the financial year for which the project is being planned.
        /// </summary>
        [Required(ErrorMessage = "Year selection is required")]
        [Display(Name = "Selected Year")]
        [StringLength(20, ErrorMessage = "Year cannot exceed 20 characters")]
        public string SelectedYear { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the logged-in user name (e.g., Ken Rod).
        /// Represents the current user managing the project planning.
        /// </summary>
        [Display(Name = "User Name")]
        [StringLength(200, ErrorMessage = "User name cannot exceed 200 characters")]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of staff booked records with Name, Rate, Hrs, Days, and StaffCost fields.
        /// Contains all staff resources allocated to the project.
        /// </summary>
        [Display(Name = "Staff Booked")]
        public DataGridConfig<StaffJobItemViewModel> StaffBookedGrid { get; set; } = new DataGridConfig<StaffJobItemViewModel>();

        /// <summary>
        /// Gets or sets the data grid configuration for animals booked.
        /// Contains all animal resources required for the project.
        /// </summary>
        [Display(Name = "Animals Booked")]
        public DataGridConfig<AnimalPlanItem> AnimalsBookedGrid { get; set; } = new DataGridConfig<AnimalPlanItem>();

        /// <summary>
        /// Gets or sets the data grid configuration for tests booked.
        /// Contains all laboratory tests and procedures planned for the project.
        /// </summary>
        [Display(Name = "Tests Booked")]
        public DataGridConfig<TestPlanItem> TestsBookedGrid { get; set; } = new DataGridConfig<TestPlanItem>();

        /// <summary>
        /// Gets or sets the data grid configuration for exceptional costs.
        /// Contains all additional costs not covered by standard staff, animal, or test expenses.
        /// </summary>
        [Display(Name = "Exceptional Costs")]
        public DataGridConfig<AdditionalCostItemViewModel> ExceptionalCostsGrid { get; set; } = new DataGridConfig<AdditionalCostItemViewModel>();

        /// <summary>
        /// Gets or sets the total staff cost calculation.
        /// Represents the sum of all staff costs in the StaffBookedList.
        /// </summary>
        [Display(Name = "Total Staff Cost")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal TotalStaffCost { get; set; }

        /// <summary>
        /// Gets or sets the total animal cost calculation.
        /// Represents the sum of all animal costs in the AnimalsBookedList.
        /// </summary>
        [Display(Name = "Total Animal Cost")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal TotalAnimalCost { get; set; }

        /// <summary>
        /// Gets or sets the total test cost calculation.
        /// Represents the sum of all test costs in the TestsBookedList.
        /// </summary>
        [Display(Name = "Total Test Cost")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal TotalTestCost { get; set; }

        /// <summary>
        /// Gets or sets the total additional/exceptional cost calculation.
        /// Represents the sum of all exceptional costs in the ExceptionalCostsList.
        /// </summary>
        [Display(Name = "Total Additional Cost")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal TotalAdditionalCost { get; set; }

        /// <summary>
        /// Gets or sets the Transfer Income value for financial summary.
        /// Represents income transferred from other sources or projects.
        /// </summary>
        [Display(Name = "Transfer Income")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal TransferIncome { get; set; }

        /// <summary>
        /// Gets or sets the External Income value for financial summary.
        /// Represents income from external sources or clients.
        /// </summary>
        [Display(Name = "External Income")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal ExternalIncome { get; set; }

        /// <summary>
        /// Gets or sets the Budget CVL (Central Veterinary Laboratory) value for financial summary.
        /// Represents the allocated budget from CVL for this project.
        /// </summary>
        [Display(Name = "Budget CVL")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal BudgetCVL { get; set; }

        /// <summary>
        /// Gets or sets the Total Costs value for financial summary.
        /// Represents the sum of all project costs (staff, animals, tests, and exceptional costs).
        /// Calculated as: TotalStaffCost + TotalAnimalCost + TotalTestCost + TotalAdditionalCost
        /// </summary>
        [Display(Name = "Total Costs")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal TotalCosts { get; set; }

        /// <summary>
        /// Gets or sets the Profit/(Loss) value for financial summary.
        /// Represents the calculated profit or loss for the project.
        /// Calculated as: (TransferIncome + ExternalIncome + BudgetCVL) - TotalCosts
        /// Negative values indicate a loss and are typically displayed in parentheses (e.g., (£31,663)).
        /// </summary>
        [Display(Name = "Profit/(Loss)")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal ProfitLoss { get; set; }

        /// <summary>
        /// Gets or sets the Target Profit value for financial summary.
        /// Represents the desired profit target for the project.
        /// </summary>
        [Display(Name = "Target Profit")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal TargetProfit { get; set; }

        /// <summary>
        /// Gets or sets the Off-Target value for financial summary.
        /// Represents the difference between actual profit/loss and target profit.
        /// Calculated as: ProfitLoss - TargetProfit
        /// Negative values indicate the project is below target.
        /// </summary>
        [Display(Name = "Off - Target")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal OffTarget { get; set; }
    }
}
