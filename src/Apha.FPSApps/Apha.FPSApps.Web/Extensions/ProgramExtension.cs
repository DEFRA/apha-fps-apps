using Amazon;
using Amazon.S3;
using Apha.FPSApps.Infrastructure.Mappings;
using Apha.FPSApps.Web.Filters;
using Apha.FPSApps.Web.Mappings;
using Apha.FPSApps.Web.Middleware;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace Apha.FPSApps.Web.Extensions
{
    public static class ProgramExtension
    {
        public static void ConfigureServices(this WebApplicationBuilder builder)
        {
            var services = builder.Services;
            var configuration = builder.Configuration;

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

            services.AddSession(options =>
            {
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Name = "VIR.Session";
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

            // Mapster
            var mapperConfig = new TypeAdapterConfig();
            mapperConfig.Default.NameMatchingStrategy(NameMatchingStrategy.IgnoreCase);
            mapperConfig.Scan(typeof(FpsApiDtoMapper).Assembly);
            mapperConfig.Scan(typeof(FpsViewModelMapper).Assembly);
            services.AddSingleton(mapperConfig);
            services.AddScoped<IMapper, ServiceMapper>();

            // HTTP Context
            services.AddHttpContextAccessor();

            // MVC
            services.AddControllersWithViews(options =>
            {
                // Centralised DataGrid Excel export — controllers only set AllowExcelExport = true.
                options.Filters.Add<DataGridExcelExportFilter>();
            });

            // Authentication
            services.AddAuthenticationServices(configuration, builder.Environment);

            //API clients
            services.AddApiClient(builder.Configuration);

            // Application services
            services.AddApplicationServices();

            // AWS S3 client
            var regionName = configuration["S3Storage:Region"]
                ?? throw new InvalidOperationException("S3Storage:Region is not configured.");

            services.AddSingleton<IAmazonS3>(_ =>
            {
                var region = RegionEndpoint.GetBySystemName(regionName);
                return new AmazonS3Client(region);
            });

            // In-memory cache (used by FpsYearMiddleware)
            services.AddMemoryCache();

            // Configure forwarded headers for proxy/load balancer support
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
            });

            // Health checks
            services.AddHealthChecks();

            // Bundling & minification of CSS/JS (LigerShark.WebOptimizer)
            services.AddWebOptimizerBundles();
        }

        /// <summary>
        /// Registers WebOptimizer with named CSS/JS bundles per layout/area.
        /// Bundle order is preserved to respect the Bootstrap → GOV.UK → custom cascade.
        /// Individually-loaded third-party libs (jQuery, jQuery-validation) remain unbundled
        /// because they are required at specific points in the page lifecycle.
        /// </summary>
        private static void AddWebOptimizerBundles(this IServiceCollection services)
        {
            services.AddWebOptimizer(pipeline =>
            {
                // ── Root shared layout ──────────────────────────────────────
                pipeline.AddCssBundle("/css/bundles/root.css",
                    "lib/bootstrap/dist/css/bootstrap.min.css",
                    "css/govuk-frontend-6.0.0.min.css",
                    "css/main_style.css",
                    "css/site.css");

                pipeline.AddJavaScriptBundle("/js/bundles/root.js",
                    "js/common/keyboard/global-dropdown-keyboard.js",
                    "js/site.js");

                // ── FPS area ────────────────────────────────────────────────
                pipeline.AddCssBundle("/css/bundles/fps.css",
                    "lib/bootstrap/dist/css/bootstrap.min.css",
                    "css/govuk-frontend-6.0.0.min.css",
                    "css/supplement_style_for_gov.uk_style.css",
                    "css/fps_styles/styles.css",
                    "DataGrid/datagrid.css",
                    "css/main_style.css",
                    "DataGrid/editable-grid.css",
                    "css/common/headernav/navstyle.css");

                pipeline.AddJavaScriptBundle("/js/bundles/fps.js",
                    "js/common/headernav/navmenu.js",
                    "js/common/numeric-decimal-input.js",
                    "js/common/js-alphanumeric-field.js");

                // ── PACT area ───────────────────────────────────────────────
                pipeline.AddCssBundle("/css/bundles/pact.css",
                    "css/govuk-frontend-6.0.0.min.css",
                    "css/supplement_style_for_gov.uk_style.css",
                    "css/pact_styles/styles.css",
                    "css/main_style.css",
                    "DataGrid/editable-grid.css",
                    "css/common/headernav/navstyle.css");

                pipeline.AddJavaScriptBundle("/js/bundles/pact.js",
                    "js/common/headernav/navmenu.js",
                    "js/number-validation.js",
                    "js/site.js");

                // ── PIMS area ───────────────────────────────────────────────
                pipeline.AddCssBundle("/css/bundles/pims.css",
                    "lib/bootstrap/dist/css/bootstrap.min.css",
                    "css/common/_govuk_tabs.css",
                    "css/govuk-frontend-6.0.0.min.css",
                    "css/supplement_style_for_gov.uk_style.css",
                    "css/pims_styles/styles.css",
                    "css/main_style.css",
                    "DataGrid/editable-grid.css",
                    "css/common/headernav/navstyle.css");

                pipeline.AddJavaScriptBundle("/js/bundles/pims.js",
                    "js/common/headernav/navmenu.js",
                    "js/common/numeric-decimal-input.js");

                // ── CostBook area ───────────────────────────────────────────
                pipeline.AddCssBundle("/css/bundles/costbook.css",
                    "lib/bootstrap/dist/css/bootstrap.min.css",
                    "css/govuk-frontend-6.0.0.min.css",
                    "css/supplement_style_for_gov.uk_style.css",
                    "css/costbook_styles/styles.css",
                    "css/main_style.css",
                    "DataGrid/editable-grid.css",
                    "css/common/headernav/navstyle.css");
            });
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

            // Use forwarded headers - must be before authentication
            app.UseForwardedHeaders();

            // Bundling & minification - must run before static files
            app.UseWebOptimizer();

            app.UseStaticFiles();
            app.UseRouting();

            app.UseSession();
            app.UseMiddleware<ExceptionMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            // FpsYearMiddleware must run after authentication to access API with bearer token
            app.UseMiddleware<FpsYearMiddleware>();

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