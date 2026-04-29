namespace Apha.BatchJobs.Api.Services;

/// <summary>
/// Configuration used to prepare ECS task trigger requests from the API.
/// </summary>
public sealed class AwsEcsTriggerOptions
{
    public string Cluster { get; set; } = "apha-batchjobs-cluster";
    public string TaskDefinition { get; set; } = "apha-batchjobs-worker";
    public string LaunchType { get; set; } = "FARGATE";
    public string ContainerName { get; set; } = "batch-jobs-worker";
    public List<string> Subnets { get; set; } = new();
    public List<string> SecurityGroups { get; set; } = new();
    public bool AssignPublicIp { get; set; } = true;
    public string PlatformVersion { get; set; } = "LATEST";
    public string PowerShellExecutable { get; set; } = "pwsh";
    public string PowerShellScriptPath { get; set; } = "scripts/trigger-healthcheck.ps1";
}
