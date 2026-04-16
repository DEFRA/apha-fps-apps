# Idempotency and Re-entrancy Strategy

This document defines idempotency expectations and implementation patterns for batch jobs in the Apha.BatchJobs system.

## Idempotency Requirement

Every batch job **MUST** be safely re-executable without duplicating data or creating unwanted side effects.

### Why Idempotency Matters

- **Retries**: Transient failures (network, timeout) trigger automatic retries
- **Manual Re-runs**: Operators may re-execute jobs after partial failures
- **Cold start edge cases**: Incomplete execution on shutdown requires clean restart
- **Race conditions**: Concurrent triggers or late deliveries must not corrupt state

---

## Idempotency Strategy Patterns

Choose ONE primary strategy per job. All strategies assume the execution boundary (database transaction or distributed lock) is held during execution.

### Strategy 1: Upsert (Recommended for most jobs)

**When to use**: Job output is deterministic and side-effect-free (e.g., load reference data, update materialized views).

**Implementation**:
```csharp
// Pseudo-code: Upsert existing data or insert new
var existing = await db.LoadAsync(idempotencyKey);
if (existing == null)
{
    await db.InsertAsync(newData);
}
else
{
    // Update only if needed, or skip if identical
    if (existing.Hash != newData.Hash)
        await db.UpdateAsync(idempotencyKey, newData);
}
```

**Idempotency boundary**: Database
- Single execution record per RunId ensures no duplicate writes
- Repeated execution on same RunId overwrites or skips
- **Safe for external systems**: None (data is internal only)

**Edge cases covered**:
- Partial commit + retry: Same upsert logic applies, benign reduplicate
- Concurrent duplicate trigger: Lock prevents concurrent execution

---

### Strategy 2: Dedup Key (For incremental or append-only operations)

**When to use**: Job appends/increments data based on unique event identifier (e.g., import batch records, ledger entries).

**Implementation**:
```csharp
// Pseudo-code: Check if already processed, then append
var deduplicationKey = $"{jobName}_{externalEventId}_{runDate}";
var processed = await db.IsDedupKeyProcessedAsync(deduplicationKey);

if (!processed)
{
    await db.AppendAsync(newData);
    await db.RecordDedupKeyAsync(deduplicationKey, runId);
}
```

**Idempotency boundary**: Database dedup table
- Cross-job dedup table prevents duplicates across retries and concurrent triggers
- Each job records unique dedup key on first successful write
- **Safe for external systems**: Depends on external API idempotency

**Edge cases covered**:
- Partial commit + retry: Dedup key check prevents re-append
- Concurrent duplicate trigger: First to write wins, second is skipped
- External API retry: Caller (orchestrator) must use same RunId for consistent retry

**Constraints**:
- Dedup key must be globally unique (include job name, event ID, date)
- Dedup table must be transaction-isolated (READ_COMMITTED minimum)

---

### Strategy 3: Checkpointing (For long-running or multi-step jobs)

**When to use**: Job processes in phases and can resume after failure (e.g., bulk data transformation, ETL with intermediate steps).

**Implementation**:
```csharp
// Pseudo-code: Resume from checkpoint
var checkpoint = await db.GetCheckpointAsync(runId);
var startFromPhase = checkpoint?.Phase ?? 1;

for (int phase = startFromPhase; phase <= totalPhases; phase++)
{
    await ExecutePhaseAsync(phase);
    await db.SaveCheckpointAsync(runId, phase);
}
```

**Idempotency boundary**: Checkpoint table + transaction per phase
- Each phase is atomic: either completes fully or rolls back
- Retry restarts from the last completed phase
- **Safe for external systems**: Yes, if phase writes are deterministic

**Edge cases covered**:
- Partial commit + retry: Failed phase re-executes from checkpoint
- Shutdown during phase: Checkpoint already persisted, restart continues cleanly
- Concurrent duplicate trigger: Lock ensures only one execution per job

**Constraints**:
- Each phase must be independent and re-executable (idempotent itself)
- Phase output must be deterministic (no random IDs without idempotency key)
- Checkpoint state must survive process crash (persisted before phase completion)

---

## Applying the Strategy: Integration with Batch Jobs Framework

### 1. Define Strategy in Job Class

```csharp
public sealed class ImportReferenceDataJob : IBatchJob
{
    public string JobName => "ImportReferenceData";
    
    /// <summary>
    /// Idempotency strategy: Upsert.
    /// - Safe idempotency boundary: single execution per RunId
    /// - External side effects: None (data is read-only reference)
    /// </summary>
    public IdempotencyStrategy Strategy => IdempotencyStrategy.Upsert;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // Job implementation using Upsert pattern
    }
}
```

### 2. Orchestrator Enforcement

The JobOrchestrator automatically enforces:
- Single lock per job prevents concurrent execution
- Execution record (RunId) is persisted before job starts
- On retry, same execution record is reused (not a new one)

