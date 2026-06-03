---
title: "PACT API + EventBridge Integration Guide"
version: "1.0"
audience: "PACT Team, DevOps, Production Deployment"
---

# PACT API + EventBridge Integration Guide

## Overview

Your PACT API implementation is **already aligned for EventBridge production use**. The codebase supports both:
- **Local Development**: Direct worker process dispatch (no EventBridge needed)
- **Production**: AWS EventBridge for cloud-scale job triggering

This document confirms alignment and explains the configuration.

---

## Current Architecture

### Local Development (No EventBridge)

```
User/UI
  ↓
POST /api/v1/batch-jobs/trigger
  ↓
BatchJobTriggerController.Trigger()
  ↓
LocalWorkerProcessTriggerDispatcher.DispatchAsync()
  ├─ Checks TriggerDispatch:Mode = "LocalProcess"
  ├─ Spawns .NET worker process locally
  └─ Returns eventId = "localproc-{ProcessId}"
  ↓
Worker Process
  ├─ jobName: RecreateSummaries
  ├─ jobExecutionId: {UUID from trigger}
  └─ Writes to fps.job_queue table
  ↓
Status Endpoint
  ├─ Queries fps.job_queue (authoritative)
  └─ Returns currentState: "Pending" / "Running" / etc.
```

**Configuration** (appsettings.Development.json):
```json
{
  "TriggerDispatch": {
    "Mode": "LocalProcess"
  }
}
```

### Production (EventBridge)

```
User/UI (EventGrid or external caller)
  ↓
POST /api/v1/batch-jobs/trigger
  ↓
BatchJobTriggerController.Trigger()
  ↓
EventBridgeTriggerDispatcher.DispatchAsync()
  ├─ Checks TriggerDispatch:Mode != "LocalProcess" (default)
  ├─ Publishes BatchTriggerEventDetail to AWS EventBridge
  │   ├─ jobExecutionId: {UUID}
  │   ├─ jobName: RecreateSummaries
  │   ├─ requestedBy: user@example.com
  │   └─ acceptedAtUtc: 2026-06-03T14:30:00.000Z
  └─ Returns eventId = {AWS EventBridge Event ID}
  ↓
AWS EventBridge (configured rule)
  ├─ Matches on jobName
  ├─ Routes to ECS Task Definition
  └─ Passes event detail to worker container
  ↓
ECS Fargate Worker
  ├─ Container image: apha/batchjobs:latest
  ├─ Environment: BATCH_JOB_NAME, BATCH_JOBQUEUE_ID, etc.
  ├─ Connection: RDS PostgreSQL (batch_jobs_foundation_db_cloud)
  └─ Writes to fps.job_queue table
  ↓
Status Endpoint
  ├─ Queries RDS fps.job_queue (authoritative)
  └─ Returns currentState: "Pending" / "Running" / etc.
```

**Configuration** (appsettings.Production.json):
```json
{
  "TriggerDispatch": {
    "Mode": "EventBridge"
  },
  "EventBridge": {
    "EventBusName": "default",
    "DetailType": "BatchJobTrigger",
    "Source": "apha.pact.api"
  },
  "ConnectionStrings": {
    "BatchJobsConnectionString": "Host=rds-endpoint;Database=batch_jobs_foundation_db_cloud;..."
  }
}
```

---

## Dispatcher Resolution Logic

### Code (Program.cs)

```csharp
builder.Services.AddScoped<ITriggerDispatcher>(serviceProvider =>
{
    var options = serviceProvider.GetRequiredService<IOptions<TriggerDispatchOptions>>().Value;
    var environment = serviceProvider.GetRequiredService<IHostEnvironment>();
    var logger = loggerFactory.CreateLogger("TriggerDispatcherResolver");

    // LOCAL DEVELOPMENT: Use process-based dispatch if mode is "LocalProcess"
    if (string.Equals(options.Mode, "LocalProcess", StringComparison.OrdinalIgnoreCase))
    {
        if (environment.IsDevelopment() || environment.IsEnvironment("Local"))
        {
            logger.LogInformation("Dispatch mode: LocalProcess");
            return serviceProvider.GetRequiredService<LocalWorkerProcessTriggerDispatcher>();
        }

        // Safety: Don't allow LocalProcess outside dev/local
        logger.LogWarning("LocalProcess not allowed in {Environment}; falling back to EventBridge", environment.EnvironmentName);
    }

    // PRODUCTION: Use EventBridge (default)
    logger.LogInformation("Dispatch mode: EventBridge");
    return serviceProvider.GetRequiredService<EventBridgeTriggerDispatcher>();
});
```

