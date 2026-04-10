using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2.AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to execute sp_qryJobMonth_Final stored procedure logic.
    /// Inserts final project month data into ProjectMonthFinal table.
    /// Converts SQL Server stored procedure to PostgreSQL with exact logic preservation.
    /// </summary>
    public class QryJobMonthFinalService
    {
        private readonly string _connectionString;
        private readonly ILogger<QryJobMonthFinalService> _logger;
        private const int CommandTimeoutSeconds = 300;

        public QryJobMonthFinalService(
            string connectionString,
            ILogger<QryJobMonthFinalService> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes INSERT INTO projectmonthfinal with month parameter.
        /// Implements sp_qryJobMonth_Final stored procedure logic.
        /// Uses 300 second timeout and logs step start, end, duration with correlation id.
        /// </summary>
        /// <param name="month">Month parameter for filtering data</param>
        /// <param name="correlationId">Correlation ID for tracking execution</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of rows inserted</returns>
        public async Task<int> ExecuteAsync(int month, string correlationId, CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation(
                "[{CorrelationId}] QryJobMonthFinalService.ExecuteAsync started for month {Month} at {StartTime}",
                correlationId, month, startTime);

            try
            {
                // Use await using for proper async disposal in .NET 8
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                // Use await using for command disposal
                await using var command = connection.CreateCommand();
                command.CommandTimeout = CommandTimeoutSeconds;
                command.CommandType = CommandType.Text;

                // PostgreSQL uses lowercase table/column names by default unless quoted
                // Ensure consistent casing with database schema
                command.CommandText = @"
                    INSERT INTO projectmonthfinal (
                        project,
                        monthno,
                        costprofile,
                        subcontracts,
                        animals,
                        nonanimals,
                        timecosts,
                        transfercosts,
                        totalcost,
                        invoices,
                        coiw,
                        portsales,
                        cumcost,
                        cumprofile,
                        periodname,
                        sumofcostprofile,
                        cuminvoices,
                        cumcoiw,
                        cumportsales,
                        mstonedue,
                        due__done,
                        ontime,
                        sumofmstonedue,
                        sumofdue__done,
                        sumofontime,
                        cumflag,
                        cwdebit,
                        cwcredit,
                        cumcwdebit,
                        cumcwcredit,
                        totalhours,
                        cumtotalhours,
                        cumsubcontracts,
                        paycosts,
                        cumpaycosts
                    )
                    SELECT DISTINCT
                        pm2.project,
                        pm2.monthno,
                        pm2.costprofile,
                        pm2.subcontracts,
                        pm2.animals,
                        pm2.nonanimal AS nonanimals,
                        pm2.timecosts,
                        pm2.transfercosts,
                        pm2.totalcost,
                        pm2.invoices,
                        pm2.coiw,
                        pm2.portsales,
                        pm3.cumcost,
                        pm3.cumprofile,
                        pm3.periodname,
                        pm3.sumofcostprofile,
                        pm3.cuminvoices,
                        pm3.cumcoiw,
                        pm3.cumportsales,
                        pm2.mstonedue,
                        pm2.due__done,
                        pm2.ontime,
                        pm3.sumofmstonedue,
                        pm3.sumofdue__done,
                        pm3.sumofontime,
                        CASE WHEN pm3.endperiod = @month THEN 1 ELSE 0 END AS cumflag,
                        pmcw.cwdebit,
                        pmcw.cwcredit,
                        pm3.cumcwdebit,
                        pm3.cumcwcredit,
                        pm2.totalhours,
                        pm3.cumtotalhours,
                        pm3.cumsubcontracts,
                        pm2.paycosts,
                        pm3.cumpaycosts
                    FROM projectmonth2 pm2
                    LEFT JOIN projectmonth3 pm3 
                        ON pm2.project = pm3.project 
                        AND pm3.endperiod = @month
                    LEFT JOIN projectmonthcasework pmcw 
                        ON pm2.project = pmcw.project 
                        AND pm2.monthno = pmcw.monthno
                    WHERE pm2.monthno <= @month";

                // Use NpgsqlDbType for better type safety and performance
                command.Parameters.Add(new NpgsqlParameter("@month", NpgsqlTypes.NpgsqlDbType.Integer) { Value = month });

                var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation(
                    "[{CorrelationId}] QryJobMonthFinalService.ExecuteAsync completed for month {Month}. " +
                    "Rows inserted: {RowsAffected}. Duration: {Duration}ms. End time: {EndTime}",
                    correlationId, month, rowsAffected, duration.TotalMilliseconds, endTime);

                return rowsAffected;
            }
            catch (OperationCanceledException)
            {
                // Log cancellation separately for better observability
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogWarning(
                    "[{CorrelationId}] QryJobMonthFinalService.ExecuteAsync cancelled for month {Month}. " +
                    "Duration: {Duration}ms",
                    correlationId, month, duration.TotalMilliseconds);

                throw;
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(ex,
                    "[{CorrelationId}] QryJobMonthFinalService.ExecuteAsync failed for month {Month}. " +
                    "Duration: {Duration}ms. Error: {ErrorMessage}",
                    correlationId, month, duration.TotalMilliseconds, ex.Message);

                throw;
            }
        }
    }
}


**Key improvements made:**

1. **Async Disposal (.NET 8)**: Changed `using` to `await using` for `NpgsqlConnection` and `NpgsqlCommand` to properly support async disposal patterns in .NET 8.

2. **Type-Safe Parameters**: Replaced `AddWithValue` with explicit `NpgsqlParameter` using `NpgsqlDbType.Integer` for better type safety, performance, and to avoid potential type inference issues.

3. **Cancellation Handling**: Added separate catch block for `OperationCanceledException` to distinguish between cancellation and actual errors, improving observability in distributed systems like ECS Fargate.

4. **Resource Management**: Improved resource cleanup with async disposal patterns, which is important for connection pooling in containerized environments like ECS Fargate.

5. **Code Comments**: Added clarifying comment about PostgreSQL casing conventions to help future maintainers understand potential schema considerations.