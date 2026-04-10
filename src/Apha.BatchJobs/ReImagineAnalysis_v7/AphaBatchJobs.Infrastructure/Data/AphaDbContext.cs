using Microsoft.EntityFrameworkCore;

namespace AphaBatchJobs.Infrastructure.Data
{
    /// <summary>
    /// Database context for Apha batch jobs application.
    /// Provides foundation for database access using Entity Framework Core with PostgreSQL.
    /// </summary>
    public class AphaDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the AphaDbContext class.
        /// </summary>
        /// <param name="options">The options to be used by the DbContext.</param>
        public AphaDbContext(DbContextOptions<AphaDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Configures the model that was discovered by convention from the entity types.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure PostgreSQL-specific conventions
            // Use snake_case naming convention for PostgreSQL (common practice)
            // This can be configured here or via Npgsql.EntityFrameworkCore.PostgreSQL conventions
            
            // Apply configurations from assembly if entity configurations exist
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AphaDbContext).Assembly);
        }

        /// <summary>
        /// Configures the database connection and other options.
        /// </summary>
        /// <param name="optionsBuilder">The builder being used to configure the context.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Enable sensitive data logging only in development (should be configured via DI)
            // Enable detailed errors for better debugging (should be configured via DI)
            // Connection pooling is enabled by default in Npgsql
            // Command timeout should be configured based on AWS RDS PostgreSQL requirements
            
            base.OnConfiguring(optionsBuilder);
        }
    }
}


**Review Comments:**

1. **Added `ApplyConfigurationsFromAssembly`**: This is a best practice for organizing entity configurations in separate files using `IEntityTypeConfiguration<T>` instead of cluttering the `OnModelCreating` method.

2. **Added `OnConfiguring` override**: Included as a placeholder with comments about PostgreSQL and AWS best practices. Actual configuration should be done via dependency injection in `Program.cs` or `Startup.cs`.

3. **PostgreSQL Naming Conventions**: Added comment about snake_case naming convention which is standard for PostgreSQL databases.

4. **Connection Pooling**: Added comment noting that Npgsql (PostgreSQL provider) enables connection pooling by default, which is important for AWS RDS PostgreSQL performance.

5. **AWS Considerations**: The code structure supports AWS best practices like:
   - Connection pooling (handled by Npgsql)
   - Proper timeout configuration (should be set in connection string)
   - Prepared for IAM authentication if needed (configured via connection string)
   - Ready for AWS Secrets Manager integration (connection string from configuration)

6. **Maintained existing functionality**: No new features added, only enhanced the existing structure with best practices.