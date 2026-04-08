using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AphaBatchJobsConsole.Core.Entities
{
    /// <summary>
    /// Entity model representing the query result for total staff costs aggregation.
    /// This entity corresponds to the qryTotalStaffCosts query in the legacy Access database.
    /// Used in LEFT JOIN operations with tlkpProject during FPS year totals calculation.
    /// 
    /// Business Context:
    /// - Aggregates staff costs and pay costs per project
    /// - Part of the year-end financial operations for APHA FPS system
    /// - Used by sp_createFPSTotals procedure to calculate total project costs
    /// - Nullable cost fields default to 0 in aggregation logic
    /// 
    /// Legacy Source: qryTotalStaffCosts query from Microsoft Access Database
    /// Migration Target: PostgreSQL view or materialized view
    /// </summary>
    [Table("qry_total_staff_costs")]
    public class QryTotalStaffCosts : IEquatable<QryTotalStaffCosts>
    {
        /// <summary>
        /// Project code identifier used as the join key with tlkpProject.ParentProject.
        /// This is the primary key for the query result set.
        /// 
        /// Business Rule: Must match ParentProject in tlkpProject table
        /// Database: VARCHAR, indexed for join performance
        /// </summary>
        [Key]
        [Column("project_code")]
        [Required]
        [MaxLength(50)]
        public string ProjectCode { get; set; } = string.Empty;

        /// <summary>
        /// Total staff costs aggregated for the project.
        /// Represents the sum of all staff-related expenses excluding direct pay costs.
        /// 
        /// Business Rule: 
        /// - NULL values are treated as 0 in sp_createFPSTotals
        /// - Contributes to TotalCosts calculation
        /// - Includes overhead, benefits, and indirect staff expenses
        /// 
        /// Database: DECIMAL(18,2), nullable
        /// Legacy: Calculated field from staff cost queries
        /// </summary>
        [Column("total_staff_costs")]
        [Precision(18, 2)]
        public decimal? TotalStaffCosts { get; set; }

        /// <summary>
        /// Total direct pay costs for staff assigned to the project.
        /// Represents the sum of actual salary/wage payments.
        /// 
        /// Business Rule:
        /// - NULL values are treated as 0 in sp_createFPSTotals
        /// - Separate from TotalStaffCosts for financial reporting
        /// - Used for pay cost analysis and budget tracking
        /// 
        /// Database: DECIMAL(18,2), nullable
        /// Legacy: Calculated field from staff pay records
        /// </summary>
        [Column("total_pay_costs")]
        [Precision(18, 2)]
        public decimal? TotalPayCosts { get; set; }

        /// <summary>
        /// Default constructor for entity initialization.
        /// Initializes nullable properties to null as per database schema.
        /// </summary>
        public QryTotalStaffCosts()
        {
            // Nullable properties initialized to null by default
            // ProjectCode initialized to empty string to satisfy non-nullable reference type
        }

        /// <summary>
        /// Parameterized constructor for creating instances with all properties.
        /// Useful for query result mapping and unit testing.
        /// </summary>
        /// <param name="projectCode">The project code identifier</param>
        /// <param name="totalStaffCosts">Total staff costs for the project (nullable)</param>
        /// <param name="totalPayCosts">Total pay costs for the project (nullable)</param>
        /// <exception cref="ArgumentNullException">Thrown when projectCode is null</exception>
        public QryTotalStaffCosts(string projectCode, decimal? totalStaffCosts, decimal? totalPayCosts)
        {
            ProjectCode = projectCode ?? throw new ArgumentNullException(nameof(projectCode));
            TotalStaffCosts = totalStaffCosts;
            TotalPayCosts = totalPayCosts;
        }

        /// <summary>
        /// Gets the total staff costs with NULL coalesced to 0.
        /// Implements the business rule from sp_createFPSTotals:
        /// CASE WHEN TotalStaffCosts IS NULL THEN 0 ELSE TotalStaffCosts END
        /// </summary>
        /// <returns>Total staff costs or 0 if null</returns>
        [NotMapped]
        public decimal TotalStaffCostsOrZero => TotalStaffCosts ?? 0m;

        /// <summary>
        /// Gets the total pay costs with NULL coalesced to 0.
        /// Implements the business rule from sp_createFPSTotals:
        /// CASE WHEN TotalPayCosts IS NULL THEN 0 ELSE TotalPayCosts END
        /// </summary>
        /// <returns>Total pay costs or 0 if null</returns>
        [NotMapped]
        public decimal TotalPayCostsOrZero => TotalPayCosts ?? 0m;

        /// <summary>
        /// Returns a string representation of the entity for logging and debugging.
        /// </summary>
        /// <returns>Formatted string with key entity properties</returns>
        public override string ToString()
        {
            return $"QryTotalStaffCosts [ProjectCode={ProjectCode}, " +
                   $"TotalStaffCosts={TotalStaffCosts?.ToString("N2") ?? "NULL"}, " +
                   $"TotalPayCosts={TotalPayCosts?.ToString("N2") ?? "NULL"}]";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// Equality is based on ProjectCode as it is the primary key.
        /// </summary>
        /// <param name="obj">The object to compare with the current object</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as QryTotalStaffCosts);
        }

        /// <summary>
        /// Determines whether the specified QryTotalStaffCosts is equal to the current object.
        /// Equality is based on ProjectCode as it is the primary key.
        /// </summary>
        /// <param name="other">The QryTotalStaffCosts to compare with the current object</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public bool Equals(QryTotalStaffCosts? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return string.Equals(ProjectCode, other.ProjectCode, StringComparison.Ordinal);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// Hash code is based on ProjectCode as it is the primary key.
        /// </summary>
        /// <returns>A hash code for the current object</returns>
        public override int GetHashCode()
        {
            return ProjectCode?.GetHashCode() ?? 0;
        }

        /// <summary>
        /// Equality operator overload.
        /// </summary>
        public static bool operator ==(QryTotalStaffCosts? left, QryTotalStaffCosts? right)
        {
            if (left is null)
                return right is null;

            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator overload.
        /// </summary>
        public static bool operator !=(QryTotalStaffCosts? left, QryTotalStaffCosts? right)
        {
            return !(left == right);
        }
    }
}


**Key improvements made:**

1. **IEquatable<T> Implementation**: Implemented `IEquatable<QryTotalStaffCosts>` interface for type-safe equality comparisons and better performance.

2. **Nullable Reference Types**: Updated `Equals(object? obj)` parameter to use nullable annotation (`object?`) for better null-safety.

3. **Improved Equals Implementation**: 
   - Added strongly-typed `Equals(QryTotalStaffCosts? other)` method
   - Added reference equality check for performance optimization
   - Used `StringComparison.Ordinal` for explicit string comparison semantics

4. **Equality Operators**: Added `==` and `!=` operator overloads to complete the equality pattern, which is a .NET best practice when implementing `IEquatable<T>`.

5. **Non-nullable String Property**: Initialized `ProjectCode` to `string.Empty` to satisfy non-nullable reference type requirements and avoid potential null reference warnings.

6. **XML Documentation**: Added `<exception>` tag to parameterized constructor for better API documentation.

7. **Pattern Matching**: Used `as` operator with pattern matching in `Equals(object?)` for cleaner null checking.