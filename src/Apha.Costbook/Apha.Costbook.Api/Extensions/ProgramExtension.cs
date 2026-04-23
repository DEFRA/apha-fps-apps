using Apha.Costbook.Api.Filters;
using Apha.Costbook.Api.Mappings;
using Apha.Costbook.Api.Middleware;
using Apha.Costbook.Application.Mappings;
using Apha.Costbook.DataAccess.Data;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Apha.Costbook.Api.Extensions
{
    public static class ProgramExtension
    {
        public static void ConfigureServices(this WebApplicationBuilder builder)
        {
            var services = builder.Services;
            var configuration = builder.Configuration;

            // Add database context
            services.AddDbContext<CostbookDbContext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("FPSConnectionString")
                    ?? throw new InvalidOperationException("Connection string 'FPSConnectionString' not found.");

                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);

                    npgsqlOptions.CommandTimeout(60);
                });
            });

            if (builder.Environment.IsEnvironment("local"))
            {
                services.AddDistributedMemoryCache();
            }
            else
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = configuration.GetConnectionString("RedisConnectionString");
                    options.InstanceName = "RedisInstance";
                });
            }


            // AutoMapper
            services.AddAutoMapper(config =>
            {
                config.AddMaps(typeof(EntityMapper).Assembly);
                config.AddMaps(typeof(RequestMapper));
            });

            // MVC API
            services.AddControllers(options =>
            {
                options.Filters.Add<ApiResponseActionFilter>();
            });

            // Application services
            services.AddApplicationServices();

            // Authentication
            services.AddAuthenticationServices(configuration);

            // HTTP Context
            services.AddHttpContextAccessor();

            // Health checks
            services.AddHealthChecks();

            //Swagger
            services.AddSwaggerGen();    
        }

        public static void ConfigureMiddleware(this WebApplication app)
        {
            var env = app.Environment;

            // Set the default culture to en-GB (Great Britain)
            var cultureSet = "en-GB";
            var supportedCultures = new[] { new CultureInfo(cultureSet) };

            var localizationOptions = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(cultureSet),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            };
            app.UseRequestLocalization(localizationOptions);

            // Health checks endpoint
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                Predicate = _ => false
            });

            // Error handling
            if (env.IsDevelopment() || env.IsEnvironment("local"))
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }            

            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseMiddleware<ExceptionMiddleware>();
            app.UseMiddleware<RequestContextMiddleware>();

            //app.UseAuthentication();
            //app.UseAuthorization();

            // Default route
            app.MapControllers();
        }
    }
}