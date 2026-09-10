using System.Collections.Concurrent;
using System.Globalization;
using System.Text;

namespace Apha.Common.Diagnostics
{
    /// <summary>
    /// Thread-safe in-memory store of recent connection-pool snapshots.
    /// Registered as a singleton so that even when the web server denies file-write
    /// permission, the diagnostics CSV can still be produced and downloaded straight
    /// from memory (no disk access required).
    /// </summary>
    public sealed class ConnectionPoolDiagnosticsStore
    {
        private const string HeaderLine = "TimestampUtc,DbPool,ApiPool";

        private readonly ConcurrentQueue<string> _rows = new();
        private readonly int _maxRows;

        public ConnectionPoolDiagnosticsStore(int maxRows = 5000)
        {
            _maxRows = Math.Max(100, maxRows);
        }

        /// <summary>Appends a snapshot and trims the buffer to the configured maximum.</summary>
        public void Add(DateTime timestampUtc, string dbPool, string apiPool)
        {
            var row = string.Join(",",
                Csv(timestampUtc.ToString("o", CultureInfo.InvariantCulture)),
                Csv(dbPool),
                Csv(apiPool));

            _rows.Enqueue(row);

            while (_rows.Count > _maxRows && _rows.TryDequeue(out _))
            {
                // Trim oldest rows to bound memory usage.
            }
        }

        /// <summary>Returns the full CSV content (header + buffered rows) for download.</summary>
        public string ToCsv()
        {
            var sb = new StringBuilder();
            sb.AppendLine(HeaderLine);
            foreach (var row in _rows.ToArray())
            {
                sb.AppendLine(row);
            }

            return sb.ToString();
        }

        public bool HasData => !_rows.IsEmpty;

        private static string Csv(string? value)
        {
            value ??= string.Empty;
            value = value.Replace("\"", "\"\"", StringComparison.Ordinal);
            return $"\"{value}\"";
        }
    }
}
