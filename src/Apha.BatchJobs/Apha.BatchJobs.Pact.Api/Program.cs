using Amazon.EventBridge;
using Apha.BatchJobs.Triggering.Options;
using Apha.BatchJobs.Triggering.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.Configure<EventBridgePublisherOptions>(builder.Configuration.GetSection("EventBridge"));
builder.Services.AddAWSService<IAmazonEventBridge>();
builder.Services.AddScoped<IEventBridgePublisher, EventBridgePublisher>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "pact.api", timestamp = DateTime.UtcNow }));

app.Run();
