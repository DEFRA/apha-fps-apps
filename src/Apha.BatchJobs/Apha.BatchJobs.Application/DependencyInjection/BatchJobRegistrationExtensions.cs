using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Jobs.HealthCheck;
using Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates;
using Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive;
using Microsoft.Extensions.DependencyInjection;

namespace Apha.BatchJobs.Application.DependencyInjection;

public static class BatchJobRegistrationExtensions
{
    // Registers only the current-branch supported job set.
    // Year End and MilestoneNotification are excluded until those branches merge.
    public static IServiceCollection RegisterBatchJobImplementations(
        this IServiceCollection services)
    {
        services.AddJob<BulkAnimalRatesUpdateJob>();
        services.AddJob<BulkStaffRatesUpdateJob>();
        services.AddJob<BulkTestRatesUpdateJob>();
        services.AddJob<HealthCheckJobHandler>();
        services.AddJob<MabArchiveJob>();
        services.AddJob<RecreateSummaryJob>();
        return services;
    }

    private static IServiceCollection AddJob<TJob>(this IServiceCollection services)
        where TJob : class, IBatchJob
    {
        services.AddScoped<TJob>();
        services.AddScoped<IBatchJob>(sp => sp.GetRequiredService<TJob>());
        return services;
    }
}
