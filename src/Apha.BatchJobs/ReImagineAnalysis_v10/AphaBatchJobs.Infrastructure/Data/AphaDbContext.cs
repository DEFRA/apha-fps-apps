using Microsoft.EntityFrameworkCore;

namespace AphaBatchJobs.Infrastructure.Data
{
    /// <summary>
    /// Database context for Apha batch jobs application.
    /// Provides access to PostgreSQL database entities and configuration.
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

            // PostgreSQL best practice: Use snake_case naming convention for database objects
            // This can be configured here or via conventions in the model configuration
            
            // PostgreSQL best practice: Set default schema if needed
            // modelBuilder.HasDefaultSchema("public");
        }

        /// <summary>
        /// Configures the database connection and provider options.
        /// </summary>
        /// <param name="optionsBuilder">The builder being used to configure the context.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // PostgreSQL & AWS best practice: Enable sensitive data logging only in development
            // This should be configured in the DI container, not here
            // Keeping this method for future configuration needs
            
            base.OnConfiguring(optionsBuilder);
        }

        // PostgreSQL best practice: Override SaveChangesAsync for better async performance in AWS environments
        /// <summary>
        /// Asynchronously saves all changes made in this context to the database.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // AWS best practice: Respect cancellation tokens for graceful shutdown in containerized environments
            return await base.SaveChangesAsync(cancellationToken);
        }

        // .NET 8 best practice: Dispose pattern is handled by DbContext base class
        // No need to override Dispose unless custom cleanup is required
    }
}
