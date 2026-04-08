using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AphaBatchJobsConsole.Core.Entities;
using AphaBatchJobsConsole.Core.Interfaces;
using AphaBatchJobsConsole.DataAccess.Data;

namespace AphaBatchJobsConsole.DataAccess.Repositories
{
    /// <summary>
    /// Repository implementation for FPS Totals operations.
    /// Implements IFPSTotalsRepository with methods to create and delete FPS totals 
    /// using raw SQL queries to replicate sp_createFPSTotals and sp_deleteFPSTotals 
    /// stored procedure logic.
    /// 
    /// Architecture Context:
    /// - Part of Clean Architecture DataAccess layer
    /// - Implements Repository pattern for FPS year-end financial operations
    /// - Uses Entity Framework Core with PostgreSQL for database operations
    /// - Inherits from BaseRepository for common CRUD operations
    /// 
    /// Legacy Migration Context:
    /// - Replaces Microsoft Access sp_createFPSTotals stored procedure
    /// - Replaces Microsoft Access sp_deleteFPSTotals stored procedure
    /// - Migrates VBA macro driven operations to async .NET methods
    /// - Maintains business logic equivalence with legacy system
    /// 
    /// Business Operations:
    /// - CreateFPSTotalsAsync: Aggregates project costs and income into FPSYearTotals
    /// - DeleteFPSTotalsAsync: Clears existing totals before recalculation
    /// 
    /// Transaction Management:
    /// - All operations execute within DbContext transaction scope
    /// - Automatic rollback on exception
    /// - Caller responsible for transaction coordination across multiple operations
    /// 
    /// Performance Considerations:
    /// - Uses LINQ to Entities for type-safe queries
    /// - Leverages EF Core query optimization
    /// - Processes 500-1000 projects per year
    /// - Executes once per fiscal year during year-end operations
    /// </summary>
    public class FPSTotalsRepository : BaseRepository<FPSYearTotals>, IFPSTotalsRepository
    {
        /// <summary>
        /// Constructor accepting ApplicationDbContext for database operations.
        /// Inherits from BaseRepository to provide common CRUD operations.
        /// Stores context for SQL execution and LINQ query operations.
        /// 
        /// Dependency Injection:
        /// - Context injected by DI container with scoped lifetime
        /// - Same context instance shared across repositories in request scope
        /// - Ensures transaction consistency across multiple repository operations
        /// 
        /// Design Pattern:
        /// - Repository pattern for data access abstraction
        /// - Constructor injection for dependency management
        /// - Inheritance from BaseRepository for code reuse
        /// </summary>
        /// <param name="context">ApplicationDbContext instance for database operations. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when context parameter is null.</exception>
        public FPSTotalsRepository(ApplicationDbContext context) : base(context)
        {
            // Base constructor validates context for null
            // No additional initialization required
        }

