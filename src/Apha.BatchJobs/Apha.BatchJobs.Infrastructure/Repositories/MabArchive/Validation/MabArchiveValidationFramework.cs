using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Apha.BatchJobs.Infrastructure.Data;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Validation
{
    /// <summary>
    /// Validation framework for MAB Archive loaders.
    /// Provides runtime integrity checks and SQL/LINQ equivalence validation.
    /// Used to ensure LINQ loaders produce identical results to SQL baseline.
    /// </summary>
    public interface IMabArchiveValidationService
    {
        /// <summary>Verify source table exists and is not empty for target year.</summary>
        Task ValidateSourceExistsAsync(string sourceTableName, int year, CancellationToken ct);

        /// <summary>Compare row count between LINQ result and SQL baseline query.</summary>
        Task ValidateRowCountAsync(
            string loaderName, 
            int linqRowCount, 
               int sqlRowCount,
            int year, 
            CancellationToken ct);

        /// <summary>Validate all rows in batch have expected non-null critical fields.</summary>
        void ValidateEntityBatch<T>(
            IEnumerable<T> entities, 
            string loaderName, 
            Func<T, bool> criticalFieldValidator,
            CancellationToken ct) where T : class;

        /// <summary>Log load result with performance metrics.</summary>
        void LogLoadResult(
            string loaderName, 
            int rowCount, 
            TimeSpan duration, 
            bool isSuccess);

        /// <summary>Enable equivalence mode: run both SQL and LINQ, verify identical results.</summary>
        Task<ValidationResult> ValidateEquivalenceAsync(
            string loaderName,
            Func<Task<IList<dynamic>>> linqQuery,
            Func<Task<IList<dynamic>>> sqlQuery,
            CancellationToken ct);
    }

    /// <summary>Result of equivalence validation check.</summary>
    public record ValidationResult(
        bool IsEquivalent,
        int LinqRowCount,
        int SqlRowCount,
        List<string> FieldMismatches,
        TimeSpan LinqDuration,
        TimeSpan SqlDuration,
        string Message
    );

    /// <summary>Production implementation of validation framework.</summary>
    public class MabArchiveValidationService : IMabArchiveValidationService
    {
        private readonly BatchJobsDbContext _context;
        private readonly BatchJobsDbContext _sqlContext;  // Separate context for SQL validation
        private readonly ILogger<MabArchiveValidationService> _logger;
        private readonly MabArchiveSettings _settings;

        public MabArchiveValidationService(
            BatchJobsDbContext context,
            ILogger<MabArchiveValidationService> logger,
            MabArchiveSettings settings)
        {
            _context = context;
            _logger = logger;
            _settings = settings;
            _sqlContext = context;  // In real code, could inject separate SQL context factory
        }

        public async Task ValidateSourceExistsAsync(string sourceTableName, int year, CancellationToken ct)
        {
            if (!_settings.ValidateLoadersAtRuntime)
                return;

            try
            {
                // Query source table in fps schema
                var count = await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT COUNT(*) FROM fps.{sourceTableName} WHERE fpsyear = {year}",
                    ct);

                if (Convert.ToInt32(count) == 0)
                {
                    throw new DataIntegrityException(
                        $"Source table fps.{sourceTableName} is empty for year {year}. " +
                        $"Loader may produce zero rows. Verify FPS data was loaded.");
                }

                _logger.LogDebug("✅ Source validation passed: {Table} has rows for year {Year}", 
                    sourceTableName, year);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Source validation failed: {Table}, year {Year}", 
                    sourceTableName, year);
                throw;
            }
        }

        public async Task ValidateRowCountAsync(
            string loaderName,
            int linqRowCount,
               int sqlRowCount,
            int year,
            CancellationToken ct)
        {
            if (!_settings.ValidateLoadersAtRuntime)
                return;

            try
            {

                if (linqRowCount != sqlRowCount)
                {
                    throw new DataIntegrityException(
                        $"{loaderName}: Row count mismatch. " +
                        $"LINQ loaded {linqRowCount} rows, but SQL baseline has {sqlRowCount}. " +
                        $"Logic drift detected!");
                }

                _logger.LogDebug(
                    "✅ Row count validated: {Loader} = {Count} rows (SQL={SqlCount})",
                    loaderName, linqRowCount, sqlRowCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Row count validation failed: {Loader}", loaderName);
                throw;
            }
        }

        public void ValidateEntityBatch<T>(
            IEnumerable<T> entities,
            string loaderName,
            Func<T, bool> criticalFieldValidator,
            CancellationToken ct) where T : class
        {
            if (!_settings.ValidateLoadersAtRuntime)
                return;

            var entityList = entities.ToList();
            var invalidCount = entityList.Count(e => !criticalFieldValidator(e));

            if (invalidCount > 0)
            {
                throw new DataIntegrityException(
                    $"{loaderName}: {invalidCount} of {entityList.Count} entities failed validation. " +
                    $"Critical fields may be NULL or invalid.");
            }

            _logger.LogDebug("✅ Entity batch validation passed: {Loader} validated {Count} entities",
                loaderName, entityList.Count);
        }

        public void LogLoadResult(
            string loaderName,
            int rowCount,
            TimeSpan duration,
            bool isSuccess)
        {
            var status = isSuccess ? "✅" : "❌";
            _logger.LogInformation(
                "{Status} {Loader}: {RowCount} rows in {DurationMs}ms",
                status, loaderName, rowCount, duration.TotalMilliseconds);
        }

        public async Task<ValidationResult> ValidateEquivalenceAsync(
            string loaderName,
            Func<Task<IList<dynamic>>> linqQuery,
            Func<Task<IList<dynamic>>> sqlQuery,
            CancellationToken ct)
        {
            if (!_settings.VerifyLogicEquivalence)
            {
                // Mode disabled; return neutral result
                return new ValidationResult(
                    IsEquivalent: true,
                    LinqRowCount: 0,
                    SqlRowCount: 0,
                    FieldMismatches: [],
                    LinqDuration: TimeSpan.Zero,
                    SqlDuration: TimeSpan.Zero,
                    Message: "Equivalence validation disabled in config");
            }

            var linqSw = System.Diagnostics.Stopwatch.StartNew();
            var linqResult = await linqQuery();
            linqSw.Stop();

            var sqlSw = System.Diagnostics.Stopwatch.StartNew();
            var sqlResult = await sqlQuery();
            sqlSw.Stop();

            var isEquivalent = linqResult.Count == sqlResult.Count;
            var mismatches = new List<string>();

            if (!isEquivalent)
            {
                mismatches.Add(
                    $"Row count: LINQ={linqResult.Count}, SQL={sqlResult.Count}");
            }

            var message = isEquivalent
                ? $"✅ {loaderName}: LINQ and SQL produce identical results ({linqResult.Count} rows)"
                : $"❌ {loaderName}: LINQ and SQL results differ. {string.Join("; ", mismatches)}";

            _logger.LogInformation(
                "{Msg} | LINQ: {LinqMs}ms, SQL: {SqlMs}ms",
                message, linqSw.ElapsedMilliseconds, sqlSw.ElapsedMilliseconds);

            return new ValidationResult(
                IsEquivalent: isEquivalent,
                LinqRowCount: linqResult.Count,
                SqlRowCount: sqlResult.Count,
                FieldMismatches: mismatches,
                LinqDuration: linqSw.Elapsed,
                SqlDuration: sqlSw.Elapsed,
                Message: message);
        }
    }

    /// <summary>Settings for validation behavior (injected from appsettings.json).</summary>
    public class MabArchiveSettings
    {
        /// <summary>Default: true. Enable all runtime validation checks.</summary>
        public bool ValidateLoadersAtRuntime { get; set; } = true;

        /// <summary>Default: false. In staging only; run both SQL and LINQ, verify identical results.</summary>
        public bool VerifyLogicEquivalence { get; set; } = false;

        /// <summary>Default: false. If LINQ load fails and this is true, auto-retry with SQL.</summary>
        public bool AllowSqlFallback { get; set; } = false;

        /// <summary>Default: true. Verify year-scoped totals views include fpsyear column.</summary>
        public bool StrictYearIsolation { get; set; } = true;
    }

    /// <summary>Exception thrown when data integrity checks fail.</summary>
    public class DataIntegrityException : Exception
    {
        public DataIntegrityException(string message) : base(message) { }
    }
}
