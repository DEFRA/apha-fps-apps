<#
.SYNOPSIS
Bootstraps AWS resources for Apha.BatchJobs and optionally pushes Docker image to ECR.

.DESCRIPTION
This script is focused on BatchJobs scope only. It can:
1) Validate local connectivity to AWS.
2) Print AWS service prerequisites.
3) Create core AWS resources.
4) Push container image to ECR first.

.EXAMPLE
# Push to ECR first (minimal path)
./database/scripts/bootstrap-aws-and-push-ecr.ps1 -PushOnly -Region eu-west-2

.EXAMPLE
# Full bootstrap + push
./database/scripts/bootstrap-aws-and-push-ecr.ps1 -CreateCoreResources -PushImage -Region eu-west-2
#>

[CmdletBinding()]
param(
    [string]$Region = "eu-west-2",
    [string]$AccountId,
    [string]$RepositoryName = "apha/batchjobs",
    [string]$ClusterName = "apha-batchjobs-cluster",
    [string]$LogGroupName = "/apha/batch-jobs",
    [string]$ContextPath = "src/Apha.BatchJobs",
    [string]$DockerfilePath = "src/Apha.BatchJobs/Dockerfile",
    [string]$ImageTag,
    [switch]$ConnectivityOnly,
    [switch]$CreateCoreResources,
    [switch]$PushImage,
    [switch]$PushOnly,
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"

function Write-Info([string]$Message) {
    Write-Host "[INFO] $Message" -ForegroundColor Cyan
}

function Write-Ok([string]$Message) {
    Write-Host "[OK]   $Message" -ForegroundColor Green
}

function Write-Warn([string]$Message) {
    Write-Host "[WARN] $Message" -ForegroundColor Yellow
}

function Test-Tool([string]$ToolName) {
    return $null -ne (Get-Command $ToolName -ErrorAction SilentlyContinue)
}

function Assert-ToolingForConnectivity {
    if (-not (Test-Tool "aws")) {
        throw "AWS CLI not found. Install AWS CLI v2 and add it to PATH."
    }
}

function Assert-ToolingForPush {
    if (-not (Test-Tool "docker")) {
        throw "Docker CLI not found. Install Docker Desktop (Linux containers mode) and add it to PATH."
    }
    if (-not (Test-Tool "git")) {
        throw "git not found. Install git and add it to PATH."
    }
}

function Show-Prerequisites {
    Write-Host ""
    Write-Host "BatchJobs AWS prerequisites" -ForegroundColor Magenta
    Write-Host "- AWS account + region selected"
    Write-Host "- IAM permissions for: sts, ecr, logs, ecs (if creating core resources)"
    Write-Host "- AWS CLI v2 authenticated (aws configure sso or aws configure)"
    Write-Host "- Docker daemon running in Linux container mode"
    Write-Host "- ECR repository: $RepositoryName"
    Write-Host "- CloudWatch Logs group: $LogGroupName"
    Write-Host "- ECS cluster (optional now, required later): $ClusterName"
    Write-Host ""
}

function Get-AwsIdentity {
    $raw = aws sts get-caller-identity --output json | Out-String
    if ([string]::IsNullOrWhiteSpace($raw)) {
        throw "Unable to read AWS caller identity."
    }
    return $raw | ConvertFrom-Json
}

function Resolve-AccountId {
    param([string]$InputAccountId)

    if (-not [string]::IsNullOrWhiteSpace($InputAccountId)) {
        return $InputAccountId
    }

    $identity = Get-AwsIdentity
    if ([string]::IsNullOrWhiteSpace($identity.Account)) {
        throw "AWS AccountId could not be resolved from sts get-caller-identity."
    }
    return $identity.Account
}

function Ensure-EcrRepository {
    param(
        [string]$Repo,
        [string]$AwsRegion
    )

    try {
        aws ecr describe-repositories --region $AwsRegion --repository-names $Repo *> $null
        Write-Ok "ECR repository already exists: $Repo"
    }
    catch {
        Write-Info "Creating ECR repository: $Repo"
        aws ecr create-repository --region $AwsRegion --repository-name $Repo --image-scanning-configuration scanOnPush=true *> $null
        Write-Ok "Created ECR repository: $Repo"
    }
}

function Ensure-LogGroup {
    param(
        [string]$GroupName,
        [string]$AwsRegion
    )

    $existing = aws logs describe-log-groups --region $AwsRegion --log-group-name-prefix $GroupName --output json | ConvertFrom-Json
    $match = $existing.logGroups | Where-Object { $_.logGroupName -eq $GroupName }

    if ($null -ne $match) {
        Write-Ok "CloudWatch log group already exists: $GroupName"
        return
    }

    Write-Info "Creating CloudWatch log group: $GroupName"
    aws logs create-log-group --region $AwsRegion --log-group-name $GroupName *> $null
    Write-Ok "Created CloudWatch log group: $GroupName"
}

function Ensure-EcsCluster {
    param(
        [string]$Name,
        [string]$AwsRegion
    )

    $arn = "arn:aws:ecs:${AwsRegion}:$ResolvedAccountId:cluster/$Name"
    $result = aws ecs describe-clusters --region $AwsRegion --clusters $arn --output json | ConvertFrom-Json
    $exists = $result.clusters | Where-Object { $_.clusterName -eq $Name }

    if ($null -ne $exists) {
        Write-Ok "ECS cluster already exists: $Name"
        return
    }

    Write-Info "Creating ECS cluster: $Name"
    aws ecs create-cluster --region $AwsRegion --cluster-name $Name *> $null
    Write-Ok "Created ECS cluster: $Name"
}

function Get-DefaultImageTag {
    try {
        $sha = (git rev-parse --short HEAD).Trim()
        if (-not [string]::IsNullOrWhiteSpace($sha)) {
            return "dev-$sha"
        }
    }
    catch {
        Write-Warn "Could not read git SHA, falling back to timestamp tag."
    }

    return "dev-$(Get-Date -Format 'yyyyMMddHHmmss')"
}

function Push-ToEcr {
    param(
        [string]$AwsRegion,
        [string]$AwsAccountId,
        [string]$Repo,
        [string]$Tag,
        [string]$DockerContext,
        [string]$Dockerfile,
        [switch]$SkipBuild
    )

    $registry = "$AwsAccountId.dkr.ecr.$AwsRegion.amazonaws.com"
    $localImage = "batchjobs-local:$Tag"
    $remoteImage = "$registry/$Repo:$Tag"

    Write-Info "Logging in to ECR: $registry"
    aws ecr get-login-password --region $AwsRegion | docker login --username AWS --password-stdin $registry *> $null
    Write-Ok "ECR login successful"

    if (-not $SkipBuild) {
        Write-Info "Building Docker image: $localImage"
        docker build --file $Dockerfile --tag $localImage $DockerContext
        Write-Ok "Docker build complete"
    }
    else {
        Write-Warn "Skipping docker build as requested."
    }

    Write-Info "Tagging image for ECR: $remoteImage"
    docker tag $localImage $remoteImage

    Write-Info "Pushing image to ECR"
    docker push $remoteImage

    Write-Host ""
    Write-Ok "Push complete"
    Write-Host "Image URI: $remoteImage" -ForegroundColor Green
    Write-Host ""
}

Show-Prerequisites

if ($PushOnly) {
    $PushImage = $true
    $CreateCoreResources = $false
}

Assert-ToolingForConnectivity
$identity = Get-AwsIdentity
$ResolvedAccountId = Resolve-AccountId -InputAccountId $AccountId

Write-Ok "AWS connectivity established"
Write-Host "Account: $($identity.Account)" -ForegroundColor Gray
Write-Host "Arn:     $($identity.Arn)" -ForegroundColor Gray
Write-Host "Region:  $Region" -ForegroundColor Gray

if ($ConnectivityOnly) {
    Write-Info "Connectivity check only requested. Exiting."
    exit 0
}

Ensure-EcrRepository -Repo $RepositoryName -AwsRegion $Region

if ($CreateCoreResources) {
    Ensure-LogGroup -GroupName $LogGroupName -AwsRegion $Region
    Ensure-EcsCluster -Name $ClusterName -AwsRegion $Region
}

if ($PushImage) {
    Assert-ToolingForPush

    $resolvedTag = if ([string]::IsNullOrWhiteSpace($ImageTag)) {
        Get-DefaultImageTag
    }
    else {
        $ImageTag
    }

    Push-ToEcr -AwsRegion $Region -AwsAccountId $ResolvedAccountId -Repo $RepositoryName -Tag $resolvedTag -DockerContext $ContextPath -Dockerfile $DockerfilePath -SkipBuild:$NoBuild
}
else {
    Write-Info "No image push requested. Use -PushImage or -PushOnly."
}
