using Microsoft.EntityFrameworkCore;
using AphaBatchJobsConsole.DataAccess.Data;

namespace AphaBatchJobsConsole.DataAccess.Repositories
{
    /// <summary>
    /// Generic base repository implementation providing common data access operations.
    /// Implements basic CRUD operations with Entity Framework Core and transaction support.
    /// Provides foundation for specific repository implementations.
    /// 
    /// Architecture Context:
    /// - Part of Clean Architecture DataAccess layer
    /// - Implements Repository pattern for data access abstraction
    /// - Uses Entity Framework Core for PostgreSQL database operations
    /// - Supports async/await pattern for non-blocking I/O operations
    /// - Provides transaction management through DbContext
    /// 
    /// Legacy Migration Context:
    /// - Replaces direct Access database queries with EF Core LINQ
    /// - Provides type-safe data access replacing VBA recordset operations
    /// - Enables unit testing through repository abstraction
    /// - Supports transaction rollback replacing manual error handling
    /// 
    /// Design Patterns:
    /// - Repository Pattern: Abstracts data access logic
    /// - Generic Programming: Reusable for any entity type
    /// - Async/Await: Non-blocking database operations
    /// - Dependency Injection: DbContext injected via constructor
    /// 
    /// Usage:
    /// - Base class for specific repositories (FPSYearTotalsRepository, ProjectRepository)
    /// - Provides common CRUD operations to derived classes
    /// - Derived classes add domain-specific query methods
    /// - Used by service layer for data persistence operations
    /// 
    /// Transaction Management:
    /// - All operations use DbContext transaction scope
    /// - SaveChangesAsync commits changes atomically
    /// - Exceptions trigger automatic rollback
    /// - Supports explicit transaction control in derived classes
    /// 
    /// Performance Considerations:
    /// - Async operations prevent thread blocking
    /// - FindAsync uses primary key for optimized lookups
    /// - ToListAsync materializes queries efficiently
    /// - Change tracking enabled for Update operations
    /// </summary>
    /// <typeparam name="T">Entity type that this repository manages. Must be a reference type (class).</typeparam>
    public abstract class BaseRepository<T> where T : class
    {
        /// <summary>
        /// Entity Framework Core database context for PostgreSQL operations.
        /// Provides access to DbSet collections and transaction management.
        /// Protected to allow derived repositories to access context for complex queries.
        /// 
        /// Context Lifecycle:
        /// - Injected via constructor (scoped lifetime in DI container)
        /// - Shared across all repositories in same request scope
        /// - Disposed automatically by DI container after request completion
        /// - Tracks entity changes for Update operations
        /// 
        /// Usage in Derived Classes:
        /// - Access specific DbSets: _context.FPSYearTotals
        /// - Execute raw SQL: _context.Database.ExecuteSqlRaw()
        /// - Manage transactions: _context.Database.BeginTransaction()
        /// - Complex LINQ queries: _context.Set<T>().Where(...).Include(...)
        /// </summary>
        protected readonly ApplicationDbContext _context;

        /// <summary>
        /// Protected constructor accepting ApplicationDbContext for database operations.
        /// Stores context in protected field for derived classes.
        /// 
        /// Dependency Injection:
        /// - Context injected by DI container with scoped lifetime
        /// - Same context instance shared across repositories in request scope
        /// - Ensures transaction consistency across multiple repository operations
        /// 
        /// Design Rationale:
        /// - Protected constructor enforces inheritance requirement
        /// - Prevents direct instantiation of base repository
        /// - Derived classes must call base constructor
        /// - Enables constructor injection in derived classes
        /// 
        /// Transaction Scope:
        /// - All operations within same context share transaction
        /// - SaveChangesAsync commits all tracked changes atomically
        /// - Exception during SaveChanges triggers automatic rollback
        /// </summary>
        /// <param name="context">ApplicationDbContext instance for database operations. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when context parameter is null.</exception>
        protected BaseRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Generic async method to retrieve entity by ID using FindAsync.
        /// Returns Task of T where T is entity type. Returns null if not found.
        /// 
        /// Performance Characteristics:
        /// - Uses FindAsync for optimized primary key lookup
        /// - Checks local change tracker before database query
        /// - Single database round-trip if not in change tracker
        /// - Returns tracked entity enabling subsequent updates
        /// 
        /// Business Usage:
        /// - Retrieve project by ParentProject code
        /// - Fetch FPSYearTotals for specific project
        /// - Load entity for update or delete operations
        /// - Verify entity existence before operations
        /// 
        /// Error Handling:
        /// - Returns null if entity not found (no exception thrown)
        /// - Database exceptions propagate to caller
        /// - Caller responsible for null checking
        /// 
        /// Legacy Equivalent:
        /// - Replaces Access DLookup function
        /// - Replaces VBA recordset.FindFirst operations
        /// - Type-safe alternative to dynamic SQL queries
        /// </summary>
        /// <param name="id">Primary key value of entity to retrieve. Type must match entity key type.</param>
        /// <returns>Task containing entity if found, null otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown when id parameter is null.</exception>
        /// <exception cref="DbUpdateException">Thrown when database operation fails.</exception>
        public virtual async Task<T?> GetByIdAsync(object id)
        {
            // Best Practice: Use ArgumentNullException.ThrowIfNull (available in .NET 6+)
            // This is more concise and follows modern .NET conventions
            ArgumentNullException.ThrowIfNull(id);

            return await _context.Set<T>().FindAsync(id).ConfigureAwait(false);
        }

