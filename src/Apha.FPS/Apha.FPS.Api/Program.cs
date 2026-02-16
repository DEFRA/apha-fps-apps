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

var httpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();
app.ConfigureMiddleware();

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