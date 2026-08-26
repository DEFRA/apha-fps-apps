using Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries.Execution;
using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Context;
using Apha.BatchJobs.Infrastructure.Email;
using Apha.BatchJobs.Infrastructure.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.BulkRates.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Infrastructure.DependencyInjection;

/// <summary>
/// Top-level composition root for Infrastructure registrations.
/// Contains no direct service registrations — delegates to focused extensions.
/// <see cref="IExecutionYearContext"/> and <see cref="ICorrelationContextAccessor"/> are registered
/// here because their implementations live in the Infrastructure layer.
/// </summary>
public static class BatchInfrastructureServiceExtensions
{
    public static IServiceCollection AddBatchInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddBatchPersistence(configuration);
        services.AddGraphEmailIntegration(configuration);
        services.AddEmailService();

        // Shared execution-context services whose implementations are in Infrastructure.
        services.AddScoped<IExecutionYearContext, ExecutionYearContext>();
        services.AddScoped<ICurrentJobExecutionContext, CurrentJobExecutionContext>();
        services.AddScoped<IRecreateSummariesContext, RecreateSummariesContext>();
        services.AddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
        services.AddScoped<IBulkRatesRepository, BulkRatesRepository>();
        services.AddScoped<IRecreateSummariesStepCatalog>(sp =>
            new RecreateSummariesStepCatalog(sp.GetRequiredService<ILoggerFactory>()));
        services.AddScoped<IRecreateSummariesExecutionRunner, RecreateSummariesExecutionRunner>();
        services.AddMilestoneNotificationInfrastructure(configuration);
        services.AddMabArchiveInfrastructure(configuration);

        services.Configure<BulkRatesEmailSettings>(configuration.GetSection(BulkRatesEmailSettings.SectionName));
        services.AddScoped<IPostCompletionNotifier, BulkRatesCompletionNotifier>();

        return services;
    }
}
