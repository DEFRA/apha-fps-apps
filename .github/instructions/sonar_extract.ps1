param(
    [Parameter(Mandatory=$true)][string]$SonarToken,
    [Parameter(Mandatory=$true)][string]$ProjectKey,
    [Parameter(Mandatory=$true)][string]$Organization,
    [Parameter(Mandatory=$true)][string]$BranchName,
    [string]$OutputFile = "sonar-report.json",
    [bool]$IncludeHotspots = $true
)

# -----------------------------
# CONFIG
# -----------------------------
$issuesApi  = "https://sonarcloud.io/api/issues/search".Trim()
$hotspotApi = "https://sonarcloud.io/api/hotspots/search".Trim()

if ([string]::IsNullOrWhiteSpace($issuesApi)) {
    throw "issuesApi is EMPTY"
}
if ([string]::IsNullOrWhiteSpace($hotspotApi)) {
    throw "hotspotApi is EMPTY"
}

Add-Type -AssemblyName System.Web
$encodedBranch = [System.Web.HttpUtility]::UrlEncode($BranchName)

$headers = @{
    Authorization = "Basic " + [Convert]::ToBase64String(
        [Text.Encoding]::ASCII.GetBytes("${SonarToken}:")
    )
}

$pageSize = 500
$allIssues = @()

Write-Host "Fetching ALL issues for branch: $BranchName" -ForegroundColor Cyan

# -----------------------------
# FUNCTION: BUILD URL SAFELY
# -----------------------------
function Build-Url {
    param(
        [string]$baseUrl,
        [hashtable]$params
    )

    if ([string]::IsNullOrWhiteSpace($baseUrl)) {
        throw "Base URL is EMPTY inside Build-Url"
    }

    $query = ($params.GetEnumerator() | ForEach-Object {
        "$($_.Key)=$($_.Value)"
    }) -join "&"

    return "$baseUrl`?$query"
}

# -----------------------------
# FETCH ISSUES
# -----------------------------
$page = 1
$total = 0

do {
    $params = @{
        componentKeys    = $ProjectKey
        organization     = $Organization
        branch           = $encodedBranch
        ps               = $pageSize
        p                = $page
        issueStatuses    = "OPEN,CONFIRMED"
        impactSeverities = "LOW,MEDIUM,HIGH"
    }

    $url = Build-Url -baseUrl $issuesApi -params $params

    Write-Host "`nCalling URL:" -ForegroundColor Yellow
    Write-Host $url

    try {
        $response = Invoke-RestMethod -Uri $url -Headers $headers -Method Get
    }
    catch {
        Write-Host "ERROR calling Sonar API" -ForegroundColor Red
        Write-Host $_.Exception.Message
        break
    }

    if ($page -eq 1) {
        $total = $response.paging.total
        Write-Host "Total issues: $total" -ForegroundColor Green
    }

    $issues = @($response.issues)
    $allIssues += $issues

    Write-Host "Fetched $($allIssues.Count) / $total"

    $page++

} while ($allIssues.Count -lt $total)

# -----------------------------
# FETCH HOTSPOTS (OPTIONAL)
# -----------------------------
$allHotspots = @()

if ($IncludeHotspots) {
    Write-Host "`nFetching Security Hotspots..." -ForegroundColor Cyan

    $page = 1
    $totalHotspots = 0

    do {
        $params = @{
            projectKey = $ProjectKey
            branch     = $encodedBranch
            ps         = $pageSize
            p          = $page
        }

        $url = Build-Url $hotspotApi $params

        Write-Host "Calling URL:"
        Write-Host $url

        try {
            $response = Invoke-RestMethod -Uri $url -Headers $headers -Method Get
        }
        catch {
            Write-Host "ERROR calling Hotspots API" -ForegroundColor Red
            Write-Host $_.Exception.Message
            break
        }

        if ($page -eq 1) {
            $totalHotspots = $response.paging.total
            Write-Host "Total hotspots: $totalHotspots" -ForegroundColor Green
        }

        $hotspots = @($response.hotspots)
        $allHotspots += $hotspots

        Write-Host "Fetched $($allHotspots.Count) / $totalHotspots hotspots"

        $page++

    } while ($allHotspots.Count -lt $totalHotspots)
}

# -----------------------------
# NORMALIZE OUTPUT
# -----------------------------
$cleanIssues = @($allIssues | ForEach-Object {
    [PSCustomObject]@{
        type     = "ISSUE"
        file     = ($_."component" -split ":")[1]
        line     = $_.line
        severity = $_.severity
        message  = $_.message
        rule     = $_.rule
        status   = $_.status
        author   = $_.author
    }
})

$cleanHotspots = @($allHotspots | ForEach-Object {
    [PSCustomObject]@{
        type     = "HOTSPOT"
        file     = $_.component
        line     = $_.line
        severity = $_.vulnerabilityProbability
        message  = $_.message
        rule     = $_.ruleKey
        status   = $_.status
        author   = $_.author
    }
})

# -----------------------------
# MERGE SAFE
# -----------------------------
$final = @($cleanIssues + $cleanHotspots)

# -----------------------------
# EXPORT
# -----------------------------
$final | ConvertTo-Json -Depth 5 | Out-File $OutputFile

# -----------------------------
# SUMMARY
# -----------------------------
Write-Host "`nExport complete: $OutputFile" -ForegroundColor Green
Write-Host "Issues: $($cleanIssues.Count)"
Write-Host "Hotspots: $($cleanHotspots.Count)"
Write-Host "Total: $($final.Count)"

Write-Host "`nFiles with issues:" -ForegroundColor Cyan
$final | Select-Object -ExpandProperty file -Unique | Sort-Object | ForEach-Object {
    Write-Host "  $_"
}