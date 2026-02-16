using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.CostBookApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients;
using Apha.FPSApps.Web.Handler;

namespace Apha.FPSApps.Web.Extensions
{
    public static class ApiClientExtension
    {
        public static IServiceCollection AddApiClient(this IServiceCollection services, IConfiguration configuration)
        {
            // FPS
            services.AddHttpClient("FpsApiHttpClient", client =>
            {
                client.BaseAddress = new Uri(
                    configuration["FPSApiSettings:BaseUrl"]
                        ?? throw new InvalidOperationException("FPS base URL not configured"));                                
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            }).AddHttpMessageHandler<FinancialYearHeaderHandler>(); 


            services.AddScoped<IFpsHttpExecutor>(sp =>
            {
                var factory = sp.GetRequiredService<IHttpClientFactory>();
                return new FpsHttpExecutor(factory.CreateClient("FpsApiHttpClient"));
            });

            services.AddScoped<IFpsApiClient, FpsApiClient>();

            // PACT
            services.AddHttpClient("PactApiHttpClient", client =>
            {
                client.BaseAddress = new Uri(configuration["PACTApiSettings:BaseUrl"]
                    ?? throw new InvalidOperationException("PACT base URL not configured"));
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            }).AddHttpMessageHandler<FinancialYearHeaderHandler>();

            services.AddScoped<IPactHttpExecutor>(sp =>
            {
                var factory = sp.GetRequiredService<IHttpClientFactory>();
                return new PactHttpExecutor(factory.CreateClient("PactApiHttpClient"));
            });

            services.AddScoped<IPactApiClient, PactApiClient>();

            // PIMS
            services.AddHttpClient("PimsApiHttpClient", client =>
            {
                client.BaseAddress = new Uri(configuration["PIMSApiSettings:BaseUrl"]
                    ?? throw new InvalidOperationException("PIMS base URL not configured"));
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            }).AddHttpMessageHandler<FinancialYearHeaderHandler>();

            services.AddScoped<IPimsHttpExecutor>(sp =>
            {
                var factory = sp.GetRequiredService<IHttpClientFactory>();
                return new PimsHttpExecutor(factory.CreateClient("PimsApiHttpClient"));
            });

            services.AddScoped<IPimsApiClient, PimsApiClient>();

            // CostBook
            services.AddHttpClient("CostBookApiHttpClient", client =>
            {
                client.BaseAddress = new Uri(configuration["CostBookApiSettings:BaseUrl"]
                    ?? throw new InvalidOperationException("CostBook base URL not configured"));
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            }).AddHttpMessageHandler<FinancialYearHeaderHandler>();

            services.AddScoped<ICostBookHttpExecutor>(sp =>
            {
                var factory = sp.GetRequiredService<IHttpClientFactory>();
                return new CostBookHttpExecutor(factory.CreateClient("CostBookApiHttpClient"));
            });

            services.AddScoped<ICostBookApiClient, CostBookApiClient>();

            return services;
        }
    }
}
