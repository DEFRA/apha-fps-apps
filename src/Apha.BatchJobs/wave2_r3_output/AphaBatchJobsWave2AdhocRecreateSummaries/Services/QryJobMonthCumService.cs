using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to execute cumulative month job query inserting aggregated cumulative data into ProjectMonth3 table via PostgreSQL.
    /// Implements INSERT INTO ProjectMonth3 with SUM aggregations joining tblPeriod, tblkPeriodMonth, ProjectMonth2, ProjectMonthCasework.
    /// Converts sp_qryJobMonthCum stored procedure logic to PostgreSQL-compatible command execution.
    /// </summary>
    public class QryJobMonthCumService
    {
        private readonly string _connectionString;
        private readonly ILogger<QryJobMonthCumService> _logger;
        private const int CommandTimeoutSeconds = 300;

        public QryJobMonthCumService(string connectionString, ILogger<QryJobMonthCumService> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes INSERT INTO ProjectMonth3 with cumulative aggregations from ProjectMonth2 and ProjectMonthCasework.
        /// Joins tblPeriod, tblkPeriodMonth, ProjectMonth2, and ProjectMonthCasework to compute cumulative metrics.
        /// Returns true on success, false on failure.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for logging and tracing</param>
        /// <param name="cancellationToken">Cancellation token for timeout enforcement</param>
        /// <returns>True if execution succeeded, false otherwise</returns>
        public async Task<bool> ExecuteAsync(string correlationId, CancellationToken cancellationToken)
        {
            const string stepName = "sp_qryJobMonthCum";
            var startTime = DateTime.UtcNow;

            _logger.LogInformation(
                "[{CorrelationId}] Step {StepName} started at {StartTime}",
                correlationId,
                stepName,
                startTime);

            try
            {
                // Use await using for proper async disposal in .NET 8
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                // SQL query with proper formatting and indentation
                const string sql = @"
                    INSERT INTO ""ProjectMonth3""
                    (
                        ""EndPeriod"",
                        ""PeriodName"",
                        ""Project"",
                        ""CumCost"",
                        ""CumInvoices"",
                        ""CumCOIW"",
                        ""CumPortSales"",
                        ""CumProfile"",
                        ""SumOfCostProfile"",
                        ""SumOfMstoneDue"",
                        ""SumOfDue__Done"",
                        ""SumOfOnTime"",
                        ""CumCWDebit"",
                        ""CumCWCredit"",
                        ""CumTotalHours"",
                        ""CumSubcontracts"",
                        ""CumTestCosts"",
                        ""CumPayCosts""
                    )
                    SELECT DISTINCT 
                        tp.""EndPeriod"",
                        tp.""PeriodName"",
                        pm2.""Project"",
                        SUM(pm2.""TotalCost"") AS ""CumCost"",
                        SUM(pm2.""Invoices"") AS ""CumInvoices"",
                        SUM(pm2.""COIW"") AS ""CumCOIW"",
                        SUM(pm2.""PortSales"") AS ""CumPortSales"",
                        SUM(pm2.""CostProfile"") AS ""CumProfile"",
                        pm2.""SumOfCostProfile"",
                        SUM(pm2.""MstoneDue"") AS ""SumOfMstoneDue"",
                        SUM(pm2.""Due__Done"") AS ""SumOfDue__Done"",
                        SUM(pm2.""OnTime"") AS ""SumOfOnTime"",
                        SUM(pmcw.""CWDebit"") AS ""CumCWDebit"",
                        SUM(pmcw.""CWCredit"") AS ""CumCWCredit"",
                        SUM(pm2.""TotalHours"") AS ""CumTotalHours"",
                        SUM(pm2.""Subcontracts"") AS ""CumSubcontracts"",
                        SUM(pm2.""TransferCosts"") AS ""CumTestCosts"",
                        SUM(pm2.""PayCosts"") AS ""CumPayCosts""
                    FROM 
                        ""tblPeriod"" tp
                        INNER JOIN ""tblkPeriodMonth"" tpm ON tp.""PeriodName"" = tpm.""PeriodName""
                        INNER JOIN ""ProjectMonth2"" pm2 ON tpm.""MonthNo"" = pm2.""MonthNo""
                        INNER JOIN ""ProjectMonthCasework"" pmcw ON pm2.""MonthNo"" = pmcw.""MonthNo""
                            AND pm2.""Project"" = pmcw.""Project""
                    GROUP BY 
                        tp.""EndPeriod"",
                        tp.""PeriodName"",
                        pm2.""Project"",
                        pm2.""SumOfCostProfile""";

                // Use await using for proper async disposal in .NET 8
                await using var command = new NpgsqlCommand(sql, connection)
                {
                    CommandTimeout = CommandTimeoutSeconds,
                    CommandType = CommandType.Text
                };

                // Removed redundant CancellationTokenSource - NpgsqlCommand.CommandTimeout handles timeout
                // Pass the original cancellationToken directly for proper cancellation handling
                var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation(
                    "[{CorrelationId}] Step {StepName} completed successfully at {EndTime}. Duration: {Duration}ms. Rows affected: {RowsAffected}",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds,
                    rowsAffected);

                return true;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogWarning(
                    "[{CorrelationId}] Step {StepName} was cancelled at {EndTime}. Duration: {Duration}ms",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds);

                return false;
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} failed at {EndTime}. Duration: {Duration}ms",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds);

                return false;
            }
        }
    }
}


**Key improvements made:**

1. **Async disposal (.NET 8)**: Changed `using` to `await using` for `NpgsqlConnection` and `NpgsqlCommand` to properly leverage async disposal patterns in .NET 8.

2. **Removed redundant timeout handling**: Eliminated the manual `CancellationTokenSource` with timeout since `NpgsqlCommand.CommandTimeout` already handles command-level timeouts. This avoids double timeout management and simplifies the code.

3. **Const for local variables**: Changed `stepName` to `const` since it's a compile-time constant, and made `sql` const for better performance.

4. **Improved SQL formatting**: Removed unnecessary parentheses around the FROM clause tables for cleaner PostgreSQL syntax.

5. **Better logging level**: Changed cancellation log from `LogError` to `LogWarning` since cancellation is an expected operational scenario, not an error.

6. **Removed redundant error message**: Removed `ex.Message` from the error log since the exception object already contains this information and structured logging will capture it.