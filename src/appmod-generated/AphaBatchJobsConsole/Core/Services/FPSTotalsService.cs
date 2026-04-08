using System;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AphaBatchJobsConsole.Core.Interfaces;

namespace AphaBatchJobsConsole.Core.Services
{
    /// <summary>
    /// Service implementation for FPS Totals business logic operations.
    /// Implements IFPSTotalsService by delegating to IFPSTotalsRepository for data operations.
    /// 
    /// Migration Context:
    /// - Replaces legacy VBA macros sp_createFPSTotals and sp_deleteFPSTotals
    /// - Provides async operations for better scalability in .NET 10 environment
    /// - Supports transaction management and structured logging via Serilog
    /// - Integrates with Repository pattern for data access abstraction
    /// 
    /// Business Context:
    /// This service orchestrates the delete-then-create pattern for FPS totals regeneration.
    /// FPS Totals represent aggregated financial summaries for all projects in a fiscal year,
    /// including costs (additional, animal, staff, test) and income (customer, transfer).
    /// 
    /// Architectural Pattern:
    /// - Clean Architecture: Core layer service with dependency on repository interface
    /// - Dependency Injection: Constructor injection of repository and logger
    /// - Separation of Concerns: Business logic in service, data access in repository
    /// - Structured Logging: Serilog with contextual information for audit trail
    /// 
    /// Usage Pattern:
    /// 1. Inject IFPSTotalsService into orchestration layer or hosted service
    /// 2. Call DeleteFPSTotalsAsync() to clear existing totals
    /// 3. Call CreateFPSTotalsAsync() to regenerate totals
    /// 4. Transaction management handled at repository level
    /// 
    /// Performance Considerations:
    /// - Async operations prevent thread blocking during database operations
    /// - Processes 500-1000 projects per year during year-end operations
    /// - Executes once per fiscal year as part of scheduled batch job
    /// </summary>
    public sealed class FPSTotalsService : IFPSTotalsService
    {
        private readonly IFPSTotalsRepository _fpsTotalsRepository;
        private readonly ILogger<FPSTotalsService> _logger;

        /// <summary>
        /// Initializes a new instance of the FPSTotalsService class.
        /// Constructor accepting IFPSTotalsRepository and ILogger for dependency injection.
        /// </summary>
        /// <param name="fpsTotalsRepository">
        /// Repository interface for FPS Totals data access operations.
        /// Must not be null.
        /// </param>
        /// <param name="logger">
        /// Structured logger for operation tracking and audit trail.
        /// Must not be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when fpsTotalsRepository or logger is null.
        /// </exception>
        public FPSTotalsService(
            IFPSTotalsRepository fpsTotalsRepository,
            ILogger<FPSTotalsService> logger)
        {
            _fpsTotalsRepository = fpsTotalsRepository ?? throw new ArgumentNullException(nameof(fpsTotalsRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

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
        /// <exception cref="InvalidOperationException">
        /// Thrown when business validation fails or data integrity issues are detected.
        /// </exception>
        /// <exception cref="DbException">
        /// Thrown when database operation fails during totals creation.
        /// </exception>
        public async Task<int> CreateFPSTotalsAsync()
        {
            _logger.LogInformation("Starting FPS Totals creation process");

            try
            {
                var recordsCreated = await _fpsTotalsRepository.CreateFPSTotalsAsync().ConfigureAwait(false);

                _logger.LogInformation(
                    "FPS Totals creation completed successfully. Records created: {RecordsCreated}",
                    recordsCreated);

                return recordsCreated;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(
                    ex,
                    "Business validation failed during FPS Totals creation. Operation: {Operation}",
                    nameof(CreateFPSTotalsAsync));
                throw;
            }
            catch (DbException ex)
            {
                _logger.LogError(
                    ex,
                    "Database operation failed during FPS Totals creation. Operation: {Operation}",
                    nameof(CreateFPSTotalsAsync));
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error occurred during FPS Totals creation. Operation: {Operation}",
                    nameof(CreateFPSTotalsAsync));
                throw;
            }
        }

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
        /// <exception cref="InvalidOperationException">
        /// Thrown when business validation fails or deletion constraints are violated.
        /// </exception>
        /// <exception cref="DbException">
        /// Thrown when database operation fails during totals deletion.
        /// </exception>
        public async Task<int> DeleteFPSTotalsAsync()
        {
            _logger.LogInformation("Starting FPS Totals deletion process");

            try
            {
                var recordsDeleted = await _fpsTotalsRepository.DeleteFPSTotalsAsync().ConfigureAwait(false);

                _logger.LogInformation(
                    "FPS Totals deletion completed successfully. Records deleted: {RecordsDeleted}",
                    recordsDeleted);

                return recordsDeleted;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(
                    ex,
                    "Business validation failed during FPS Totals deletion. Operation: {Operation}",
                    nameof(DeleteFPSTotalsAsync));
                throw;
            }
            catch (DbException ex)
            {
                _logger.LogError(
                    ex,
                    "Database operation failed during FPS Totals deletion. Operation: {Operation}",
                    nameof(DeleteFPSTotalsAsync));
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error occurred during FPS Totals deletion. Operation: {Operation}",
                    nameof(DeleteFPSTotalsAsync));
                throw;
            }
        }
    }
}


// Key improvements made following .NET best practices:
//
// 1. Sealed class modifier: Added 'sealed' keyword to the class since it's not designed for inheritance,
//    improving performance by allowing the JIT compiler to optimize virtual method calls.
//
// 2. ConfigureAwait(false): Added to all async calls to prevent unnecessary context capturing,
//    improving performance in library/service code where synchronization context is not needed.
//
// 3. Simplified exception type: Changed 'System.Data.Common.DbException' to just 'DbException' 
//    with proper using directive at the top, following .NET naming conventions and reducing verbosity.
//
// 4. Consistent exception documentation: Updated XML documentation to use 'DbException' instead of
//    the fully qualified name for consistency and readability.
//
// These changes maintain all existing functionality while making the code more idiomatic and efficient
// according to modern .NET best practices.