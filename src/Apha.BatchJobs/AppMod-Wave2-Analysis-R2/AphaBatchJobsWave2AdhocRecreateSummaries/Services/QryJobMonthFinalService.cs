using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to insert final project month data into ProjectMonthFinal table.
    /// Converts sp_qryJobMonth_Final stored procedure with month parameter, 300-second timeout and correlation-id logging.
    /// </summary>
    public class QryJobMonthFinalService
    {
        private readonly string _connectionString;
        private readonly ILogger<QryJobMonthFinalService> _logger;
        private const int CommandTimeoutSeconds = 300;

        public QryJobMonthFinalService(string connectionString, ILogger<QryJobMonthFinalService> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes INSERT INTO ProjectMonthFinal combining single month and cumulative data for specified month.
        /// </summary>
        /// <param name="month">Month parameter (1-12)</param>
        /// <param name="correlationId">Correlation ID for logging</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> ExecuteAsync(int month, string correlationId, CancellationToken cancellationToken = default)
        {
            const string stepName = "sp_qryJobMonth_Final";
            var startTime = DateTime.UtcNow;

            _logger.LogInformation("[{CorrelationId}] Step {StepName} started at {StartTime:O}", 
                correlationId, stepName, startTime);

            try
            {
                // Use await using for proper async disposal in .NET 8
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                const string insertQuery = @"
                    INSERT INTO ""ProjectMonthFinal"" (
                        ""Project"",
                        ""MonthNo"",
                        ""CostProfile"",
                        ""Subcontracts"",
                        ""Animals"",
                        ""NonAnimals"",
                        ""TimeCosts"",
                        ""TransferCosts"",
                        ""TotalCost"",
                        ""Invoices"",
                        ""COIW"",
                        ""PortSales"",
                        ""CumCost"",
                        ""CumProfile"",
                        ""PeriodName"",
                        ""SumOfCostProfile"",
                        ""CumInvoices"",
                        ""CumCOIW"",
                        ""CumPortSales"",
                        ""MstoneDue"",
                        ""Due__Done"",
                        ""OnTime"",
                        ""SumOfMstoneDue"",
                        ""SumOfDue__Done"",
                        ""SumOfOnTime"",
                        ""CumFlag"",
                        ""CWDebit"",
                        ""CWCredit"",
                        ""CumCWDebit"",
                        ""CumCWCredit"",
                        ""TotalHours"",
                        ""CumTotalHours"",
                        ""CumSubcontracts"",
                        ""PayCosts"",
                        ""CumPayCosts""
                    )
                    SELECT 
                        pm2.""Project"",
                        pm2.""MonthNo"",
                        pm2.""CostProfile"",
                        pm2.""Subcontracts"",
                        pm2.""Animals"",
                        pm2.""NonAnimal"" AS ""NonAnimals"",
                        pm2.""TimeCosts"",
                        pm2.""TransferCosts"",
                        pm2.""TotalCost"",
                        pm2.""Invoices"",
                        pm2.""COIW"",
                        pm2.""PortSales"",
                        pm3.""CumCost"",
                        pm3.""CumProfile"",
                        pm3.""PeriodName"",
                        pm2.""SumOfCostProfile"",
                        pm3.""CumInvoices"",
                        pm3.""CumCOIW"",
                        pm3.""CumPortSales"",
                        pm2.""MstoneDue"",
                        pm2.""Due__Done"",
                        pm2.""OnTime"",
                        pm3.""SumOfMstoneDue"",
                        pm3.""SumOfDue__Done"",
                        pm3.""SumOfOnTime"",
                        CASE WHEN pm3.""EndPeriod"" = $1 THEN 1 ELSE 0 END AS ""CumFlag"",
                        pmc.""CWDebit"",
                        pmc.""CWCredit"",
                        pm3.""CumCWDebit"",
                        pm3.""CumCWCredit"",
                        pm2.""TotalHours"",
                        pm3.""CumTotalHours"",
                        pm3.""CumSubcontracts"",
                        pm2.""PayCosts"",
                        pm3.""CumPayCosts""
                    FROM ""ProjectMonth2"" pm2
                    LEFT JOIN ""ProjectMonth3"" pm3 
                        ON pm2.""Project"" = pm3.""Project"" 
                        AND pm3.""EndPeriod"" = $1
                    LEFT JOIN ""ProjectMonthCasework"" pmc 
                        ON pm2.""Project"" = pmc.""Project"" 
                        AND pm2.""MonthNo"" = pmc.""MonthNo""
                    WHERE pm2.""MonthNo"" = $1";

                // Use await using for proper async disposal
                await using var command = new NpgsqlCommand(insertQuery, connection)
                {
                    CommandTimeout = CommandTimeoutSeconds
                };
                
                // Use positional parameters ($1) for PostgreSQL best practice
                command.Parameters.Add(new NpgsqlParameter { Value = month });

                // Npgsql handles cancellation internally; no need for separate CancellationTokenSource
                var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation(
                    "[{CorrelationId}] Step {StepName} completed at {EndTime:O}. Duration: {Duration}ms. Rows affected: {RowsAffected}",
                    correlationId, stepName, endTime, duration.TotalMilliseconds, rowsAffected);

                return true;
            }
            catch (OperationCanceledException ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogWarning(ex,
                    "[{CorrelationId}] Step {StepName} was cancelled after {Duration}ms",
                    correlationId, stepName, duration.TotalMilliseconds);

                throw;
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(ex,
                    "[{CorrelationId}] Step {StepName} failed at {EndTime:O}. Duration: {Duration}ms",
                    correlationId, stepName, endTime, duration.TotalMilliseconds);

                return false;
            }
        }
    }
}


**Key improvements made:**

1. **Async disposal**: Changed `using` to `await using` for `NpgsqlConnection` and `NpgsqlCommand` - proper .NET 8 async pattern
2. **PostgreSQL positional parameters**: Changed from `@Month` to `$1` which is the PostgreSQL native parameter syntax and more efficient
3. **Removed redundant CancellationTokenSource**: Npgsql handles cancellation internally via the passed `cancellationToken`, creating a linked token source is unnecessary overhead
4. **Const for query string**: Made `insertQuery` a const for better performance
5. **Const for stepName**: Changed to const since it never changes
6. **ISO 8601 datetime formatting**: Added `:O` format specifier for consistent datetime logging
7. **OperationCanceledException logging**: Changed from `LogError` to `LogWarning` as cancellation is expected behavior, not an error
8. **Removed redundant error message**: Removed `ex.Message` from log since the exception is already being logged
9. **Simplified parameter addition**: Used object initializer for cleaner code