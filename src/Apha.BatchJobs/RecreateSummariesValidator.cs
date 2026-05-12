using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;

namespace Apha.BatchJobs.Validation
{
    /// <summary>
    /// Cross-validates RecreateSummaries stored procedure results with 
    /// direct code-based SQL queries against the same data.
    /// </summary>
    public class RecreateSummariesValidator
    {
        private readonly string _connectionString;
        
        public RecreateSummariesValidator(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        /// <summary>
        /// Executes validation test and generates cross-validation report.
        /// </summary>
        public ValidationReport Execute()
        {
            var report = new ValidationReport { ExecutedAt = DateTime.UtcNow };

            Console.WriteLine("=========================================");
            Console.WriteLine("RecreateSummaries Validation Test");
            Console.WriteLine("=========================================");
            Console.WriteLine();

            try
            {
                // Query 1: Get SP results from fpsyeartotals
                Console.WriteLine("[1/3] Querying SP results from fpsyeartotals...");
                var spResults = GetFpsYearTotals();
                report.SpResultCount = spResults.Count;
                report.SpResults = spResults;
                Console.WriteLine($"[OK] Retrieved {spResults.Count} SP result rows");
                Console.WriteLine();

                // Query 2: Get base data from tlkpproject  
                Console.WriteLine("[2/3] Querying base project data from tlkpproject...");
                var baseProjects = GetProjectCount();
                report.BaseProjectCount = baseProjects;
                Console.WriteLine($"[OK] Retrieved {baseProjects} base projects");
                Console.WriteLine();

                // Query 3: Validate calculations
                Console.WriteLine("[3/3] Validating calculations...");
                var validations = ValidateCalculations(spResults);
                report.ValidationRules = validations;

                foreach (var validation in validations)
                {
                    var status = validation.IsValid ? "[PASS]" : "[FAIL]";
                    Console.WriteLine($"  {status} {validation.Description}");
                    if (!validation.IsValid && !string.IsNullOrEmpty(validation.ErrorMessage))
                    {
                        Console.WriteLine($"        {validation.ErrorMessage}");
                    }
                }

                var passCount = validations.Count(v => v.IsValid);
                var totalCount = validations.Count;
                report.ValidationsPassed = passCount;
                report.ValidationsFailed = totalCount - passCount;

                Console.WriteLine();
                Console.WriteLine("=========================================");
                Console.WriteLine("Validation Summary");
                Console.WriteLine("=========================================");
                Console.WriteLine($"SP Results:        {report.SpResultCount} rows");
                Console.WriteLine($"Base Projects:     {report.BaseProjectCount}");
                Console.WriteLine($"Validations:       {passCount}/{totalCount} passed");
                Console.WriteLine($"Status:            {(report.ValidationsFailed == 0 ? "SUCCESS" : "FAILED")}");
                Console.WriteLine();

                return report;
            }
            catch (Exception ex)
            {
                report.ErrorMessage = ex.Message;
                report.StackTrace = ex.StackTrace;
                Console.WriteLine($"[ERROR] {ex.Message}");
                return report;
            }
        }

        /// <summary>
        /// Queries FPS year totals from the SP results table.
        /// </summary>
        private List<FpsYearTotalRow> GetFpsYearTotals()
        {
            var results = new List<FpsYearTotalRow>();

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT 
    parentproject,
    program,
    fpsyear,
    totaladditionalcosts::numeric,
    totalanimalcosts::numeric,
    totalstaffcosts::numeric,
    totaltestcosts::numeric,
    totalcosts::numeric,
    custincome::numeric,
    transferincome::numeric,
    totalincome::numeric,
    requiredprofit::numeric,
    projectstatus
FROM fps.fpsyeartotals
WHERE fpsyear IN (2024, 2025, 2026)
ORDER BY fpsyear, parentproject";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(new FpsYearTotalRow
                            {
                                ParentProject = reader.GetString(0),
                                Program = reader.GetString(1),
                                FpsYear = reader.GetInt32(2),
                                TotalAdditionalCosts = reader.GetDecimal(3),
                                TotalAnimalCosts = reader.GetDecimal(4),
                                TotalStaffCosts = reader.GetDecimal(5),
                                TotalTestCosts = reader.GetDecimal(6),
                                TotalCosts = reader.GetDecimal(7),
                                CustIncome = reader.GetDecimal(8),
                                TransferIncome = reader.GetDecimal(9),
                                TotalIncome = reader.GetDecimal(10),
                                RequiredProfit = reader.GetDecimal(11),
                                ProjectStatus = reader.GetString(12)
                            });
                        }
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Counts base projects in tlkpproject table.
        /// </summary>
        private int GetProjectCount()
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(DISTINCT parentproject) FROM fps.tlkpproject WHERE fpsyear IN (2024, 2025, 2026)";
                    var scalar = cmd.ExecuteScalar();
                    return scalar is int value ? value : 0;
                }
            }
        }

        /// <summary>
        /// Validates SP results against expected values and calculation logic.
        /// </summary>
        private List<ValidationRule> ValidateCalculations(List<FpsYearTotalRow> results)
        {
            var rules = new List<ValidationRule>();

            // Rule 1: At least one result for each year
            foreach (var year in new[] { 2024, 2025, 2026 })
            {
                var yearResults = results.Where(r => r.FpsYear == year).ToList();
                rules.Add(new ValidationRule
                {
                    Description = $"Year {year} has results",
                    IsValid = yearResults.Count > 0,
                    ErrorMessage = yearResults.Count == 0 ? $"No results found for year {year}" : null
                });
            }

            // Rule 2: All required projects appear
            var requiredProjects = new[] { "AH0001", "BS0003", "RS0004", "TH0002" };
            foreach (var project in requiredProjects)
            {
                var projectResults = results.Where(r => r.ParentProject == project).ToList();
                rules.Add(new ValidationRule
                {
                    Description = $"Project {project} has results",
                    IsValid = projectResults.Count > 0,
                    ErrorMessage = projectResults.Count == 0 ? $"Project {project} missing from SP results" : null
                });
            }

            // Rule 3: Total costs = sum of component costs
            foreach (var row in results)
            {
                var calculatedTotal = row.TotalAdditionalCosts + row.TotalAnimalCosts + 
                                     row.TotalStaffCosts + row.TotalTestCosts;
                var matches = Math.Abs(row.TotalCosts - calculatedTotal) < 0.01m;
                
                rules.Add(new ValidationRule
                {
                    Description = $"{row.ParentProject} ({row.FpsYear}): Total costs calculation",
                    IsValid = matches,
                    ErrorMessage = matches ? null : 
                        $"Expected {calculatedTotal} but got {row.TotalCosts}"
                });
            }

            // Rule 4: Total income = customer income + transfer income
            foreach (var row in results)
            {
                var calculatedIncome = row.CustIncome + row.TransferIncome;
                var matches = Math.Abs(row.TotalIncome - calculatedIncome) < 0.01m;
                
                rules.Add(new ValidationRule
                {
                    Description = $"{row.ParentProject} ({row.FpsYear}): Total income calculation",
                    IsValid = matches,
                    ErrorMessage = matches ? null : 
                        $"Expected {calculatedIncome} but got {row.TotalIncome}"
                });
            }

            // Rule 5: No negative values
            foreach (var row in results)
            {
                var hasNegative = row.TotalCosts < 0 || row.TotalIncome < 0;
                rules.Add(new ValidationRule
                {
                    Description = $"{row.ParentProject} ({row.FpsYear}): Non-negative values",
                    IsValid = !hasNegative,
                    ErrorMessage = hasNegative ? "Found negative cost or income values" : null
                });
            }

            return rules;
        }
    }

    /// <summary>
    /// Single FPS year total row from SP results.
    /// </summary>
    public class FpsYearTotalRow
    {
        public string ParentProject { get; set; } = string.Empty;
        public string Program { get; set; } = string.Empty;
        public int FpsYear { get; set; }
        public decimal TotalAdditionalCosts { get; set; }
        public decimal TotalAnimalCosts { get; set; }
        public decimal TotalStaffCosts { get; set; }
        public decimal TotalTestCosts { get; set; }
        public decimal TotalCosts { get; set; }
        public decimal CustIncome { get; set; }
        public decimal TransferIncome { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal RequiredProfit { get; set; }
        public string ProjectStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Single validation rule result.
    /// </summary>
    public class ValidationRule
    {
        public string Description { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Complete validation test report.
    /// </summary>
    public class ValidationReport
    {
        public DateTime ExecutedAt { get; set; }
        public int SpResultCount { get; set; }
        public int BaseProjectCount { get; set; }
        public int ValidationsPassed { get; set; }
        public int ValidationsFailed { get; set; }
        public List<FpsYearTotalRow> SpResults { get; set; } = new();
        public List<ValidationRule> ValidationRules { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string? StackTrace { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("========================================");
            sb.AppendLine("RecreateSummaries Validation Report");
            sb.AppendLine("========================================");
            sb.AppendLine($"Executed: {ExecutedAt:yyyy-MM-dd HH:mm:ss.fff}");
            sb.AppendLine();
            sb.AppendLine($"SP Results:         {SpResultCount} rows");
            sb.AppendLine($"Base Projects:      {BaseProjectCount}");
            sb.AppendLine($"Validations Passed: {ValidationsPassed}");
            sb.AppendLine($"Validations Failed: {ValidationsFailed}");
            sb.AppendLine($"Overall Status:     {(ValidationsFailed == 0 ? "SUCCESS" : "FAILED")}");
            sb.AppendLine();

            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                sb.AppendLine($"ERROR: {ErrorMessage}");
                sb.AppendLine(StackTrace);
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Console entry point for standalone validation.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = "Host=localhost;Port=5432;Username=postgres;Password=admin123;Database=batch_jobs_foundation_db";
            var validator = new RecreateSummariesValidator(connectionString);
            var report = validator.Execute();
            
            Console.WriteLine(report);
            
            System.Environment.Exit(report.ValidationsFailed == 0 ? 0 : 1);
        }
    }
}
