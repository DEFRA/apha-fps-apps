using System.Data;
using System.Data.Common;
using Apha.FPS.DataAccess.Data;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Apha.FPS.Api.TestTools
{
    /// <summary>
    /// Temporary, test-only controller that resets the 2026 Year End test data back to the
    /// pre-initiation baseline (see <see cref="YearEndResetSql"/>). Every action here is guarded
    /// by all three of: Environment = Development/local, YearEndTestTools:Enabled = true, and a live
    /// current_database() = 'batchjobs' check - any one failing throws UnauthorizedAccessException,
    /// which the existing ExceptionMiddleware turns into a 403 in the same ApiResponse envelope
    /// shape as every other failure in this API. This is deliberately redundant with the embedded
    /// script's own internal guard so a misconfigured environment fails closed at the API boundary
    /// before the script ever runs.
    /// Delete this controller, <see cref="YearEndResetSql"/> and <see cref="YearEndTestToolsOptions"/>
    /// once the real UI is no longer needed against the test database.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/yearend/test")]
    public class YearEndTestToolsController : ControllerBase
    {
        private const string RequiredDatabaseName = "batchjobs";

        // This repo uses a distinct ASPNETCORE_ENVIRONMENT="local" name (see Program.cs) with
        // its own appsettings.local.json, separate from the built-in "Development". IsDevelopment()
        // alone would not recognise it, so both must be checked - matching the existing convention.
        private static bool IsAllowedEnvironment(IHostEnvironment environment) =>
            environment.IsDevelopment() || environment.IsEnvironment("local");

        private readonly FpsDbContext _context;
        private readonly IHostEnvironment _environment;
        private readonly YearEndTestToolsOptions _options;
        private readonly ILogger<YearEndTestToolsController> _logger;

        public YearEndTestToolsController(
            FpsDbContext context,
            IHostEnvironment environment,
            IOptions<YearEndTestToolsOptions> options,
            ILogger<YearEndTestToolsController> logger)
        {
            _context = context;
            _environment = environment;
            _options = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// Tells the UI whether the reset button should render. Does not touch the database -
        /// only reflects the environment/config half of the gate, so a stale "enabled" response
        /// can never bypass the live database check the reset endpoint performs.
        /// </summary>
        [HttpGet("enabled")]
        public ActionResult<bool> IsEnabled()
        {
            return Ok(IsAllowedEnvironment(_environment) && _options.Enabled);
        }

        /// <summary>
        /// Resets the 2026 Year End test data to the pre-initiation baseline. Refuses unless
        /// Environment = Development AND YearEndTestTools:Enabled = true AND the live database
        /// is literally named 'batchjobs'.
        /// </summary>
        // Throws UnauthorizedAccessException (not Forbid()/Problem()) so the existing
        // ExceptionMiddleware produces the same ApiResponse<object> envelope shape every other
        // failure in this API produces - Forbid()'s empty body and a hand-rolled Problem() both
        // fall outside that convention and the client's ToApiResponse<T> parsing around it.
        [HttpPost("reset/2026")]
        public async Task<IActionResult> Reset2026(CancellationToken cancellationToken)
        {
            if (!IsAllowedEnvironment(_environment))
            {
                _logger.LogWarning(
                    "YearEnd test reset refused: environment is '{Environment}', not Development/local.",
                    _environment.EnvironmentName);
                throw new UnauthorizedAccessException("YearEnd test reset is not available in this environment.");
            }

            if (!_options.Enabled)
            {
                _logger.LogWarning("YearEnd test reset refused: YearEndTestTools:Enabled is false.");
                throw new UnauthorizedAccessException("YearEnd test reset tool is disabled.");
            }

            var connection = _context.Database.GetDbConnection();
            var wasClosed = connection.State != ConnectionState.Open;
            if (wasClosed)
            {
                await connection.OpenAsync(cancellationToken);
            }

            try
            {
                var currentDatabase = await GetCurrentDatabaseAsync(connection, cancellationToken);
                if (!string.Equals(currentDatabase, RequiredDatabaseName, StringComparison.Ordinal))
                {
                    _logger.LogWarning(
                        "YearEnd test reset refused: current_database() returned '{Database}', expected '{Required}'.",
                        currentDatabase, RequiredDatabaseName);
                    throw new UnauthorizedAccessException("YearEnd test reset refused: wrong database.");
                }

                var summary = await RunResetAsync(connection, cancellationToken);
                _logger.LogInformation("YearEnd test reset for 2026 completed. {@Summary}", summary);
                return Ok(true);
            }
            finally
            {
                if (wasClosed)
                {
                    await connection.CloseAsync();
                }
            }
        }

        private static async Task<string?> GetCurrentDatabaseAsync(DbConnection connection, CancellationToken cancellationToken)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT current_database()";
            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result?.ToString();
        }

        private static async Task<YearEndResetSummary?> RunResetAsync(DbConnection connection, CancellationToken cancellationToken)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = YearEndResetSql.Reset2026;
            command.CommandTimeout = 120;

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            // BEGIN/DO/COMMIT produce no result set; the trailing summary SELECT is the only
            // one that does, so step through result sets until we find it.
            YearEndResetSummary? summary = null;
            do
            {
                if (reader.FieldCount > 0 && await reader.ReadAsync(cancellationToken))
                {
                    summary = new YearEndResetSummary(
                        Year2025Open: Convert.ToInt64(reader["year_2025_open"]),
                        Year2026RowCount: Convert.ToInt64(reader["year_2026_row_count"]),
                        StagingRowsRemaining: Convert.ToInt64(reader["staging_rows_remaining"]),
                        Config2026RowsRemaining: Convert.ToInt64(reader["config_2026_rows_remaining"]),
                        YearEnd2026JobRowsRemaining: Convert.ToInt64(reader["yearend_2026_job_rows_remaining"]),
                        ActiveYearEndLocks: Convert.ToInt64(reader["active_yearend_locks"]));
                }
            } while (await reader.NextResultAsync(cancellationToken));

            return summary;
        }
    }

    /// <summary>
    /// Mirrors the final SELECT in <see cref="YearEndResetSql.Reset2026"/> - every field should
    /// read back a value proving the pre-initiation baseline was reached (2025 Open, no 2026 row,
    /// no staging/config/job residue, no active lock).
    /// </summary>
    public record YearEndResetSummary(
        long Year2025Open,
        long Year2026RowCount,
        long StagingRowsRemaining,
        long Config2026RowsRemaining,
        long YearEnd2026JobRowsRemaining,
        long ActiveYearEndLocks);
}
