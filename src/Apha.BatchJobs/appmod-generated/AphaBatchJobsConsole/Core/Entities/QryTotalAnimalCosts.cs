using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AphaBatchJobsConsole.Core.Entities
{
    /// <summary>
    /// Entity model representing the query result for total animal costs aggregation.
    /// This entity corresponds to the qryTotalAnimalCosts query in the legacy Access database.
    /// Used in LEFT JOIN operations with tlkpProject during FPS year-end totals calculation.
    /// 
    /// Legacy Context:
    /// - Source: qryTotalAnimalCosts query in Microsoft Access
    /// - Purpose: Aggregates animal-related costs per project (JobCode)
    /// - Usage: Referenced in sp_createFPSTotals stored procedure
    /// 
    /// Business Rules:
    /// - JobCode links to tlkpProject.ParentProject
    /// - TotalAnimalCosts can be NULL (handled with CASE statements in aggregation)
    /// - NULL values default to 0 in cost calculations
    /// </summary>
    [Table("qryTotalAnimalCosts")]
    public class QryTotalAnimalCosts
    {
        /// <summary>
        /// Project identifier that links to the parent project.
        /// Corresponds to tlkpProject.ParentProject in JOIN operations.
        /// This is the primary key for the query result.
        /// </summary>
        /// <remarks>
        /// In the legacy sp_createFPSTotals procedure, this field is used as:
        /// LEFT JOIN qryTotalAnimalCosts ON tlkpProject.ParentProject = qryTotalAnimalCosts.JobCode
        /// </remarks>
        [Key]
        [Required]
        [Column("JobCode")]
        [StringLength(50)]
        public string JobCode { get; set; } = string.Empty; // Initialize to avoid null reference warnings

        /// <summary>
        /// Aggregated total of all animal-related costs for the project.
        /// Nullable to support LEFT JOIN operations where no animal costs exist.
        /// 
        /// Legacy Handling:
        /// CASE
        ///     WHEN qryTotalAnimalCosts.TotalAnimalCosts IS NULL THEN 0
        ///     ELSE qryTotalAnimalCosts.TotalAnimalCosts
        /// END AS TotalAnimalCosts
        /// </summary>
        /// <remarks>
        /// This value contributes to the overall TotalCosts calculation:
        /// TotalCosts = TotalAdditionalCosts + TotalAnimalCosts + TotalStaffCosts + TotalTestCosts + PlanCaseworkDebit
        /// 
        /// Precision: 18,2 to match financial data standards in PostgreSQL
        /// </remarks>
        [Column("TotalAnimalCosts", TypeName = "decimal(18,2)")]
        public decimal? TotalAnimalCosts { get; set; }

        /// <summary>
        /// Default constructor for Entity Framework Core and repository operations.
        /// </summary>
        public QryTotalAnimalCosts()
        {
        }

        /// <summary>
        /// Constructor with parameters for creating instances with initial values.
        /// </summary>
        /// <param name="jobCode">Project identifier</param>
        /// <param name="totalAnimalCosts">Aggregated animal costs (nullable)</param>
        /// <exception cref="ArgumentNullException">Thrown when jobCode is null</exception>
        public QryTotalAnimalCosts(string jobCode, decimal? totalAnimalCosts)
        {
            JobCode = jobCode ?? throw new ArgumentNullException(nameof(jobCode));
            TotalAnimalCosts = totalAnimalCosts;
        }

        /// <summary>
        /// Gets the animal costs value with NULL-safe handling.
        /// Returns 0 if TotalAnimalCosts is NULL, matching legacy CASE statement behavior.
        /// </summary>
        /// <returns>Animal costs value or 0 if NULL</returns>
        public decimal GetAnimalCostsOrDefault()
        {
            return TotalAnimalCosts.GetValueOrDefault();
        }

        /// <summary>
        /// Returns a string representation of the entity for logging and debugging.
        /// </summary>
        /// <returns>Formatted string with JobCode and TotalAnimalCosts</returns>
        public override string ToString()
        {
            var costsDisplay = TotalAnimalCosts.HasValue 
                ? TotalAnimalCosts.Value.ToString("C") 
                : "NULL";
            return $"QryTotalAnimalCosts [JobCode: {JobCode}, TotalAnimalCosts: {costsDisplay}]";
        }
    }
}


// Changes made:
// 1. Initialized JobCode property to string.Empty to avoid nullable reference warnings (C# 8.0+ best practice)
// 2. Used GetValueOrDefault() method instead of ?? operator in GetAnimalCostsOrDefault() - more idiomatic for nullable value types
// 3. Refactored ToString() method to use HasValue property check for better readability and performance
// 4. Added XML documentation exception tag for ArgumentNullException in parameterized constructor
// 5. Improved ToString() formatting logic by extracting conditional logic to a variable for better readability