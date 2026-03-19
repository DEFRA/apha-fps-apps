using System.Globalization;
using Apha.FPSApps.Infrastructure.Mappings;
using Apha.FPSApps.Web.Mappings;
using Apha.FPSApps.Web.Middleware;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Localization;

namespace Apha.FPSApps.Web.Extensions
{
    public static class ProgramExtension
    {
        public static void ConfigureServices(this WebApplicationBuilder builder)
        {
            var services = builder.Services;                        

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration.GetConnectionString("RedisConnectionString");
                options.InstanceName = "RedisInstance";
            });

            services.AddSession(options =>
            {
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Name = "VIR.Session";
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

            // AutoMapper  
            services.AddAutoMapper(config =>
            {
                config.AddMaps(typeof(ApiDtoMapper).Assembly);
                config.AddMaps(typeof(ViewModelMapper));
            });

            // HTTP Context
            services.AddHttpContextAccessor();

            // MVC
            services.AddControllersWithViews();

            // Authentication
            //services.AddAuthenticationServices(configuration);

            // Save tokens in cookie
            //services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
            //{
            //    options.SaveTokens = true;
            //});

            // Configure cookie expiration
            services.ConfigureApplicationCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
            });
            //API clients
            services.AddApiClient(builder.Configuration);

            // Application services
            services.AddApplicationServices();

           

           

            // Health checks
            services.AddHealthChecks();
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
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseSession();
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseMiddleware<FPSYearMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            // Default route
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        }
    }
}