using Apha.BatchJobs.Application.Factory;
using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Orchestration;
using Apha.BatchJobs.Application.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Application.DependencyInjection;

/// <summary>Registers application-layer orchestration services. Contains no references to Infrastructure namespaces — only Application, Domain, and framework abstractions.</summary>
public static class BatchApplicationServiceExtensions
{
    public static IServiceCollection AddBatchApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<BatchJobSettings>(configuration.GetSection("BatchJobs"));
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<BatchJobSettings>, BatchJobSettingsValidator>());
        services.AddOptions<BatchJobSettings>().ValidateOnStart();

        services.AddScoped<IBatchJobFactory>(sp => new BatchJobFactory(sp));
        services.AddScoped<IBatchLockReconciliationService, BatchLockReconciliationService>();
        services.AddScoped<IDispatchTimeoutSweepService, DispatchTimeoutSweepService>();
        services.AddScoped<IJobOrchestrator, JobOrchestrator>();

        return services;
    }
}
