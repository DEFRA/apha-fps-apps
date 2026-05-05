# Implementation Action Checklist

Date: 2026-04-29

## ✅ Completed

### Folder Structure
- [x] Created ScheduledJobs/ directory
- [x] Created ScheduledJobs/ScheduledLoadFromFps/ directory structure
- [x] Created ScheduledJobs/MABArchive/Services/ directory structure
- [x] Created ManualJobs/ directory
- [x] Created ManualJobs/FECProcess/ directory
- [x] Created ManualJobs/HealthCheck/ directory
- [x] Created ManualJobs/RecreateSummaries/ directory

### MABArchive Core Files
- [x] MabArchiveJobHandler.cs - IBatchJob implementation
- [x] MabArchiveLoadOrchestrator.cs - orchestration logic
- [x] MabArchiveExecutionContext.cs - immutable execution state
- [x] MabArchiveSettings.cs - configuration model

### MABArchive Service Interfaces
- [x] IReloadFpsTotalsService.cs
- [x] IMyFpsYearlyDataService.cs
- [x] IEmailNotificationService.cs

### MABArchive Service Implementations
- [x] ReloadFpsTotalsService.cs
- [x] MyFpsYearlyDataService.cs
- [x] EmailNotificationService.cs

### Documentation
- [x] MABARCHIVE-IMPLEMENTATION-AND-REORGANIZATION.md
- [x] PROCEDURE-COVERAGE-MATRIX.md
- [x] IMPLEMENTATION-SUMMARY.md

---

## ⏳ TODO - Next Steps (Manual)

### 1. Migrate Existing Job Files
```
Old Location                              New Location
─────────────────────────────────────────────────────────────
Jobs/FECProcess/*              →         Jobs/ManualJobs/FECProcess/*
Jobs/HealthCheck/*             →         Jobs/ManualJobs/HealthCheck/*
Jobs/RecreateSummaries/*       →         Jobs/ManualJobs/RecreateSummaries/*
Jobs/ScheduledLoadFromFps/*    →         Jobs/ScheduledJobs/ScheduledLoadFromFps/*
```

**Action**: Move these directories to their new locations.

### 2. Delete Obsolete Folders
- [ ] Delete `Jobs/ScheduleJobs/` (functionality moved to MABArchive)
- [ ] Remove old `Jobs/ScheduledLoadFromFps/` if successfully moved

### 3. Update Project References (.csproj files)

Check and update any file references that include these folders:
- Apha.BatchJobs.Application.csproj
- Apha.BatchJobs.Infrastructure.csproj

### 4. Add DI Registrations

**File**: `src/Apha.BatchJobs/Apha.BatchJobs.Application/DependencyInjection/ServiceCollectionSetup.cs`

**Add these registrations in `ConfigureBatchJobServices()` method**:

```csharp
// MABArchive Configuration
services.Configure<MabArchiveSettings>(config.GetSection("MabArchive"));

// MABArchive Services
services.AddScoped<IReloadFpsTotalsService, ReloadFpsTotalsService>();
services.AddScoped<IMyFpsYearlyDataService, MyFpsYearlyDataService>();
services.AddScoped<IEmailNotificationService, EmailNotificationService>();

// MABArchive Orchestrator
services.AddScoped<MabArchiveLoadOrchestrator>();

// MabArchiveJobHandler will be auto-discovered via IBatchJob assembly scan
```

### 5. Update Configuration

**File**: `src/Apha.BatchJobs/appsettings.json`

**Add MabArchive section**:

```json
{
  "MabArchive": {
    "LockTimeoutSeconds": 3600,
    "TransactionTimeoutSeconds": 1800,
    "AdminNotificationEmail": "batch-admin@defra.gov.uk",
    "NotificationFromEmail": "batch-jobs@defra.gov.uk",
    "SmtpHost": "smtp.example.com",
    "SmtpPort": 587,
    "EnableEmailNotifications": true,
    "CloudWatchLogGroup": "/aws/ecs/batch-jobs"
  }
}
```

### 6. Compile & Test

