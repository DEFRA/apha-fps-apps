# Observability and Correlation Strategy

This document defines the structured logging schema, correlation fields, and dashboard queries for end-to-end observability of batch job operations.

## Structured Log Schema

All log entries in the Apha.BatchJobs system **MUST** be structured (JSON/key-value format), never plain text concatenation.

### Core Correlation Fields (on every log line)

These fields enable querying any run end-to-end by RunId alone:

| Field | Type | Example | Presence |
|-------|------|---------|----------|
| `RunId` | string | `a1b2c3d4e5f6` | ✅ Always |
| `JobName` | string | `ImportSalesOrders` | ✅ Always |
| `ExecutionId` | int | `12345` | ✅ After DB record created |
| `RunMode` | string | `Scheduled` or `Manual` | ✅ Always |
| `Attempt` | int | `1`, `2`, `3` | ✅ In retry loops |
| `Status` | string | `Running`, `Completed`, `Failed`, `Cancelled` | ✅ On status change |

---

## Log Levels and Signal Discipline

Enforce strict log level semantics for ops clarity:

| Level | Signal | Example |
|-------|--------|---------|
| **Info** | Normal flow, successful transitions | Job started, lock acquired, attempt succeeded, job completed |
| **Warning** | Transient issues, retries, temporary state changes | Retrying after timeout, lock not immediately acquired, dependency outage detected then recovered |
| **Error** | Terminal failures that stop execution | Non-retryable exception, retries exhausted, configuration error |

**❌ Anti-pattern**: Everything logged as Info or mixed levels
```csharp
// WRONG: Retry logged as Error
logger.LogError(ex, "Retry after timeout");

// WRONG: Non-error logged as Error
logger.LogError("Job completed successfully");
```

**✓ Correct**: Discipline filtering
```csharp
logger.LogWarning(ex, "Transient timeout, retrying | Attempt={Attempt}", attempt);
logger.LogInformation("Job completed successfully | Status={Status}", "Completed");
logger.LogError(ex, "Non-retryable exception | ExceptionType={Type}", ex.GetType().Name);
```

---

## Per-Layer Log Schema

### Worker Layer (Program.cs)

**Startup log**:
```
{
  "EventType": "WorkerStarted",
  "Timestamp": "2026-04-16T10:30:45.123Z",
  "ProcessId": 1234,
  "Environment": "Development",
  "JobName": "ImportSalesOrders",
  "RunMode": "Scheduled",
  "RunId": "a1b2c3d4e5f6"
}
```

**Summary log** (emitted once per run in finally block):
```json
{
  "EventType": "RunCompleted",
  "RunId": "a1b2c3d4e5f6",
  "ExecutionId": 12345,
  "JobName": "ImportSalesOrders",
  "RunMode": "Scheduled",
  "Outcome": "Succeeded",
  "FailureCategory": "None",
  "ExitCode": 0,
  "Message": "Job completed successfully within the graceful shutdown window.",
  "StartedAt": "2026-04-16T10:30:45.123Z",
  "EndedAt": "2026-04-16T10:31:02.456Z",
  "TotalDurationMs": 17333,
  "GracefulShutdownCompleted": true,
  "Level": "Information"
}
```

**Cancellation log** (on SIGTERM or timeout):
```json
{
  "EventType": "JobCancelled",
  "RunId": "a1b2c3d4e5f6",
  "JobName": "ImportSalesOrders",
  "RemainingShutdownWindowMs": 12000,
  "Level": "Warning"
}
```

---

### Orchestrator Layer (JobOrchestrator.cs)

**Execution start**:
```json
{
  "EventType": "ExecutionStarted",
  "RunId": "a1b2c3d4e5f6",
  "JobName": "ImportSalesOrders",
  "RunMode": "Scheduled"
}
```

**Retry classification**:
```json
{
  "EventType": "ExceptionClassified",
  "RunId": "a1b2c3d4e5f6",
  "JobName": "ImportSalesOrders",
  "Attempt": 1,
  "ExceptionType": "TimeoutException",
  "IsRetryable": true,
  "Level": "Information"
}
```

