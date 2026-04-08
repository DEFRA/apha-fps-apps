using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AphaBatchJobsConsole.Core.Entities
{
    /// <summary>
    /// Entity model representing the query result for total additional costs aggregation.
    /// This entity corresponds to the qryTotalAdditionalCosts query in the legacy Access database.
    /// Used in LEFT JOIN operations with tlkpProject during FPS year totals calculation.
    /// 
    /// Business Context:
    /// - Aggregates additional costs per project (JobCode)
    /// - Part of the year-end financial totals calculation process
    /// - Nullable TotalAdditionalCosts defaults to 0 in sp_createFPSTotals logic
    /// </summary>
    [Table("qry_total_additional_costs")]
    public class QryTotalAdditionalCosts
    {
        /// <summary>
        /// Project identifier code used for joining with tlkpProject.ParentProject.
        /// Primary key for this query result entity.
        /// Maps to JobCode field in the legacy qryTotalAdditionalCosts query.
        /// </summary>
        [Key]
        [Column("job_code")]
        [Required]
        [StringLength(50)]
        public string JobCode { get; set; } = string.Empty;

        /// <summary>
        /// Aggregated total of all additional costs for the project.
        /// Nullable to handle projects with no additional costs.
        /// In sp_createFPSTotals, NULL values are coalesced to 0 using CASE statements.
        /// 
        /// Business Rule:
        /// - When NULL, treated as 0 in total cost calculations
        /// - Contributes to TotalCosts calculation in FPSYearTotals
        /// </summary>
        [Column("total_additional_costs")]
        [Precision(18, 2)]
        public decimal? TotalAdditionalCosts { get; set; }

        /// <summary>
        /// Constructor with parameters for creating instances with values.
        /// </summary>
        /// <param name="jobCode">Project identifier code</param>
        /// <param name="totalAdditionalCosts">Aggregated additional costs amount</param>
        /// <exception cref="ArgumentNullException">Thrown when jobCode is null</exception>
        public QryTotalAdditionalCosts(string jobCode, decimal? totalAdditionalCosts)
        {
            JobCode = jobCode ?? throw new ArgumentNullException(nameof(jobCode));
            TotalAdditionalCosts = totalAdditionalCosts;
        }

        /// <summary>
        /// Parameterless constructor required by Entity Framework.
        /// </summary>
        public QryTotalAdditionalCosts()
        {
        }

        /// <summary>
        /// Gets the additional costs value, returning 0 if null.
        /// Implements the CASE WHEN logic from sp_createFPSTotals for consistent null handling.
        /// </summary>
        /// <returns>Total additional costs or 0 if null</returns>
        public decimal GetAdditionalCostsOrDefault()
        {
            return TotalAdditionalCosts ?? 0m;
        }

        /// <summary>
        /// Returns a string representation of the entity for debugging purposes.
        /// </summary>
        /// <returns>Formatted string with JobCode and TotalAdditionalCosts</returns>
        public override string ToString()
        {
            return $"QryTotalAdditionalCosts [JobCode: {JobCode}, TotalAdditionalCosts: {TotalAdditionalCosts?.ToString("C") ?? "NULL"}]";
        }
    }
}


// Changes made:
// 1. Replaced [MaxLength(50)] with [StringLength(50)] - StringLength is the preferred attribute for string length validation in .NET
// 2. Reordered constructors - Parameterless constructor now comes after the parameterized constructor for better readability
// 3. Updated parameterless constructor XML comment to clarify it's required by Entity Framework
// 4. Added <exception> XML documentation tag to the parameterized constructor to document the ArgumentNullException
// 5. Removed redundant default constructor comment as the updated comment is more specific about EF requirements