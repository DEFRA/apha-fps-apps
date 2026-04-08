using System.Threading.Tasks;

namespace AphaBatchJobsConsole.Core.Interfaces
{
    /// <summary>
    /// Service interface defining business logic operations for FPS Totals management.
    /// Orchestrates the creation and deletion of FPS yearly totals with proper transaction management,
    /// validation, and logging. This interface is part of the Core layer and defines the contract
    /// for FPS totals business operations.
    /// 
    /// Migration Context:
    /// - Replaces legacy VBA macros sp_createFPSTotals and sp_deleteFPSTotals
    /// - Provides async operations for better scalability in .NET 10 environment
    /// - Supports transaction management and structured logging via Serilog
    /// - Integrates with Repository pattern for data access abstraction
    /// </summary>
    public interface IFPSTotalsService
    {
        /// <summary>
        /// Orchestrates the FPS totals creation process by aggregating project costs and income data.
        /// This method coordinates the business logic for creating yearly financial totals including:
        /// - Total Additional Costs aggregation
        /// - Total Animal Costs aggregation
        /// - Total Staff Costs aggregation
        /// - Total Test Costs aggregation
        /// - Total Income calculation (Customer Income + Transfer Income)
        /// - Budget and profit calculations
        /// 
        /// Business Rules Applied:
        /// - NULL cost values default to 0
        /// - TotalCosts = sum of all cost categories + PlanCaseworkDebit
        /// - TotalIncome = CustIncome + TransferIncome
        /// - Transactional operation with rollback on failure
        /// - Structured logging for audit trail
        /// 
        /// Legacy Equivalent: sp_createFPSTotals stored procedure
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the number 
        /// of FPS total records successfully created. Returns 0 if no records were created or operation failed.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when business validation fails or data integrity issues are detected.
        /// </exception>
        /// <exception cref="System.Data.Common.DbException">
        /// Thrown when database operation fails during totals creation.
        /// </exception>
        Task<int> CreateFPSTotalsAsync();

        /// <summary>
        /// Orchestrates the FPS totals deletion process to clear existing yearly totals.
        /// This method is typically executed before recalculating totals to ensure data consistency.
        /// Implements proper transaction management to ensure atomic deletion operation.
        /// 
        /// Business Rules Applied:
        /// - Deletes all existing FPS year totals records
        /// - Transactional operation with rollback on failure
        /// - Structured logging for audit trail
        /// - Validates deletion success before committing transaction
        /// 
        /// Legacy Equivalent: sp_deleteFPSTotals stored procedure
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the number 
        /// of FPS total records successfully deleted. Returns 0 if no records existed or operation failed.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when business validation fails or deletion constraints are violated.
        /// </exception>
        /// <exception cref="System.Data.Common.DbException">
        /// Thrown when database operation fails during totals deletion.
        /// </exception>
        Task<int> DeleteFPSTotalsAsync();
    }
}


// Review Summary:
// 1. Updated XML documentation for <returns> tags to follow .NET conventions by starting with 
//    "A task that represents the asynchronous operation" for async methods
// 2. Added periods at the end of exception documentation for consistency
// 3. All other aspects of the code follow .NET best practices:
//    - Proper namespace structure
//    - Clear interface naming with 'I' prefix
//    - Async suffix for async methods
//    - Comprehensive XML documentation
//    - Appropriate use of Task<T> return types
//    - Well-documented exceptions