**Retry attempt**:
```json
{
  "EventType": "RetryDelayed",
  "RunId": "a1b2c3d4e5f6",
  "JobName": "ImportSalesOrders",
  "Attempt": 1,
  "TotalAttempts": 3,
  "ExceptionType": "TimeoutException",
  "RetryDelaySeconds": 60,
  "JitterSeconds": 28,
  "Level": "Warning"
}
```

**Execution completed**:
```json
{
  "EventType": "ExecutionCompleted",
  "RunId": "a1b2c3d4e5f6",
  "ExecutionId": 12345,
  "JobName": "ImportSalesOrders",
  "Status": "Completed",
  "Duration": "00:00:17.333",
  "Level": "Information"
}
```

---

### Repository Layer (Data access)

**Lock acquired**:
```json
{
  "EventType": "LockAcquired",
  "RunId": "a1b2c3d4e5f6",
  "JobName": "ImportSalesOrders",
  "LockTimeoutSeconds": 3600
}
```

**Lock released**:
```json
{
  "EventType": "LockReleased",
  "RunId": "a1b2c3d4e5f6",
  "JobName": "ImportSalesOrders"
}
```

**Execution record persisted**:
```json
{
  "EventType": "ExecutionRecordCreated",
  "RunId": "a1b2c3d4e5f6",
  "ExecutionId": 12345,
  "JobName": "ImportSalesOrders",
  "Status": "Running"
}
```

---

## Correlation Flow Across Boundaries

### Challenge: Preserving Correlation Across Async/Task Boundaries

When using `Task.Run`, `Task.Delay`, or other async operations, correlation fields may be lost if not explicitly propagated.

**❌ Wrong**: Correlation lost in async context
```csharp
public async Task ExecuteJobAsync(string runId)
{
    await Task.Run(() =>
    {
        // runId is captured, but not accessible in logging scope
        logger.LogInformation("Processing...");  // No RunId in this log!
    });
}
```

**✓ Correct**: Inject correlation into logger or scope
```csharp
public async Task ExecuteJobAsync(string runId)
{
    using var scope = logger.BeginScope(new Dictionary<string, object>
    {
        ["RunId"] = runId,
        ["JobName"] = "ImportSalesOrders"
    });

    await Task.Run(() =>
    {
        logger.LogInformation("Processing...");  // RunId included via scope
    });
}
```

### Implementation Pattern: Scoped Correlation

In the Orchestrator, establish correlation scope at entry:

```csharp
public async Task<JobExecutionResult> RunAsync(
    string jobName,
    RunMode runMode,
    CancellationToken cancellationToken)
{
    var runId = Guid.NewGuid().ToString("N");
    
    // Begin correlation scope for entire run
    using var scope = _logger.BeginScope(new Dictionary<string, object>
    {
        ["RunId"] = runId,
        ["JobName"] = jobName,
        ["RunMode"] = runMode.ToString()
    });

    _logger.LogInformation("Job started");  // Includes RunId, JobName, RunMode

    // All logs within this scope automatically include correlation fields
    var lockAcquired = await _lockRepository.TryAcquireLockAsync(jobName, runId, timeout);
    
    // ... job execution ...
    
    return result;  // All logs up to here have RunId propagated
}
```

---

## CloudWatch and App Insights Queries

### Query 1: All Logs for a Single Run

**CloudWatch Insights**:
```
fields @timestamp, @message, Level, Status, FailureCategory
| filter RunId = "a1b2c3d4e5f6"
| sort @timestamp asc
```

**Result**: Complete timeline of one run, queryable by RunId alone.

---

### Query 2: Failed Runs in Last Hour

**CloudWatch Insights**:
```
fields @timestamp, RunId, JobName, FailureCategory, ExitCode
| filter EventType = "RunCompleted" and ExitCode != 0
| filter @timestamp > ago(1h)
| sort @timestamp desc
```

**Result**: All failures in last hour, grouped by failure category.

---

### Query 3: Retry Exhaustion Events

