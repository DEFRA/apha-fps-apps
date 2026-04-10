# Re-Architect Wave 1: Scheduled Orchestrator Submission Guide

## Foundation Baseline

- **Version:** v0.1.0-foundation
- **Projects:** AphaBatchJobs.Host, Application, Core, Infrastructure
- **Framework:** net8.0
- **DB:** PostgreSQL
- **Infra:** AWS ECS Fargate

## Objective

Convert sp_LoadFromFPS master orchestrator (and its 5 dependent procedures) from SQL Server to C# as the first Re-Architect wave via AppMod.

## Required Artifacts

### 1. V4 Foundation Codebase (Available in Workspace)

The foundation files already exist in [src/Apha.BatchJobs/ReImagineAnalysis_v4/](src/Apha.BatchJobs/ReImagineAnalysis_v4/):

- AphaBatchJobs.sln
- All 4 project files (.csproj) with net8.0 targets
- Program.cs with CLI argument parsing
- appsettings.json and appsettings.Development.json
- Core interfaces: IScheduledJob, IAdhocJob, IJobOrchestrator
- Application services: IJobRunnerService, JobRunnerService
- Infrastructure DI: ServiceCollectionExtensions.cs
- Dockerfile (multi-stage sdk:8.0 → runtime:8.0)

This is the base upon which AppMod will add Wave 1. After generation, AppMod output should merge into this structure.

### 2. Wave 1 Specification & SQL Reference (In tech-details.zip)

## AppMod Re-Architect Form Entry

### User Story Field (short, 150-200 words)

```
Given the AphaBatchJobs v0.1.0-foundation with four projects (Host, Application, Core, Infrastructure) 
and established Quartz scheduling, create ScheduledLoadFromFpsJob implementing IScheduledJob 
that orchestrates five stored procedure calls in strict sequence: (1) sp_DeleteFPSJobAdhocResults, 
(2) sp_LoadFPSTotals, (3) sp_RecreateYearData, (4) sp_LoadPreviousYearData, (5) sp_RecreateArchives. 
Each step must validate before proceeding. If any step fails, halt and log error, returning exit code 1. 
Return exit code 0 only if all 5 complete successfully. Timeout each step at 300 seconds. 
Register job in Infrastructure DI. Integrate into Application layer with full logging via ILogger<ScheduledLoadFromFpsJob>.
```

### Package Name

```
AphaBatchJobsWave1ScheduledOrchestrator
```

### Tech Stack

```
Dotnet8 PostgreSQL AWS
```

### Additional Info

```
[LEAVE BLANK]
```

### Upload File

Upload [src/Apha.BatchJobs/KBUploads/rearchitect_kb/tech-details.zip](src/Apha.BatchJobs/KBUploads/rearchitect_kb/tech-details.zip)

**Contents (single merged file):**
- **tech-details.txt** — Complete specification and SQL procedures reference:
  - Wave 1 orchestrator requirements (ScheduledLoadFromFpsJob spec)
  - 5-step execution sequence: sp_DeleteFPSJobAdhocResults → sp_LoadFPSTotals → sp_RecreateYearData → sp_LoadPreviousYearData → sp_RecreateArchives
  - Acceptance criteria (exit codes, logging, DI registration)
  - Full SQL procedure implementations (sp_createFPSTotals, sp_deleteFPSTotals, sp_LoadFromFPS master orchestrator, sp_AddYearsFPSData, sp_DeleteYearsFPSData)
  - References to all additional sp_AddMY_* procedures in the chain

## Expected Output from AppMod

AppMod will merge the following into the v4 foundation structure:

- **New file:** AphaBatchJobs.Application/Scheduled/ScheduledLoadFromFpsJob.cs (~200-250 lines) implementing IScheduledJob with 5-step orchestration
- **Modified file:** AphaBatchJobs.Infrastructure/Extensions/ServiceCollectionExtensions.cs (add DI registration for new job)
- **No changes to:** Program.cs, Core interfaces, existing Application files, config files, Dockerfile
- **Output structure:** Complete merged solution preserving v4 foundation + new Wave 1 job
- **Build target:** `dotnet build AphaBatchJobs.sln` should pass clean with new job registered
- **Efficacy target:** 85-90% (operational code ready to deploy or requiring minimal tweaks)

## Post-Generation Validation

After download:

1. Extract to folder: ReImagineAnalysis_Wave1Output
2. Verify structure: check ScheduledLoadFromFpsJob.cs exists
3. Check frameworks: all should be net8.0
4. Scan for prose: strip any trailing commentary after closing braces
5. Run build: `dotnet build AphaBatchJobs.sln` — should pass
6. Run with CLI: `dotnet run --project AphaBatchJobs.Host -- --scheduled` — should execute job

## Success Criteria

- ✅ All 5 stored procedures execute in strict order (DeleteFPSJobAdhocResults → LoadFPSTotals → RecreateYearData → LoadPreviousYearData → RecreateArchives)
- ✅ Exit code 0 on full success, 1 on failure, 2 on timeout
- ✅ Logging shows each step entry/exit with timing
- ✅ Correlation ID propagated through all logs
- ✅ Solution compiles and runs CLI triggers
- ✅ No placeholder methods or TODO comments
- ✅ DI registration auto-discovered by IEnumerable<IScheduledJob>

## If Build Fails Post-Generation

Common fixes after generation:

1. **csproj trailing content:** Strip everything after `</Project>` closing tag
2. **Duplicate constructors:** Keep primary constructor, remove secondary if detected
3. **Missing using statements:** Add `using Npgsql;` for DbContext if needed
4. **Package version mismatches:** Sync across projects to 8.0.1 for Extensions packages

## Next Phases

- **Wave 2:** sp_RecreateSummaries (adhoc orchestrator)
- **Wave 3:** Data loaders (sp_LoadFromFPS dependencies as individual jobs)
- **Wave 4:** Notifications and post-execution cleanup jobs
- **Post-ReArchitect:** Global net8.0 → net10.0 upgrade, tag v0.2.0-foundation-net10, release v1.0.0-batchjobs-ga
