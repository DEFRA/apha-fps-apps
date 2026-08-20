using Apha.BatchJobs.Domain.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications;

public static class MilestoneNotificationServiceExtensions
{
    public static IServiceCollection AddMilestoneNotificationJob(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MilestoneNotificationsSettings>(configuration.GetSection("MilestoneNotifications"));
        services.Configure<BatchAlertingSettings>(configuration.GetSection("BatchAlerting"));

        return services;
    }
}
