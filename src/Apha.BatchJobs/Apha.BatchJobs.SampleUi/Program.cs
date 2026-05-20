using System.Net.Http.Json;
using Apha.BatchJobs.Triggering.Models;
using Apha.BatchJobs.Triggering.Policy;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DownstreamApisOptions>(builder.Configuration.GetSection("DownstreamApis"));
builder.Services.AddHttpClient();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/jobs", () =>
{
	var jobs = BatchJobRoutingPolicy.GetCatalog().Select(route => new
	{
		route.JobName,
		route.Description,
		route.RouteKind,
		TriggerPath = route.RouteKind switch
		{
			JobRouteKind.FpsApi => "FPS API",
			JobRouteKind.PactApi => "PACT API",
			JobRouteKind.ScheduledOnly => "Scheduled only",
			_ => "Neutral (FPS or PACT)"
		}
	});

	return Results.Ok(jobs);
});

app.MapPost("/api/trigger", async (
	BatchTriggerRequest request,
	string? preferredApi,
	IHttpClientFactory clientFactory,
	IConfiguration configuration,
	CancellationToken cancellationToken) =>
{
	if (!BatchJobRoutingPolicy.TryResolveRoute(request.JobName, out var route, out var error) || route is null)
		return Results.BadRequest(new { accepted = false, reason = error });

	if (route.RouteKind == JobRouteKind.ScheduledOnly)
	{
		return Results.Conflict(new
		{
			accepted = false,
			jobName = route.JobName,
			reason = "MABArchive is a scheduled job and is not manually triggered via API."
		});
	}

	var source = route.RouteKind switch
	{
		JobRouteKind.FpsApi => "fps",
		JobRouteKind.PactApi => "pact",
		_ => string.Equals(preferredApi, "pact", StringComparison.OrdinalIgnoreCase) ? "pact" : "fps"
	};

	var baseUrl = source == "pact"
		? configuration["DownstreamApis:PactBaseUrl"]
		: configuration["DownstreamApis:FpsBaseUrl"];

	if (string.IsNullOrWhiteSpace(baseUrl))
		return Results.Problem("Downstream API base URL is not configured.", statusCode: StatusCodes.Status500InternalServerError);

	var client = clientFactory.CreateClient();
	var response = await client.PostAsJsonAsync(
		$"{baseUrl.TrimEnd('/')}/api/v1/batch-jobs/trigger",
		new BatchTriggerRequest
		{
			JobName = route.JobName,
			RequestedBy = string.IsNullOrWhiteSpace(request.RequestedBy) ? "sample-ui@local" : request.RequestedBy
		},
		cancellationToken);

	var body = await response.Content.ReadAsStringAsync(cancellationToken);
	return Results.Content(body, "application/json", statusCode: (int)response.StatusCode);
});

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "sample-ui", timestamp = DateTime.UtcNow }));

app.Run();

public sealed class DownstreamApisOptions
{
	public string FpsBaseUrl { get; init; } = "http://localhost:5160";

	public string PactBaseUrl { get; init; } = "http://localhost:5089";
}
