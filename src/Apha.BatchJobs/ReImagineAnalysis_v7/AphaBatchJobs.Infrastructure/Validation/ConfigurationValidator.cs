namespace AphaBatchJobs.Infrastructure.Validation;

/// <summary>
/// Provides static validation methods for application configuration.
/// </summary>
public static class ConfigurationValidator
{
    /// <summary>
    /// Validates the required configuration settings.
    /// </summary>
    /// <param name="configuration">The configuration instance to validate.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the configuration parameter is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the DefaultConnection connection string is null or empty.
    /// </exception>
    public static void Validate(IConfiguration configuration)
    {
        // Best Practice: Validate method parameters to prevent null reference exceptions
        ArgumentNullException.ThrowIfNull(configuration);
        
        var defaultConnection = configuration.GetConnectionString("DefaultConnection");
        
        // Best Practice: Use IsNullOrWhiteSpace instead of IsNullOrEmpty to catch whitespace-only strings
        if (string.IsNullOrWhiteSpace(defaultConnection))
        {
            throw new InvalidOperationException(
                "Configuration validation failed: ConnectionStrings:DefaultConnection is required and cannot be null or empty.");
        }
    }
}


// Key improvements made:
// 1. Added null check for the configuration parameter using ArgumentNullException.ThrowIfNull() (.NET 6+)
// 2. Changed string.IsNullOrEmpty to string.IsNullOrWhiteSpace to handle whitespace-only connection strings
// 3. Added ArgumentNullException to the XML documentation
// 4. Maintained existing functionality without adding new features