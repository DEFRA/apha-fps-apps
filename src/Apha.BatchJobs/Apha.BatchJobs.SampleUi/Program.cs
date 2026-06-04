using System.Net.Http.Json;
using Apha.BatchJobs.Triggering.Models;
using Apha.BatchJobs.Triggering.Policy;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DownstreamApisOptions>(builder.Configuration.GetSection("DownstreamApis"));
builder.Services.AddHttpClient("triggering", client =>
{
	client.Timeout = TimeSpan.FromMinutes(10);
});
builder.Services.AddHttpClient("status-proxy", client =>
{
	client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/batch-jobs/{jobName}/can-run", async (
	string jobName,
	IHttpClientFactory clientFactory,
	IConfiguration configuration,
	CancellationToken cancellationToken) =>
{
	var baseUrl = configuration["DownstreamApis:PactBaseUrl"];
	if (string.IsNullOrWhiteSpace(baseUrl))
		return Results.Problem("PACT API base URL is not configured.", statusCode: StatusCodes.Status500InternalServerError);

	var client = clientFactory.CreateClient("status-proxy");
	try
	{
		var response = await SendWithTransientRetriesAsync(
			() => client.GetAsync(
				$"{baseUrl.TrimEnd('/')}/api/batch-jobs/{Uri.EscapeDataString(jobName)}/can-run",
				cancellationToken),
			cancellationToken);

		var body = await response.Content.ReadAsStringAsync(cancellationToken);
		return Results.Content(body, "application/json", statusCode: (int)response.StatusCode);
	}
	catch (Exception ex)
	{
		return Results.Problem($"Failed to check can-run status: {ex.Message}", statusCode: StatusCodes.Status503ServiceUnavailable);
	}
});

app.MapGet("/api/batch-jobs/{jobName}/status", async (
	string jobName,
	[FromQuery] string? jobExecutionId,
	[FromQuery] DateTime? acceptedAtUtc,
	[FromQuery] bool? debugView,
	IHttpClientFactory clientFactory,
	IConfiguration configuration,
	CancellationToken cancellationToken) =>
{
	var baseUrl = configuration["DownstreamApis:PactBaseUrl"];
	if (string.IsNullOrWhiteSpace(baseUrl))
		return Results.Problem("PACT API base URL is not configured.", statusCode: StatusCodes.Status500InternalServerError);

	var client = clientFactory.CreateClient("status-proxy");
	try
	{
		var queryParts = new List<string>();
		if (!string.IsNullOrWhiteSpace(jobExecutionId))
			queryParts.Add($"jobExecutionId={Uri.EscapeDataString(jobExecutionId)}");
		if (acceptedAtUtc.HasValue)
			queryParts.Add($"acceptedAtUtc={Uri.EscapeDataString(acceptedAtUtc.Value.ToString("O"))}");
		if (debugView == true)
			queryParts.Add("debugView=true");

		var queryString = queryParts.Count > 0 ? "?" + string.Join("&", queryParts) : "";
		var response = await SendWithTransientRetriesAsync(
			() => client.GetAsync(
				$"{baseUrl.TrimEnd('/')}/api/batch-jobs/{Uri.EscapeDataString(jobName)}/status{queryString}",
				cancellationToken),
			cancellationToken);

		var body = await response.Content.ReadAsStringAsync(cancellationToken);
		return Results.Content(body, "application/json", statusCode: (int)response.StatusCode);
	}
	catch (Exception ex)
	{
		return Results.Problem($"Failed to fetch status: {ex.Message}", statusCode: StatusCodes.Status503ServiceUnavailable);
	}
});

