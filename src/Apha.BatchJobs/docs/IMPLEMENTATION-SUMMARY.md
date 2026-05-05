# MABArchive & Reorganization Implementation Summary

Date: 2026-04-29
Branch: B-ScheduledJobs

## What Has Been Completed

### 1. Folder Structure Reorganization ✅

**New Directory Structure**:
```
Apha.BatchJobs.Application/Jobs/
├── ScheduledJobs/
│   ├── ScheduledLoadFromFps/  (existing, moved)
│   └── MABArchive/             (new)
│       └── Services/
└── ManualJobs/
    ├── FECProcess/             (moved)
    ├── HealthCheck/            (moved)
    └── RecreateSummaries/      (moved)
```

### 2. MABArchive Job Implementation ✅

#### Core Job Handler
- **MabArchiveJobHandler.cs** - implements IBatchJob
  - Schedule: `cron(0 20 ? * MON-FRI *)` (8 PM weekdays)
  - Lock-based concurrency control
  - Transaction management
  - Failure handling

#### Orchestration
- **MabArchiveLoadOrchestrator.cs** - orchestrates load sequence
  - Year determination (month-based logic)
  - Full Load vs Partial Refresh branching
  - Service invocation in deterministic order
  - Transaction lifecycle management

#### Execution Context
- **MabArchiveExecutionContext.cs** - immutable execution state
  - CurrentYear, PreviousYear, CurrentMonth
  - PrimaryYear (computed from month)
  - PartialRefreshYear flag & value

#### Service Interfaces
- **IReloadFpsTotalsService.cs** - rebuild FPS source totals
- **IMyFpsYearlyDataService.cs** - manage yearly archive data
- **IEmailNotificationService.cs** - send failure notifications

#### Service Implementations
- **ReloadFpsTotalsService.cs** - delete and recreate fps.fpsyeartotals
- **MyFpsYearlyDataService.cs** - delete/load archive data in dependency order, partial refresh
- **EmailNotificationService.cs** - stub for email notifications (Phase 2)

#### Configuration
- **MabArchiveSettings.cs** - configuration model with lock/transaction timeouts, SMTP settings, notification flags

### 3. Documentation ✅

- **MABARCHIVE-IMPLEMENTATION-AND-REORGANIZATION.md**
  - Complete architecture overview
  - Folder reorganization rationale
  - Implementation phases (1, 2, 3)
  - DI integration guide
  - Testing strategy
  - Rollback plan

- **PROCEDURE-COVERAGE-MATRIX.md** (previously created)
  - Full 32-row procedure-to-job mapping
  - Implementation status per procedure
  - Scheduling safety guidelines

## What Still Needs to Be Done

### Phase 2 Implementation

1. **Service Enhancement**
   - [ ] Implement full sp_createFPSTotals formula parity in ReloadFpsTotalsService
     - Cost component calculations (additional, animal, staff, test costs)
     - Join query parity with legacy qry* views
   - [ ] Implement remaining 22 sp_AddMY_* loaders in MyFpsYearlyDataService
   - [ ] Real SMTP integration in EmailNotificationService

2. **Cross-Validation Engine**
   - [ ] Implement full assertion suite (13+ checks)
   - [ ] Link assertions to repository validation tables
   - [ ] Add pre/post-load reconciliation

3. **Code Path Updates**
   - [ ] Update project references (.csproj) if using file links
   - [ ] Add MabArchive DI registrations to ServiceCollectionSetup.cs
   - [ ] Update README.md to reflect new folder structure and job list

4. **Testing**
   - [ ] Unit tests for MabArchiveLoadOrchestrator
   - [ ] Integration tests with test database
   - [ ] Transaction rollback failure scenario
   - [ ] Lock contention handling
   - [ ] Email notification on failure

### Phase 3 Implementation

1. **AWS Integration**
   - [ ] Create EventBridge Scheduler rule
   - [ ] Configure ECS task definition with MABArchive job
   - [ ] CloudWatch log group setup
   - [ ] SNS/SES email delivery configuration

2. **Monitoring & Alerting**
   - [ ] CloudWatch dashboards for job execution
   - [ ] CloudWatch alarms for failures
   - [ ] Log group retention policies

3. **Deployment**
   - [ ] Container image build
   - [ ] AWS ECR push
   - [ ] Task role/permissions setup

## Files Created

### Application Layer
```
src/Apha.BatchJobs/Apha.BatchJobs.Application/Jobs/ScheduledJobs/MABArchive/
├── MabArchiveJobHandler.cs
├── MabArchiveLoadOrchestrator.cs
├── MabArchiveExecutionContext.cs
└── Services/
    ├── IReloadFpsTotalsService.cs
    ├── IMyFpsYearlyDataService.cs
    └── IEmailNotificationService.cs
```

### Domain Layer
```
src/Apha.BatchJobs/Apha.BatchJobs.Domain/Configuration/
└── MabArchiveSettings.cs
```

### Infrastructure Layer
```
src/Apha.BatchJobs/Apha.BatchJobs.Infrastructure/Repositories/MabArchive/
├── ReloadFpsTotalsService.cs
├── MyFpsYearlyDataService.cs
└── EmailNotificationService.cs
```

### Documentation
```
src/Apha.BatchJobs/docs/
├── MABARCHIVE-IMPLEMENTATION-AND-REORGANIZATION.md (new)
└── PROCEDURE-COVERAGE-MATRIX.md (previous)
```

## Next Steps (Recommended)

1. **Immediate (this session)**
   - [ ] Review generated code for correctness
   - [ ] Verify folder path structure in VS Code
   - [ ] Add DI registrations to ServiceCollectionSetup.cs
   - [ ] Run `dotnet build` to validate no compilation errors

2. **Short-term (next few days)**
   - [ ] Implement Phase 2 service enhancements
   - [ ] Add unit and integration tests
   - [ ] Verify transaction semantics

3. **Medium-term (next sprint)**
   - [ ] Full AWS deployment preparation
   - [ ] Production readiness checklist

## Verification Commands

```powershell
# Verify folder structure
Get-ChildItem -Path "src/Apha.BatchJobs/Apha.BatchJobs.Application/Jobs" -Recurse -Directory

# Build and validate
dotnet build src/Apha.BatchJobs/BatchJobs.sln

# Check DI registration errors (after adding registrations)
dotnet run --project src/Apha.BatchJobs/Apha.BatchJobs.Worker
```

## Key Decision Points

1. **Folder Structure**: ✅ Approved (ScheduledJobs vs ManualJobs separation)
2. **Year Determination Logic**: ✅ Confirmed (month > 4 vs ≤ 4)
3. **Transaction Boundary**: ✅ Confirmed (single transaction for all operations)
4. **Lock Mechanism**: ✅ Using existing IBatchLockRepository
5. **Service Interfaces**: ✅ Defined and segregated by responsibility

## Related Documentation

- [PROCEDURE-COVERAGE-MATRIX.md](./PROCEDURE-COVERAGE-MATRIX.md)
- [SP-TO-DOTNET-PARITY-TRACKER.md](./SP-TO-DOTNET-PARITY-TRACKER.md)
- [SCHEDULED-LOAD-FPS-DATA-FLOW-AND-CALCULATIONS.md](./SCHEDULED-LOAD-FPS-DATA-FLOW-AND-CALCULATIONS.md)

---

**Status**: Phase 1 Complete, Ready for Phase 2  
**Owner**: Batch Jobs Development Team  
**Last Updated**: 2026-04-29
