# ReImagineAnalysis_v4 Framework vs Current Batch Job: Architecture Analysis

## 🏗️ ReImagineAnalysis_v4 Project Structure

The previous version (V3) provides **framework infrastructure** that the current project (Apha.BatchJobs) has adopted/evolved:

### Core Components Already in Current Project

| Component | V3 Location | Current Equivalent | Status |
|---|---|---|---|
| **IBatchJob interface** | Core/Interfaces/IBatchJob.cs | Apha.BatchJobs.Application/Interfaces/IBatchJob.cs | ✅ Same contract |
| **JobScheduler (Quartz)** | Infrastructure/Scheduling/QuartzJobScheduler.cs | Apha.BatchJobs.Worker (uses similar pattern) | ✅ Pattern adopted |
| **CliJobExecutor** | Host/Services/CliJobExecutor.cs | (embedded in Worker) | ✅ Pattern adopted |
| **CorrelationService** | Infrastructure/Services/CorrelationService.cs | Apha.BatchJobs.Application/Services/CorrelationService.cs | ✅ Ported |
| **DbContext** | Infrastructure/Data/BatchJobDbContext.cs | Apha.BatchJobs.Infrastructure/Data/BatchJobsDbContext.cs | ✅ Evolved |
| **Hosted Service** | Host/Services/SchedulerHostedService.cs | Apha.BatchJobs.Worker | ✅ Pattern adopted |
| **CommandLine Options** | Host/Configuration/CommandLineOptions.cs | (implicit in Worker Program.cs) | ✅ Pattern adopted |

---

## 🔧 Key Design Patterns from V3

### 1. **Exit Code Classification** (Core/Enums/ExitCode.cs)
```csharp
public enum ExitCode
{
    Success = 0,
    ValidationError = 1,           // Input validation failures
    ConfigurationError = 2,        // Configuration/setup issues
    GeneralError = 3,              // Runtime errors
    UnhandledException = 4         // Unhandled exceptions
}
```

**Current adoption**: ✅ Same pattern in Apha.BatchJobs.Domain/Enums/ExitCode.cs

---

### 2. **Service Resolution & Correlation ID Tracking**
```csharp
// V3 Pattern:
public async Task<ExitCode> ExecuteJobAsync(string jobName, CancellationToken cancellationToken)
{
    var correlationId = _correlationService.GenerateCorrelationId();
    _correlationService.SetCorrelationId(correlationId);
    
    await using var scope = _serviceProvider.CreateAsyncScope();
    var jobs = scope.ServiceProvider.GetServices<IBatchJob>();
    var job = jobs.FirstOrDefault(j => j.JobName.Equals(jobName, StringComparison.OrdinalIgnoreCase));
    
    var exitCode = await job.ExecuteAsync(cancellationToken).ConfigureAwait(false);
    return exitCode;
}
```

**Current adoption**: ✅ Pattern adopted in Apha.BatchJobs.Application/JobOrchestrator.cs

---

### 3. **AsyncLocal for Correlation ID** (Infrastructure/Services/CorrelationService.cs)
```csharp
// V3 Pattern:
private static readonly AsyncLocal<string?> _correlationId = new();

public string GetCorrelationId()
{
    return _correlationId.Value ?? _correlationId.Value = GenerateCorrelationId();
}

public void SetCorrelationId(string correlationId)
{
    _correlationId.Value = correlationId;
}
```

**Why AsyncLocal?**
- Thread-safe across async contexts
- Preserves correlation ID across await boundaries
- No need to pass correlation ID parameter through every method call

**Current adoption**: ✅ Same pattern in Apha.BatchJobs.Application/Services/CorrelationService.cs

---

### 4. **PostgreSQL snake_case Naming Convention** (Infrastructure/Data/BatchJobDbContext.cs)
```csharp
// V3 Pattern:
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    foreach (var entity in modelBuilder.Model.GetEntityTypes())
    {
        // Convert PascalCase props to snake_case columns
        var tableName = entity.GetTableName();
        if (!string.IsNullOrEmpty(tableName))
            entity.SetTableName(tableName.ToSnakeCase());
        
        foreach (var property in entity.GetProperties())
        {
            var columnName = property.GetColumnName();
            if (!string.IsNullOrEmpty(columnName))
                property.SetColumnName(columnName.ToSnakeCase());
        }
        // ...apply to indexes, keys, FKs
    }
}
```

**Current adoption**: ✅ Explicit column mapping in Apha.BatchJobs.Infrastructure/Data/BatchJobsDbContext.cs (lines 48-100)

---

### 5. **Quartz Job Wrapper Pattern** (Infrastructure/Scheduling/QuartzJobWrapper.cs)
```csharp
// V3 Pattern:
public class QuartzJobWrapper : IJob
{
    public const string JobTypeKey = "JobType";
    
    public async Task Execute(IJobExecutionContext context)
    {
        var correlationId = _correlationService.GenerateCorrelationId();
        _correlationService.SetCorrelationId(correlationId);
        
        await using var scope = _serviceProvider.CreateAsyncScope();
        var jobType = context.JobDetail.JobDataMap.GetString(JobTypeKey);
        var batchJob = scope.ServiceProvider.GetService(Type.GetType(jobType)) as IBatchJob;
        
        var exitCode = await batchJob.ExecuteAsync(context.CancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Job executed with exit code {ExitCode}", exitCode);
    }
}
```