```powershell
# Build solution
cd src/Apha.BatchJobs
dotnet build Apha.BatchJobs.sln

# Run unit tests
dotnet test Apha.BatchJobs.UnitTests.csproj

# Verify no compilation errors
dotnet build --no-restore
```

### 7. Update Documentation References

- [ ] Update [README.md](../../README.md) to reflect new folder structure
- [ ] Update job execution guide with MABArchive schedule
- [ ] Link IMPLEMENTATION-SUMMARY.md from main docs index
- [ ] Update PROCEDURE-COVERAGE-MATRIX.md with cross-reference to MABARCHIVE-IMPLEMENTATION-AND-REORGANIZATION.md

### 8. Code Review Checklist

- [ ] All DI registrations added
- [ ] No namespace conflicts
- [ ] All service implementations follow interface contracts
- [ ] Logging is consistent across all services
- [ ] Error handling is comprehensive
- [ ] Configuration keys match appsettings.json

---

## 📋 Testing Checklist (Phase 2)

### Unit Tests
- [ ] MabArchiveLoadOrchestrator year determination (month-based logic)
- [ ] MabArchiveExecutionContext partial refresh flags
- [ ] Year determination: month > 4 branch
- [ ] Year determination: month ≤ 4 branch

### Integration Tests
- [ ] Full Load sequence with test database
- [ ] Partial Refresh sequence with test database
- [ ] Transaction rollback on simulated failure
- [ ] Lock acquisition and release
- [ ] Lock contention handling

### End-to-End Tests
- [ ] Local execution: `dotnet run -- --job MABArchive`
- [ ] Verify RunId correlation in logs
- [ ] Verify lock table entries
- [ ] Verify archive table row counts

---

## 📊 Verification Steps

After completing the TODOs above, verify:

```powershell
# 1. Check all services are registered
$services = (Get-Content appsettings.json | ConvertFrom-Json)
$services.MabArchive | ConvertTo-Json

# 2. Build without errors
dotnet build src/Apha.BatchJobs/BatchJobs.sln

# 3. List available jobs
dotnet run --project src/Apha.BatchJobs/Apha.BatchJobs.Worker -- --list-jobs

# Expected output should include:
# - MABArchive
# - ScheduledLoadFromFps
# - FECProcess
# - HealthCheck
# - RecreateSummaries
```

---

## 📝 Document Cross-References

All new documentation is linked below for easy access:

1. **[MABARCHIVE-IMPLEMENTATION-AND-REORGANIZATION.md](./MABARCHIVE-IMPLEMENTATION-AND-REORGANIZATION.md)**
   - Complete architecture overview
   - DI integration guide
   - Implementation phases
   - Testing strategy

2. **[IMPLEMENTATION-SUMMARY.md](./IMPLEMENTATION-SUMMARY.md)**
   - Files created
   - Remaining work
   - Next steps
   - Verification commands

3. **[PROCEDURE-COVERAGE-MATRIX.md](./PROCEDURE-COVERAGE-MATRIX.md)**
   - 32-row procedure status matrix
   - Scheduling safety guidance
   - Governance rules

4. **[SP-TO-DOTNET-PARITY-TRACKER.md](./SP-TO-DOTNET-PARITY-TRACKER.md)**
   - Stored procedure parity status
   - Implementation gaps
   - Closure checklist

---

## 🎯 Success Criteria

- [x] Folder reorganization complete (ScheduledJobs vs ManualJobs)
- [x] MABArchive job handler implemented with lock/transaction management
- [x] Orchestrator with month-based year determination
- [x] Service interfaces and stubs defined
- [ ] DI registrations added (TODO)
- [ ] Code compiles without errors (TODO)
- [ ] All jobs auto-discovered by factory (TODO)
- [ ] No namespace or reference conflicts (TODO)
- [ ] Unit tests passing (Phase 2)
- [ ] Integration tests passing (Phase 2)

---

**Status**: Phase 1 Complete - Ready for Manual Migration Steps
**Estimated Time**: 1-2 hours to complete migration + testing
**Owner**: Batch Jobs Development Team

---

**Last Updated**: 2026-04-29
**Branch**: B-ScheduledJobs
**Approval**: Pending Code Review
