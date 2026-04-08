using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AphaBatchJobsConsole.Core.Entities
{
    /// <summary>
    /// Entity model representing the tlkpProject lookup table.
    /// Contains project master data including parent project, program, income fields, 
    /// budget, profit, manager, customer, and status.
    /// Maps to tlkpProject table in PostgreSQL database.
    /// 
    /// This entity is used in year-end financial operations, particularly in the 
    /// sp_createFPSTotals aggregation process where it serves as the base table 
    /// for joining cost and income data.
    /// </summary>
    [Table("tlkpProject")]
    public class TlkpProject
    {
        /// <summary>
        /// Primary key representing the parent project identifier.
        /// Used as the main reference for project aggregations and cost calculations.
        /// </summary>
        [Key]
        [Column("ParentProject")]
        [Required]
        [MaxLength(50)]
        public string ParentProject { get; set; } = string.Empty;

        /// <summary>
        /// Program identifier associated with the project.
        /// Used for program-level aggregations and reporting.
        /// </summary>
        [Column("Program")]
        [MaxLength(50)]
        public string? Program { get; set; }

        /// <summary>
        /// Customer income amount for the project.
        /// Nullable decimal field that contributes to TotalIncome calculation.
        /// Defaults to 0 in aggregation queries when NULL.
        /// </summary>
        [Column("CustIncome")]
        [Precision(18, 2)]
        public decimal? CustIncome { get; set; }

        /// <summary>
        /// Transfer income amount for the project.
        /// Nullable decimal field that contributes to TotalIncome calculation.
        /// Defaults to 0 in aggregation queries when NULL.
        /// </summary>
        [Column("TransferIncome")]
        [Precision(18, 2)]
        public decimal? TransferIncome { get; set; }

        /// <summary>
        /// CVL (Central Veterinary Laboratory) budget allocation for the project.
        /// Nullable decimal field used in financial planning and tracking.
        /// </summary>
        [Column("Budget_CVL")]
        [Precision(18, 2)]
        public decimal? Budget_CVL { get; set; }

        /// <summary>
        /// Required profit margin or target profit for the project.
        /// Nullable decimal field used in financial analysis and reporting.
        /// Referenced as RequiredProfit in FPSYearTotals aggregation.
        /// </summary>
        [Column("Profit")]
        [Precision(18, 2)]
        public decimal? Profit { get; set; }

        /// <summary>
        /// Project manager identifier or name.
        /// Used for management reporting and project oversight.
        /// </summary>
        [Column("Manager")]
        [MaxLength(100)]
        public string? Manager { get; set; }

        /// <summary>
        /// Customer identifier or name associated with the project.
        /// Used for customer-level reporting and analysis.
        /// </summary>
        [Column("Customer")]
        [MaxLength(100)]
        public string? Customer { get; set; }

        /// <summary>
        /// Current status of the project (e.g., Active, Completed, On Hold).
        /// Used for filtering and status-based reporting.
        /// </summary>
        [Column("ProjectStatus")]
        [MaxLength(50)]
        public string? ProjectStatus { get; set; }

        /// <summary>
        /// PVS (Pathology and Veterinary Services) income amount.
        /// Nullable decimal field used in specialized income calculations.
        /// Defaults to 0 in aggregation queries when NULL.
        /// </summary>
        [Column("PVSIncome")]
        [Precision(18, 2)]
        public decimal? PVSIncome { get; set; }

        /// <summary>
        /// Planned casework debit amount for the project.
        /// Nullable decimal field that contributes to TotalCosts calculation.
        /// Defaults to 0 in aggregation queries when NULL.
        /// Critical component in year-end cost aggregation.
        /// </summary>
        [Column("PlanCaseworkDebit")]
        [Precision(18, 2)]
        public decimal? PlanCaseworkDebit { get; set; }

        /// <summary>
        /// Audit timestamp indicating when the record was created.
        /// Automatically set by database on insert.
        /// </summary>
        [Column("CreatedDate")]
        public DateTime? CreatedDate { get; set; }

        /// <summary>
        /// Audit timestamp indicating when the record was last modified.
        /// Automatically updated by database on update.
        /// </summary>
        [Column("ModifiedDate")]
        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// Calculates the total income for the project.
        /// Sum of CustIncome and TransferIncome, treating NULL values as 0.
        /// This matches the business logic in sp_createFPSTotals.
        /// </summary>
        [NotMapped]
        public decimal TotalIncome => (CustIncome ?? 0m) + (TransferIncome ?? 0m);

        /// <summary>
        /// Indicates whether the project has any income recorded.
        /// Useful for filtering and validation logic.
        /// </summary>
        [NotMapped]
        public bool HasIncome => (CustIncome ?? 0m) > 0m || (TransferIncome ?? 0m) > 0m;

        /// <summary>
        /// Indicates whether the project has a planned casework debit.
        /// Useful for cost tracking and validation.
        /// </summary>
        [NotMapped]
        public bool HasPlannedCosts => (PlanCaseworkDebit ?? 0m) > 0m;
    }
}


// Changes made:
// 1. Added 'm' suffix to all decimal literals (0m instead of 0) for explicit decimal type specification
//    This is a .NET best practice to avoid implicit conversions and improve code clarity
// 2. All other aspects of the code follow .NET conventions and best practices correctly:
//    - Proper use of nullable reference types (string?)
//    - Appropriate data annotations for EF Core
//    - Clear XML documentation
//    - Expression-bodied members for computed properties
//    - Proper naming conventions (PascalCase for properties)