### Decision Tree

```
Is TriggerDispatch:Mode = "LocalProcess"?
    ├─ YES → Is environment Development or Local?
    │        ├─ YES → Use LocalWorkerProcessTriggerDispatcher ✅
    │        └─ NO → Fall back to EventBridge (safety) ⚠️
    └─ NO → Use EventBridgeTriggerDispatcher ✅
```

---

## API Contract Is Environment-Agnostic

**CRITICAL**: The PACT API contract is **identical** regardless of dispatch mechanism:

### Client Perspective (Same for Both Local + Production)

**Trigger Request**:
```http
POST /api/v1/batch-jobs/trigger
Content-Type: application/json

{
  "jobName": "RecreateSummaries",
  "requestedBy": "user@example.com"
}
```

**Trigger Response (Both Local + Production)**:
```http
HTTP 202 Accepted
Content-Type: application/json

{
  "accepted": true,
  "jobName": "RecreateSummaries",
  "jobExecutionId": "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c",
  "eventId": "localproc-12345" (local) OR "{AWS Event ID}" (prod),
  "acceptedAtUtc": "2026-06-03T14:30:00.000Z",
  "message": "Trigger accepted..."
}
```

**Status Query (Both Local + Production)**:
```http
GET /api/batch-jobs/RecreateSummaries/status

HTTP 200 OK
{
  "jobName": "RecreateSummaries",
  "isRunning": true,
  "sourceOfTruth": "BatchJobs",
  "correlatedJobExecutionId": "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c",
  "lastExecution": {
    "currentState": "Running",    ← Same field name in both paths
    "startDateTime": "2026-06-03T14:30:00.000Z",
    ...
  },
  "startupWatchdog": null
}
```

**Key Point**: Whether the worker was dispatched via local process or EventBridge, **the database queries return identical results**.

---

## Watchdog SLA Configuration

The watchdog SLA deadline is **environment-aware**:

```csharp
// PACT API status computation
var slaSeconds = isProd ? 600 : 180;  // 10 min prod, 3 min dev
var deadline = acceptedAtUtc.AddSeconds(slaSeconds);
var projectedState = now > deadline ? "StartFailedTimeout" : "TriggerAccepted";
```

| Environment | SLA Seconds | Use Case | Rationale |
|-------------|------------|----------|-----------|
| **Development (Local)** | 180 (3 min) | Fast feedback on failures | Local processes start quickly |
| **Production (AWS)** | 600 (10 min) | Account for ECS cold start | Container spin-up + network latency |

Both paths respect the same watchdog contract:
- If DB record appears before deadline → watchdog returns null (execution visible)
- If deadline exceeded with no DB record → watchdog projects "StartFailedTimeout"

---

## Alignment Verification Checklist

### ✅ Code Structure Verified

- [x] `ITriggerDispatcher` interface (abstraction)
- [x] `LocalWorkerProcessTriggerDispatcher` implementation (dev)
- [x] `EventBridgeTriggerDispatcher` implementation (prod)
- [x] Dependency injection resolver (Program.cs)
- [x] `TriggerDispatchOptions.Mode` configuration switch
- [x] Safety fallback (LocalProcess only in dev)

### ✅ API Contract Verified

- [x] POST /api/v1/batch-jobs/trigger returns 202 Accepted
- [x] Response includes `jobExecutionId` (correlation ID)
- [x] Response includes `acceptedAtUtc` (watchdog reference)
- [x] GET /api/batch-jobs/{jobName}/status returns 200 OK
- [x] Status response includes `lastExecution` (DB state)
- [x] Status response includes `startupWatchdog` (projection)
- [x] Both dispatchers produce identical status responses

### ✅ Database Contract Verified

- [x] Both dispatchers write to same `fps.job_queue` table
- [x] Same `statusid` foreign key to `fps.job_status`
- [x] Watchdog queries same table for state visibility
- [x] No dispatcher-specific DB schema differences

### ✅ Configuration Verified

- [x] `TriggerDispatch:Mode` configuration switch exists
- [x] Default behavior is EventBridge (safe for production)
- [x] LocalProcess only active in Development environment
- [x] Error logging guides operations teams

---

## Deployment Recommendations

### Local/Demo Deployment

