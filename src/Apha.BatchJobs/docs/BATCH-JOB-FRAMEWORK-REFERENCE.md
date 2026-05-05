# Batch Job Framework Reference

## Overview
This reference describes how to build and register batch jobs using the foundation framework in this repository.

## 1) Create a New Job
Implement `IBatchJob` with:
- `Name`: stable unique job name.
- `IdempotencyStrategy`: explicit strategy description.
- `ExecuteAsync`: orchestration-safe async execution.

Guidance:
- Keep business logic in dedicated services/handlers.
- Keep `ExecuteAsync` focused on sequencing and error boundaries.

## 2) Register Job in DI
Jobs are discovered by scanning `IBatchJob` implementations in application assembly.
Ensure the class is concrete and included in compiled project.

Main registrations live in:
- `Apha.BatchJobs.Worker/DependencyInjection.cs`

## 3) Job Factory Usage
`IBatchJobFactory` resolves jobs by `Name`.
Behavior:
- Throws on unknown job name.
- Throws on duplicate handler names.

## 4) Correlation Service
Use `ICorrelationService` to assign and propagate correlation IDs across run lifecycle, step logs, and repositories.

Patterns:
- Read existing correlation ID first.
- Generate if missing.
- Persist in run-level audit record.

## 5) Repository Patterns
Use repository interfaces for DB access.

Repository design expectations:
- Async-only methods with `CancellationToken`.
- Strictly bounded responsibilities (run lifecycle, step audit, domain DML).
- SQL should be idempotent where possible (`ON CONFLICT`, deterministic year-slice delete/load).

## 6) Exit Code Contract
Batch execution uses `ExitCode` enum in domain:
- `Success`
- `GeneralError`
- `ConfigurationError`
- `DatabaseError`
- `ValidationError`
- `UnhandledException`

Map failures deterministically and avoid swallowing exceptions.

## 7) Orchestration Patterns

### Sequential pattern
Execute ordered steps from plan builder and stop on first failure.

### Conditional pattern
Plan builder decides inclusion/exclusion of conditional steps (for example cutover month rule).

### Parallel pattern
Use only when data dependencies are independent and audit model can represent concurrent execution.
Current ScheduledLoadFromFps flow is sequential.

## 8) Testing Strategy

### Unit tests
- Handler logic in isolation.
- Branch behavior (cutover and error paths).
- Audit side-effects through repository mocks/substitutes.

### Integration tests
- Repository SQL behavior against local Postgres.
- Seed/read/flush cycle validation.

### E2E tests
- Full orchestrator run with real data.
- Validate run record, step record, and output tables.

## 9) Adding a New Scheduled Job Checklist
1. Add `IBatchJob` implementation.
2. Add step enum/plan builder if job is multi-step.
3. Add repository interfaces and concrete implementations.
4. Wire DI registrations.
5. Add seed and flush support as needed.
6. Add unit + integration + E2E tests.
7. Add runbook and status documentation.
