using System.Data.Common;
using System.Globalization;
using System.Text;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.DataAccess.Logging;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Apha.Costbook.DataAccess.Interceptors
{
    /// <summary>
    /// Centralised EF Core command interceptor that captures every executed SQL command
    /// (query text, parameters, start time, end time and total execution duration) and
    /// hands it to the background <see cref="IPerformanceLogQueue"/> for persistence to the
    /// <c>performance_log</c> table. All I/O happens off the query hot path.
    /// </summary>
    public sealed class QueryProfilingInterceptor : DbCommandInterceptor
    {
        private const string LogTypeQuery = "Query";
        private readonly IPerformanceLogQueue _queue;
        private readonly long _slowQueryThresholdMs;

        /// <param name="queue">Background queue that persists entries to the database.</param>
        /// <param name="slowQueryThresholdMs">
        /// Only commands taking at least this many milliseconds are logged.
        /// Set to 0 to log every command.
        /// </param>
        public QueryProfilingInterceptor(IPerformanceLogQueue queue, long slowQueryThresholdMs = 0)
        {
            _queue = queue;
            _slowQueryThresholdMs = slowQueryThresholdMs;
        }

        public override DbDataReader ReaderExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result)
        {
            Enqueue(command, eventData, "Reader");
            return base.ReaderExecuted(command, eventData, result);
        }

        public override async ValueTask<DbDataReader> ReaderExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result,
            CancellationToken cancellationToken = default)
        {
            Enqueue(command, eventData, "Reader");
            return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override object? ScalarExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            object? result)
        {
            Enqueue(command, eventData, "Scalar");
            return base.ScalarExecuted(command, eventData, result);
        }

        public override async ValueTask<object?> ScalarExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            object? result,
            CancellationToken cancellationToken = default)
        {
            Enqueue(command, eventData, "Scalar");
            return await base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override int NonQueryExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result)
        {
            Enqueue(command, eventData, "NonQuery");
            return base.NonQueryExecuted(command, eventData, result);
        }

        public override async ValueTask<int> NonQueryExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            Enqueue(command, eventData, "NonQuery");
            return await base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
        }

        private void Enqueue(DbCommand command, CommandExecutedEventData eventData, string commandKind)
        {
            var durationMs = (long)eventData.Duration.TotalMilliseconds;
            if (durationMs < _slowQueryThresholdMs)
            {
                return;
            }

            // Guard against infinite recursion: never log the writes to the log table itself.
            if (command.CommandText.Contains("performance_log", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var endTimeUtc = DateTime.UtcNow;
            var startTimeUtc = endTimeUtc - eventData.Duration;

            _queue.Enqueue(new PerformanceLog
            {
                LogType = LogTypeQuery,
                StartTimeUtc = startTimeUtc,
                EndTimeUtc = endTimeUtc,
                DurationMs = durationMs,
                CommandKind = commandKind,
                CommandId = eventData.CommandId.ToString(),
                CommandText = command.CommandText,
                Parameters = FormatParameters(command),
                CreatedAt = endTimeUtc
            });
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
    }
}