        /// <summary>
        /// Generic async method to retrieve all entities using ToListAsync.
        /// Returns Task of IEnumerable of T where T is entity type.
        /// 
        /// Performance Characteristics:
        /// - Materializes entire table into memory
        /// - Single database query with no filtering
        /// - Returns tracked entities enabling updates
        /// - Use with caution on large tables
        /// 
        /// Business Usage:
        /// - Load all projects for year-end processing
        /// - Retrieve complete lookup tables
        /// - Bulk operations requiring all records
        /// - Small to medium sized tables only
        /// 
        /// Best Practices:
        /// - Consider pagination for large datasets
        /// - Use specific query methods with filtering in derived classes
        /// - Prefer AsNoTracking() for read-only operations
        /// - Add Where clauses in derived classes for filtered queries
        /// 
        /// Legacy Equivalent:
        /// - Replaces SELECT * FROM table queries
        /// - Replaces VBA recordset.GetRows operations
        /// - Type-safe alternative to dynamic recordsets
        /// 
        /// Optimization Note:
        /// - Derived classes should override with filtered queries
        /// - Consider implementing GetAllAsync(Expression<Func<T, bool>> predicate)
        /// - Use projection (Select) to reduce data transfer
        /// </summary>
        /// <returns>Task containing collection of all entities in table.</returns>
        /// <exception cref="DbUpdateException">Thrown when database operation fails.</exception>
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Generic async method to add new entity using AddAsync and SaveChangesAsync.
        /// Returns Task of T with added entity including generated keys.
        /// 
        /// Transaction Behavior:
        /// - AddAsync marks entity for insertion
        /// - SaveChangesAsync commits transaction atomically
        /// - Database-generated keys populated after SaveChanges
        /// - Automatic rollback on exception
        /// 
        /// Business Usage:
        /// - Insert new FPSYearTotals records
        /// - Create new project entries
        /// - Add records during year-end transfer operations
        /// - Bulk insert operations (call multiple times before SaveChanges)
        /// 
        /// Key Generation:
        /// - Identity columns populated by database
        /// - Returned entity contains generated key values
        /// - Timestamp columns set by database triggers
        /// - Audit fields populated if configured
        /// 
        /// Legacy Equivalent:
        /// - Replaces INSERT INTO SQL statements
        /// - Replaces VBA recordset.AddNew operations
        /// - Automatic parameter binding vs manual SQL construction
        /// 
        /// Error Handling:
        /// - Constraint violations throw DbUpdateException
        /// - Duplicate key errors propagate to caller
        /// - Foreign key violations throw DbUpdateException
        /// - Caller should wrap in try-catch for business logic
        /// </summary>
        /// <param name="entity">Entity instance to add to database. Must not be null.</param>
        /// <returns>Task containing added entity with generated keys populated.</returns>
        /// <exception cref="ArgumentNullException">Thrown when entity parameter is null.</exception>
        /// <exception cref="DbUpdateException">Thrown when database constraints violated or operation fails.</exception>
        public virtual async Task<T> AddAsync(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            await _context.Set<T>().AddAsync(entity).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return entity;
        }

