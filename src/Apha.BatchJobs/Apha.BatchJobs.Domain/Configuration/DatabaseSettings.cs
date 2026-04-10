namespace Apha.BatchJobs.Domain.Configuration;

/// <summary>
/// Database connection settings.
/// </summary>
public sealed class DatabaseSettings
{
    /// <summary>
    /// PostgreSQL server hostname or IP.
    /// </summary>
    public required string Server { get; set; }

    /// <summary>
    /// PostgreSQL port number (default: 5432).
    /// </summary>
    public int Port { get; set; } = 5432;

    /// <summary>
    /// Database name.
    /// </summary>
    public required string Database { get; set; }

    /// <summary>
    /// Database user username.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// Database user password.
    /// </summary>
    public required string Password { get; set; }

    /// <summary>
    /// Connection timeout in seconds.
    /// </summary>
    public int Timeout { get; set; } = 30;

    /// <summary>
    /// Builds a PostgreSQL connection string from the current settings.
    /// </summary>
    public string BuildConnectionString() => 
        $"Host={Server};Port={Port};Database={Database};Username={Username};Password={Password};Timeout={Timeout}";
}