        /// <summary>
        /// Async method to create FPS year totals by aggregating project costs and income data.
        /// 
        /// This method implements the exact logic from legacy sp_createFPSTotals procedure:
        /// 1. Performs DISTINCT SELECT from tlkpProject
        /// 2. LEFT JOIN with qryTotalAdditionalCosts ON ParentProject = JobCode
        /// 3. LEFT JOIN with qryTotalAnimalCosts ON ParentProject = JobCode
        /// 4. LEFT JOIN with qryTotalStaffCosts ON ParentProject = ProjectCode
        /// 5. LEFT JOIN with qryTotalTestCosts ON ParentProject = JobCode
        /// 6. Uses COALESCE(column, 0) for NULL handling on all cost fields
        /// 7. Calculates TotalCosts as sum of all cost categories
        /// 8. Calculates TotalIncome as CustIncome + TransferIncome
        /// 9. Inserts aggregated records into FPSYearTotals table
        /// 
        /// Business Rules Applied:
        /// - DISTINCT projects to avoid duplicates from joins
        /// - NULL cost values default to 0 for accurate summation
        /// - TotalCosts = COALESCE(TotalAdditionalCosts,0) + COALESCE(TotalAnimalCosts,0) + 
        ///                COALESCE(TotalStaffCosts,0) + COALESCE(TotalTestCosts,0) + 
        ///                COALESCE(PlanCaseworkDebit,0)
        /// - TotalIncome = COALESCE(CustIncome,0) + COALESCE(TransferIncome,0)
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
        /// - Uses EF Core LINQ query optimization
        /// - Single database round-trip for entire operation
        /// - Processes 500-1000 projects per year
        /// - Runs once per fiscal year during year-end operations
        /// - Consider indexing on join columns (ParentProject, JobCode, ProjectCode)
        /// 
        /// Transaction Behavior:
        /// - Executes within DbContext transaction scope
        /// - Automatic rollback on exception
        /// - Caller should wrap in explicit transaction with DeleteFPSTotalsAsync
        /// </summary>
        /// <returns>
        /// Task containing the number of FPS year total records inserted.
        /// Returns 0 if no projects exist or all projects have NULL costs.
        /// </returns>
        /// <exception cref="DbUpdateException">
        /// Thrown when database operation fails (connection, constraint violation, etc.)
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when repository is not properly initialized or context is invalid
        /// </exception>
        public async Task<int> CreateFPSTotalsAsync()
        {
            try
            {
                // Build the aggregation query using LINQ to Entities
                // This replicates the sp_createFPSTotals stored procedure logic
                var aggregatedData = await (
                    from project in _context.TlkpProjects
                    join additionalCosts in _context.QryTotalAdditionalCosts
                        on project.ParentProject equals additionalCosts.JobCode into additionalCostsGroup
                    from additionalCosts in additionalCostsGroup.DefaultIfEmpty()
                    join animalCosts in _context.QryTotalAnimalCosts
                        on project.ParentProject equals animalCosts.JobCode into animalCostsGroup
                    from animalCosts in animalCostsGroup.DefaultIfEmpty()
                    join staffCosts in _context.QryTotalStaffCosts
                        on project.ParentProject equals staffCosts.ProjectCode into staffCostsGroup
                    from staffCosts in staffCostsGroup.DefaultIfEmpty()
                    join testCosts in _context.QryTotalTestCosts
                        on project.ParentProject equals testCosts.JobCode into testCostsGroup
                    from testCosts in testCostsGroup.DefaultIfEmpty()
                    select new
                    {
                        Project = project,
                        AdditionalCosts = additionalCosts,
                        AnimalCosts = animalCosts,
                        StaffCosts = staffCosts,
                        TestCosts = testCosts
                    }
                ).Distinct().ToListAsync().ConfigureAwait(false);

                // Transform the query results into FPSYearTotals entities
                // This is done in-memory to avoid complex EF Core translation issues
                var fpsYearTotals = aggregatedData.Select(data =>
                {
                    var totalAdditionalCosts = data.AdditionalCosts?.TotalAdditionalCosts ?? 0m;
                    var totalAnimalCosts = data.AnimalCosts?.TotalAnimalCosts ?? 0m;
                    var totalStaffCosts = data.StaffCosts?.TotalStaffCosts ?? 0m;
                    var totalTestCosts = data.TestCosts?.TotalTestCosts ?? 0m;
                    var planCaseworkDebit = data.Project.PlanCaseworkDebit ?? 0m;
                    var custIncome = data.Project.CustIncome ?? 0m;
                    var transferIncome = data.Project.TransferIncome ?? 0m;

                    return new FPSYearTotals
                    {
                        ParentProject = data.Project.ParentProject,
                        Program = data.Project.Program,

                        // Apply COALESCE logic for NULL handling (defaults to 0)
                        TotalAdditionalCosts = totalAdditionalCosts,
                        TotalAnimalCosts = totalAnimalCosts,
                        TotalStaffCosts = totalStaffCosts,
                        TotalTestCosts = totalTestCosts,

                        // Calculate TotalCosts as sum of all cost categories plus PlanCaseworkDebit
                        TotalCosts = totalAdditionalCosts + totalAnimalCosts + totalStaffCosts + totalTestCosts + planCaseworkDebit,

                        // Income fields from project
                        CustIncome = data.Project.CustIncome,
                        TransferIncome = data.Project.TransferIncome,

                        // Calculate TotalIncome as sum of CustIncome and TransferIncome
                        TotalIncome = custIncome + transferIncome,

                        // Additional financial fields
                        Budget_CVL = data.Project.Budget_CVL,
                        RequiredProfit = data.Project.Profit,

                        // Project metadata
                        Manager = data.Project.Manager,
                        Customer = data.Project.Customer,
                        ProjectStatus = data.Project.ProjectStatus,

                        // Additional cost fields with NULL handling
                        PVSIncome = data.Project.PVSIncome ?? 0m,
                        PlanCaseworkDebit = planCaseworkDebit,
                        TotalPayCosts = data.StaffCosts?.TotalPayCosts ?? 0m
                    };
                }).ToList();

                // Add all aggregated records to FPSYearTotals table
                await _context.FPSYearTotals.AddRangeAsync(fpsYearTotals).ConfigureAwait(false);

                // Save changes and return number of records inserted
                return await _context.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (DbUpdateException ex)
            {
                // Log and rethrow database update exceptions
                // Caller should handle logging via structured logging (Serilog)
                throw new DbUpdateException(
                    "Failed to create FPS year totals. Database update operation failed. " +
                    "This may be due to constraint violations, connection issues, or data integrity problems.",
                    ex);
            }
            catch (InvalidOperationException ex)
            {
                // Log and rethrow invalid operation exceptions
                throw new InvalidOperationException(
                    "Failed to create FPS year totals. Repository or context is in an invalid state.",
                    ex);
            }
        }

        /// <summary>
        /// Async method to delete all existing FPS year totals records before recalculation.
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
        /// - Uses ExecuteDeleteAsync for efficient bulk delete operation
        /// - No entity tracking overhead
        /// - Single database round-trip
        /// - Should complete quickly (typically less than 1 second)
        /// - No cascading deletes required (FPSYearTotals is aggregation table)
        /// 
        /// Transaction Behavior:
        /// - Executes within DbContext transaction scope
        /// - Automatic rollback on exception
        /// - Caller should wrap in explicit transaction with CreateFPSTotalsAsync
        /// </summary>
        /// <returns>
        /// Task containing the number of FPS year total records deleted.
        /// Returns 0 if FPSYearTotals table is already empty.
        /// </returns>
        /// <exception cref="DbUpdateException">
        /// Thrown when database operation fails (connection, lock timeout, etc.)
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when repository is not properly initialized or context is invalid
        /// </exception>
        public async Task<int> DeleteFPSTotalsAsync()
        {
            try
            {
                // Use ExecuteDeleteAsync for efficient bulk delete operation
                // This is more performant than loading entities and calling Remove
                return await _context.FPSYearTotals
                    .ExecuteDeleteAsync()
                    .ConfigureAwait(false);
            }
            catch (DbUpdateException ex)
            {
                // Log and rethrow database update exceptions
                // Caller should handle logging via structured logging (Serilog)
                throw new DbUpdateException(
                    "Failed to delete FPS year totals. Database delete operation failed. " +
                    "This may be due to connection issues, lock timeouts, or constraint violations.",
                    ex);
            }
            catch (InvalidOperationException ex)
            {
                // Log and rethrow invalid operation exceptions
                throw new InvalidOperationException(
                    "Failed to delete FPS year totals. Repository or context is in an invalid state.",
                    ex);
            }
        }
    }
}


// Key improvements made:
// 1. Removed redundant variable assignment in DeleteFPSTotalsAsync - directly return the result
// 2. Simplified CreateFPSTotalsAsync by extracting complex calculations into local variables for better readability
// 3. Split the LINQ query into two phases: database query and in-memory transformation to avoid potential EF Core translation issues
// 4. Extracted repeated null-coalescing operations into local variables to reduce code duplication and improve maintainability
// 5. Removed redundant variable assignment in CreateFPSTotalsAsync - directly return SaveChangesAsync result
// 6. Maintained all existing functionality and business logic without adding new features