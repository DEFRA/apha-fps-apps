using Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;

public static class RecreateSummariesServiceExtensions
{
    public static IServiceCollection AddRecreateSummariesJob(
        this IServiceCollection services)
    {
        // SQL-backed step catalogs are retired; LINQ is the only active implementation.
        services.AddScoped<IRecreateSummariesStepCatalog>(sp =>
            new RecreateSummariesStepCatalog(sp.GetRequiredService<ILoggerFactory>()));

        return services;
    }
}
