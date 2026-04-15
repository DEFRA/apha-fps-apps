using Microsoft.Extensions.Hosting;

namespace Apha.BatchJobs.Worker.Extensions;

/// <summary>
/// Configures worker services using the shared BatchJobs registration pattern.
/// </summary>
public static class ProgramExtension
{
    /// <summary>
    /// Registers worker dependencies for the batch jobs host.
    /// </summary>
    public static void ConfigureServices(this HostApplicationBuilder builder)
    {
        ServiceCollectionSetup.ConfigureBatchJobServices(builder.Services, builder.Configuration);
    }
}
