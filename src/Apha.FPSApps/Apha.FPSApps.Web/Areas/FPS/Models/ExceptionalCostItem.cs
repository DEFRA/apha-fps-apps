using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class ExceptionalCostItem
    {
        /// <summary>
        /// Gets or sets the unique identifier for the exceptional cost record.
        /// This property serves as the primary key for database operations and record tracking.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the description of the exceptional cost expense.
        /// Examples include: Expense_001, Expense_002, Expense_003, etc.
        /// This field provides a human-readable identifier for the cost item.
        /// </summary>
        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the account code associated with this exceptional cost.
        /// Examples include: Account_001, Account_002, Account_003, etc.
        /// This field links the cost to specific accounting categories for financial reporting.
        /// </summary>
        [Required(ErrorMessage = "Account is required")]
        [StringLength(100, ErrorMessage = "Account code cannot exceed 100 characters")]
        [Display(Name = "Account")]
        public string Account { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the total cost amount for this exceptional expense.
        /// This value is stored as a decimal to maintain precision for financial calculations.
        /// Currency formatting (e.g., £1,000.00) should be applied at the presentation layer.
        /// </summary>
        [Required(ErrorMessage = "Total Cost is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Total Cost must be a positive value")]
        [DataType(DataType.Currency)]
        [Display(Name = "Total Cost")]
        public decimal TotalCost { get; set; }

        /// <summary>
        /// Gets or sets the frequency or month information for this exceptional cost.
        /// This field indicates whether the cost is recurring (frequency) or associated with a specific month.
        /// Can be null or empty if not applicable to the cost item.
        /// </summary>
        [StringLength(100, ErrorMessage = "Frequency or Month cannot exceed 100 characters")]
        [Display(Name = "Freq or Mnth")]
        public string? FreqOrMnth { get; set; }

        /// <summary>
        /// Gets or sets the supplier name associated with this exceptional cost.
        /// Examples include: Supplier_001, Supplier_002, etc.
        /// This field identifies the vendor or service provider for the expense.
        /// Can be null or empty if no supplier is associated.
        /// </summary>
        [StringLength(200, ErrorMessage = "Supplier name cannot exceed 200 characters")]
        [Display(Name = "Supplier")]
        public string? Supplier { get; set; }
    }
}
