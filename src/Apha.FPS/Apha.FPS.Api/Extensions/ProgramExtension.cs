using System.Globalization;
using Apha.FPS.Api.Extensions;
using Apha.FPS.Api.Filters;
using Apha.FPS.Api.Mappings;
using Apha.FPS.Api.Middleware;
using Apha.FPS.Application.Mappings;
using Apha.FPS.DataAccess.Data;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.Api.Extensions
{
    public static class ProgramExtension
    {
        public static void ConfigureServices(this WebApplicationBuilder builder)
        {
            var services = builder.Services;
            var configuration = builder.Configuration;

            // Add database context
            services.AddDbContext<WeatherForecastDbContext>(options =>
             options.UseSqlServer(builder.Configuration.GetConnectionString("WeatherForecastConnectionString")
             ?? throw new InvalidOperationException("Connection string 'WeatherForecastConnectionString' not found.")));

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration.GetConnectionString("RedisConnectionString");
                options.InstanceName = "RedisInstance";
            });


            // AutoMapper
            services.AddAutoMapper(typeof(EntityMapper).Assembly);
            services.AddAutoMapper(typeof(RequestModelMapper));

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

            app.UseAuthentication();
            app.UseAuthorization();

            // Default route
            app.MapControllers();
        }
    }
}