using System.Data.Common;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Apha.Costbook.DataAccess.Interceptors
{
    /// <summary>
    /// Centralised EF Core command interceptor that captures every executed SQL command
    /// (query text, parameters, start time, end time and total execution duration) and
    /// appends it as a row to a CSV file. Useful for diagnosing slow queries.
    /// </summary>
    public sealed class QueryProfilingInterceptor : DbCommandInterceptor
    {
        private static readonly object FileLock = new();
        private readonly string _csvFilePath;
        private readonly long _slowQueryThresholdMs;

        /// <param name="csvFilePath">Full path to the CSV file the profiler writes to.</param>
        /// <param name="slowQueryThresholdMs">
        /// Only commands taking at least this many milliseconds are logged.
        /// Set to 0 to log every command.
        /// </param>
        public QueryProfilingInterceptor(string csvFilePath, long slowQueryThresholdMs = 0)
        {
            _csvFilePath = csvFilePath;
            _slowQueryThresholdMs = slowQueryThresholdMs;
            EnsureHeader();
        }

        public override DbDataReader ReaderExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result)
        {
            WriteEntry(command, eventData, "Reader");
            return base.ReaderExecuted(command, eventData, result);
        }

        public override async ValueTask<DbDataReader> ReaderExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result,
            CancellationToken cancellationToken = default)
        {
            WriteEntry(command, eventData, "Reader");
            return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override object? ScalarExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            object? result)
        {
            WriteEntry(command, eventData, "Scalar");
            return base.ScalarExecuted(command, eventData, result);
        }

        public override async ValueTask<object?> ScalarExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            object? result,
            CancellationToken cancellationToken = default)
        {
            WriteEntry(command, eventData, "Scalar");
            return await base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override int NonQueryExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result)
        {
            WriteEntry(command, eventData, "NonQuery");
            return base.NonQueryExecuted(command, eventData, result);
        }

        public override async ValueTask<int> NonQueryExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            WriteEntry(command, eventData, "NonQuery");
            return await base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
        }

        private void WriteEntry(DbCommand command, CommandExecutedEventData eventData, string commandKind)
        {
            var durationMs = (long)eventData.Duration.TotalMilliseconds;
            if (durationMs < _slowQueryThresholdMs)
            {
                return;
            }

            var endTimeUtc = DateTime.UtcNow;
            var startTimeUtc = endTimeUtc - eventData.Duration;

            var row = string.Join(",",
                Csv(startTimeUtc.ToString("o", CultureInfo.InvariantCulture)),
                Csv(endTimeUtc.ToString("o", CultureInfo.InvariantCulture)),
                Csv(durationMs.ToString(CultureInfo.InvariantCulture)),
                Csv(commandKind),
                Csv(eventData.CommandId.ToString()),
                Csv(command.CommandText),
                Csv(FormatParameters(command)));

            lock (FileLock)
            {
                File.AppendAllText(_csvFilePath, row + Environment.NewLine, Encoding.UTF8);
            }
        }

        private static string FormatParameters(DbCommand command)
        {
            if (command.Parameters.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            for (var i = 0; i < command.Parameters.Count; i++)
            {
                var p = command.Parameters[i];
                if (i > 0)
                {
                    sb.Append("; ");
                }

                sb.Append(p.ParameterName)
                  .Append('=')
                  .Append(p.Value is null || p.Value == DBNull.Value
                      ? "NULL"
                      : Convert.ToString(p.Value, CultureInfo.InvariantCulture));
            }

            return sb.ToString();
        }

        private void EnsureHeader()
        {
            lock (FileLock)
            {
                var directory = Path.GetDirectoryName(_csvFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (!File.Exists(_csvFilePath))
                {
                    var header = string.Join(",",
                        "StartTimeUtc",
                        "EndTimeUtc",
                        "DurationMs",
                        "CommandKind",
                        "CommandId",
                        "CommandText",
                        "Parameters");

                    File.AppendAllText(_csvFilePath, header + Environment.NewLine, Encoding.UTF8);
                }
            }
        }

        // Escapes a value for safe inclusion in a CSV field.
        private static string Csv(string? value)
        {
            value ??= string.Empty;
            value = value.Replace("\"", "\"\"", StringComparison.Ordinal);
            return $"\"{value}\"";
        }
    }
}