**Retry behavior**:
```
Attempt 1: ExecutionId=1, RunId=abc123 → FAILED
  [Implicit retry delay with jitter]
Attempt 2: ExecutionId=1, RunId=abc123 → SUCCEEDS
  (Same RunId, can safely reuse database keys)
```

### 3. Idempotency Key Construction

When implementing Strategy 2 (Dedup Key), construct the key carefully:

**❌ Wrong**: Just external event ID
```csharp
var dedupKey = externalEventId;  // Not unique across jobs/dates
```

**✓ Correct**: Globally unique within domain
```csharp
var dedupKey = $"ImportReferenceData_{sourceSystemId}_{importDate:yyyy-MM-dd}_{externalEventId}";
```

---

## Boundary Clarification: Database vs External Systems

### Database-Only Jobs (Strategy: Upsert or Checkpointing)

**Examples**: Load reference data, rebuild materialized views, update denormalized caches.

**Idempotency guarantee**:
- RunId + lock ensure single execution per cycle
- Upsert/checkpointing ensures deterministic state
- **No external API calls** (or calls are read-only)

### External System Integration (Strategy: Dedup Key)

**Examples**: Send notifications, trigger external workflows, commit to payment gateway.

**Idempotency guarantee**:
- External API **must support idempotency key header** (e.g., `Idempotency-Key: RunId`)
- Dedup table prevents duplicate processing locally
- External system sees same RunId for all retries → handles duplication itself

**Implementation**:
```csharp
// Include RunId as idempotency key for external API
var response = await _externalApi.SubmitAsync(payload, 
    idempotencyKey: executionRecord.RunId);

// Record dedup key after success (confirms external write)
await db.RecordDedupKeyAsync($"external_api_{jobName}", runId);
```

---

## Testing Idempotency

### Test Scenario: Re-entrancy After Success

```csharp
[Fact]
public async Task ExecuteSameJobTwice_ProducesIdempotentResult()
{
    var runId = Guid.NewGuid().ToString("N");
    var job = new ImportReferenceDataJob();

    // First execution
    var result1 = await job.ExecuteAsync(runId);
    var count1 = await db.CountResultRowsAsync(runId);

    // Second execution with same RunId (simulated retry)
    var result2 = await job.ExecuteAsync(runId);
    var count2 = await db.CountResultRowsAsync(runId);

    // Output must be identical
    Assert.Equal(result1.Status, result2.Status);
    Assert.Equal(count1, count2);
    Assert.Equal(0, count2 - count1);  // No additional rows
}
```

### Test Scenario: Concurrent Duplicate Trigger

```csharp
[Fact]
public async Task ConcurrentDuplicateTrigger_OnlyOneCompletes()
{
    var jobName = "ImportReferenceData";
    
    // Simulate two triggers with tiny delay
    var task1 = _orchestrator.RunAsync(jobName, RunMode.Scheduled, default);
    await Task.Delay(10);
    var task2 = _orchestrator.RunAsync(jobName, RunMode.Scheduled, default);

    var results = await Task.WhenAll(task1, task2);

    // One succeeds (exit 0), one is skipped (exit 4 = lock contention)
    Assert.Contains(results, r => r.Status == JobStatus.Completed);
    Assert.Contains(results, r => r.Status == JobStatus.Skipped);
}
```

### Test Scenario: Partial Commit + Retry

```csharp
[Fact]
public async Task PartialCommit_RetryRecoveredCleanly()
{
    var runId = Guid.NewGuid().ToString("N");
    var job = new ImportReferenceDataJob();

    // First attempt: Simulate crash after partial write
    try
    {
        await job.ExecuteAsync(runId, forceCrashAfterRecords: 5);
    }
    catch { /* Expected */ }

    // Verify partial state exists
    var partialCount = await db.CountResultRowsAsync(runId);
    Assert.Equal(5, partialCount);

    // Second attempt with same RunId (retry)
    await job.ExecuteAsync(runId);

    // Final state must be complete and correct (upserted, not doubled)
    var finalCount = await db.CountResultRowsAsync(runId);
    Assert.Equal(expectedTotalRows, finalCount);
    Assert.Equal(0, finalCount - expectedTotalRows);
}
```

---

## Checklist for New Jobs

When implementing a new batch job:

- [ ] Choose and document idempotency strategy (Upsert / Dedup / Checkpoint)
- [ ] Clarify idempotency boundary: database-only or external systems?
- [ ] If external API calls: Verify external API supports idempotency keys
- [ ] Implement idempotent data writes (upsert, dedup check, or checkpoint)
- [ ] Add unit test: same job executed twice produces identical result
- [ ] Add integration test: concurrent triggers with lock contention
- [ ] Document idempotency key construction if using Dedup strategy
- [ ] Add comments in code explaining which strategy is used and why

---

## Future Enhancements

- **Per-job idempotency metadata**: Store strategy and key construction rules in database
- **Automated idempotency key validation**: CI checks that external APIs are called with Idempotency-Key header
- **Audit trail**: Log all retries with attempt number and outcome for operations visibility
- **Chaos testing**: Inject failures mid-transaction and verify recovery

