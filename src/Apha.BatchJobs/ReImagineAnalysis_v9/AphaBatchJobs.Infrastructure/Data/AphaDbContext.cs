using Microsoft.EntityFrameworkCore;

namespace AphaBatchJobs.Infrastructure.Data
{
    /// <summary>
    /// Entity Framework DbContext for PostgreSQL database access in the Apha Batch Jobs system.
    /// Provides database connectivity and entity configuration for batch job operations.
    /// </summary>
    public class AphaDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the AphaDbContext class.
        /// </summary>
        /// <param name="options">The options to be used by the DbContext, including connection string and provider configuration.</param>
        public AphaDbContext(DbContextOptions<AphaDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Configures the model that was discovered by convention from the entity types.
        /// This method is called when the model for a derived context has been initialized,
        /// but before the model has been locked down and used to initialize the context.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure PostgreSQL-specific conventions
            // Use snake_case naming convention for PostgreSQL (common practice)
            // This can be configured here or via Npgsql.EntityFrameworkCore.PostgreSQL extensions
            
            // Apply entity configurations from the current assembly
            // This allows for separation of entity configurations into separate files
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AphaDbContext).Assembly);
        }

        /// <summary>
        /// Configures the database connection and provider options.
        /// Override this method to configure additional database-specific options for PostgreSQL and AWS.
        /// </summary>
        /// <param name="optionsBuilder">The builder being used to configure the context.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Only configure if not already configured (useful for design-time scenarios)
            if (!optionsBuilder.IsConfigured)
            {
                // Configuration should be done via dependency injection in production
                // This is a fallback for design-time tools like migrations
            }
            
            base.OnConfiguring(optionsBuilder);
        }
    }
}


**Key improvements made:**

1. **Added `ApplyConfigurationsFromAssembly`**: This is a best practice for organizing entity configurations in separate files using `IEntityTypeConfiguration<T>` implementations, making the codebase more maintainable and scalable.

2. **Added `OnConfiguring` method with guard**: Provides a hook for design-time configuration while ensuring it doesn't override runtime DI configuration. This is useful for EF Core tools like migrations.

3. **Added comments for PostgreSQL-specific conventions**: Highlighted where PostgreSQL-specific configurations (like snake_case naming) should be applied, which is a common practice when working with PostgreSQL databases.

4. **Maintained existing structure**: No new features added, only enhanced the existing methods with best practices for .NET 8, PostgreSQL, and AWS deployments.

5. **Assembly scanning**: The `ApplyConfigurationsFromAssembly` call enables automatic discovery of entity configurations, which is the recommended approach for larger applications and follows the separation of concerns principle.