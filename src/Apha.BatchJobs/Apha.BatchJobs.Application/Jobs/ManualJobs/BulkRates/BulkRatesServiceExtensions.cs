using Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates;

public static class BulkRatesServiceExtensions
{
    public static IServiceCollection AddBulkRatesJobs(
        this IServiceCollection services)
    {
        services.AddScoped<IBulkTestRatesService, BulkTestRatesService>();
        services.AddScoped<IBulkStaffRatesService, BulkStaffRatesService>();
        services.AddScoped<IBulkAnimalRatesService, BulkAnimalRatesService>();

        return services;
    }
}
