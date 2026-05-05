# ScheduledLoadFromFps Implementation Status

## Overview
- Status date: 2026-04-17
- Solution area: BatchJobs ScheduledLoadFromFps
- Overall status: In progress

## Phase Completion Matrix

### Phase 1: Tables
- Complete for current runtime footprint in `fps` + `mabarchive`.
- EF mappings are in place for control tables and source/archive fixtures.

### Phase 2: Seed Data
- `database/sql/seeds/001_seed_scheduled_job_master.sql` implemented.
- `database/sql/seeds/002_seed_scheduled_source_baseline.sql` implemented.
- `database/sql/seeds/003_seed_scheduled_validation_baseline.sql` implemented.

### Phase 3: Business Logic
- 5-step plan builder implemented.
- Step handlers implemented:
  - ProcessPreviousYearTotals
  - ProcessCurrentYearTotals
  - DeleteYearsFpsData
  - AddYearsFpsData
  - HandleCurrentYearProjectAll
- Orchestrated run/step audit persistence implemented through `IScheduledLoadFromFpsRepository`.
- Cross-validation engine (12+ assertions) is pending.

### Phase 4: Tests
- Phase 1 schema tests implemented.
- Phase 2 seed-data tests implemented.
- Handler unit tests implemented (baseline and cutover behavior).
- Orchestration tests implemented (sequence, context propagation, failure path, conditional branch).
- E2E scenario suite still pending.

### Phase 5: Flush/Reset Scripts
- `database/sql/flush/002_flush_scheduled_load_tables.sql` implemented.
- `database/sql/reset_scheduled_load_locally.sh` implemented and aligned with targeted flush + reseed flow.

### Phase 6: Documentation
- This implementation status document implemented.
- Local execution runbook implemented.
- Batch framework reference implemented.

## Code Location Map

| Component | Path | Status |
|---|---|---|
| Plan builder | `Apha.BatchJobs.Application/Jobs/ScheduledLoadFromFps/ScheduledLoadFromFpsPlanBuilder.cs` | Complete |
| Job handler | `Apha.BatchJobs.Application/Jobs/ScheduledLoadFromFps/ScheduledLoadFromFpsJobHandler.cs` | In progress (validation engine pending) |
| Step handlers | `Apha.BatchJobs.Application/Jobs/ScheduledLoadFromFps/Handlers/` | Complete |
| Runtime repository | `Apha.BatchJobs.Infrastructure/Repositories/ScheduledLoadFromFpsRepository.cs` | Complete |
| DI wiring | `Apha.BatchJobs.Worker/DependencyInjection.cs` | Complete |
| Story 4 test files | `Apha.BatchJobs.UnitTests/ScheduledLoadFromFps/` | In progress |

## Known Limitations
- Cross-validation engine is not yet implemented.
- E2E test scenarios are not yet implemented.
- Runtime verification via `dotnet test` depends on .NET SDK availability in the host/container.

## Next Steps
1. Implement Story 3.6 cross-validation engine and assertion persistence.
2. Implement Story 4.5 E2E scenarios.
3. Run full local verification cycle (flush, seed, execute job, verify outputs).
4. Finalize documentation updates in the phase plan and testing guide.
