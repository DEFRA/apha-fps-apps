# MABArchive Job Implementation & Folder Reorganization

Status date: 2026-04-29
Scope: New MABArchive scheduled job + reorganization of existing job folders

## Overview

This document describes the folder reorganization of the batch jobs structure and the implementation of the new MABArchive scheduled job for loading FPS data into the MABArchive schema for financial reporting.

## Folder Reorganization

### Previous Structure
```
Apha.BatchJobs.Application/Jobs/
├── FECProcess/
├── HealthCheck/
├── RecreateSummaries/
├── ScheduledLoadFromFps/
└── ScheduleJobs/
```

### New Structure
```
Apha.BatchJobs.Application/Jobs/
├── ScheduledJobs/                 # Automated scheduled workloads
│   ├── ScheduledLoadFromFps/       # FPS yearly batch load (existing)
│   └── MABArchive/                 # MABArchive yearly load (new)
│       └── Services/
└── ManualJobs/                     # User-triggered / on-demand workloads
    ├── FECProcess/
    ├── HealthCheck/
    └── RecreateSummaries/
```

### Rationale

**ScheduledJobs Folder**
- Houses jobs that execute on a fixed schedule via AWS EventBridge Scheduler.
- Deterministic, time-driven execution (e.g., "8 PM every weekday").
- Current members: ScheduledLoadFromFps (Mon-Sat 8 PM), MABArchive (Mon-Fri 8 PM).

**ManualJobs Folder**
- Houses jobs triggered on-demand via REST API or console execution.
- No fixed schedule; execution is user-initiated.
- Current members: FECProcess, HealthCheck, RecreateSummaries.
- ScheduleJobs (previously a placeholder) is deprecated; scheduling responsibility moves to MABArchive.

### Migration Notes

1. **ScheduleJobs folder** is removed (placeholder now subsumed into MABArchive orchestration).
2. **ScheduledLoadFromFps** moves to `ScheduledJobs/ScheduledLoadFromFps/` (no code changes, only path).
3. **FECProcess, HealthCheck, RecreateSummaries** move to `ManualJobs/` (no code changes, only path).

## MABArchive Job Implementation

### Purpose

Processes and loads FPS data from the current and previous calendar years to support financial year reporting into the MABArchive schema within PostgreSQL database.

**Execution Schedule**: Weekly on weekdays (Monday-Friday) at 8:00 PM UTC.

### Architecture

#### Class Hierarchy

```
IBatchJob (interface)
    └── MabArchiveJobHandler
            ├── MabArchiveLoadOrchestrator
            │   ├── IReloadFpsTotalsService
            │   ├── IMyFpsYearlyDataService
            │   └── IEmailNotificationService
            └── IBatchLockRepository
```

#### Key Components

1. **MabArchiveJobHandler.cs** (Apha.BatchJobs.Application/Jobs/ScheduledJobs/MABArchive/)
   - Implements IBatchJob contract.
   - Entry point for scheduled execution.
   - Manages lock acquisition/release and transaction boundaries.
   - Properties:
     - Name: "MABArchive"
     - Schedule: `cron(0 20 ? * MON-FRI *)` (8 PM, weekdays)
     - MaxExecutionSeconds: 1800 (30 minutes)
     - IdempotencyStrategy: "YearScopedRebuildWithDeterministicOrdering"

2. **MabArchiveLoadOrchestrator.cs**
   - Orchestrates the complete load sequence.
   - Builds execution context based on current calendar month.
   - Executes year determination logic.
   - Manages transaction lifecycle.
   - Invokes services in deterministic order.

3. **MabArchiveExecutionContext.cs**
   - Immutable record holding computed execution state.
   - Properties:
     - CurrentYear, PreviousYear, CurrentMonth
     - PrimaryYear (determined by month)
     - RequiresPartialRefresh (boolean)
     - PartialRefreshYear (current year if month ≤ 4)

