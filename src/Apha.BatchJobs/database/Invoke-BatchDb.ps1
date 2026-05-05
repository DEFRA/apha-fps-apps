<#
.SYNOPSIS
Runs foundation DB operations for local Docker Desktop PostgreSQL.

.DESCRIPTION
Supports repeatable foundation workflows:
- apply: apply foundational migration scripts
- seed: apply seed scripts
- flush: truncate the local ScheduledLoadFromFps seeded footprint
- reset: flush then apply then seed
- all: apply then seed

No business data is required. Seed files are optional templates.
#>

param(
    [ValidateSet("apply", "seed", "flush", "reset", "all", "validate", "list")]
    [string]$Action = "list",

    [string]$ContainerName = "batch_jobs_postgres",
    [string]$Database = "batch_jobs_foundation_db",
    [string]$Username = "postgres",
    [string]$ScriptsRoot = "$PSScriptRoot/sql"
)

$ErrorActionPreference = "Stop"

function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Cyan
}

function Write-Ok {
    param([string]$Message)
    Write-Host "[OK]   $Message" -ForegroundColor Green
}

function Write-Warn {
    param([string]$Message)
    Write-Host "[WARN] $Message" -ForegroundColor Yellow
}

function Assert-Docker {
    if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
        throw "Docker CLI not found. Install Docker Desktop and ensure docker is on PATH."
    }

    docker ps *> $null
}

function Assert-ContainerRunning {
    param([string]$Name)

    $isRunning = docker inspect -f "{{.State.Running}}" $Name 2>$null
    if ($LASTEXITCODE -ne 0 -or "$isRunning".Trim() -ne "true") {
        throw "Container '$Name' is not running. Start it first (docker compose up -d postgres)."
    }
}

function Invoke-SqlFile {
    param(
        [string]$File,
        [string]$Container,
        [string]$Db,
        [string]$User
    )

    Write-Info "Applying $File"
    Get-Content -Path $File -Raw |
        docker exec -i $Container psql -U $User -d $Db -v ON_ERROR_STOP=1 -f /dev/stdin

    if ($LASTEXITCODE -ne 0) {
        throw "Failed while applying: $File"
    }

    Write-Ok "Applied $File"
}

function Get-SqlFiles {
    param([string]$Path)

    if (-not (Test-Path $Path)) {
        return @()
    }

    return Get-ChildItem -Path $Path -Filter "*.sql" -File | Sort-Object Name
}

function Invoke-SqlFolder {
    param(
        [string]$Path,
        [string]$Container,
        [string]$Db,
        [string]$User,
        [string]$EmptyMessage
    )

    $files = Get-SqlFiles -Path $Path
    if ($files.Count -eq 0) {
        Write-Warn $EmptyMessage
        return
    }

    foreach ($file in $files) {
        Invoke-SqlFile -File $file.FullName -Container $Container -Db $Db -User $User
    }
}

function Invoke-Apply {
    param([string]$Root, [string]$Container, [string]$Db, [string]$User)

    $foundationFiles = Get-SqlFiles -Path $Root | Where-Object { $_.Name -match '^\d+_.*\.sql$' }
    if ($foundationFiles.Count -eq 0) {
        Write-Warn "No top-level migration SQL files found in $Root"
        return
    }

    foreach ($file in $foundationFiles) {
        Invoke-SqlFile -File $file.FullName -Container $Container -Db $Db -User $User
    }
}

function Invoke-Seed {
    param([string]$Root, [string]$Container, [string]$Db, [string]$User)

    Invoke-SqlFolder -Path (Join-Path $Root "seeds") -Container $Container -Db $Db -User $User -EmptyMessage "No seed files found under sql/seeds"
}

function Invoke-Flush {
    param([string]$Root, [string]$Container, [string]$Db, [string]$User)

    Invoke-SqlFolder -Path (Join-Path $Root "flush") -Container $Container -Db $Db -User $User -EmptyMessage "No flush files found under sql/flush"
}

function Invoke-Validate {
    param([string]$Root, [string]$Container, [string]$Db, [string]$User)

    Invoke-SqlFolder -Path (Join-Path $Root "validate") -Container $Container -Db $Db -User $User -EmptyMessage "No validation files found under sql/validate"
}

function Show-Plan {
    param([string]$Root)

    Write-Host ""
    Write-Host "Batch DB script plan" -ForegroundColor Cyan
    Write-Host "  Root: $Root"
    Write-Host "  Top-level migrations:"
    (Get-SqlFiles -Path $Root | Where-Object { $_.Name -match '^\d+_.*\.sql$' }).Name | ForEach-Object { Write-Host "    - $_" }
    Write-Host "  Seed scripts:"
    (Get-SqlFiles -Path (Join-Path $Root "seeds")).Name | ForEach-Object { Write-Host "    - $_" }
    Write-Host "  Flush scripts:"
    (Get-SqlFiles -Path (Join-Path $Root "flush")).Name | ForEach-Object { Write-Host "    - $_" }
    Write-Host "  Validation scripts:"
    (Get-SqlFiles -Path (Join-Path $Root "validate")).Name | ForEach-Object { Write-Host "    - $_" }
    Write-Host ""
}

Assert-Docker
Assert-ContainerRunning -Name $ContainerName

switch ($Action) {
    "list" {
        Show-Plan -Root $ScriptsRoot
    }
    "apply" {
        Invoke-Apply -Root $ScriptsRoot -Container $ContainerName -Db $Database -User $Username
    }
    "seed" {
        Invoke-Seed -Root $ScriptsRoot -Container $ContainerName -Db $Database -User $Username
    }
    "flush" {
        Invoke-Flush -Root $ScriptsRoot -Container $ContainerName -Db $Database -User $Username
    }
    "reset" {
        Invoke-Flush -Root $ScriptsRoot -Container $ContainerName -Db $Database -User $Username
        Invoke-Apply -Root $ScriptsRoot -Container $ContainerName -Db $Database -User $Username
        Invoke-Seed -Root $ScriptsRoot -Container $ContainerName -Db $Database -User $Username
    }
    "all" {
        Invoke-Apply -Root $ScriptsRoot -Container $ContainerName -Db $Database -User $Username
        Invoke-Seed -Root $ScriptsRoot -Container $ContainerName -Db $Database -User $Username
    }
    "validate" {
        Invoke-Validate -Root $ScriptsRoot -Container $ContainerName -Db $Database -User $Username
    }
    default {
        throw "Unknown action: $Action"
    }
}

Write-Ok "Action '$Action' completed."
