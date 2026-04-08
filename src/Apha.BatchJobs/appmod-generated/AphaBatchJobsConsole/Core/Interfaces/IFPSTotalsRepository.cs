using System.Threading.Tasks;

namespace AphaBatchJobsConsole.Core.Interfaces
{
    /// <summary>
    /// Repository interface defining data access operations for FPS Totals management.
    /// Provides abstraction layer for FPS year-end financial aggregation operations.
    /// 
    /// This interface supports the migration from legacy Microsoft Access stored procedures:
    /// - sp_createFPSTotals: Aggregates project costs and income into FPSYearTotals
    /// - sp_deleteFPSTotals: Clears existing totals before recalculation
    /// 
    /// Business Context:
    /// FPS Totals represent the aggregated financial summary for all projects in a fiscal year.
    /// The creation process joins tlkpProject with multiple cost query tables:
    /// - qryTotalAdditionalCosts: Additional project costs
    /// - qryTotalAnimalCosts: Animal-related costs
    /// - qryTotalStaffCosts: Staff and pay costs
    /// - qryTotalTestCosts: Testing and product costs
    /// 
    /// All NULL values are handled with COALESCE to default to 0.
    /// TotalCosts = sum of all cost categories + PlanCaseworkDebit
    /// TotalIncome = CustIncome + TransferIncome
    /// 
    /// Implementation Notes:
    /// - Follows Repository pattern for data access abstraction
    /// - Async operations for non-blocking database access
    /// - Returns affected row counts for audit and logging purposes
    /// - Should be implemented with proper transaction management
    /// - Must handle NULL values consistently with legacy CASE WHEN logic
    /// </summary>
    public interface IFPSTotalsRepository
    {
        /// <summary>
        /// Creates FPS year totals by aggregating project costs and income data.
        /// 
        /// This method implements the logic from legacy sp_createFPSTotals procedure:
        /// 1. Joins tlkpProject with cost query tables (LEFT JOIN to preserve all projects)
        /// 2. Applies NULL handling using COALESCE for all cost fields (defaults to 0)
        /// 3. Calculates TotalCosts as sum of all cost categories plus PlanCaseworkDebit
        /// 4. Calculates TotalIncome as CustIncome plus TransferIncome
        /// 5. Inserts aggregated records into FPSYearTotals table
        /// 
        /// Business Rules Applied:
        /// - DISTINCT projects to avoid duplicates from joins
        /// - NULL cost values default to 0 for accurate summation
        /// - All decimal calculations preserve precision (18,2)
        /// - Includes project metadata (Manager, Customer, ProjectStatus)
        /// - Captures PVSIncome and Budget_CVL for financial tracking
        /// 
        /// Data Sources:
        /// - tlkpProject: Master project table with income and metadata
        /// - qryTotalAdditionalCosts: Aggregated additional costs by JobCode
        /// - qryTotalAnimalCosts: Aggregated animal costs by JobCode
        /// - qryTotalStaffCosts: Aggregated staff costs and pay costs by ProjectCode
        /// - qryTotalTestCosts: Aggregated test costs by JobCode
        /// 
        /// Performance Considerations:
        /// - Should execute within transaction for data consistency
        /// - May process 500-1000 projects per year
        /// - Runs once per fiscal year during year-end operations
        /// - Consider indexing on join columns (ParentProject, JobCode, ProjectCode)
        /// </summary>
        /// <returns>
        /// Task containing the number of FPS year total records inserted.
        /// Returns 0 if no projects exist or all projects have NULL costs.
        /// </returns>
        /// <exception cref="System.Data.Common.DbException">
        /// Thrown when database operation fails (connection, constraint violation, etc.)
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when repository is not properly initialized or context is invalid
        /// </exception>
        Task<int> CreateFPSTotalsAsync();

        /// <summary>
        /// Deletes all existing FPS year totals records before recalculation.
        /// 
        /// This method implements the logic from legacy sp_deleteFPSTotals procedure.
        /// It clears the FPSYearTotals table to ensure clean state before running
        /// CreateFPSTotalsAsync for fresh aggregation.
        /// 
        /// Business Context:
        /// - Must be called before CreateFPSTotalsAsync to prevent duplicate records
        /// - Part of year-end financial reconciliation process
        /// - Ensures totals reflect current state of cost and income data
        /// - Should execute within same transaction as CreateFPSTotalsAsync
        /// 
        /// Usage Pattern:
        /// 1. Begin transaction
        /// 2. Call DeleteFPSTotalsAsync() to clear existing totals
        /// 3. Call CreateFPSTotalsAsync() to regenerate totals
        /// 4. Commit transaction if both succeed, rollback on any failure
        /// 
        /// Performance Considerations:
        /// - Truncate operation if supported for better performance
        /// - Should complete quickly (typically less than 1 second)
        /// - No cascading deletes required (FPSYearTotals is aggregation table)
        /// </summary>
        /// <returns>
        /// Task containing the number of FPS year total records deleted.
        /// Returns 0 if FPSYearTotals table is already empty.
        /// </returns>
        /// <exception cref="System.Data.Common.DbException">
        /// Thrown when database operation fails (connection, lock timeout, etc.)
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when repository is not properly initialized or context is invalid
        /// </exception>
        Task<int> DeleteFPSTotalsAsync();
    }
}


// Review Summary:
// The code follows .NET best practices and conventions. No changes were required because:
// 1. Namespace follows PascalCase convention
// 2. Interface naming follows IInterfaceName convention
// 3. Async method names properly end with "Async" suffix
// 4. Return types correctly use Task<T> for async operations
// 5. XML documentation is comprehensive and well-structured
// 6. Exception documentation uses fully qualified type names
// 7. Interface is properly focused with single responsibility (FPS Totals operations)
// 8. Method signatures are clean and follow async/await patterns
// 9. Using directives are minimal and necessary (only System.Threading.Tasks)
// 10. Code formatting and indentation are consistent