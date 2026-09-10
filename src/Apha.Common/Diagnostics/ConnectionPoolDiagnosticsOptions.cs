namespace Apha.Common.Diagnostics
{
    /// <summary>
    /// Configuration for <see cref="ConnectionPoolDiagnosticsService"/>.
    /// Bind from the "ConnectionPoolDiagnostics" configuration section.
    /// </summary>
    public sealed class ConnectionPoolDiagnosticsOptions
    {
        public const string SectionName = "ConnectionPoolDiagnostics";

        /// <summary>When false the background sampler does nothing.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>How often (in seconds) a pool snapshot is logged. Minimum 5s.</summary>
        public int SampleIntervalSeconds { get; set; } = 15;

        /// <summary>
        /// When true, each snapshot is also appended to a CSV file for download/analysis.
        /// </summary>
        public bool WriteToCsv { get; set; } = false;

        /// <summary>
        /// Relative (to content root) or absolute path for the CSV file. When relative, it is
        /// resolved against the application's content root at runtime.
        /// </summary>
        public string CsvFilePath { get; set; } = "Logs/connection-pool-diagnostics.csv";

        /// <summary>
        /// When true, each snapshot is inserted into a database table. This avoids the
        /// file-permission issues on locked-down web servers because the app already has
        /// database access.
        /// </summary>
        public bool WriteToDatabase { get; set; } = true;

        /// <summary>
        /// Name of the connection string (from the "ConnectionStrings" section) used to write
        /// diagnostics rows. Defaults to the shared "FPSConnectionString".
        /// </summary>
        public string ConnectionStringName { get; set; } = "FPSConnectionString";

        /// <summary>
        /// Fully-qualified table name (schema-qualified) that stores the diagnostics rows.
        /// The table is created automatically if it does not exist.
        /// </summary>
        public string TableName { get; set; } = "public.connection_pool_diagnostics";

        /// <summary>
        /// Maximum number of snapshots retained in memory for download when disk writing is
        /// unavailable (e.g. server denies file-write permission). Oldest rows are trimmed.
        /// </summary>
        public int MaxInMemoryRows { get; set; } = 5000;
    }
}
