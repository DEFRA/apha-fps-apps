$vlaMethods = @(
	'GetProjectsByProgramProjectProfitabilityVLAAsync',
	'GetProjectsByProjectGroupProjectProfitabilityVLAAsync'
)

$files = @(
	'D:\FPS-Project-Profitability-VLA-Regression\coverage\fps-app\coverage.cobertura.xml',
	'D:\FPS-Project-Profitability-VLA-Regression\coverage\fps-api\coverage.cobertura.xml',
	'D:\FPS-Project-Profitability-VLA-Regression\coverage\fpsapps-app\coverage.cobertura.xml',
	'D:\FPS-Project-Profitability-VLA-Regression\coverage\fpsapps-infra\coverage.cobertura.xml'
)

$results = @()

foreach ($file in $files) {
	if (-not (Test-Path $file)) { Write-Host "MISSING: $file"; continue }
	[xml]$xml = Get-Content $file
	$classes = $xml.coverage.packages.package.classes.class
	foreach ($class in $classes) {
		foreach ($method in $class.methods.method) {
			$mname = $method.name
			$matched = $vlaMethods | Where-Object { $mname -like "*$_*" }
			if ($matched) {
				$lines  = $method.lines.line
				$total   = ($lines | Measure-Object).Count
				$covered = ($lines | Where-Object { [int]$_.hits -gt 0 } | Measure-Object).Count
				$pct     = if ($total -gt 0) { [math]::Round(($covered / $total) * 100, 1) } else { 'N/A' }
				$results += [PSCustomObject]@{
					Assembly = (Split-Path (Split-Path $file -Parent) -Leaf)
					Class    = ($class.name -split '\.')[-1]
					Method   = $mname -replace '\(.*',''
					Covered  = $covered
					Total    = $total
					Pct      = $pct
					Pass     = if ($pct -ne 'N/A' -and [double]$pct -ge 90) { 'YES' } else { 'NO' }
				}
			}
		}
	}
}

Write-Host "`n=== VLA Method Coverage Report ===" -ForegroundColor Cyan
$results | Sort-Object Assembly, Class, Method | Format-Table -AutoSize

$failing = $results | Where-Object { $_.Pass -eq 'NO' }
if ($failing) {
	Write-Host "`n[BELOW 90%] The following methods need more tests:" -ForegroundColor Red
	$failing | Format-Table -AutoSize
} else {
	Write-Host "`n[ALL PASS] Every new VLA method is >= 90% covered." -ForegroundColor Green
}
