using Npgsql;
using System;
using System.IO;

namespace Apha.BatchJobs.DatabaseSetup
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=====================================");
            Console.WriteLine("  Batch Jobs Database Setup");
            Console.WriteLine("=====================================");
            Console.WriteLine();

            var host = "localhost";
            var port = "5432";
            var user = "postgres";
            Console.Write("Enter PostgreSQL password: ");
            var password = Console.ReadLine();

            // Connection to postgres database
            var postgresConnString = $"Host={host};Port={port};Database=postgres;Username={user};Password={password}";
            var batchjobsConnString = $"Host={host};Port={port};Database=batchjobs;Username={user};Password={password}";

            try
            {
                Console.WriteLine("Step 1: Creating batchjobs database...");
                using (var conn = new NpgsqlConnection(postgresConnString))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT 1 FROM pg_database WHERE datname = 'batchjobs'";
                        var result = cmd.ExecuteScalar();

                        if (result == null)
                        {
                            cmd.CommandText = "CREATE DATABASE batchjobs";
                            cmd.ExecuteNonQuery();
                            Console.WriteLine("  ✓ Database created");
                        }
                        else
                        {
                            Console.WriteLine("  ✓ Database already exists");
                        }
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Step 2: Creating schema and tables...");

                using (var conn = new NpgsqlConnection(batchjobsConnString))
                {
                    conn.Open();

                    var sqlFiles = new[] {
                        "001_batch_foundation_tables.sql",
                        "003_runtime_orchestrator_tables.sql"
                    };

                    foreach (var sqlFile in sqlFiles)
                    {
                        Console.WriteLine($"  Executing {sqlFile}...");
                        var sqlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "database", "sql", sqlFile);
                        if (File.Exists(sqlPath))
                        {
                            var sql = File.ReadAllText(sqlPath);
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = sql;
                                cmd.ExecuteNonQuery();
                            }
                            Console.WriteLine($"  ✓ {sqlFile} executed");
                        }
                    }
                }

                Console.WriteLine();
                Console.WriteLine("=====================================");
                Console.WriteLine("  Setup completed successfully!");
                Console.WriteLine("=====================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.WriteLine();
                Environment.Exit(1);
            }
        }
    }
}
