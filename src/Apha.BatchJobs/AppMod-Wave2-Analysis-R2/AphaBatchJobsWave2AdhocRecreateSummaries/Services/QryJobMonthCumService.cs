using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to insert cumulative project month data into ProjectMonth3 table.
    /// Converts sp_qryJobMonthCum stored procedure with complex aggregations joining 
    /// tblPeriod, tblkPeriodMonth, ProjectMonth2, ProjectMonthCasework.
    /// Enforces 300-second timeout and correlation-id logging.
    /// </summary>
    public class QryJobMonthCumService
    {
        private readonly string _connectionString;
        private readonly ILogger<QryJobMonthCumService> _logger;
        private const int CommandTimeoutSeconds = 300;

        public QryJobMonthCumService(
            string connectionString,
            ILogger<QryJobMonthCumService> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes INSERT INTO ProjectMonth3 with SELECT performing SUM aggregations 
        /// on ProjectMonth2 and ProjectMonthCasework joined with tblPeriod and tblkPeriodMonth,
        /// grouped by EndPeriod, PeriodName, Project, SumOfCostProfile.
        /// Uses parameterized PostgreSQL command with 300-second timeout.
        /// Logs step start, end, duration with correlation id.
        /// Returns success/failure status.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for tracking execution</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> ExecuteAsync(string correlationId, CancellationToken cancellationToken = default)
        {
            const string stepName = "sp_qryJobMonthCum";
            var startTime = DateTime.UtcNow;

            _logger.LogInformation(
                "[{CorrelationId}] Step {StepName} started at {StartTime:O}",
                correlationId,
                stepName,
                startTime);

            try
            {
                // Use await using for proper async disposal in .NET 8
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                // SQL query - removed DISTINCT as it's redundant with GROUP BY and can cause performance issues
                // DISTINCT with GROUP BY is typically unnecessary and adds overhead
                var insertSql = @"
                    INSERT INTO ""ProjectMonth3"" (
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
                    SELECT 
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
                        SUM(pmc.""CWDebit"") AS ""CumCWDebit"",
                        SUM(pmc.""CWCredit"") AS ""CumCWCredit"",
                        SUM(pm2.""TotalHours"") AS ""CumTotalHours"",
                        SUM(pm2.""Subcontracts"") AS ""CumSubcontracts"",
                        SUM(pm2.""TransferCosts"") AS ""CumTestCosts"",
                        SUM(pm2.""PayCosts"") AS ""CumPayCosts""
                    FROM ""tblPeriod"" tp
                    INNER JOIN ""tblkPeriodMonth"" tpm ON tp.""PeriodName"" = tpm.""PeriodName""
                    INNER JOIN ""ProjectMonth2"" pm2 ON tpm.""MonthNo"" = pm2.""MonthNo""
                    INNER JOIN ""ProjectMonthCasework"" pmc ON pm2.""MonthNo"" = pmc.""MonthNo"" 
                        AND pm2.""Project"" = pmc.""Project""
                    GROUP BY 
                        tp.""EndPeriod"",
                        tp.""PeriodName"",
                        pm2.""Project"",
                        pm2.""SumOfCostProfile""";

                // Use await using for proper async disposal in .NET 8
                await using var command = new NpgsqlCommand(insertSql, connection)
                {
                    CommandTimeout = CommandTimeoutSeconds,
                    CommandType = CommandType.Text
                };

                // Npgsql respects CommandTimeout, so manual CancellationTokenSource is redundant
                // The cancellationToken parameter already handles cancellation
                var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation(
                    "[{CorrelationId}] Step {StepName} completed successfully at {EndTime:O}. Duration: {Duration}ms. Rows affected: {RowsAffected}",
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
                    "[{CorrelationId}] Step {StepName} was cancelled at {EndTime:O}. Duration: {Duration}ms",
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
                    "[{CorrelationId}] Step {StepName} failed at {EndTime:O}. Duration: {Duration}ms",
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

1. **Replaced `using` with `await using`**: .NET 8 best practice for async disposal of `NpgsqlConnection` and `NpgsqlCommand`
2. **Removed redundant DISTINCT**: When using GROUP BY, DISTINCT is unnecessary and adds performance overhead
3. **Removed manual timeout CancellationTokenSource**: Npgsql's `CommandTimeout` property already handles timeouts; creating a linked CancellationTokenSource was redundant
4. **Changed `const string stepName`**: Made it a const instead of var for better performance (compile-time constant)
5. **Changed cancellation log level**: Changed from `LogError` to `LogWarning` for cancellation - cancellation is expected behavior, not an error
6. **Added ISO 8601 format specifier**: Used `:O` format for DateTime logging for better consistency and parseability
7. **Removed redundant error message**: Removed `ex.Message` from log since the exception object already contains this information