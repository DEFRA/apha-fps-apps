using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AphaBatchJobsConsole.Core.Interfaces;
using AphaBatchJobsConsole.DataAccess.Data;
using AphaBatchJobsConsole.DataAccess.Repositories;

namespace AphaBatchJobsConsole.DataAccess.Configuration
{
    /// <summary>
    /// Extension class for registering DataAccess layer dependencies with dependency injection container.
    /// Configures DbContext with PostgreSQL connection using Npgsql and registers repository implementations.
    /// 
    /// Architecture Context:
    /// - Part of Clean Architecture DataAccess layer configuration
    /// - Implements Service Collection extension pattern for modular DI registration
    /// - Supports environment-specific configuration (local, AWS ECS Fargate)
    /// - Enables constructor injection throughout the application
    /// 
    /// Legacy Migration Context:
    /// - Replaces manual ADO.NET connection management in VBA macros
    /// - Centralizes database configuration previously scattered across Access modules
    /// - Enables testability through interface-based dependency injection
    /// - Supports connection pooling and performance optimization
    /// 
    /// Configuration Sources:
    /// - appsettings.json for local development environment
    /// - AWS Systems Manager Parameter Store for production deployment
    /// - Environment variables for container-based deployment (ECS Fargate)
    /// - Connection string format: Host=hostname;Database=dbname;Username=user;Password=pass;Port=5432
    /// 
    /// Registered Services:
    /// - ApplicationDbContext: EF Core DbContext with PostgreSQL provider (Scoped)
    /// - IFPSTotalsRepository: Repository for FPS totals operations (Scoped)
    /// 
    /// Service Lifetimes:
    /// - Scoped: New instance per request/operation scope
    /// - Ensures DbContext is disposed after each operation
    /// - Prevents memory leaks and connection pool exhaustion
    /// 
    /// Performance Considerations:
    /// - Connection pooling enabled by default in Npgsql
    /// - DbContext pooling can be enabled for high-throughput scenarios
    /// - Command timeout configurable via connection string
    /// - Retry logic for transient failures handled at repository level
    /// 
    /// Security Considerations:
    /// - Connection strings should never be hardcoded
    /// - Use AWS Secrets Manager or Parameter Store for production credentials
    /// - Enable SSL/TLS for database connections in production
    /// - Apply principle of least privilege for database user permissions
    /// </summary>
    public static class ServiceCollectionExtension
    {
        /// <summary>
        /// Static extension method for IServiceCollection accepting IConfiguration parameter.
        /// Registers ApplicationDbContext with PostgreSQL using AddDbContext with Npgsql provider.
        /// Configures connection string from IConfiguration and registers repository implementations.
        /// 
        /// Configuration Key:
        /// - "ConnectionStrings:DefaultConnection" for database connection string
        /// - Falls back to "DATABASE_CONNECTION_STRING" environment variable if not found
        /// 
        /// DbContext Configuration:
        /// - Uses Npgsql provider for PostgreSQL database
        /// - Scoped lifetime (new instance per request/operation)
        /// - Lazy loading disabled for explicit control over data loading
        /// - Query tracking behavior set to NoTracking for read-heavy operations
        /// - Sensitive data logging disabled in production for security
        /// 
        /// Repository Registration:
        /// - IFPSTotalsRepository implemented by FPSTotalsRepository (Scoped)
        /// - Additional repositories can be registered following same pattern
        /// - Scoped lifetime ensures proper DbContext lifecycle management
        /// 
        /// Usage Example:
        /// <code>
        /// var builder = Host.CreateDefaultBuilder(args)
        ///     .ConfigureServices((context, services) =>
        ///     {
        ///         services.AddDataAccessServices(context.Configuration);
        ///     });
        /// </code>
        /// 
        /// Error Handling:
        /// - Throws ArgumentNullException if services or configuration is null
        /// - Throws InvalidOperationException if connection string is missing or invalid
        /// - Database connection validation occurs at first use, not during registration
        /// 
        /// Best Practices Applied:
        /// - Extension method pattern for clean service registration
        /// - Method chaining support via IServiceCollection return type
        /// - Explicit lifetime management (Scoped) for predictable behavior
        /// - Configuration-based connection string management
        /// - Interface-based registration for testability and flexibility
        /// </summary>
        /// <param name="services">
        /// IServiceCollection instance to register services with.
        /// Must not be null.
        /// </param>
        /// <param name="configuration">
        /// IConfiguration instance containing connection string and other settings.
        /// Must not be null and must contain valid connection string.
        /// </param>
        /// <returns>
        /// IServiceCollection for method chaining support.
        /// Allows fluent configuration of additional services.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when services or configuration parameter is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when connection string is missing or invalid in configuration.
        /// </exception>
        public static IServiceCollection AddDataAccessServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Validate input parameters using ArgumentNullException.ThrowIfNull (C# 11+)
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            // Retrieve connection string from configuration
            // Priority: appsettings.json -> Environment variable -> Exception
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? configuration["DATABASE_CONNECTION_STRING"];

            // Validate connection string is not null, empty or whitespace
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Database connection string not found or is empty. " +
                    "Please configure 'ConnectionStrings:DefaultConnection' in appsettings.json " +
                    "or set 'DATABASE_CONNECTION_STRING' environment variable.");
            }