4. **Services** (Interfaces + Implementations)

   a. **IReloadFpsTotalsService** → ReloadFpsTotalsService
   - Rebuilds FPS source totals for a specified year.
   - Deletes and recreates fps.fpsyeartotals.
   - Mirrors legacy sp_createFPSTotals logic.

   b. **IMyFpsYearlyDataService** → MyFpsYearlyDataService
   - DeleteYearDataAsync(): deletes archive rows for a year (all 24 tables, dependency order).
   - LoadYearDataAsync(): loads fresh data from FPS source into archive tables.
   - RefreshProjectAllOnlyAsync(): partial refresh of my_tlkpproject_all only.

   c. **IEmailNotificationService** → EmailNotificationService
   - Sends email notifications on job failure.
   - Includes RunId, job name, error details, and diagnostic guidance.

5. **MabArchiveSettings.cs** (Apha.BatchJobs.Domain/Configuration/)
   - Configuration model for the MABArchive job.
   - Properties:
     - LockTimeoutSeconds (default: 3600)
     - TransactionTimeoutSeconds (default: 1800)
     - AdminNotificationEmail
     - NotificationFromEmail
     - SmtpHost, SmtpPort
     - EnableEmailNotifications
     - CloudWatchLogGroup

### Execution Flow

#### Year Determination

Based on current calendar month:

- **Month > 4 (after April)**
  - PrimaryYear = current calendar year
  - Full Load on current year only
  - No partial refresh

- **Month ≤ 4 (April or earlier)**
  - PrimaryYear = previous calendar year
  - Full Load on previous year
  - Partial Refresh on current calendar year

#### Full Load Sequence (PrimaryYear — always)

1. **Rebuild Source Totals**
   - ReloadFpsTotalsService.RebuildSourceTotalsAsync()
   - Deletes and recreates fps.fpsyeartotals for the year

2. **Delete Archive Data**
   - MyFpsYearlyDataService.DeleteYearDataAsync()
   - Removes archive rows for the year across all 24 tables
   - Respects foreign key dependency order

3. **Load Archive Data**
   - MyFpsYearlyDataService.LoadYearDataAsync()
   - Executes 24 sequential INSERT-SELECT operations
   - Copies FPS source data into archive tables

#### Partial Refresh (Current Year — month ≤ 4 only)

- **MyFpsYearlyDataService.RefreshProjectAllOnlyAsync()**
- Deletes and reloads my_tlkpproject_all only
- No other archive tables touched

#### Transaction & Lock Lifecycle

1. Lock acquired before any work begins (tbljobqueue as lock table).
2. Single transaction opened.
3. All year operations execute within transaction.
4. If any step fails, entire transaction rolled back.
5. Lock released in finally block (success or failure).

#### Failure & Notification

- On unhandled exception: transaction rolled back.
- EmailNotificationService dispatches alert to administrator.
- Email includes: RunId, job name, error message, timestamp.
- Guidance provided for CloudWatch Logs filtering by RunId.

### File Structure

```
Apha.BatchJobs/
├── Apha.BatchJobs.Application/
│   └── Jobs/
│       └── ScheduledJobs/
│           └── MABArchive/
│               ├── MabArchiveJobHandler.cs
│               ├── MabArchiveLoadOrchestrator.cs
│               ├── MabArchiveExecutionContext.cs
│               └── Services/
│                   ├── IReloadFpsTotalsService.cs
│                   ├── IMyFpsYearlyDataService.cs
│                   └── IEmailNotificationService.cs
├── Apha.BatchJobs.Domain/
│   └── Configuration/
│       └── MabArchiveSettings.cs
└── Apha.BatchJobs.Infrastructure/
    └── Repositories/
        └── MabArchive/
            ├── ReloadFpsTotalsService.cs
            ├── MyFpsYearlyDataService.cs
            └── EmailNotificationService.cs
```

## Dependency Injection Integration

### Required Registrations (to be added to ServiceCollectionSetup.cs)

