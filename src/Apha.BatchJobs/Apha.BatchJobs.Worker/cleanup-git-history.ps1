# Git History Cleanup Script - Remove Passwords
# Run this script to remove passwords from git history

Write-Host "========================================" -ForegroundColor Red
Write-Host "  GIT HISTORY CLEANUP - REMOVE SECRETS" -ForegroundColor Red
Write-Host "========================================" -ForegroundColor Red
Write-Host ""
Write-Host "ΓÜá∩╕Å  WARNING: This will rewrite git history!" -ForegroundColor Yellow
Write-Host ""
Write-Host "Before proceeding:" -ForegroundColor Yellow
Write-Host "1. Γ£à Create a backup of your repository" -ForegroundColor Gray
Write-Host "2. Γ£à Notify your team about the history rewrite" -ForegroundColor Gray
Write-Host "3. Γ£à Ensure everyone has pushed their changes" -ForegroundColor Gray
Write-Host "4. Γ£à You will need to FORCE PUSH after this" -ForegroundColor Gray
Write-Host ""

$confirm = Read-Host "Do you want to proceed? (Type 'YES' to continue)"

if ($confirm -ne "YES") {
    Write-Host "Cancelled. No changes made." -ForegroundColor Green
    exit 0
}

Write-Host ""
Write-Host "Step 1: Creating backup..." -ForegroundColor Cyan

$repoPath = "D:\Users\atos.user8\source\repos\apha-fps-apps-A-Foundation"
$backupPath = "D:\Users\atos.user8\source\repos\apha-fps-apps-A-Foundation-BACKUP-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

if (Test-Path $backupPath) {
    Write-Host "Backup already exists at: $backupPath" -ForegroundColor Yellow
} else {
    Copy-Item -Path $repoPath -Destination $backupPath -Recurse
    Write-Host "Γ£ô Backup created at: $backupPath" -ForegroundColor Green
}

Write-Host ""
Write-Host "Step 2: Rewriting history to remove passwords..." -ForegroundColor Cyan

cd $repoPath

# Create a temporary file with the passwords to replace
$replacementsFile = Join-Path $env:TEMP "git-replacements-$(Get-Date -Format 'yyyyMMddHHmmss').txt"

@"
Password=postgres
Password=your-production-password
your-production-db-host
batchjobuser
"@ | Out-File -FilePath $replacementsFile -Encoding UTF8

Write-Host "  Replacement file created: $replacementsFile" -ForegroundColor Gray

# Use git filter-branch to remove sensitive data
Write-Host "  Running git filter-branch..." -ForegroundColor Gray

$files = @(
    "src/Apha.BatchJobs/Apha.BatchJobs.Worker/appsettings.Development.json",
    "src/Apha.BatchJobs/Apha.BatchJobs.Worker/appsettings.Production.json"
)

foreach ($file in $files) {
    Write-Host "    Processing: $file" -ForegroundColor Gray

    git filter-branch --force --index-filter `
        "git rm --cached --ignore-unmatch '$file' || true" `
        --prune-empty --tag-name-filter cat -- --all 2>&1 | Out-Null
}

Write-Host "Γ£ô History rewritten" -ForegroundColor Green

Write-Host ""
Write-Host "Step 3: Cleaning up..." -ForegroundColor Cyan

git reflog expire --expire=now --all
git gc --prune=now --aggressive

Write-Host "Γ£ô Cleanup complete" -ForegroundColor Green

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  CLEANUP COMPLETED" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "NEXT STEPS:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Verify the cleanup:" -ForegroundColor Cyan
Write-Host "   git log -S 'Password=postgres' --all" -ForegroundColor Gray
Write-Host "   (Should return no results)" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Force push to remote:" -ForegroundColor Cyan
Write-Host "   git push origin A-Foundation --force-with-lease" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Team members must re-sync:" -ForegroundColor Cyan
Write-Host "   git fetch origin" -ForegroundColor Gray
Write-Host "   git reset --hard origin/A-Foundation" -ForegroundColor Gray
Write-Host ""
Write-Host "4. ROTATE ALL EXPOSED CREDENTIALS!" -ForegroundColor Red
Write-Host "   - Change database passwords" -ForegroundColor Gray
Write-Host "   - Update connection strings" -ForegroundColor Gray
Write-Host ""
Write-Host "Backup location: $backupPath" -ForegroundColor Yellow
Write-Host ""

Remove-Item $replacementsFile -Force
