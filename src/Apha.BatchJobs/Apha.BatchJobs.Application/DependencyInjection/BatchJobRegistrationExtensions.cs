using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Jobs.HealthCheck;
using Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates;
using Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive;
using Microsoft.Extensions.DependencyInjection;

namespace Apha.BatchJobs.Application.DependencyInjection;

public static class BatchJobRegistrationExtensions
{
    // Phase 7F (main-port, 2026-08-28): Year End is now registered here — main-port Phases 1-7E
    // implemented it for real, but nobody had updated this list, so BatchJobFactory could resolve
    // "YearEnd-DataSetup"/"YearEnd-CutOver" to the right *type* (Phase 7D) yet still fail to
    // construct it, because neither handler was ever registered in the container. Discovered live
    // during Phase 8 Gate C4b — the first real Worker invocation to reach this far. Every prior
    // phase's tests either constructed the handler directly or exercised the pipeline steps/services
    // below it, never through this composition root, so nothing caught the gap until now.
    // MilestoneNotification remains excluded — MilestoneUpdateNotificationsJob is still unregistered
    // here too, but that is out of this port's scope; flag separately if it matters.
    public static IServiceCollection RegisterBatchJobImplementations(
        this IServiceCollection services)
    {
        services.AddJob<BulkAnimalRatesUpdateJob>();
        services.AddJob<BulkStaffRatesUpdateJob>();
        services.AddJob<BulkTestRatesUpdateJob>();
        services.AddJob<HealthCheckJobHandler>();
        services.AddJob<MabArchiveJob>();
        services.AddJob<RecreateSummaryJob>();
        services.AddJob<YearEndDataSetupJobHandler>();
        services.AddJob<YearEndCutoverJobHandler>();
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