```csharp
// Configuration
services.Configure<MabArchiveSettings>(config.GetSection("MabArchive"));

// Services
services.AddScoped<IReloadFpsTotalsService, ReloadFpsTotalsService>();
services.AddScoped<IMyFpsYearlyDataService, MyFpsYearlyDataService>();
services.AddScoped<IEmailNotificationService, EmailNotificationService>();

// Orchestrator
services.AddScoped<MabArchiveLoadOrchestrator>();

// Job Handler (auto-discovered via IBatchJob scan)
// MabArchiveJobHandler will be auto-registered as part of existing IBatchJob assembly scan
```

### appsettings.json Configuration

```json
{
  "MabArchive": {
    "LockTimeoutSeconds": 3600,
    "TransactionTimeoutSeconds": 1800,
    "AdminNotificationEmail": "batch-admin@example.com",
    "NotificationFromEmail": "batch-jobs@example.com",
    "SmtpHost": "smtp.example.com",
    "SmtpPort": 587,
    "EnableEmailNotifications": true,
    "CloudWatchLogGroup": "/aws/ecs/batch-jobs"
  }
}
```

## Implementation Phases

### Phase 1 (Current)
- ✅ Folder reorganization complete.
- ✅ MABArchive job handler skeleton implemented.
- ✅ Orchestrator with month-based year determination implemented.
- ✅ Service interfaces defined.
- ⚠️ Service implementations: partial (key paths, stub formulas).
- ⏳ Full formula parity for sp_createFPSTotals (pending).
- ⏳ Full 24-table fan-out for LoadYearDataAsync (pending).
- ⏳ SMTP integration for EmailNotificationService (pending).

### Phase 2 (Planned)
- Implement full formula parity for sp_createFPSTotals (cost component calculations).
- Implement remaining 22 sp_AddMY_* loaders.
- Full cross-validation assertion engine.
- Real SMTP email notification delivery.

### Phase 3 (Planned)
- Containerization and deployment to AWS ECS Fargate.
- EventBridge Scheduler rule creation.
- CloudWatch alarms and monitoring setup.

## Testing Strategy

### Unit Tests
- MabArchiveLoadOrchestrator year determination logic.
- MabArchiveExecutionContext partial refresh determination.
- Service mock behavior validation.

### Integration Tests
- Full load sequence with real database (Phase 2).
- Partial refresh sequence (Phase 2).
- Transaction rollback on simulated failure (Phase 2).
- Lock contention handling (Phase 2).

### End-to-End Tests
- Local execution via `dotnet run -- --job MABArchive`.
- AWS ECS Fargate deployment simulation (Phase 3).

## Rollback & Migration Plan

### If Issues Occur

1. **Before Deployment**
   - Verify folder paths in project references (.csproj files).
   - Ensure DI registration is added to ServiceCollectionSetup.
   - Run `dotnet build` to validate no breaking changes.

2. **Rollback Steps**
   - Revert folder reorganization via git.
   - Comment out MabArchive DI registrations.
   - Re-run build and tests.

### Cross-reference Updates

The following documentation should be linked/updated:
- [src/Apha.BatchJobs/docs/PROCEDURE-COVERAGE-MATRIX.md](../PROCEDURE-COVERAGE-MATRIX.md)
- [src/Apha.BatchJobs/docs/SP-TO-DOTNET-PARITY-TRACKER.md](../SP-TO-DOTNET-PARITY-TRACKER.md)
- [src/Apha.BatchJobs/README.md](../../README.md) - update job list and schedule.

## References

- Legacy Procedure Analysis: [SCHEDULED-LOAD-FPS-DATA-FLOW-AND-CALCULATIONS.md](../SCHEDULED-LOAD-FPS-DATA-FLOW-AND-CALCULATIONS.md)
- Parity Tracker: [SP-TO-DOTNET-PARITY-TRACKER.md](../SP-TO-DOTNET-PARITY-TRACKER.md)
- Procedure Coverage: [PROCEDURE-COVERAGE-MATRIX.md](../PROCEDURE-COVERAGE-MATRIX.md)

## Owner & Contact

- Job Implementation: Batch Jobs Team
- Folder Structure: Apha Platform Architecture
- Infrastructure: Cloud Engineering

---

**Last Updated**: 2026-04-29
**Status**: Phase 1 Complete, Phase 2 Pending
**Branch**: B-ScheduledJobs