app.MapPost("/api/batch-jobs/{jobName}/cancel", async (
	string jobName,
	BatchCancelRequest request,
	IHttpClientFactory clientFactory,
	IConfiguration configuration,
	CancellationToken cancellationToken) =>
{
	var baseUrl = configuration["DownstreamApis:PactBaseUrl"];
	if (string.IsNullOrWhiteSpace(baseUrl))
		return Results.Problem("PACT API base URL is not configured.", statusCode: StatusCodes.Status500InternalServerError);

	var client = clientFactory.CreateClient("status-proxy");
	try
	{
		var response = await SendWithTransientRetriesAsync(
			() => client.PostAsJsonAsync(
				$"{baseUrl.TrimEnd('/')}/api/v1/batch-jobs/{Uri.EscapeDataString(jobName)}/cancel",
				request,
				cancellationToken),
			cancellationToken);

		var body = await response.Content.ReadAsStringAsync(cancellationToken);
		return Results.Content(body, "application/json", statusCode: (int)response.StatusCode);
	}
	catch (Exception ex)
	{
		return Results.Problem($"Failed to send cancellation request: {ex.Message}", statusCode: StatusCodes.Status503ServiceUnavailable);
	}
});

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
			reason = "MABArchive is a scheduled job and is not manually triggered via API. Its execution year is derived internally from the run date."
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

	var client = clientFactory.CreateClient("triggering");
	var readiness = await WaitForServiceReadyAsync(client, baseUrl, cancellationToken);
	if (!readiness.IsReady)
	{
		return Results.Problem(
			$"Downstream API '{baseUrl.TrimEnd('/')}' is not reachable before trigger. {readiness.ErrorMessage}",
			statusCode: StatusCodes.Status503ServiceUnavailable);
	}

	var response = await SendWithTransientRetriesAsync(
		() => client.PostAsJsonAsync(
			$"{baseUrl.TrimEnd('/')}/api/v1/batch-jobs/trigger",
			new BatchTriggerRequest
			{
				JobName = route.JobName,
				RequestedBy = string.IsNullOrWhiteSpace(request.RequestedBy) ? "sample-ui@local" : request.RequestedBy
			},
			cancellationToken),
		cancellationToken);

	var body = await response.Content.ReadAsStringAsync(cancellationToken);
	return Results.Content(body, "application/json", statusCode: (int)response.StatusCode);
});

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "sample-ui", timestamp = DateTime.UtcNow }));

app.Run();

static async Task<(bool IsReady, string ErrorMessage)> WaitForServiceReadyAsync(
	HttpClient client,
	string baseUrl,
	CancellationToken cancellationToken)
{
	var healthUrl = $"{baseUrl.TrimEnd('/')}/health";
	Exception? lastError = null;

	for (var attempt = 1; attempt <= 10; attempt++)
	{
		try
		{
			using var healthResponse = await client.GetAsync(healthUrl, cancellationToken);
			if (healthResponse.IsSuccessStatusCode)
			{
				return (true, string.Empty);
			}

			lastError = new HttpRequestException($"Health check returned {(int)healthResponse.StatusCode} ({healthResponse.ReasonPhrase}).");
		}
		catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
		{
			lastError = ex;
		}

		if (attempt < 10)
		{
			await Task.Delay(250, cancellationToken);
		}
	}

	return (false, lastError?.Message ?? "No response from downstream API.");
}

static async Task<HttpResponseMessage> SendWithTransientRetriesAsync(
	Func<Task<HttpResponseMessage>> send,
	CancellationToken cancellationToken,
	int maxAttempts = 3)
{
	HttpResponseMessage? lastResponse = null;
	Exception? lastException = null;

	for (var attempt = 1; attempt <= maxAttempts; attempt++)
	{
		try
		{
			var response = await send();
			if ((int)response.StatusCode >= 500 && attempt < maxAttempts)
			{
				response.Dispose();
				await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt), cancellationToken);
				continue;
			}

			return response;
		}
		catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
		{
			lastException = ex;
			if (attempt < maxAttempts)
			{
				await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt), cancellationToken);
				continue;
			}
		}
	}

	if (lastResponse is not null)
		return lastResponse;

	throw lastException ?? new HttpRequestException("Downstream request failed after retries.");
}

public sealed class DownstreamApisOptions
{
	public string FpsBaseUrl { get; init; } = "http://localhost:5160";

	public string PactBaseUrl { get; init; } = "http://localhost:5189";
}

public sealed class BatchCancelRequest
{
	public string JobExecutionId { get; init; } = string.Empty;

	public string RequestedBy { get; init; } = string.Empty;
}
