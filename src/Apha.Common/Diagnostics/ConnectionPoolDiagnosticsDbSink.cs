using Microsoft.Extensions.Logging;
using Npgsql;

namespace Apha.Common.Diagnostics
{
    /// <summary>
    /// Writes connection-pool diagnostics snapshots to a PostgreSQL table.
    /// The table is created automatically on first use. This is the preferred sink on
    /// locked-down web servers because it relies on the existing database permissions
    /// rather than file-system write access.
    /// </summary>
    public sealed class ConnectionPoolDiagnosticsDbSink
    {
        private readonly ILogger _logger;
        private readonly string? _connectionString;
        private readonly string _tableName;
        private readonly string _applicationName;
        private bool _tableEnsured;
        private bool _disabled;

        public ConnectionPoolDiagnosticsDbSink(
            ILogger logger,
            string? connectionString,
            string tableName,
            string applicationName)
        {
            _logger = logger;
            _connectionString = connectionString;
            _tableName = string.IsNullOrWhiteSpace(tableName)
                ? "public.connection_pool_diagnostics"
                : tableName;
            _applicationName = applicationName;

            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                _disabled = true;
                _logger.LogWarning(
                    "ConnectionPoolDiagnostics DB sink disabled: no connection string was resolved.");
            }
        }

        public bool IsEnabled => !_disabled;

        public async Task WriteAsync(
            DateTime timestampUtc, string dbPool, string apiPool, CancellationToken cancellationToken)
        {
            if (_disabled)
            {
                return;
            }

            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                if (!_tableEnsured)
                {
                    await EnsureTableAsync(connection, cancellationToken);
                    _tableEnsured = true;
                }

                const string insertSql = @"
INSERT INTO {0} (captured_at_utc, application_name, db_pool, api_pool)
VALUES (@capturedAt, @application, @dbPool, @apiPool);";

                await using var command = new NpgsqlCommand(
                    string.Format(insertSql, _tableName), connection);
                command.Parameters.AddWithValue("capturedAt", timestampUtc);
                command.Parameters.AddWithValue("application", _applicationName);
                command.Parameters.AddWithValue("dbPool", (object?)dbPool ?? string.Empty);
                command.Parameters.AddWithValue("apiPool", (object?)apiPool ?? string.Empty);

                await command.ExecuteNonQueryAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // Never let diagnostics break the application; disable after a hard failure.
                _disabled = true;
                _logger.LogWarning(ex,
                    "ConnectionPoolDiagnostics DB sink failed writing to '{Table}'. Disabling DB logging; snapshots remain available in memory.",
                    _tableName);
            }
        }

        private async Task EnsureTableAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
        {
            var createSql = $@"
CREATE TABLE IF NOT EXISTS {_tableName} (
    id               BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    captured_at_utc  TIMESTAMPTZ NOT NULL,
    application_name TEXT NOT NULL,
    db_pool          TEXT NOT NULL,
    api_pool         TEXT NOT NULL
);";

            await using var command = new NpgsqlCommand(createSql, connection);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
