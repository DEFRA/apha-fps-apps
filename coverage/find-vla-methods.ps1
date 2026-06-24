Select-String -Path "D:\FPS-Project-Profitability-VLA-Regression\coverage\fps-app\coverage.cobertura.xml" -Pattern "VLA" | Select-Object -First 10 | ForEach-Object { Write-Output $_.Line.Trim() }
Write-Output "---fps-api---"
Select-String -Path "D:\FPS-Project-Profitability-VLA-Regression\coverage\fps-api\coverage.cobertura.xml" -Pattern "VLA" | Select-Object -First 10 | ForEach-Object { Write-Output $_.Line.Trim() }
Write-Output "---fpsapps-app---"
Select-String -Path "D:\FPS-Project-Profitability-VLA-Regression\coverage\fpsapps-app\coverage.cobertura.xml" -Pattern "VLA" | Select-Object -First 10 | ForEach-Object { Write-Output $_.Line.Trim() }
Write-Output "---fpsapps-infra---"
Select-String -Path "D:\FPS-Project-Profitability-VLA-Regression\coverage\fpsapps-infra\coverage.cobertura.xml" -Pattern "VLA" | Select-Object -First 10 | ForEach-Object { Write-Output $_.Line.Trim() }
