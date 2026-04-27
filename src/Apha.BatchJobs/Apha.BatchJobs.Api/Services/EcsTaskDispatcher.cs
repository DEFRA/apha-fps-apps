using Amazon.ECS;
using Amazon.ECS.Model;

namespace Apha.BatchJobs.Api.Services;

/// <summary>
/// Triggers batch jobs by dispatching an ECS Fargate RunTask request,
/// injecting BATCH_JOB_NAME and BATCH_RUN_MODE as container environment overrides.
/// </summary>
public sealed class EcsTaskDispatcher : IEcsTaskDispatcher
{
    private readonly IAmazonECS _ecs;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EcsTaskDispatcher> _logger;

    public EcsTaskDispatcher(
        IAmazonECS ecs,
        IConfiguration configuration,
        ILogger<EcsTaskDispatcher> logger)
    {
        _ecs = ecs;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> RunBatchJobAsync(
        string jobName,
        CancellationToken cancellationToken = default)
    {
        var subnets = _configuration
            .GetSection("Ecs:Subnets")
            .Get<List<string>>() ?? [];

        var securityGroups = _configuration
            .GetSection("Ecs:SecurityGroups")
            .Get<List<string>>() ?? [];

        var assignPublicIp = _configuration.GetValue<bool>("Ecs:AssignPublicIp");

        var request = new RunTaskRequest
        {
            Cluster = _configuration["Ecs:Cluster"],
            TaskDefinition = _configuration["Ecs:TaskDefinition"],
            LaunchType = LaunchType.FARGATE,
            Count = 1,
            NetworkConfiguration = new NetworkConfiguration
            {
                AwsvpcConfiguration = new AwsVpcConfiguration
                {
                    Subnets = subnets,
                    SecurityGroups = securityGroups,
                    AssignPublicIp = assignPublicIp
                        ? AssignPublicIp.ENABLED
                        : AssignPublicIp.DISABLED
                }
            },
            Overrides = new TaskOverride
            {
                ContainerOverrides =
                [
                    new ContainerOverride
                    {
                        Name = _configuration["Ecs:ContainerName"],
                        Environment =
                        [
                            new Amazon.ECS.Model.KeyValuePair
                            {
                                Name = "BATCH_JOB_NAME",
                                Value = jobName
                            },
                            new Amazon.ECS.Model.KeyValuePair
                            {
                                Name = "BATCH_RUN_MODE",
                                Value = "Manual"
                            }
                        ]
                    }
                ]
            }
        };

        var response = await _ecs.RunTaskAsync(request, cancellationToken);

        if (response.Failures?.Count > 0)
        {
            var failureText = string.Join(
                "; ",
                response.Failures.Select(f => $"{f.Arn} - {f.Reason} - {f.Detail}"));

            _logger.LogError("ECS RunTask failed: {FailureText}", failureText);

            throw new InvalidOperationException($"ECS RunTask failed: {failureText}");
        }

        var taskArn = response.Tasks.FirstOrDefault()?.TaskArn;

        if (string.IsNullOrWhiteSpace(taskArn))
        {
            throw new InvalidOperationException("ECS RunTask returned no task ARN.");
        }

        _logger.LogInformation(
            "Started ECS task {TaskArn} for job {JobName}",
            taskArn,
            jobName);

        return taskArn;
    }
}
