using Amazon;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Apha.Common.Utilities.Storage
{
    public static class S3ServiceCollectionExtensions
    {
        public static IServiceCollection AddAwsS3Storage(this IServiceCollection services, IConfiguration configuration)
        {
            var regionName = configuration["AWS:Region"]
                ?? throw new InvalidOperationException("AWS:Region is not configured.");

            services.AddSingleton<IAmazonS3>(_ =>
            {
                var region = RegionEndpoint.GetBySystemName(regionName);
                return new AmazonS3Client(region);
            });
            services.AddScoped<IS3StorageService, S3StorageService>();

            return services;
        }
    }
}