**CloudWatch Insights**:
```
fields @timestamp, RunId, JobName, Attempt, ExceptionType
| filter EventType = "ExceptionClassified" and IsRetryable = false
| stats count() as non_retryable_count, count_distinct(RunId) as affected_runs by JobName
```

**Result**: Jobs with non-retryable errors (likely config issues).

---

### Query 4: Graceful Shutdown Success Rate

**CloudWatch Insights**:
```
fields @timestamp, GracefulShutdownCompleted
| filter EventType = "RunCompleted"
| stats count() as total_runs,
        sum(case when GracefulShutdownCompleted = true then 1 else 0 end) as graceful_runs
| fields total_runs, graceful_runs,
         round(graceful_runs * 100.0 / total_runs, 2) as success_rate_percent
```

**Result**: Percentage of runs that completed graceful shutdown within window.

---

### Query 5: Dependency Outage Detection

**CloudWatch Insights**:
```
fields @timestamp, RunId, JobName, FailureCategory
| filter EventType = "RunCompleted" and FailureCategory = "DependencyOutage"
| stats count() as outage_count, pct(@timestamp, 50) as median_time by JobName
```

**Result**: Which jobs are impacted by dependency outages and frequency.

---

### Query 6: Lock Contention Analysis

**CloudWatch Insights**:
```
fields @timestamp, RunId, JobName
| filter EventType = "ExecutionStarted"
| stats count() as total_runs, 
        sum(case when Status = "Skipped" then 1 else 0 end) as skipped_runs
        by JobName, RunMode
| filter skipped_runs > 0
```

**Result**: Jobs with high lock contention (frequent skips).

---

### Query 7: App Insights: Query Retry Patterns

**App Insights KQL**:
```kusto
customEvents
| where name == "ExceptionClassified"
| extend RunId = tostring(customDimensions.RunId),
         JobName = tostring(customDimensions.JobName),
         IsRetryable = tobool(customDimensions.IsRetryable),
         Attempt = tostring(customDimensions.Attempt)
| summarize retry_count = count() by JobName, IsRetryable
| order by retry_count desc
```

**Result**: Jobs with highest retry rates (potential instability).

---

## Dashboard Example: CloudWatch (AWS Console)

Create a custom dashboard with these widgets:

1. **Run Success Rate (Line graph)**
   - Query: Success count vs Total count over time
   - Y-axis: Percentage
   - Helps identify degradation periods

2. **Failure Category Breakdown (Pie chart)**
   - Query: Count by FailureCategory
   - Shows which failure types dominate

3. **Graceful Shutdown Metric (Number widget)**
   - Query: Sum of GracefulShutdownCompleted / Total, formatted as %
   - Immediate ops visibility

4. **Recent Failures (Table)**
   - Query: Latest 10 failed runs with RunId, JobName, FailureCategory, ExitCode
   - Click RunId to drill down to full logs

5. **Retry Exhaustion Alert (Number widget)**
   - Query: Count of runs with RetryAttemptsExhausted = true
   - Threshold alert if > 5 in last hour

---

## Implementation Checklist

- [ ] All log entries use structured key-value format (JSON)
- [ ] Every log includes RunId and JobName (via scope or explicit field)
- [ ] ExecutionId added after DB record is created
- [ ] Attempt number logged in retry loops
- [ ] Status field logged on status transitions
- [ ] Log level discipline: Info for normal, Warning for transient, Error for terminal
- [ ] Correlation scope established at orchestrator entry point
- [ ] Dashboard queries defined in CloudWatch or App Insights
- [ ] Team trained on how to query by RunId for ops troubleshooting
- [ ] Runbooks reference the log schema and dashboard queries

---

## Future Enhancements

- **Distributed tracing**: Integrate OpenTelemetry for cross-service correlation (if jobs call external services)
- **Custom metrics**: Track graceful_shutdown_completed, retry_jitter_seconds, total_retry_duration_seconds
- **Anomaly detection**: Automated alerts on high error rate or long durations
- **Log retention policy**: Archive logs after 90 days, keep summary events for 1 year
- **Performance baseline**: Dashboard showing p50, p95, p99 job duration per job type

