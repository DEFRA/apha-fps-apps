using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;

namespace AphaBatchJobs.Infrastructure.Data
{
    /// <summary>
    /// Entity Framework Core DbContext for PostgreSQL database access.
    /// Provides database connection and command execution capabilities for the MAB_Archive equivalent database.
    /// Configured for use with Npgsql provider to execute stored procedures and direct SQL commands.
    /// </summary>
    /// <remarks>
    /// This DbContext is designed to support the scheduled job orchestration framework,
    /// particularly for executing the LoadFromFPS data loading operations and related procedures.
    /// Connection string is configured via dependency injection in the Infrastructure layer.
    /// </remarks>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the ApplicationDbContext with the specified options.
        /// </summary>
        /// <param name="options">The options to be used by the DbContext, including Npgsql provider configuration.</param>
        /// <remarks>
        /// Options are typically configured in the Infrastructure DI registration with connection string
        /// pointing to the PostgreSQL database (MAB_Archive equivalent).
        /// </remarks>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {
        }

        /// <summary>
        /// Configures the model that was discovered by convention from the entity types
        /// exposed in DbSet properties on the derived context.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
        /// <remarks>
        /// Override this method to configure entity mappings, table schemas, and relationships for PostgreSQL.
        /// Currently minimal configuration as the context is primarily used for raw SQL execution.
        /// Future entity mappings for FPS data tables can be added here as needed.
        /// </remarks>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // PostgreSQL-specific schema configuration
            // Default schema is 'public' in PostgreSQL
            modelBuilder.HasDefaultSchema("public");

            // Future entity configurations will be added here as the application evolves
            // Example: modelBuilder.Entity<FPSYearTotals>().ToTable("fps_year_totals");
        }

        /// <summary>
        /// Gets the database connection for raw SQL command execution.
        /// </summary>
        /// <returns>The underlying database connection (NpgsqlConnection).</returns>
        /// <remarks>
        /// This property provides access to the raw database connection for executing
        /// stored procedures and SQL commands that are not mapped to entities.
        /// Used by scheduled jobs to execute PostgreSQL functions and procedures.
        /// Connection lifecycle is managed by Entity Framework Core.
        /// Caller is responsible for ensuring the connection is opened before use.
        /// </remarks>
        public IDbConnection GetDbConnection()
        {
            return Database.GetDbConnection();
        }

        // Removed redundant Database property exposure
        // The base.Database property is already accessible and does not need to be re-exposed
        // Consumers can directly access Database property from the DbContext base class
    }
}


**Key improvements made:**

1. **Removed redundant `Database` property**: The `public new DatabaseFacade Database` property was unnecessary since `Database` is already a public property on the `DbContext` base class. Re-exposing it with `new` keyword creates confusion and is not idiomatic.

2. **Enhanced documentation**: Added clarification in the `GetDbConnection()` remarks that the caller is responsible for ensuring the connection is opened before use, which is important for proper connection management.

3. **Maintained existing functionality**: All existing features remain intact - the DbContext initialization, model configuration, and connection access method are preserved.

4. **Cleaner API surface**: By removing the redundant property, the class has a cleaner, more maintainable API surface that follows .NET conventions.

5. **Better encapsulation**: Consumers can still access `Database` directly from the base class without the confusion of a shadowed property.