**Purpose**: Bridge between Quartz.NET IJob interface and IBatchJob contract

**Current adoption**: ⚠️ Similar pattern exists, but not directly shown in current codebase

---

### 6. **Dependency Injection Setup** (Host/Extensions/HostDependencyInjection.cs)
```csharp
// V3 Pattern (inferred from structure):
public static class HostDependencyInjection
{
    public static IServiceCollection AddHostServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Register batch job implementations
        // services.AddScoped<IBatchJob, SpecificJobHandler>();
        
        // Register services
        services.AddScoped<CliJobExecutor>();
        services.AddScoped<IJobScheduler, QuartzJobScheduler>();
        
        return services;
    }
}
```

**Current adoption**: ✅ Pattern in Apha.BatchJobs.Worker/DependencyInjection.cs

---

## 🎯 What's Missing: Business Logic Integration

### V3 Doesn't Show:
- ❌ **Actual job handler implementations** (no ScheduledLoadFromFps handler)
- ❌ **Database-level step execution** (no repository wiring)
- ❌ **Cross-validation logic** (no assertion queries)
- ❌ **Stored procedure porting** (skeleton only)

### Current Project Must Implement:
1. **ScheduledLoadFromFpsJobHandler** — Already has skeleton in place ✅
2. **5 Step Handlers** — Domain logic for each phase (ProcessPreviousYearTotals, etc.)
3. **7 New Table DDL** — Missing from migration
4. **EF Core Entities** — Missing mappings for fps_year_totals, etc.
5. **Repositories** — IScheduledLoadRunRepository, IScheduledLoadValidationResultRepository, etc.

---

## 🏆 Architecture Lessons from V3

### Strengths of V3 Design
1. **Clear separation of concerns**
   - Core (interfaces, enums)
   - Infrastructure (DbContext, repositories, services)
   - Host (CLI/scheduler hosting)
   
2. **Correlation ID isolation**
   - Uses AsyncLocal for thread-safe propagation
   - No need to pass through all method parameters
   
3. **Exit code standardization**
   - 5 predefined codes for common scenarios
   - Enables monitoring and alerting on process exit

4. **Quartz abstraction**
   - QuartzJobWrapper delegates to IBatchJob
   - Decouples scheduler from business logic

5. **CLI + Scheduler dual mode**
   - Can run jobs on-demand or scheduled
   - Single codebase for both execution paths

---

## 📋 Implementation Checklist for Current Project

### Phase 1: Data Layer (MUST DO FIRST)
- [ ] Create migration `004_scheduled_load_tables.sql` with 7 table DDL
- [ ] Create EF Core entities for 7 tables
- [ ] Add DbSet mappings to BatchJobsDbContext
- [ ] Create repository interfaces + implementations

### Phase 2: Step Handlers (MIDDLEWARE PHASE)
- [ ] ProcessPreviousYearTotals handler (archive + truncate)
- [ ] ProcessCurrentYearTotals handler (load new data)
- [ ] DeleteYearsFpsData handler (cleanup retention)
- [ ] AddYearsFpsData handler (multi-year insert)
- [ ] HandleCurrentYearProjectAll handler (snapshot)

### Phase 3: Validation (QUALITY GATE)
- [ ] Cross-validation query pack (12+ assertions)
- [ ] ScheduledLoadValidationResultRepository
- [ ] Assertion result logging

### Phase 4: Integration (ORCHESTRATION WIRING)
- [ ] Wire handlers into ScheduledLoadFromFpsJobHandler
- [ ] Replace no-op placeholders with real business logic
- [ ] Add handler factory pattern (if needed)

### Phase 5: Testing
- [ ] Create seed data for scenario testing
- [ ] Create flush scripts for local reset
- [ ] Integration test suite

---

## 🔗 V3 ↔ Current Project Mapping

```
ReImagineAnalysis_v4 (V3)               →    Apha.BatchJobs (Current)
─────────────────────────────────────         ─────────────────────────────
Core/                                  →     Apha.BatchJobs.Domain/
  Interfaces/IBatchJob.cs              →     Application/Interfaces/IBatchJob.cs
  Enums/ExitCode.cs                    →     Domain/Enums/ExitCode.cs

Infrastructure/                        →     Apha.BatchJobs.Infrastructure/
  Data/BatchJobDbContext.cs            →     Data/BatchJobsDbContext.cs
  Services/CorrelationService.cs       →     Application/Services/CorrelationService.cs
  Scheduling/QuartzJobScheduler.cs     →     (embedded in Worker pattern)

Host/                                  →     Apha.BatchJobs.Worker/
  Program.cs                           →     Program.cs
  Services/CliJobExecutor.cs           →     DependencyInjection.cs patterns
  Services/SchedulerHostedService.cs   →     (HostedService pattern)
```

---

## 💡 Key Takeaway

**V3 provides the architectural foundation; the current project extends it with:**
1. Multi-tenant job planning (ScheduledLoadFromFpsPlanBuilder)
2. Scheduled-specific orchestration (5-step pipeline)
3. Deep database integration (7 new tables + cross-validation)
4. Rich idempotency strategy (YearScopedRebuildWithDeterministicOrdering)

**Next step**: Port the stored procedure business logic from tech-details.txt into the 5 step handlers, backed by the 7 new tables.