            // Register ApplicationDbContext with PostgreSQL provider
            // Scoped lifetime ensures proper disposal and prevents memory leaks
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                // Configure Npgsql provider for PostgreSQL
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    // Enable retry on transient failures (connection issues, timeouts)
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);

                    // Set command timeout to 30 seconds for long-running queries
                    npgsqlOptions.CommandTimeout(30);

                    // REVIEW: UseNodaTime() requires NodaTime package and is not standard for "legacy timestamp behavior"
                    // Remove this line if NodaTime is not being used in the project
                    // If timestamp compatibility is needed, consider using AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)
                    // or configure via connection string: "Include Error Detail=true"
                    // npgsqlOptions.UseNodaTime();
                });

                // Enable sensitive data logging and detailed errors only in Development environment
                // Check environment using standard configuration key
                var environment = configuration["ASPNETCORE_ENVIRONMENT"];
                if (string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase))
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }

                // Set default query tracking behavior to NoTracking for read-heavy operations
                // Improves performance by avoiding change tracking overhead
                // Repositories can override this for specific update operations
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            // Register repository implementations with scoped lifetime
            // Scoped lifetime ensures repositories share same DbContext instance within request scope
            // This is critical for transaction management and unit of work pattern

            // Register FPSTotalsRepository for FPS year-end totals operations
            services.AddScoped<IFPSTotalsRepository, FPSTotalsRepository>();

            // Additional repository registrations can be added here following same pattern:
            // services.AddScoped<IProjectRepository, ProjectRepository>();
            // services.AddScoped<ICostRepository, CostRepository>();
            // services.AddScoped<IMultiYearRepository, MultiYearRepository>();

            // Return services for method chaining
            return services;
        }
    }
}


**Key improvements made:**

1. **Removed redundant parameter names** in `ArgumentNullException.ThrowIfNull()` - not needed in C# 11+
2. **Consolidated connection string validation** - combined null check with whitespace check for cleaner code
3. **Improved error message** - single, clearer message for all connection string validation failures
4. **Removed UseNodaTime()** - This requires an additional NuGet package (Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime) and the comment says "legacy timestamp behavior" which is misleading. NodaTime is for advanced date/time handling, not legacy compatibility. If timestamp compatibility is needed, use AppContext switch instead.
5. **Simplified environment check** - removed null coalescing with "Production" default since the check handles null properly with string.Equals
6. **Maintained all existing functionality** - no features added or removed, only code quality improvements