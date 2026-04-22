using Apha.FPS.Api.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsEnvironment("local"))
{
    builder.Host.UseSerilog((ctx, lc) =>
    {
        lc.WriteTo.Console();
        string srvpath = ctx.Configuration.GetValue<string>("LogsPath") ?? string.Empty;
        string logpath = $"{(ctx.HostingEnvironment.IsDevelopment() || ctx.HostingEnvironment.IsEnvironment("local") ? "Logs" : srvpath)}\\Logsample.log";
        lc.WriteTo.File(logpath, Serilog.Events.LogEventLevel.Verbose, rollingInterval: RollingInterval.Day);
    });
}
else
{
    Serilog.Debugging.SelfLog.Enable(Console.Error);
    builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    {
        loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)  // load min levels from appsettings.json
        .UseStructuredConsoleLogging();
    });
}

// Extracted to methods for testability
builder.ConfigureServices();

var app = builder.Build();

// Test database connection at startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<Apha.FPS.DataAccess.Data.FpsDbContext>();
        var canConnect = await dbContext.Database.CanConnectAsync();
        Console.WriteLine($"[DB CONNECTION TEST] Can connect to database: {canConnect}");

        if (canConnect)
        {
            var configuration = services.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("FPSConnectionString");
            Console.WriteLine($"[DB CONNECTION TEST] Connection string (masked): {MaskConnectionString(connectionString)}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DB CONNECTION TEST] FAILED: {ex.Message}");
        Console.WriteLine($"[DB CONNECTION TEST] Exception Type: {ex.GetType().Name}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"[DB CONNECTION TEST] Inner Exception: {ex.InnerException.Message}");
        }
    }
}

var httpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();
app.ConfigureMiddleware();

static string MaskConnectionString(string? connectionString)
{
    if (string.IsNullOrEmpty(connectionString)) return "NULL";
    var parts = connectionString.Split(';');
    var masked = parts.Select(part =>
    {
        if (part.Contains("Password=", StringComparison.OrdinalIgnoreCase))
            return "Password=***";
        return part;
    });
    return string.Join(";", masked);
}

#if false
// Middleware to log request headers, Only for debugging purposes
app.Use(async (context, next) =>
{
    var logObject = new
    {
        Tag = "RequestLog", // Static text for easy CloudWatch search
        Method = context.Request.Method,
        Path = context.Request.Path.ToString(),
        Headers = context.Request.Headers
            .Where(h => !string.Equals(h.Key, "Cookie", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(h => h.Key, h => h.Value.ToString())
    };

    // Serialize to JSON (compact)..
    var json = JsonSerializer.Serialize(logObject);

    Console.WriteLine(json); // One row in CloudWatch
    await next();
});
#endif

await app.RunAsync();

// Required for WebApplicationFactory<Program> in tests
public partial class Program
{
    // Prevent direct instantiation but still works with WebApplicationFactory
    protected Program() { }
}