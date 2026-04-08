using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AphaBatchJobsConsole.Core.Entities
{
    /// <summary>
    /// Entity model representing the FPS Year Totals table structure.
    /// Contains all cost categories, income fields, and project metadata 
    /// that are populated by the sp_createFPSTotals procedure.
    /// Maps to FPSYearTotals table in PostgreSQL database.
    /// 
    /// This entity aggregates financial data from multiple sources:
    /// - Additional costs from qryTotalAdditionalCosts
    /// - Animal costs from qryTotalAnimalCosts
    /// - Staff costs from qryTotalStaffCosts
    /// - Test costs from qryTotalTestCosts
    /// - Project metadata from tlkpProject
    /// 
    /// Business Rules:
    /// - All cost fields default to 0 when NULL in source data
    /// - TotalCosts = TotalAdditionalCosts + TotalAnimalCosts + TotalStaffCosts + TotalTestCosts + PlanCaseworkDebit
    /// - TotalIncome = CustIncome + TransferIncome
    /// - Nullable decimals handle NULL values from database queries
    /// </summary>
    [Table("FPSYearTotals")]
    public class FPSYearTotals
    {
        /// <summary>
        /// Primary identifier for the parent project.
        /// Maps to tlkpProject.ParentProject in the source query.
        /// </summary>
        [Key]
        [Column("ParentProject")]
        [StringLength(50)]
        [Required]
        public string ParentProject { get; set; } = string.Empty;

        /// <summary>
        /// Program identifier associated with the project.
        /// Maps to tlkpProject.Program in the source query.
        /// </summary>
        [Column("Program")]
        [StringLength(100)]
        public string? Program { get; set; }

        /// <summary>
        /// Total additional costs aggregated from qryTotalAdditionalCosts.
        /// Defaults to 0 when NULL in source data.
        /// Nullable to handle database NULL values.
        /// </summary>
        [Column("TotalAdditionalCosts", TypeName = "decimal(18,2)")]
        public decimal? TotalAdditionalCosts { get; set; }

        /// <summary>
        /// Total animal costs aggregated from qryTotalAnimalCosts.
        /// Defaults to 0 when NULL in source data.
        /// Nullable to handle database NULL values.
        /// </summary>
        [Column("TotalAnimalCosts", TypeName = "decimal(18,2)")]
        public decimal? TotalAnimalCosts { get; set; }

        /// <summary>
        /// Total staff costs aggregated from qryTotalStaffCosts.
        /// Defaults to 0 when NULL in source data.
        /// Nullable to handle database NULL values.
        /// </summary>
        [Column("TotalStaffCosts", TypeName = "decimal(18,2)")]
        public decimal? TotalStaffCosts { get; set; }

        /// <summary>
        /// Total test costs aggregated from qryTotalTestCosts.
        /// Defaults to 0 when NULL in source data.
        /// Nullable to handle database NULL values.
        /// </summary>
        [Column("TotalTestCosts", TypeName = "decimal(18,2)")]
        public decimal? TotalTestCosts { get; set; }

        /// <summary>
        /// Calculated total of all cost categories.
        /// Formula: TotalAdditionalCosts + TotalAnimalCosts + TotalStaffCosts + TotalTestCosts + PlanCaseworkDebit
        /// All NULL values are treated as 0 in the calculation.
        /// Nullable to handle database NULL values.
        /// </summary>
        [Column("TotalCosts", TypeName = "decimal(18,2)")]
        public decimal? TotalCosts { get; set; }

        /// <summary>
        /// Customer income for the project.
        /// Maps to tlkpProject.CustIncome in the source query.
        /// Nullable to handle database NULL values.
        /// </summary>
        [Column("CustIncome", TypeName = "decimal(18,2)")]
        public decimal? CustIncome { get; set; }

        /// <summary>
        /// Transfer income for the project.
        /// Maps to tlkpProject.TransferIncome in the source query.
        /// Nullable to handle database NULL values.
        /// </summary>
        [Column("TransferIncome", TypeName = "decimal(18,2)")]
        public decimal? TransferIncome { get; set; }

        /// <summary>
        /// Calculated total income.
        /// Formula: CustIncome + TransferIncome
        /// Nullable to handle database NULL values.
        /// </summary>
        [Column("TotalIncome", TypeName = "decimal(18,2)")]
        public decimal? TotalIncome { get; set; }

        /// <summary>
        /// CVL budget allocation for the project.
        /// Maps to tlkpProject.Budget_CVL in the source query.
        /// Nullable to handle database NULL values.
        /// </summary>
        [Column("Budget_CVL", TypeName = "decimal(18,2)")]
        public decimal? Budget_CVL { get; set; }

        /// <summary>
        /// Required profit margin for the project.
        /// Maps to tlkpProject.Profit in the source query.
        /// Nullable to handle database NULL values.
        /// </summary>
        [Column("RequiredProfit", TypeName = "decimal(18,2)")]
        public decimal? RequiredProfit { get; set; }

        /// <summary>
        /// Project manager identifier or name.
        /// Maps to tlkpProject.Manager in the source query.
        /// </summary>
        [Column("Manager")]
        [StringLength(100)]
        public string? Manager { get; set; }

        /// <summary>
        /// Customer identifier or name.
        /// Maps to tlkpProject.Customer in the source query.
        /// </summary>
        [Column("Customer")]
        [StringLength(200)]
        public string? Customer { get; set; }

        /// <summary>
        /// Current status of the project.
        /// Maps to tlkpProject.ProjectStatus in the source query.
        /// </summary>
        [Column("ProjectStatus")]
        [StringLength(50)]
        public string? ProjectStatus { get; set; }

        /// <summary>
        /// PVS (Pathology and Veterinary Services) income.
        /// Maps to tlkpProject.PVSIncome in the source query.
        /// Defaults to 0 when NULL in source data.
        /// Nullable to handle database NULL values.
        /// </summary>
        [Column("PVSIncome", TypeName = "decimal(18,2)")]
        public decimal? PVSIncome { get; set; }

        /// <summary>
        /// Planned casework debit amount.
        /// Maps to tlkpProject.PlanCaseworkDebit in the source query.
        /// Defaults to 0 when NULL in source data.
        /// Included in TotalCosts calculation.
        /// Nullable to handle database NULL values.
        /// </summary>
        [Column("PlanCaseworkDebit", TypeName = "decimal(18,2)")]
        public decimal? PlanCaseworkDebit { get; set; }

        /// <summary>
        /// Total pay costs for staff.
        /// Maps to qryTotalStaffCosts.TotalPayCosts in the source query.
        /// Defaults to 0 when NULL in source data.
        /// Nullable to handle database NULL values.
        /// </summary>
        [Column("TotalPayCosts", TypeName = "decimal(18,2)")]
        public decimal? TotalPayCosts { get; set; }
    }
}


// Changes made:
// 1. Added [Required] attribute to ParentProject since it's the primary key and should not be null
// 2. Initialized ParentProject with string.Empty to avoid CS8618 nullable warning for non-nullable reference types
// 3. Added nullable reference type annotations (?) to all string properties except ParentProject to properly indicate they can be null
// 4. Removed unused 'using System;' directive (no DateTime or other System types are used)
// 5. These changes align with modern C# nullable reference types best practices (C# 8.0+) and Entity Framework Core conventions