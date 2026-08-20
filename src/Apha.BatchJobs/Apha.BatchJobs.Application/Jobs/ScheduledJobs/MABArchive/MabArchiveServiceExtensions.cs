using Apha.BatchJobs.Domain.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive;

public static class MabArchiveServiceExtensions
{
    public static IServiceCollection AddMabArchiveJob(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MabArchiveSettings>(configuration.GetSection("MabArchive"));
        services.Configure<AwsLoggingSettings>(configuration.GetSection("AwsLogging"));

        return services;
    }
}
