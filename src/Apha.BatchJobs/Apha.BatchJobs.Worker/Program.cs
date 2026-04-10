using Apha.BatchJobs.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = ServiceCollectionSetup.CreateDefaultServices();
var serviceProvider = services.BuildServiceProvider();

try
{
    var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("BatchJobs.Startup");
    logger.LogInformation("Batch jobs application started");
    logger.LogInformation("Total services registered: {ServiceCount}", services.Count);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Fatal error during startup: {ex}");
    Environment.Exit(1);
}
finally
{
    await serviceProvider.DisposeAsync();
}
