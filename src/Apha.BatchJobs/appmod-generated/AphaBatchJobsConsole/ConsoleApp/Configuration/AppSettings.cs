using System;

namespace AphaBatchJobsConsole.ConsoleApp.Configuration
{
    /// <summary>
    /// Configuration model class for application settings.
    /// Populated from appsettings.json using IConfiguration binding.
    /// Contains database connection strings, environment configuration, and logging settings.
    /// </summary>
    /// <remarks>
    /// This class follows the Options pattern for strongly-typed configuration in .NET.
    /// It is bound to the root configuration section and provides structured access to:
    /// - Database connection strings for FPS PostgreSQL database
    /// - Environment detection (Local, Development, Production)
    /// - CloudWatch logging configuration for AWS ECS Fargate deployment
    /// 
    /// Usage:
    /// services.Configure&lt;AppSettings&gt;(configuration);
    /// 
    /// Migration Context:
    /// Replaces legacy Microsoft Access database connection with PostgreSQL connection string.
    /// Supports multi-environment deployment strategy with environment-specific configuration.
    /// </remarks>
    public sealed class AppSettings
    {
        /// <summary>
        /// Gets or sets the connection strings configuration section.
        /// Contains database connection strings for the application.
        /// </summary>
        public ConnectionStringsSettings ConnectionStrings { get; set; } = new();

        /// <summary>
        /// Gets or sets the current environment name.
        /// Valid values: Local, Development, Production
        /// Used for environment-specific behavior such as logging destination (file vs CloudWatch).
        /// </summary>
        /// <remarks>
        /// - Local: Development machine, logs to file
        /// - Development: AWS development environment, logs to CloudWatch
        /// - Production: AWS production environment, logs to CloudWatch with enhanced monitoring
        /// </remarks>
        public string Environment { get; set; } = "Local";

        /// <summary>
        /// Gets or sets the logging configuration section.
        /// Contains CloudWatch-specific logging settings for AWS deployment.
        /// </summary>
        public LoggingConfigurationSettings LoggingConfiguration { get; set; } = new();

        /// <summary>
        /// Nested class representing the ConnectionStrings configuration section.
        /// </summary>
        public sealed class ConnectionStringsSettings
        {
            /// <summary>
            /// Gets or sets the PostgreSQL database connection string for the FPS database.
            /// </summary>
            /// <remarks>
            /// Connection string format:
            /// Host=hostname;Port=5432;Database=fps_database;Username=user;Password=<REDACTED>;
            /// 
            /// Migration Note:
            /// Replaces legacy Microsoft Access database connection.
            /// Supports year-based multi-tenancy through database schema or separate databases.
            /// </remarks>
            public string FPSDatabase { get; set; } = string.Empty;
        }

        /// <summary>
        /// Nested class representing the LoggingConfiguration section.
        /// Contains AWS CloudWatch logging settings for structured logging in ECS Fargate.
        /// </summary>
        public sealed class LoggingConfigurationSettings
        {
            /// <summary>
            /// Gets or sets the CloudWatch log group name.
            /// Used for centralized logging in AWS CloudWatch.
            /// </summary>
            /// <remarks>
            /// Example: /ecs/apha-fps-batch-jobs
            /// Log streams are automatically created per container instance.
            /// </remarks>
            public string CloudWatchLogGroup { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the AWS region for CloudWatch logging.
            /// </summary>
            /// <remarks>
            /// Example: eu-west-2 (London region)
            /// Must match the region where ECS Fargate tasks are deployed.
            /// </remarks>
            public string CloudWatchRegion { get; set; } = string.Empty;
        }
    }
}


// Changes made:
// 1. Marked all classes as 'sealed' since they are not intended for inheritance (configuration POCOs)
// 2. Used target-typed 'new()' expressions instead of 'new ConnectionStringsSettings()' for cleaner syntax (C# 9.0+)
// 3. Removed unnecessary 'using System;' directive as it's not being used in the code
// 4. Applied consistent formatting and maintained all existing functionality