        /// <summary>
        /// Generic async method to update existing entity using Update and SaveChangesAsync.
        /// Returns Task of T with updated entity.
        /// 
        /// Change Tracking:
        /// - Update marks all properties as modified
        /// - Only changed properties sent to database (if entity tracked)
        /// - Concurrency conflicts detected via timestamp/rowversion
        /// - Optimistic concurrency enabled if configured
        /// 
        /// Business Usage:
        /// - Update project financial totals
        /// - Modify FPSYearTotals aggregated values
        /// - Change project status during year-end
        /// - Bulk update operations
        /// 
        /// Update Strategies:
        /// - Attached entities: Only modified properties updated
        /// - Detached entities: All properties updated
        /// - Explicit property modification: context.Entry(entity).Property(p => p.Name).IsModified = true
        /// 
        /// Legacy Equivalent:
        /// - Replaces UPDATE SQL statements
        /// - Replaces VBA recordset.Update operations
        /// - Automatic concurrency checking vs manual timestamp comparison
        /// 
        /// Error Handling:
        /// - Concurrency conflicts throw DbUpdateConcurrencyException
        /// - Constraint violations throw DbUpdateException
        /// - Entity not found results in no operation (0 rows affected)
        /// - Caller should handle concurrency exceptions
        /// 
        /// Best Practice:
        /// - Load entity first with GetByIdAsync for tracked updates
        /// - Use AsNoTracking() for read-only queries before update
        /// - Implement optimistic concurrency with timestamp column
        /// </summary>
        /// <param name="entity">Entity instance with updated values. Must not be null and must have valid key.</param>
        /// <returns>Task containing updated entity.</returns>
        /// <exception cref="ArgumentNullException">Thrown when entity parameter is null.</exception>
        /// <exception cref="DbUpdateConcurrencyException">Thrown when concurrency conflict detected.</exception>
        /// <exception cref="DbUpdateException">Thrown when database constraints violated or operation fails.</exception>
        public virtual async Task<T> UpdateAsync(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return entity;
        }

        /// <summary>
        /// Generic async method to delete entity using Remove and SaveChangesAsync.
        /// Returns Task of bool indicating success.
        /// 
        /// Delete Behavior:
        /// - Remove marks entity for deletion
        /// - SaveChangesAsync commits deletion atomically
        /// - Cascade delete configured relationships deleted automatically
        /// - Returns true if entity deleted, false if not found
        /// 
        /// Business Usage:
        /// - Delete FPSYearTotals before recalculation (sp_deleteFPSTotals)
        /// - Remove year data (sp_DeleteYearsFPSData equivalent)
        /// - Cleanup operations during year-end processing
        /// - Soft delete alternative: Update IsDeleted flag
        /// 
        /// Cascade Behavior:
        /// - Configured cascade deletes execute automatically
        /// - Foreign key constraints enforced
        /// - Orphan records prevented by database constraints
        /// - Consider soft delete for audit trail preservation
        /// 
        /// Legacy Equivalent:
        /// - Replaces DELETE FROM SQL statements
        /// - Replaces VBA recordset.Delete operations
        /// - Automatic cascade vs manual child record deletion
        /// 
        /// Error Handling:
        /// - Foreign key violations throw DbUpdateException
        /// - Entity not found returns false (no exception)
        /// - Constraint violations propagate to caller
        /// - Caller should handle referential integrity errors
        /// 
        /// Best Practice:
        /// - Load entity first to ensure it exists
        /// - Consider soft delete for historical data
        /// - Use transactions for multi-table deletes
        /// - Implement audit logging for delete operations
        /// </summary>
        /// <param name="entity">Entity instance to delete. Must not be null and must have valid key.</param>
        /// <returns>Task containing true if entity deleted successfully, false if entity not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when entity parameter is null.</exception>
        /// <exception cref="DbUpdateException">Thrown when foreign key constraints violated or operation fails.</exception>
        public virtual async Task<bool> DeleteAsync(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            _context.Set<T>().Remove(entity);
            var result = await _context.SaveChangesAsync().ConfigureAwait(false);
            return result > 0;
        }
    }
}


// Key improvements made following .NET best practices:
//
// 1. ConfigureAwait(false) added to all async calls:
//    - Prevents deadlocks in synchronous contexts
//    - Improves performance by avoiding unnecessary context capture
//    - Standard practice for library/infrastructure code
//    - Allows continuation on any thread pool thread
//
// 2. ArgumentNullException.ThrowIfNull() instead of manual null checks:
//    - Available in .NET 6+ (modern .NET standard)
//    - More concise and readable
//    - Consistent with framework conventions
//    - Reduces boilerplate code
//    - Provides same functionality with better performance
//
// These changes make the code more idiomatic and aligned with modern .NET practices
// without altering functionality or adding new features.