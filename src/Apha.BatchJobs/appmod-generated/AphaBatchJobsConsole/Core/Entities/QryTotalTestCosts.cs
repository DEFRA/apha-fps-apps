using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AphaBatchJobsConsole.Core.Entities
{
    /// <summary>
    /// Entity model representing the query result for total test costs aggregation.
    /// This entity corresponds to the qryTotalTestCosts query in the legacy Access database.
    /// Used in LEFT JOIN operations with tlkpProject during FPS year totals calculation.
    /// 
    /// Legacy Context:
    /// - Part of sp_createFPSTotals stored procedure
    /// - Aggregates test costs by JobCode
    /// - Nullable TotalTestCosts defaults to 0 in CASE statements
    /// 
    /// Business Rules:
    /// - JobCode links to tlkpProject.ParentProject
    /// - TotalTestCosts can be NULL (handled in aggregation logic)
    /// - Used for calculating overall project costs in year-end processing
    /// </summary>
    [Table("qry_total_test_costs")]
    public class QryTotalTestCosts : IEquatable<QryTotalTestCosts>
    {
        /// <summary>
        /// Job code identifier linking to the parent project.
        /// Corresponds to tlkpProject.ParentProject in JOIN operations.
        /// Primary key for this query result entity.
        /// </summary>
        [Key]
        [Column("job_code")]
        [Required]
        [MaxLength(50)]
        public string JobCode { get; set; } = string.Empty;

        /// <summary>
        /// Aggregated total of all test costs for the job.
        /// Nullable to handle cases where no test costs exist.
        /// In sp_createFPSTotals, NULL values are converted to 0 using CASE statements.
        /// 
        /// Business Logic:
        /// - Sum of all test/product costs associated with the job
        /// - Contributes to TotalCosts calculation
        /// - Part of the cost breakdown: Additional + Animal + Staff + Test + PlanCaseworkDebit
        /// </summary>
        [Column("total_test_costs")]
        [Precision(18, 2)]
        public decimal? TotalTestCosts { get; set; }

        /// <summary>
        /// Default constructor for entity framework and dependency injection.
        /// </summary>
        public QryTotalTestCosts()
        {
        }

        /// <summary>
        /// Constructor with parameters for creating instances with values.
        /// </summary>
        /// <param name="jobCode">The job code identifier</param>
        /// <param name="totalTestCosts">The aggregated test costs (nullable)</param>
        /// <exception cref="ArgumentNullException">Thrown when jobCode is null</exception>
        public QryTotalTestCosts(string jobCode, decimal? totalTestCosts)
        {
            JobCode = jobCode ?? throw new ArgumentNullException(nameof(jobCode));
            TotalTestCosts = totalTestCosts;
        }

        /// <summary>
        /// Gets the test costs value, returning 0 if NULL.
        /// Implements the CASE WHEN logic from sp_createFPSTotals:
        /// CASE WHEN qryTotalTestCosts.TotalTestCosts IS NULL THEN 0 ELSE qryTotalTestCosts.TotalTestCosts END
        /// </summary>
        /// <returns>The test costs value or 0 if NULL</returns>
        [NotMapped]
        public decimal GetTestCostsOrDefault()
        {
            return TotalTestCosts ?? 0m;
        }

        /// <summary>
        /// Returns a string representation of the entity for logging and debugging.
        /// </summary>
        /// <returns>Formatted string with JobCode and TotalTestCosts</returns>
        public override string ToString()
        {
            return $"QryTotalTestCosts [JobCode: {JobCode}, TotalTestCosts: {TotalTestCosts?.ToString("C") ?? "NULL"}]";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// Based on JobCode as the unique identifier.
        /// </summary>
        /// <param name="obj">The object to compare with the current object</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as QryTotalTestCosts);
        }

        /// <summary>
        /// Determines whether the specified QryTotalTestCosts is equal to the current object.
        /// Based on JobCode as the unique identifier.
        /// </summary>
        /// <param name="other">The QryTotalTestCosts to compare with the current object</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public bool Equals(QryTotalTestCosts? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(JobCode, other.JobCode, StringComparison.Ordinal);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// Based on JobCode as the unique identifier.
        /// </summary>
        /// <returns>A hash code for the current object</returns>
        public override int GetHashCode()
        {
            return JobCode?.GetHashCode() ?? 0;
        }
    }
}


// Key improvements made:
// 1. Implemented IEquatable<QryTotalTestCosts> for type-safe equality comparisons
// 2. Added ReferenceEquals check in Equals method for performance optimization
// 3. Used StringComparison.Ordinal for string comparison (more explicit and performant)
// 4. Refactored Equals(object?) to delegate to Equals(QryTotalTestCosts?) to avoid code duplication
// 5. Added XML documentation for exception thrown in constructor
// 6. Improved null checking pattern using 'is null' instead of '== null' for consistency with modern C# idioms