```json
// appsettings.Development.json
{
  "ASPNETCORE_ENVIRONMENT": "Development",
  "TriggerDispatch": {
    "Mode": "LocalProcess"
  },
  "ConnectionStrings": {
    "BatchJobsConnectionString": "Host=localhost;Database=batch_jobs_foundation_db_cloud;..."
  }
}
```

**Result**: PACT API triggers worker as local .NET process

### Production Deployment (EventBridge)

```json
// appsettings.Production.json
{
  "ASPNETCORE_ENVIRONMENT": "Production",
  "TriggerDispatch": {
    "Mode": "EventBridge"
  },
  "EventBridge": {
    "EventBusName": "default",
    "DetailType": "BatchJobTrigger",
    "Source": "apha.pact.api"
  },
  "ConnectionStrings": {
    "BatchJobsConnectionString": "Host=rds-prod.aws.com;Database=batch_jobs_foundation_db_cloud;..."
  }
}
```

**Prerequisites**:
- [ ] AWS EventBridge configured with rule(s) matching `jobName`
- [ ] EventBridge rule targets ECS Task Definition
- [ ] ECS Task Definition has `apha/batchjobs` container image
- [ ] Task role has IAM permissions to write to RDS
- [ ] RDS PostgreSQL endpoint configured
- [ ] PACT API has IAM role to publish to EventBridge
- [ ] Networking: PACT API → EventBridge, ECS worker → RDS

---

## Migration Path: Local → Production

### Step 1: Verify Local Works
```bash
# Local: appsettings.Development.json
# TriggerDispatch.Mode = "LocalProcess"

dotnet run --project Apha.BatchJobs.Pact.Api/Apha.BatchJobs.Pact.Api.csproj

# Test: POST http://localhost:5189/api/v1/batch-jobs/trigger
# Expect: HTTP 202, eventId = "localproc-{PID}"
```

### Step 2: Test EventBridge in Staging
```bash
# Staging: appsettings.Staging.json (or via environment variables)
# TriggerDispatch.Mode = "EventBridge"
# EventBridge.EventBusName = "staging-bus"

# Deploy to ECS
aws ecs register-task-definition --cli-input-json file://task-def.json
aws ecs create-service --cluster staging --service-name pact-api ...

# Test: POST https://pact-api-staging.example.com/api/v1/batch-jobs/trigger
# Expect: HTTP 202, eventId = "{AWS Event ID}"
# Monitor: CloudWatch Logs for EventBridge delivery
```

### Step 3: Deploy to Production
```bash
# Production: appsettings.Production.json
# TriggerDispatch.Mode = "EventBridge"
# EventBridge.EventBusName = "default"

# Deploy to ECS
aws ecs register-task-definition --cli-input-json file://task-def-prod.json
aws ecs create-service --cluster production --service-name pact-api ...
```

---

## Troubleshooting

### Issue: "Dispatch mode LocalProcess not allowed in Production"

**Cause**: Configuration has `Mode: "LocalProcess"` but environment is Production

**Fix**: Change configuration to EventBridge or remove the LocalProcess setting

### Issue: EventBridge events not reaching worker

**Causes**:
1. EventBridge rule not configured for jobName
2. ECS Task Definition not set as rule target
3. Task role missing IAM permissions
4. Worker environment variables not set correctly

**Debug**:
```bash
# Check EventBridge events
aws events list-rules --event-bus-name default
aws events list-targets-by-rule --rule {rule-name}

# Check CloudWatch Logs
aws logs tail /aws/ecs/pact-api --follow

# Check task definition
aws ecs describe-task-definition --task-definition pact-api:latest
```

### Issue: Watchdog timeout happening too early (180s in production)

**Cause**: Environment not set to Production, using dev SLA (180s)

**Fix**: Ensure `ASPNETCORE_ENVIRONMENT=Production` is set in ECS task

---

## Summary

✅ **Your PACT API is production-ready for EventBridge**:

1. **Abstraction**: ITriggerDispatcher interface allows multiple implementations
2. **Configuration**: TriggerDispatch:Mode switches behavior without code changes
3. **Safety**: LocalProcess only active in Development
4. **Fallback**: Production defaults to EventBridge
5. **Contract**: API response identical regardless of dispatcher
6. **Database**: Both dispatchers write to same source of truth
7. **Watchdog**: SLA adjusts per environment (180s dev, 600s prod)

**For production deployment**: Set `TriggerDispatch:Mode` to EventBridge (or omit it—default is EventBridge) and configure EventBridge rule routing.

---

**Document Version**: 1.0  
**Date**: 2026-06-03  
**Status**: ✅ Current implementation verified
