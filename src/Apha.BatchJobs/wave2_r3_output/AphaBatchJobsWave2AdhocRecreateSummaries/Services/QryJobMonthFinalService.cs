using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace AphaBatchJobsWave2AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to execute final month job query inserting comprehensive project month data 
    /// into ProjectMonthFinal table via PostgreSQL with month parameter.
    /// Implements INSERT INTO ProjectMonthFinal with all 34 fields from joined and aggregated 
    /// source data filtered by month parameter.
    /// </summary>
    public interface IQryJobMonthFinalService
    {
        /// <summary>
        /// Executes INSERT INTO ProjectMonthFinal with data from joined ProjectMonth2 and ProjectMonth3 
        /// tables filtered by month parameter using parameterized PostgreSQL command with 300-second timeout.
        /// </summary>
        /// <param name="month">Month number (1-12) to filter data</param>
        /// <param name="correlationId">Correlation ID for logging</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> ExecuteAsync(int month, string correlationId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Implementation of IQryJobMonthFinalService for executing final month job query.
    /// Converts sp_qryJobMonth_Final stored procedure logic to PostgreSQL-compatible command execution.
    /// </summary>
    public class QryJobMonthFinalService : IQryJobMonthFinalService
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
        /// Executes INSERT INTO ProjectMonthFinal with comprehensive project month data.
        /// Joins ProjectMonth2 and ProjectMonth3 tables to populate all 34 fields.
        /// </summary>
        public async Task<bool> ExecuteAsync(int month, string correlationId, CancellationToken cancellationToken = default)
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

                // SQL query remains the same but with proper formatting
                command.CommandText = @"
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
                    SELECT DISTINCT
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
                        CASE WHEN pm3.""EndPeriod"" = @Month THEN 1 ELSE 0 END AS ""CumFlag"",
                        pmcw.""CWDebit"",
                        pmcw.""CWCredit"",
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
                        AND pm3.""EndPeriod"" = @Month
                    LEFT JOIN ""ProjectMonthCasework"" pmcw 
                        ON pm2.""Project"" = pmcw.""Project"" 
                        AND pm2.""MonthNo"" = pmcw.""MonthNo""
                    WHERE pm2.""MonthNo"" = @Month";

                // Use explicit NpgsqlDbType for better type safety and performance
                command.Parameters.Add(new NpgsqlParameter("@Month", NpgsqlDbType.Integer) { Value = month });

                var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation(
                    "[{CorrelationId}] QryJobMonthFinalService.ExecuteAsync completed successfully for month {Month}. " +
                    "Rows affected: {RowsAffected}, Duration: {Duration}ms",
                    correlationId, month, rowsAffected, duration.TotalMilliseconds);

                return true;
            }
            catch (OperationCanceledException)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogWarning(
                    "[{CorrelationId}] QryJobMonthFinalService.ExecuteAsync was cancelled for month {Month} after {Duration}ms",
                    correlationId, month, duration.TotalMilliseconds);
                throw;
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(
                    ex,
                    "[{CorrelationId}] QryJobMonthFinalService.ExecuteAsync failed for month {Month} after {Duration}ms",
                    correlationId, month, duration.TotalMilliseconds);
                return false;
            }
        }
    }
}


// Key improvements made:
// 1. Changed 'using' to 'await using' for NpgsqlConnection and NpgsqlCommand - proper async disposal pattern in .NET 8
// 2. Used explicit NpgsqlParameter with NpgsqlDbType.Integer for better type safety and performance
// 3. Changed LogError to LogWarning for OperationCanceledException - cancellation is not an error condition
// 4. Removed redundant error message from LogError (already included via structured logging)
// 5. Maintained all existing functionality without adding new features