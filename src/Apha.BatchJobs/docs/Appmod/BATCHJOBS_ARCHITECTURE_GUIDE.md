# Apha.BatchJobs Architecture & Best Practices Guide

**Document Version:** 1.0  
**Date:** 2025  
**Target Framework:** .NET 10  
**Extracted from:** Apha.FPS Solution Analysis

---

## Table of Contents

1. [Architectural Patterns](#1-architectural-patterns)
2. [Project Structure & Naming](#2-project-structure--naming-conventions)
3. [Dependency Injection](#3-dependency-injection-pattern)
4. [Logging Standards](#4-logging-standards)
5. [Error Handling](#5-error-handling--exception-patterns)
6. [API Response Standardization](#6-api-response-standardization)
7. [Repository Pattern](#7-repository-pattern)
8. [Database Patterns](#8-database-patterns)
9. [AutoMapper Pattern](#9-automapper-pattern)
10. [Authentication & Authorization](#10-authentication--authorization)
11. [Testing Patterns](#11-testing-patterns)
12. [Configuration Management](#12-configuration-management)
13. [Middleware Pipeline](#13-middleware-pipeline-order)
14. [C# Coding Standards](#14-c-coding-standards)
15. [API Versioning](#15-api-versioning)
16. [Correlation ID Pattern](#16-correlation-id-pattern)
17. [Guardrails Summary](#17-guardrails--best-practices-summary)
18. [BatchJobs Specific](#18-batch-jobs-specific-recommendations)

---

## 1. Architectural Patterns

### 1.1 Clean Architecture / Layered Architecture

```
┌─────────────────────────────────────────┐
│  Presentation Layer (.Api / .Web)      │  Controllers, Middleware, Filters
├─────────────────────────────────────────┤
│  Application Layer (.Application)      │  Services, DTOs, Business Logic
├─────────────────────────────────────────┤
│  Domain Layer (.Core)                  │  Entities, Interfaces, Domain Models
├─────────────────────────────────────────┤
│  Infrastructure Layer (.DataAccess)    │  Repositories, DbContext, Data Access
└─────────────────────────────────────────┘
         Common (.Common)                   Shared utilities, contracts
```

### 1.2 BatchJobs Architecture

```
Apha.BatchJobs.Console
├── References: Apha.Common
├── References: Apha.FPS.Application
├── References: Apha.PACT.Application
├── References: Apha.PIMS.Application
└── References: Apha.Costbook.Application
```

**Key Principle:** BatchJobs is a cross-cutting concern that orchestrates operations across multiple domains.

---

## 2. Project Structure & Naming Conventions

### 2.1 Naming Pattern

```
Pattern: Apha.{Domain}.{ProjectType}

Examples:
✅ Apha.FPS.Api
✅ Apha.PACT.Application
✅ Apha.BatchJobs.Console
✅ Apha.BatchJobs.Console.UnitTests
```

### 2.2 Solution Structure

```
D:\Users\atos.user8\source\repos\apha-fps-apps\src\
├── Apha.FPS/
│   ├── Apha.FPS.Api/
│   ├── Apha.FPS.Application/
│   ├── Apha.FPS.Core/
│   ├── Apha.FPS.DataAccess/
│   └── Apha.FPS.*.UnitTests/
├── Apha.PACT/
├── Apha.PIMS/
├── Apha.Costbook/
├── Apha.FPSApps/
├── Apha.Common/
└── Apha.BatchJobs/                    ← NEW DOMAIN
    ├── Apha.BatchJobs.Console/
    ├── Apha.BatchJobs.Console.UnitTests/
    └── Apha.BatchJobs.Console.sln
```

### 2.3 Directory Structure Within BatchJobs.Console

```
Apha.BatchJobs.Console/
├── Jobs/
│   ├── Scheduled/                  # Year-end transfers, recurring jobs
│   │   └── YearEndTransferJob.cs
│   └── Adhoc/                      # Summary generation, one-off jobs
│       └── SummaryGenerationJob.cs
├── Configuration/
│   └── JobConfiguration.cs
├── Extensions/
│   ├── ServiceCollectionExtension.cs
│   └── SerilogExtensions.cs
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
└── Apha.BatchJobs.Console.csproj
```

---

## 3. Dependency Injection Pattern

### 3.1 Extension Method Pattern

**ALL DI registration MUST use extension methods**

```csharp
// Apha.BatchJobs.Console/Extensions/ServiceCollectionExtension.cs
namespace Apha.BatchJobs.Console.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddBatchJobServices(this IServiceCollection services)
        {
            services.AddScheduledJobs();
            services.AddAdhocJobs();
            services.AddCrossDomainServices();
            return services;
        }
        
        public static IServiceCollection AddScheduledJobs(this IServiceCollection services)
        {
            // Register scheduled jobs
            services.AddScoped<IYearEndTransferJob, YearEndTransferJob>();
            return services;
        }
        
        public static IServiceCollection AddAdhocJobs(this IServiceCollection services)
        {
            // Register adhoc jobs
            services.AddScoped<ISummaryGenerationJob, SummaryGenerationJob>();
            return services;
        }
        
        public static IServiceCollection AddCrossDomainServices(this IServiceCollection services)
        {
            // Services from other domains are already registered
            // via their respective Application layer DI extensions
            return services;
        }
    }
}
```

### 3.2 Service Lifetime Standards

| Type | Lifetime | Examples |
|------|----------|----------|
| **Services** | `Scoped` | `IProjectService`, `IAnimalService` |
| **Repositories** | `Scoped` | `IProjectRepository` |
| **DbContext** | `Scoped` | `FpsDbContext` |
| **Configuration** | `Singleton` | `IOptions<T>` |
| **Caching** | `Singleton` | `IDistributedCache` |
| **Jobs** | `Scoped` | `IYearEndTransferJob` |

---

## 4. Logging Standards

### 4.1 Serilog Configuration (Two Environments)

```csharp
// Program.cs
using Serilog;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting Apha.BatchJobs.Console");
            
            var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
            
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureHostConfiguration(config =>
            {
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
                          ?? "Production";
                          
                if (env.Equals("local", StringComparison.OrdinalIgnoreCase))
                {
                    config.AddJsonFile("appsettings.local.json", optional: true);
                }
            })
            .UseSerilog((context, services, configuration) =>
            {
                if (context.HostingEnvironment.IsEnvironment("local"))
                {
                    configuration
                        .WriteTo.Console()
                        .WriteTo.File(
                            "Logs/BatchJobs.log", 
                            rollingInterval: RollingInterval.Day,
                            restrictedToMinimumLevel: LogEventLevel.Verbose);
                }
                else
                {
                    // Production: AWS CloudWatch
                    configuration
                        .ReadFrom.Configuration(context.Configuration)
                        .UseStructuredConsoleLogging(); // Extension method
                }
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddBatchJobServices();
            });
}
```

### 4.2 Structured Logging Extension

```csharp
// Extensions/SerilogExtensions.cs
using Serilog;
using Serilog.Formatting.Compact;

namespace Apha.BatchJobs.Console.Extensions
{
    public static class SerilogExtensions
    {
        public static LoggerConfiguration UseStructuredConsoleLogging(
            this LoggerConfiguration loggerConfiguration)
        {
            return loggerConfiguration
                .Enrich.FromLogContext()
                .WriteTo.Console(new RenderedCompactJsonFormatter()); // JSON for CloudWatch
        }
    }
}
```

### 4.3 Logging Best Practices

```csharp
public class YearEndTransferJob
{
    private readonly ILogger<YearEndTransferJob> _logger;
    
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid().ToString();
        
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["JobName"] = "YearEndTransfer",
            ["FpsYear"] = _context.FpsYear
        }))
        {
            _logger.LogInformation("Starting year-end transfer");
            
            try
            {
                // Job logic
                _logger.LogInformation("Processed {Count} records", count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Year-end transfer failed");
                throw;
            }
            
            _logger.LogInformation("Year-end transfer completed successfully");
        }
    }
}
```

**🎯 Key Points:**
- ✅ Always use structured logging (log context properties)
- ✅ Add correlation IDs to all job executions
- ✅ Log start, progress, and completion
- ✅ Never log sensitive data (connection strings, passwords)

---

## 5. Error Handling & Exception Patterns

### 5.1 Global Exception Handling (API Pattern)

**Reference implementation from FPS.Api:**

```csharp
// Middleware/ExceptionMiddleware.cs
using Apha.Common.Contracts;

namespace Apha.FPS.Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IConfiguration _configuration;

        public ExceptionMiddleware(
            RequestDelegate next, 
            ILogger<ExceptionMiddleware> logger, 
            IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            var correlationId = context.Request.Headers["X-Correlation-ID"].ToString();

            var apiResponse = new ApiResponse<object>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiError>(),
                Meta = new ApiMeta
                {
                    CorrelationId = correlationId,
                    TimestampUtc = DateTime.UtcNow
                }
            };

            switch (ex)
            {
                case BusinessValidationErrorException validationEx:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    foreach (var err in validationEx.Errors)
                    {
                        apiResponse.Errors.Add(new ApiError
                        {
                            Code = err.Code,
                            Message = err.Message,
                            Details = err.Details
                        });
                    }
                    break;
                    
                case UnauthorizedAccessException:
                case AuthenticationFailureException:
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    apiResponse.Errors.Add(new ApiError
                    {
                        Code = "AUTH_403",
                        Message = "Access denied."
                    });
                    break;
                    
                case KeyNotFoundException:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    apiResponse.Errors.Add(new ApiError
                    {
                        Code = "RESOURCE_NOT_FOUND",
                        Message = ex.Message
                    });
                    break;
                    
                case PostgresException pgEx:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    apiResponse.Errors.Add(new ApiError
                    {
                        Code = "DB_POSTGRES_ERROR",
                        Message = "Database error occurred"
                    });
                    _logger.LogError(pgEx, "Postgres error: {ErrorCode}", pgEx.SqlState);
                    break;
                    
                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    apiResponse.Errors.Add(new ApiError
                    {
                        Code = "GENERAL_ERROR",
                        Message = "An unexpected error occurred"
                    });
                    _logger.LogError(ex, "Unhandled exception");
                    break;
            }

            await context.Response.WriteAsJsonAsync(apiResponse);
        }
    }
}
```

### 5.2 Custom Business Validation Exception

```csharp
// Application/Validation/BusinessValidationErrorException.cs
namespace Apha.FPS.Application.Validation
{
    public class BusinessValidationErrorException : Exception
    {
        public string Status { get; set; } = "error";
        public string ExceptionMessage { get; set; } = "Business validation failed.";
        public List<BusinessValidationError> Errors { get; set; } = new();

        public BusinessValidationErrorException(List<BusinessValidationError> errors)
        {
            Errors = errors;
        }
    }
    
    public class BusinessValidationError
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public object? Details { get; set; }
    }
}
```

### 5.3 BatchJobs Error Handling Pattern

```csharp
public class YearEndTransferJob
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Job started");
            
            // Job logic with validation
            ValidateYearEndConditions();
            
            await ProcessTransferAsync(cancellationToken);
            
            _logger.LogInformation("Job completed successfully");
        }
        catch (BusinessValidationErrorException validationEx)
        {
            // Log validation errors
            foreach (var error in validationEx.Errors)
            {
                _logger.LogWarning("Validation error: {Code} - {Message}", 
                    error.Code, error.Message);
            }
            throw; // Re-throw to mark job as failed
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Job was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job failed with unexpected error");
            throw; // Re-throw to mark job as failed
        }
    }
    
    private void ValidateYearEndConditions()
    {
        var errors = new List<BusinessValidationError>();
        
        if (/* validation condition */)
        {
            errors.Add(new BusinessValidationError
            {
                Code = "YEAR_END_INVALID",
                Message = "Cannot perform year-end transfer",
                Details = new { Reason = "Data incomplete" }
            });
        }
        
        if (errors.Any())
        {
            throw new BusinessValidationErrorException(errors);
        }
    }
}
```

---

## 6. API Response Standardization

### 6.1 Unified API Response Format

**Defined in `Apha.Common/Contracts/ApiResponse.cs`**

```csharp
namespace Apha.Common.Contracts
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public Pagination? Pagination { get; set; } = null;
        public List<ApiError>? Errors { get; set; } = new();
        public ApiMeta Meta { get; set; } = new();
    }

    public class ApiError
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public object? Details { get; set; }
    }

    public class ApiMeta
    {
        public string CorrelationId { get; set; } = string.Empty;
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    }

    public class Pagination
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
    }
}
```

### 6.2 Action Filter Pattern

```csharp
// Filters/ApiResponseActionFilter.cs
using Apha.Common.Contracts;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Apha.FPS.Api.Filters
{
    public class ApiResponseActionFilter : IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(
            ResultExecutingContext context,
            ResultExecutionDelegate next)
        {
            if (context.Result is not ObjectResult objectResult || 
                objectResult.Value is null)
            {
                await next();
                return;
            }

            var correlationId = GetCorrelationId(context);

            object wrappedResponse = IsPaginatedResult(objectResult.Value)
                ? CreatePaginatedResponse(objectResult.Value, correlationId)
                : CreateStandardResponse(objectResult.Value, correlationId);

            context.Result = new ObjectResult(wrappedResponse)
            {
                StatusCode = objectResult.StatusCode ?? StatusCodes.Status200OK
            };

            await next();
        }

        private static bool IsPaginatedResult(object value)
        {
            var type = value.GetType();
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(PaginationRes<>);
        }

        private static object CreateStandardResponse(object value, string correlationId)
        {
            return new ApiResponse<object>
            {
                Success = true,
                Data = value,
                Errors = null,
                Meta = CreateMeta(correlationId)
            };
        }

        private static object CreatePaginatedResponse(object value, string correlationId)
        {
            dynamic paginated = value;
            return new ApiResponse<object>
            {
                Success = true,
                Data = paginated.Data,
                Pagination = paginated.PaginationData,
                Errors = null,
                Meta = CreateMeta(correlationId)
            };
        }

        private static ApiMeta CreateMeta(string correlationId)
        {
            return new ApiMeta
            {
                CorrelationId = correlationId,
                TimestampUtc = DateTime.UtcNow
            };
        }

        private static string GetCorrelationId(ResultExecutingContext context)
        {
            return context.HttpContext.Request.Headers["X-Correlation-ID"].ToString();
        }
    }
}
```

---

## 7. Repository Pattern

### 7.1 Interface-Based Design

```csharp
// Core/Interfaces/IProjectRepository.cs
public interface IProjectRepository
{
    Task<Project?> GetProjectByIdAsync(string id);
    Task<IEnumerable<Project>> GetAllProjectsAsync();
    Task<Project> CreateProjectAsync(Project project);
    Task<Project> UpdateProjectAsync(Project project);
    Task<bool> DeleteProjectAsync(string id);
}
```

### 7.2 Base Repository Pattern

```csharp
// DataAccess/Repositories/BaseRepository.cs
namespace Apha.FPS.DataAccess.Repositories
{
    public class BaseRepository
    {
        protected readonly FpsDbContext _context;
        
        public BaseRepository(FpsDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public PagedData<T> ApplyPaging<T>(
            IEnumerable<T> source,
            int page,
            int pageSize)
        {
            var list = source.ToList();
            var totalRecords = list.Count;

            var result = list
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var pagination = new PaginationData
            {
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                TotalRecords = totalRecords
            };

            return new PagedData<T>(result, pagination);
        }
    }
}
```

### 7.3 Repository Implementation

```csharp
// DataAccess/Repositories/ProjectRepository.cs
public class ProjectRepository : BaseRepository, IProjectRepository
{
    private readonly IFpsRequestContext _requestContext;
    
    public ProjectRepository(
        FpsDbContext context, 
        IFpsRequestContext requestContext) 
        : base(context)
    {
        _requestContext = requestContext;
    }

    public async Task<Project?> GetProjectByIdAsync(string id)
    {
        return await _context.Projects
            .Where(p => p.FpsYear == _requestContext.FpsYear)
            .FirstOrDefaultAsync(p => p.ParentProject == id);
    }

    public async Task<IEnumerable<Project>> GetAllProjectsAsync()
    {
        return await _context.Projects
            .Where(p => p.FpsYear == _requestContext.FpsYear)
            .ToListAsync();
    }
}
```

**🎯 Key Principles:**
- ✅ Always use interfaces
- ✅ Never expose DbContext outside DataAccess layer
- ✅ Use BaseRepository for common operations
- ✅ Inject IFpsRequestContext for year filtering

---

## 8. Database Patterns

### 8.1 DbContext Configuration

```csharp
// Extensions/ProgramExtension.cs
services.AddDbContext<FpsDbContext>(options =>
    options.UseNpgsql(
        configuration.GetConnectionString("FPSConnectionString"),
        npgsqlOptions =>
        {
            // Resilience: Retry on transient failures
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null);
                
            // Performance: Prevent hanging queries
            npgsqlOptions.CommandTimeout(30);
        }
    ), ServiceLifetime.Scoped);
```

### 8.2 Request Context Pattern (Multi-Tenancy by Year)

```csharp
// Core/Interfaces/IFpsRequestContext.cs
public interface IFpsRequestContext
{
    int FpsYear { get; set; }
    string UserEmailId { get; set; }
}

// DataAccess/Context/FpsRequestContext.cs
public class FpsRequestContext : IFpsRequestContext
{
    public int FpsYear { get; set; }
    public string UserEmailId { get; set; } = string.Empty;
}
```

### 8.3 DbContext with Request Context

```csharp
// DataAccess/Data/FpsDbContext.cs
public partial class FpsDbContext : DbContext
{
    private readonly IFpsRequestContext _fpsYearContext;

    public FpsDbContext(
        DbContextOptions<FpsDbContext> options, 
        IFpsRequestContext fpsYearContext)
        : base(options)
    {
        _fpsYearContext = fpsYearContext;
    }

    // DbSets...
    public virtual DbSet<Project> Projects { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Entity configurations
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => new { e.ParentProject, e.FpsYear });
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            // ... more configuration
        });
    }
}
```

### 8.4 Middleware for Setting Request Context

```csharp
// Middleware/RequestContextMiddleware.cs (API pattern)
public class RequestContextMiddleware
{
    private readonly RequestDelegate _next;
    private const string FpsYearHeader = "X-FPS-Year";

    public async Task InvokeAsync(HttpContext context, IFpsRequestContext requestContext)
    {
        // Skip for health/swagger endpoints
        var path = context.Request.Path.Value?.ToLower();
        if (path != null && 
            (path.StartsWith("/swagger") || 
             path.StartsWith("/health")))
        {
            await _next(context);
            return;
        }

        // REQUIRED: X-FPS-Year header
        if (!context.Request.Headers.TryGetValue(FpsYearHeader, out var header)
            || !int.TryParse(header, out int fpsYear))
        {
            throw new ArgumentException(
                $"Required request header '{FpsYearHeader}' is missing or empty.");
        }

        requestContext.FpsYear = fpsYear;
        requestContext.UserEmailId = context.User?.Identity?.Name ?? string.Empty;
        
        await _next(context);
    }
}
```

### 8.5 BatchJobs Context Setup

**For batch jobs, set context programmatically:**

```csharp
public class YearEndTransferJob
{
    private readonly IFpsRequestContext _requestContext;
    
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // Set year context for the job
        _requestContext.FpsYear = DateTime.Now.Year;
        _requestContext.UserEmailId = "batchjobs@system.com";
        
        // Now all repository calls will use this year
        var projects = await _projectRepository.GetAllProjectsAsync();
    }
}
```

---

## 9. AutoMapper Pattern

### 9.1 AutoMapper Registration

```csharp
// Program.cs or ServiceCollectionExtension.cs
services.AddAutoMapper(config =>
{
    config.AddMaps(typeof(EntityMapper).Assembly);    // Application layer
    config.AddMaps(typeof(RequestMapper).Assembly);   // API layer
});
```

### 9.2 Service Pattern with AutoMapper

```csharp
// Application/Services/ProjectService.cs
public class ProjectService : IProjectService
{
    private readonly IProjectRepository _repository;
    private readonly IMapper _mapper;

    public ProjectService(IProjectRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(string id)
    {
        var project = await _repository.GetProjectByIdAsync(id);
        return project == null ? null : _mapper.Map<ProjectDto>(project);
    }

    public async Task<ProjectDto> CreateProjectAsync(ProjectDto projectDto)
    {
        var project = _mapper.Map<Project>(projectDto);
        var created = await _repository.CreateProjectAsync(project);
        return _mapper.Map<ProjectDto>(created);
    }
}
```

### 9.3 Mapping Profile Example

```csharp
// Application/Mappings/EntityMapper.cs
using AutoMapper;

public class EntityMapper : Profile
{
    public EntityMapper()
    {
        CreateMap<Project, ProjectDto>()
            .ReverseMap();
            
        CreateMap<PaginatedResult<Project>, PaginatedResult<ProjectDto>>();
    }
}
```

**🎯 Key Points:**
- ✅ Always use AutoMapper for Entity ↔ DTO conversion
- ✅ Never expose domain entities to API consumers
- ✅ Create separate profiles for different layers

---

## 10. Authentication & Authorization

### 10.1 Azure AD / Entra ID Integration

```csharp
// Extensions/AuthenticationExtension.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

public static class AuthenticationExtension
{
    public static IServiceCollection AddAuthenticationServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(configuration.GetSection("AzureAd"));
        
        services.AddAuthorization();
        
        return services;
    }
}
```

### 10.2 Configuration

```json
// appsettings.json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "<your-domain>.onmicrosoft.com",
    "ClientId": "<client-id>",
    "TenantId": "<tenant-id>",
    "ClientSecret": "<client-secret>",
    "CallbackPath": "/signin-oidc"
  }
}
```

### 10.3 Controller Authorization

```csharp
[Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
[ApiController]
[Route("api/v{version:apiVersion}/project")]
public class ProjectController : ControllerBase
{
    // All endpoints require authentication
}
```

### 10.4 BatchJobs Authentication

**For batch jobs running in Azure, use Managed Identity:**

```csharp
// No client secret needed - uses Azure Managed Identity
var credential = new DefaultAzureCredential();

// Or for local development with service principal:
var clientId = configuration["AzureAd:ClientId"];
var clientSecret = configuration["AzureAd:ClientSecret"];
var tenantId = configuration["AzureAd:TenantId"];

var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
```

---

## 11. Testing Patterns

### 11.1 Test Project Structure

```
Apha.BatchJobs.Console.UnitTests/
├── Jobs/
│   ├── Scheduled/
│   │   └── YearEndTransferJobTests.cs
│   └── Adhoc/
│       └── SummaryGenerationJobTests.cs
├── Configuration/
└── TestHelpers/
    └── MockHelper.cs
```

### 11.2 Test Dependencies

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.3.0" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.5">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Moq" Version="4.20.72" />
    <PackageReference Include="FluentAssertions" Version="8.9.0" />
    <PackageReference Include="NSubstitute" Version="5.3.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Apha.BatchJobs.Console\Apha.BatchJobs.Console.csproj" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>
</Project>
```

### 11.3 Repository Test Pattern

```csharp
// DataAccess.UnitTests/Repository/ProjectRepositoryTests.cs
public class ProjectRepositoryTests
{
    private const int DefaultFpsYear = 2024;
    private const string DefaultUserEmail = "test@example.com";

    private static Mock<IFpsRequestContext> CreateMockRequestContext(int year = DefaultFpsYear)
    {
        var mock = new Mock<IFpsRequestContext>();
        mock.Setup(x => x.FpsYear).Returns(year);
        mock.Setup(x => x.UserEmailId).Returns(DefaultUserEmail);
        return mock;
    }

    private static ProjectRepository CreateRepository(
        IEnumerable<Project>? projects = null,
        int fpsYear = DefaultFpsYear)
    {
        var mockRequestContext = CreateMockRequestContext(fpsYear);
        var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(
            mockRequestContext.Object);

        if (projects != null)
        {
            var mockSet = RepositoryTestHelper.CreateMockDbSet(projects);
            RepositoryTestHelper.SetupDbSetOperations(mockSet);
            mockContext.Setup(x => x.Projects).Returns(mockSet.Object);
        }

        RepositoryTestHelper.SetupSaveChanges(mockContext);
        return new ProjectRepository(mockContext.Object, mockRequestContext.Object);
    }

    [Fact]
    public async Task GetAllProjectsAsync_ReturnsEmptyList_WhenNoProjects()
    {
        // Arrange
        var repo = CreateRepository(projects: []);

        // Act
        var result = await repo.GetAllProjectsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetProjectByIdAsync_ReturnsProject_WhenExists()
    {
        // Arrange
        var testProject = new Project { ParentProject = "P001", FpsYear = 2024 };
        var repo = CreateRepository(projects: [testProject]);

        // Act
        var result = await repo.GetProjectByIdAsync("P001");

        // Assert
        result.Should().NotBeNull();
        result!.ParentProject.Should().Be("P001");
    }
}
```

### 11.4 Service Test Pattern

```csharp
public class ProjectServiceTests
{
    [Fact]
    public async Task GetProjectByIdAsync_ReturnsDto_WhenProjectExists()
    {
        // Arrange
        var mockRepo = new Mock<IProjectRepository>();
        var mockMapper = new Mock<IMapper>();
        
        var project = new Project { ParentProject = "P001" };
        var projectDto = new ProjectDto { ParentProject = "P001" };
        
        mockRepo.Setup(r => r.GetProjectByIdAsync("P001"))
                .ReturnsAsync(project);
        mockMapper.Setup(m => m.Map<ProjectDto>(project))
                  .Returns(projectDto);

        var service = new ProjectService(mockRepo.Object, mockMapper.Object);

        // Act
        var result = await service.GetProjectByIdAsync("P001");

        // Assert
        result.Should().NotBeNull();
        result!.ParentProject.Should().Be("P001");
    }
}
```

---

## 12. Configuration Management

### 12.1 appsettings.json Structure

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "FPSConnectionString": "<postgres-connection-string>",
    "PACTConnectionString": "<postgres-connection-string>",
    "PIMSConnectionString": "<postgres-connection-string>",
    "RedisConnectionString": "<redis-connection-string>"
  },
  "AllowedHosts": "*",
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "<domain>.onmicrosoft.com",
    "ClientId": "<client-id>",
    "TenantId": "<tenant-id>",
    "ClientSecret": "<client-secret>"
  },
  "AwsLogging": {
    "LogGroupName": "/aws/batch/apha-batchjobs"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning",
        "Microsoft.AspNetCore": "Warning"
      }
    }
  },
  "ExceptionTypes": {
    "General": "BATCHJOBS.GENERAL_EXCEPTION",
    "Database": "BATCHJOBS.DB_EXCEPTION",
    "Authorization": "BATCHJOBS.AUTHORIZATION_EXCEPTION"
  },
  "JobConfiguration": {
    "YearEndTransfer": {
      "Enabled": true,
      "CronExpression": "0 0 1 1 *",
      "TimeoutMinutes": 120
    },
    "SummaryGeneration": {
      "Enabled": true,
      "BatchSize": 1000
    }
  }
}
```

### 12.2 Configuration Classes

```csharp
// Configuration/JobConfiguration.cs
public class JobConfiguration
{
    public YearEndTransferConfig YearEndTransfer { get; set; } = new();
    public SummaryGenerationConfig SummaryGeneration { get; set; } = new();
}

public class YearEndTransferConfig
{
    public bool Enabled { get; set; }
    public string CronExpression { get; set; } = string.Empty;
    public int TimeoutMinutes { get; set; }
}

public class SummaryGenerationConfig
{
    public bool Enabled { get; set; }
    public int BatchSize { get; set; }
}

// Program.cs - Register configuration
services.Configure<JobConfiguration>(
    hostContext.Configuration.GetSection("JobConfiguration"));
```

### 12.3 Using Configuration in Jobs

```csharp
public class YearEndTransferJob
{
    private readonly IOptions<JobConfiguration> _config;
    
    public YearEndTransferJob(IOptions<JobConfiguration> config)
    {
        _config = config;
    }
    
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!_config.Value.YearEndTransfer.Enabled)
        {
            _logger.LogInformation("Year-end transfer is disabled");
            return;
        }
        
        var timeout = TimeSpan.FromMinutes(_config.Value.YearEndTransfer.TimeoutMinutes);
        // Use configuration...
    }
}
```

---

## 13. Middleware Pipeline Order

**⚠️ CRITICAL: Middleware order matters!**

### 13.1 API Middleware Pipeline

```csharp
// Extensions/ProgramExtension.cs
public static void ConfigureMiddleware(this WebApplication app)
{
    var env = app.Environment;

    // 1. Localization
    var localizationOptions = new RequestLocalizationOptions
    {
        DefaultRequestCulture = new RequestCulture("en-GB"),
        SupportedCultures = new[] { new CultureInfo("en-GB") },
        SupportedUICultures = new[] { new CultureInfo("en-GB") }
    };
    app.UseRequestLocalization(localizationOptions);

    // 2. Health checks (before error handling for monitoring)
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        Predicate = _ => false
    });

    // 3. Developer exception page (ONLY in Development/Local)
    if (env.IsDevelopment() || env.IsEnvironment("local"))
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
        });
    }

    // 4. GLOBAL EXCEPTION HANDLING (must be early)
    app.UseMiddleware<ExceptionMiddleware>();

    // 5. Security headers
    app.UseHsts();
    app.UseHttpsRedirection();

    // 6. Static files
    app.UseStaticFiles();

    // 7. Routing
    app.UseRouting();

    // 8. Authentication (BEFORE Authorization)
    app.UseAuthentication();

    // 9. Custom context middleware (after authentication)
    app.UseMiddleware<RequestContextMiddleware>();

    // 10. Authorization
    app.UseAuthorization();

    // 11. Endpoints (LAST)
    app.MapControllers();
}
```

**Order Summary:**
1. Localization
2. Health checks
3. Developer exception page (dev only)
4. Exception middleware
5. HSTS & HTTPS redirect
6. Static files
7. Routing
8. **Authentication**
9. Request context
10. **Authorization**
11. Endpoints

---

## 14. C# Coding Standards

### 14.1 Project Settings (MANDATORY)

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <Nullable>enable</Nullable>              <!-- ✅ ALWAYS -->
  <ImplicitUsings>enable</ImplicitUsings>  <!-- ✅ ALWAYS -->
</PropertyGroup>
```

### 14.2 Nullable Reference Types

```csharp
// ✅ CORRECT: Explicit nullability
public class ProjectService
{
    private readonly IProjectRepository _repository;  // Non-nullable
    
    public async Task<ProjectDto?> GetProjectByIdAsync(string id)  // Nullable return
    {
        var project = await _repository.GetProjectByIdAsync(id);
        return project == null ? null : _mapper.Map<ProjectDto>(project);
    }
    
    public string GetDisplayName(Project? project)
    {
        return project?.Name ?? "Unknown";  // Null-coalescing
    }
}

// ❌ WRONG: Ignoring nullability
public async Task<ProjectDto> GetProjectByIdAsync(string id)  // Non-nullable but can return null
{
    var project = await _repository.GetProjectByIdAsync(id);
    return _mapper.Map<ProjectDto>(project);  // Compiler warning!
}
```

### 14.3 Async/Await Pattern

```csharp
// ✅ CORRECT: Async all the way
public async Task<IEnumerable<ProjectDto>> GetAllProjectsAsync()
{
    var projects = await _repository.GetAllProjectsAsync();
    return _mapper.Map<IEnumerable<ProjectDto>>(projects);
}

// ❌ WRONG: Blocking on async
public IEnumerable<ProjectDto> GetAllProjects()
{
    var projects = _repository.GetAllProjectsAsync().Result;  // Deadlock risk!
    return _mapper.Map<IEnumerable<ProjectDto>>(projects);
}
```

### 14.4 Naming Conventions

```csharp
// Interfaces: I{Name}
public interface IProjectService { }

// Classes: PascalCase
public class ProjectService : IProjectService { }

// Private fields: _camelCase
private readonly IProjectRepository _repository;

// Properties: PascalCase
public string ProjectName { get; set; }

// Methods: PascalCase
public async Task<Project> CreateProjectAsync(Project project)

// Parameters: camelCase
public ProjectService(IProjectRepository repository, IMapper mapper)

// Local variables: camelCase
var projectDto = _mapper.Map<ProjectDto>(project);

// Constants: PascalCase or UPPER_CASE
private const int DefaultFpsYear = 2024;
private const string FPS_YEAR_HEADER = "X-FPS-Year";
```

### 14.5 Service Lifetime Standards

```csharp
// ✅ CORRECT: Scoped for request-scoped data
services.AddScoped<IProjectService, ProjectService>();
services.AddScoped<IProjectRepository, ProjectRepository>();
services.AddDbContext<FpsDbContext>(options => {...}, ServiceLifetime.Scoped);

// ✅ CORRECT: Singleton for stateless services
services.AddSingleton<IDistributedCache, RedisCache>();

// ❌ WRONG: Singleton with scoped dependencies
services.AddSingleton<IProjectService, ProjectService>();  // Has scoped repo!
```

---

## 15. API Versioning

### 15.1 Versioning Configuration

```csharp
// Extensions/ProgramExtension.cs
services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
```

### 15.2 Controller Versioning

```csharp
[Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/project")]
public class ProjectController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProject(string id)
    {
        // GET /api/v1/project/{id}
    }
}

// Future version
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/project")]
public class ProjectV2Controller : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProject(string id)
    {
        // GET /api/v2/project/{id}
    }
}
```

---

## 16. Correlation ID Pattern

### 16.1 Middleware Implementation

```csharp
private static void SetCorrelationId(HttpContext context, string headerName)
{
    // Get or generate correlation ID
    if (!context.Request.Headers.TryGetValue(headerName, out var correlationId)
        || string.IsNullOrWhiteSpace(correlationId))
    {
        correlationId = Guid.NewGuid().ToString();
        context.Request.Headers[headerName] = correlationId;
    }

    // Echo back in response
    context.Response.OnStarting(() =>
    {
        context.Response.Headers[headerName] = correlationId!;
        return Task.CompletedTask;
    });
}
```

### 16.2 Using in Logging

```csharp
public async Task ExecuteAsync(CancellationToken cancellationToken)
{
    var correlationId = Guid.NewGuid().ToString();
    
    using (_logger.BeginScope(new Dictionary<string, object>
    {
        ["CorrelationId"] = correlationId,
        ["JobName"] = "YearEndTransfer"
    }))
    {
        _logger.LogInformation("Job started");
        // All logs will include correlation ID
    }
}
```

---

## 17. Guardrails & Best Practices Summary

### 17.1 ✅ MUST DO

| # | Practice | Reason |
|---|----------|--------|
| 1 | **Layered Architecture** | Separation of concerns, testability |
| 2 | **Interface-based design** | Dependency inversion, mockability |
| 3 | **Extension methods for DI** | Clean Program.cs, organization |
| 4 | **Serilog structured logging** | AWS CloudWatch compatibility |
| 5 | **Global exception handling** | Consistent error responses |
| 6 | **Nullable enabled** | Compile-time null safety |
| 7 | **Scoped lifetime for services** | Request isolation |
| 8 | **AutoMapper for DTOs** | Entity-DTO separation |
| 9 | **Repository pattern** | Data access abstraction |
| 10 | **Unit tests** | Code quality, regression prevention |
| 11 | **Correlation IDs** | Request tracing |
| 12 | **Async/await** | Scalability, performance |

### 17.2 ❌ NEVER DO

| # | Anti-Pattern | Impact |
|---|--------------|--------|
| 1 | Hardcode connection strings | Security risk |
| 2 | Register services in Program.cs | Poor organization |
| 3 | Access DbContext from Application layer | Violates architecture |
| 4 | Expose entities directly | Tight coupling |
| 5 | Log sensitive data | Security/compliance violation |
| 6 | Use magic strings | Maintenance nightmare |
| 7 | Block on async (.Result, .Wait()) | Deadlock risk |
| 8 | Singleton with scoped dependencies | Concurrency issues |
| 9 | Skip error handling | Poor user experience |
| 10 | Ignore nullable warnings | Runtime null reference exceptions |

### 17.3 ⚠️ PERFORMANCE SAFEGUARDS

```csharp
// Database
options.UseNpgsql(connectionString, npgsqlOptions =>
{
    npgsqlOptions.EnableRetryOnFailure(
        maxRetryCount: 5,
        maxRetryDelay: TimeSpan.FromSeconds(10));
    npgsqlOptions.CommandTimeout(30);  // Prevent hanging queries
});

// Pagination
public async Task<PaginatedResult<T>> GetPagedAsync(int page, int pageSize)
{
    const int MaxPageSize = 100;
    pageSize = Math.Min(pageSize, MaxPageSize);  // Cap page size
}

// Caching
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = connectionString;
    options.InstanceName = "BatchJobs";
});
```

---

## 18. Batch Jobs Specific Recommendations

### 18.1 Project File

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <!-- Hosting -->
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="10.0.5" />
    
    <!-- Logging -->
    <PackageReference Include="Serilog.AspNetCore" Version="10.0.0" />
    <PackageReference Include="AWSSDK.CloudWatchLogs" Version="4.0.15.3" />
    <PackageReference Include="Serilog.Sinks.AwsCloudWatch" Version="4.4.42" />
    
    <!-- Configuration -->
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="10.0.5" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="10.0.5" />
    
    <!-- AutoMapper -->
    <PackageReference Include="AutoMapper" Version="16.1.1" />
  </ItemGroup>

  <ItemGroup>
    <!-- Common -->
    <ProjectReference Include="..\..\Apha.Common\Apha.Common.csproj" />
    
    <!-- Cross-Domain References -->
    <ProjectReference Include="..\..\Apha.FPS\Apha.FPS.Application\Apha.FPS.Application.csproj" />
    <ProjectReference Include="..\..\Apha.PACT\Apha.PACT.Application\Apha.PACT.Application.csproj" />
    <ProjectReference Include="..\..\Apha.PIMS\Apha.PIMS.Application\Apha.PIMS.Application.csproj" />
    <ProjectReference Include="..\..\Apha.Costbook\Apha.Costbook.Application\Apha.Costbook.Application.csproj" />
  </ItemGroup>
</Project>
```

### 18.2 Job Interfaces

```csharp
// Jobs/IBatchJob.cs
public interface IBatchJob
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}

// Jobs/Scheduled/IScheduledJob.cs
public interface IScheduledJob : IBatchJob
{
    string JobName { get; }
    string CronExpression { get; }
}

// Jobs/Adhoc/IAdhocJob.cs
public interface IAdhocJob : IBatchJob
{
    string JobName { get; }
}
```

### 18.3 Year-End Transfer Job Example

```csharp
// Jobs/Scheduled/YearEndTransferJob.cs
public class YearEndTransferJob : IScheduledJob
{
    private readonly IFpsRequestContext _requestContext;
    private readonly IProjectService _projectService;
    private readonly ILogger<YearEndTransferJob> _logger;
    private readonly IOptions<JobConfiguration> _config;

    public string JobName => "YearEndTransfer";
    public string CronExpression => _config.Value.YearEndTransfer.CronExpression;

    public YearEndTransferJob(
        IFpsRequestContext requestContext,
        IProjectService projectService,
        ILogger<YearEndTransferJob> logger,
        IOptions<JobConfiguration> config)
    {
        _requestContext = requestContext;
        _projectService = projectService;
        _logger = logger;
        _config = config;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid().ToString();
        
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["JobName"] = JobName,
            ["FpsYear"] = _requestContext.FpsYear
        }))
        {
            _logger.LogInformation("Starting year-end transfer for year {FpsYear}", 
                _requestContext.FpsYear);

            try
            {
                // Set context for the current year
                _requestContext.FpsYear = DateTime.Now.Year;
                _requestContext.UserEmailId = "batchjobs@system.com";

                // Validate preconditions
                await ValidateYearEndConditions(cancellationToken);

                // Process transfer
                await ProcessTransferAsync(cancellationToken);

                _logger.LogInformation("Year-end transfer completed successfully");
            }
            catch (BusinessValidationErrorException validationEx)
            {
                _logger.LogWarning("Year-end transfer validation failed: {Errors}", 
                    string.Join(", ", validationEx.Errors.Select(e => e.Message)));
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Year-end transfer failed with unexpected error");
                throw;
            }
        }
    }

    private async Task ValidateYearEndConditions(CancellationToken cancellationToken)
    {
        var errors = new List<BusinessValidationError>();

        // Example validation
        var hasOpenTransactions = await CheckOpenTransactions(cancellationToken);
        if (hasOpenTransactions)
        {
            errors.Add(new BusinessValidationError
            {
                Code = "YEAR_END_OPEN_TRANSACTIONS",
                Message = "Cannot perform year-end transfer with open transactions",
                Details = new { Year = _requestContext.FpsYear }
            });
        }

        if (errors.Any())
        {
            throw new BusinessValidationErrorException(errors);
        }
    }

    private async Task ProcessTransferAsync(CancellationToken cancellationToken)
    {
        // Transfer logic
        var projects = await _projectService.GetAllProjectsAsync();
        
        var processedCount = 0;
        foreach (var project in projects)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            // Transfer project to new year
            await TransferProjectToNextYear(project, cancellationToken);
            
            processedCount++;
            if (processedCount % 100 == 0)
            {
                _logger.LogInformation("Processed {Count} projects", processedCount);
            }
        }

        _logger.LogInformation("Transferred {TotalCount} projects", processedCount);
    }

    private async Task<bool> CheckOpenTransactions(CancellationToken cancellationToken)
    {
        // Implementation
        await Task.CompletedTask;
        return false;
    }

    private async Task TransferProjectToNextYear(ProjectDto project, CancellationToken cancellationToken)
    {
        // Implementation
        await Task.CompletedTask;
    }
}
```

### 18.4 Program.cs Implementation

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Apha.BatchJobs.Console.Extensions;

namespace Apha.BatchJobs.Console;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting Apha.BatchJobs.Console");

            var host = CreateHostBuilder(args).Build();
            
            // Execute job based on arguments
            await ExecuteJob(host, args);
            
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog((context, services, configuration) =>
            {
                if (context.HostingEnvironment.IsEnvironment("local"))
                {
                    configuration
                        .WriteTo.Console()
                        .WriteTo.File(
                            "Logs/BatchJobs.log",
                            rollingInterval: RollingInterval.Day);
                }
                else
                {
                    configuration
                        .ReadFrom.Configuration(context.Configuration)
                        .UseStructuredConsoleLogging();
                }
            })
            .ConfigureServices((hostContext, services) =>
            {
                // Register all batch job services
                services.AddBatchJobServices();
            });

    private static async Task ExecuteJob(IHost host, string[] args)
    {
        // Parse job name from arguments
        var jobName = args.Length > 0 ? args[0] : "help";
        
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;

        switch (jobName.ToLower())
        {
            case "yearendtransfer":
                var yearEndJob = services.GetRequiredService<IYearEndTransferJob>();
                await yearEndJob.ExecuteAsync(CancellationToken.None);
                break;
                
            case "summarygeneration":
                var summaryJob = services.GetRequiredService<ISummaryGenerationJob>();
                await summaryJob.ExecuteAsync(CancellationToken.None);
                break;
                
            default:
                Console.WriteLine("Available jobs:");
                Console.WriteLine("  yearendtransfer     - Year-end transfer job");
                Console.WriteLine("  summarygeneration   - Summary generation job");
                break;
        }
    }
}
```

### 18.5 Running Jobs

```bash
# Local development
dotnet run --project Apha.BatchJobs.Console -- yearendtransfer

# Production (AWS ECS/Fargate)
docker run apha-batchjobs:latest yearendtransfer

# Azure Batch
az batch job create --id year-end-transfer --pool-id batch-pool
```

---

## Appendix A: Quick Reference Checklist

### New Project Checklist

- [ ] Create project with naming pattern `Apha.{Domain}.{ProjectType}`
- [ ] Enable `Nullable` and `ImplicitUsings`
- [ ] Set `TargetFramework` to `net10.0`
- [ ] Create companion `.UnitTests` project
- [ ] Add to domain `.sln` file
- [ ] Add to `Apha.FPS.All.sln`
- [ ] Create `Extensions/ServiceCollectionExtension.cs`
- [ ] Create `Extensions/SerilogExtensions.cs`
- [ ] Configure Serilog for local and cloud
- [ ] Set up dependency injection
- [ ] Add configuration classes
- [ ] Create initial unit tests

### Code Review Checklist

- [ ] All services use interfaces
- [ ] DI registered via extension methods
- [ ] Nullable reference types respected
- [ ] Async/await used throughout
- [ ] AutoMapper for Entity ↔ DTO
- [ ] Repository pattern for data access
- [ ] Structured logging with correlation IDs
- [ ] Global exception handling
- [ ] Configuration via appsettings.json
- [ ] Unit tests mirror structure
- [ ] No magic strings
- [ ] No hardcoded values

---

## Appendix B: Common Patterns Reference

### Pattern: Service with Repository

```csharp
public class ProjectService : IProjectService
{
    private readonly IProjectRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(
        IProjectRepository repository,
        IMapper mapper,
        ILogger<ProjectService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProjectDto?> GetByIdAsync(string id)
    {
        _logger.LogInformation("Getting project {ProjectId}", id);
        
        var project = await _repository.GetProjectByIdAsync(id);
        return project == null ? null : _mapper.Map<ProjectDto>(project);
    }
}
```

### Pattern: Paginated Query

```csharp
public async Task<PaginatedResult<ProjectDto>> GetPagedAsync(
    QueryParameters<string> query)
{
    var pagedProjects = await _repository.GetPagedProjectsAsync(
        _mapper.Map<PaginationParameters<string>>(query));
        
    return _mapper.Map<PaginatedResult<ProjectDto>>(pagedProjects);
}
```

### Pattern: Validation

```csharp
private void ValidateProject(Project project)
{
    var errors = new List<BusinessValidationError>();

    if (string.IsNullOrWhiteSpace(project.ProjectName))
    {
        errors.Add(new BusinessValidationError
        {
            Code = "PROJECT_NAME_REQUIRED",
            Message = "Project name is required"
        });
    }

    if (errors.Any())
    {
        throw new BusinessValidationErrorException(errors);
    }
}
```

---

---

## 19. Microservices Patterns

### 19.1 Service Discovery

**Not applicable for current monolithic architecture, but documented for future reference:**

```csharp
// Future: Service discovery with Consul
services.AddConsul(configuration);
services.AddConsulServiceDiscovery();

// For now: Direct service references via Application layer
services.AddScoped<IProjectService, ProjectService>();
```

### 19.2 Inter-Service Communication

**Current Architecture:**
- BatchJobs → Direct references to Application layers (in-process calls)
- No network calls between services

**Future Considerations:**
- REST APIs for external integrations
- Message queues for async processing
- gRPC for internal service-to-service calls

### 19.3 Resilience Patterns

```csharp
// Circuit Breaker Pattern (using Polly)
services.AddHttpClient("ExternalApi")
    .AddPolicyHandler(Policy
        .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));

// Retry Policy
services.AddHttpClient("ExternalApi")
    .AddPolicyHandler(Policy
        .HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.ServiceUnavailable)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
```

---

## 20. Message Queue Standards

### 20.1 Queue Selection Criteria

| Requirement | RabbitMQ | Azure Service Bus | AWS SQS |
|-------------|----------|-------------------|---------|
| **Cost** | Self-hosted (free) | Pay-per-message | Pay-per-message |
| **Complexity** | High routing flexibility | Managed service | Simple FIFO |
| **Reliability** | Manual clustering | Built-in HA | Built-in HA |
| **Integration** | Any platform | Azure-native | AWS-native |
| **Current Choice** | N/A | ✅ Recommended | Alternative |

### 20.2 Message Schema Design

```csharp
// Base message contract
public abstract class MessageBase
{
    public string MessageId { get; set; } = Guid.NewGuid().ToString();
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string CorrelationId { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
}

// Year-end transfer command
public class YearEndTransferCommand : MessageBase
{
    public int SourceYear { get; set; }
    public int TargetYear { get; set; }
    public string InitiatedBy { get; set; } = string.Empty;
    public List<string> ProjectCodes { get; set; } = new();
}

// Year-end transfer event
public class YearEndTransferCompletedEvent : MessageBase
{
    public string TransferId { get; set; } = string.Empty;
    public int ProjectsTransferred { get; set; }
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
}
```

### 20.3 Dead Letter Queue Handling

```csharp
public class DeadLetterQueueHandler
{
    private readonly ILogger<DeadLetterQueueHandler> _logger;
    private readonly IServiceBusClient _serviceBus;

    public async Task ProcessDeadLetterMessagesAsync(CancellationToken cancellationToken)
    {
        var receiver = _serviceBus.CreateReceiver("queue-name/$DeadLetterQueue");

        await foreach (var message in receiver.ReceiveMessagesAsync(cancellationToken))
        {
            _logger.LogWarning("Dead letter message: {MessageId}, Reason: {Reason}",
                message.MessageId,
                message.DeadLetterReason);

            // Store for manual review
            await StoreForReviewAsync(message);

            // Complete to remove from DLQ
            await receiver.CompleteMessageAsync(message);
        }
    }
}
```

### 20.4 Idempotency Pattern

```csharp
public class IdempotentMessageHandler
{
    private readonly IDistributedCache _cache;

    public async Task<bool> HasBeenProcessedAsync(string messageId)
    {
        var key = $"processed:{messageId}";
        var value = await _cache.GetStringAsync(key);
        return value != null;
    }

    public async Task MarkAsProcessedAsync(string messageId, TimeSpan ttl)
    {
        var key = $"processed:{messageId}";
        await _cache.SetStringAsync(key, "1", new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        });
    }
}
```

---

## 21. Background Job Processing

### 21.1 Hangfire vs Quartz.NET

| Feature | Hangfire | Quartz.NET | Current Choice |
|---------|----------|------------|----------------|
| **Dashboard** | Built-in web UI | No UI | Hangfire ✅ |
| **Persistence** | SQL/Redis | SQL/RAM | SQL |
| **Ease of Use** | Very simple | More complex | Hangfire ✅ |
| **Distributed** | Yes | Yes | Yes |
| **Cron Support** | Yes | Yes | Yes |
| **Cost** | Free basic | Free | Free |

### 21.2 Hangfire Implementation

```csharp
// Program.cs
services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(configuration.GetConnectionString("HangfireConnection")));

services.AddHangfireServer();

// Configure recurring jobs
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

// Schedule jobs
RecurringJob.AddOrUpdate<YearEndTransferJob>(
    "year-end-transfer",
    job => job.ExecuteAsync(CancellationToken.None),
    "0 0 1 1 *"); // January 1st at midnight
```

### 21.3 Job State Management

```csharp
public class JobStateManager
{
    private readonly IDistributedCache _cache;

    public async Task<bool> TryAcquireLockAsync(string jobName, TimeSpan lockDuration)
    {
        var lockKey = $"job:lock:{jobName}";
        var lockValue = Guid.NewGuid().ToString();

        var acquired = await _cache.SetAsync(lockKey, 
            Encoding.UTF8.GetBytes(lockValue),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = lockDuration
            });

        return acquired;
    }

    public async Task ReleaseLockAsync(string jobName)
    {
        var lockKey = $"job:lock:{jobName}";
        await _cache.RemoveAsync(lockKey);
    }
}
```

---

## 22. File Processing Standards

### 22.1 CSV/Excel Parsing

```csharp
// Using ClosedXML for Excel
public class ExcelImporter
{
    public async Task<List<ProjectDto>> ImportProjectsAsync(Stream excelStream)
    {
        using var workbook = new XLWorkbook(excelStream);
        var worksheet = workbook.Worksheet(1);

        var projects = new List<ProjectDto>();

        foreach (var row in worksheet.RowsUsed().Skip(1)) // Skip header
        {
            projects.Add(new ProjectDto
            {
                ParentProject = row.Cell(1).GetString(),
                ProjectTitle = row.Cell(2).GetString(),
                Manager = row.Cell(3).GetString(),
                // ... map other fields
            });
        }

        return projects;
    }
}
```

### 22.2 Large File Streaming

```csharp
public class LargeFileProcessor
{
    public async Task ProcessLargeFileAsync(Stream fileStream, CancellationToken cancellationToken)
    {
        const int BatchSize = 1000;
        var batch = new List<ProjectDto>(BatchSize);

        using var reader = new StreamReader(fileStream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        await foreach (var record in csv.GetRecordsAsync<ProjectDto>(cancellationToken))
        {
            batch.Add(record);

            if (batch.Count >= BatchSize)
            {
                await ProcessBatchAsync(batch, cancellationToken);
                batch.Clear();
            }
        }

        // Process remaining records
        if (batch.Any())
        {
            await ProcessBatchAsync(batch, cancellationToken);
        }
    }
}
```

### 22.3 File Validation

```csharp
public class FileValidator
{
    public ValidationResult ValidateFile(IFormFile file)
    {
        var errors = new List<string>();

        // Size check
        const long MaxFileSize = 10 * 1024 * 1024; // 10 MB
        if (file.Length > MaxFileSize)
        {
            errors.Add($"File size exceeds maximum of {MaxFileSize / 1024 / 1024} MB");
        }

        // Extension check
        var allowedExtensions = new[] { ".xlsx", ".csv" };
        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(extension))
        {
            errors.Add($"File type {extension} not allowed");
        }

        // Content type check
        var allowedContentTypes = new[] { 
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "text/csv"
        };
        if (!allowedContentTypes.Contains(file.ContentType))
        {
            errors.Add($"Content type {file.ContentType} not allowed");
        }

        return new ValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors
        };
    }
}
```

---

## 23. Business Rules Catalog

### 23.1 Year-End Transfer Rules

| Rule ID | Description | Validation | Error Code |
|---------|-------------|------------|------------|
| YET-001 | FPS Year must be closed | `ProjectStatus = 'Closed'` | `YET.YEAR_NOT_CLOSED` |
| YET-002 | All invoices reconciled | `Sum(Invoices) = Sum(COIW)` | `YET.INVOICE_MISMATCH` |
| YET-003 | WIP within limits | `WIP_Current <= WIP_Limit` | `YET.WIP_EXCEEDED` |
| YET-004 | No open purchase orders | `COUNT(PO WHERE Status='Open') = 0` | `YET.OPEN_PO_EXISTS` |
| YET-005 | Manager approval required | `ManagerApproval = TRUE` | `YET.APPROVAL_MISSING` |

### 23.2 Project Validation Rules

| Rule ID | Description | Validation | Error Code |
|---------|-------------|------------|------------|
| PRJ-001 | Project code format | `Regex: ^[A-Z]{2}\d{4}$` | `PRJ.INVALID_CODE` |
| PRJ-002 | Manager must be active | `tlkpStaff.Active = 1` | `PRJ.INACTIVE_MANAGER` |
| PRJ-003 | Program must exist | `Program IN (SELECT ProgramNo FROM tblPrograms)` | `PRJ.INVALID_PROGRAM` |
| PRJ-004 | Budget must be positive | `Budget > 0` | `PRJ.INVALID_BUDGET` |
| PRJ-005 | Start date before end date | `StartDate < EndDate` | `PRJ.INVALID_DATES` |

### 23.3 Animal Requirements Rules

| Rule ID | Description | Validation | Error Code |
|---------|-------------|------------|------------|
| ANM-001 | Animal type must exist | `AnimalType IN tlkpAnimals` | `ANM.INVALID_TYPE` |
| ANM-002 | Days must be positive | `NumberOfDays > 0` | `ANM.INVALID_DAYS` |
| ANM-003 | Animals must be positive | `NumberOfAnimals > 0` | `ANM.INVALID_COUNT` |
| ANM-004 | Cost must be calculated | `Cost = Days * Animals * Rate` | `ANM.COST_MISMATCH` |

---

## 24. Data Quality Rules

### 24.1 Mandatory Fields by Entity

#### **tlkpProject**
```csharp
public class Project
{
    [Required] public string ParentProject { get; set; } = string.Empty;      // PK
    [Required] public string ProjectTitle { get; set; } = string.Empty;
    [Required] public string Program { get; set; } = string.Empty;            // FK
    [Required] public string Manager { get; set; } = string.Empty;            // FK
    [Required] public string ProjectStatus { get; set; } = string.Empty;
    [Required] public DateTime DateCreated { get; set; }

    // Optional but recommended
    public string? Customer { get; set; }
    public decimal? TransferIncome { get; set; }
    public decimal? CustIncome { get; set; }
}
```

#### **tblAnimalReq**
```csharp
public class AnimalRequirement
{
    [Required] public string JobCode { get; set; } = string.Empty;           // FK
    [Required] public string AnimalType { get; set; } = string.Empty;        // FK
    [Required, Range(0.01, double.MaxValue)] public double NumberOfDays { get; set; }
    [Required, Range(1, int.MaxValue)] public int NumberOfAnimals { get; set; }
}
```

### 24.2 Referential Integrity Checks

```sql
-- Orphaned animal requirements
SELECT ar.*
FROM tbl_animal_req ar
LEFT JOIN tlkp_project p ON ar.job_code = p.parent_project
WHERE p.parent_project IS NULL;

-- Invalid animal types
SELECT ar.*
FROM tbl_animal_req ar
LEFT JOIN tbl_animals a ON ar.animal_type = a.animal_type
WHERE a.animal_type IS NULL;

-- Projects without managers
SELECT p.*
FROM tlkp_project p
LEFT JOIN tlkp_staff s ON p.manager = s.staff_id
WHERE s.staff_id IS NULL;

-- Inactive managers assigned to active projects
SELECT p.parent_project, p.project_title, p.manager, s.staff_name
FROM tlkp_project p
INNER JOIN tlkp_staff s ON p.manager = s.staff_id
WHERE p.project_status = 'Active'
  AND s.active = 0;
```

### 24.3 Data Validation Queries

```sql
-- Invalid project codes (should be 2 letters + 4 digits)
SELECT parent_project, project_title
FROM tlkp_project
WHERE parent_project !~ '^[A-Z]{2}[0-9]{4}$';

-- Negative budgets
SELECT parent_project, project_title, transfer_income, cust_income
FROM tlkp_project
WHERE transfer_income < 0 OR cust_income < 0;

-- Future dated projects (created in the future)
SELECT parent_project, project_title, date_created
FROM tlkp_project
WHERE date_created > CURRENT_DATE;

-- Missing mandatory fields
SELECT parent_project
FROM tlkp_project
WHERE project_title IS NULL
   OR program IS NULL
   OR manager IS NULL
   OR project_status IS NULL;
```

---

## 25. Performance Baselines

### 25.1 Current System (Access Database)

| Operation | Avg Time | Max Time | Records | Bottleneck |
|-----------|----------|----------|---------|------------|
| Project List Query | 2.3s | 5.1s | 1,200 | No indexing |
| Year-End Transfer | 45s | 120s | 800 | Row-by-row processing |
| Summary Generation | 15s | 30s | 1,200 | Complex joins |
| Animal Req Load | 3.5s | 8s | 5,000 | Linked tables |
| Staff Lookup | 1.8s | 4s | 300 | Full table scan |

### 25.2 Target System (PostgreSQL + .NET)

| Operation | Target Avg | Target Max | Improvement | Strategy |
|-----------|------------|------------|-------------|----------|
| Project List Query | 0.5s | 1s | 78% faster | Indexes + pagination |
| Year-End Transfer | 10s | 20s | 83% faster | Bulk operations |
| Summary Generation | 3s | 6s | 80% faster | Materialized views |
| Animal Req Load | 0.8s | 2s | 77% faster | Eager loading |
| Staff Lookup | 0.3s | 0.5s | 83% faster | Caching |

### 25.3 Optimization Examples

```sql
-- Before (Access)
SELECT * FROM tlkpProject WHERE ProjectStatus = 'Active';

-- After (PostgreSQL with index)
CREATE INDEX idx_project_status ON tlkp_project(project_status);
CREATE INDEX idx_project_program ON tlkp_project(program);
CREATE INDEX idx_project_manager ON tlkp_project(manager);

-- Optimized query with specific columns
SELECT parent_project, project_title, manager, program, date_created
FROM tlkp_project
WHERE project_status = 'Active'
ORDER BY parent_project
LIMIT 100 OFFSET 0;
```

### 25.4 Performance Monitoring

```csharp
public class PerformanceMetrics
{
    private readonly ILogger<PerformanceMetrics> _logger;

    public async Task<T> MeasureAsync<T>(
        string operationName, 
        Func<Task<T>> operation)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await operation();
            stopwatch.Stop();

            _logger.LogInformation(
                "Operation {Operation} completed in {ElapsedMs}ms",
                operationName,
                stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex,
                "Operation {Operation} failed after {ElapsedMs}ms",
                operationName,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
```

---

## 26. Error Code Registry

### 26.1 Error Code Format

**Pattern:** `{DOMAIN}.{CATEGORY}_{SPECIFIC}`

### 26.2 Standard Error Codes

| Error Code | HTTP Status | Message Template | Retry? | Action |
|------------|-------------|------------------|--------|--------|
| `BATCHJOBS.DB_CONNECTION_FAILED` | 503 | Database connection failed: {details} | Yes | Check connection string |
| `BATCHJOBS.INVALID_YEAR` | 400 | Invalid FPS Year: {year} | No | Verify year parameter |
| `BATCHJOBS.JOB_ALREADY_RUNNING` | 409 | Job {jobName} is already running | No | Wait for completion |
| `BATCHJOBS.JOB_TIMEOUT` | 504 | Job {jobName} exceeded timeout of {minutes} minutes | Yes | Increase timeout |
| `BATCHJOBS.CONFIGURATION_MISSING` | 500 | Required configuration {key} is missing | No | Add to appsettings |
| `FPS.PROJECT_NOT_FOUND` | 404 | Project {projectCode} not found | No | Verify project exists |
| `FPS.YEAR_NOT_CLOSED` | 400 | FPS Year {year} is not closed | No | Close year first |
| `FPS.INVALID_PROJECT_CODE` | 400 | Project code {code} invalid format | No | Use format: XX9999 |
| `PACT.ANIMAL_TYPE_INVALID` | 400 | Animal type {type} is not valid | No | Check animal types |
| `PACT.REQUIREMENT_DUPLICATE` | 409 | Requirement already exists for {jobCode} | No | Update existing |
| `COSTBOOK.BUDGET_EXCEEDED` | 400 | Budget exceeded for project {code} | No | Request approval |
| `AUTH.TOKEN_EXPIRED` | 401 | Authentication token expired | No | Re-authenticate |
| `AUTH.INSUFFICIENT_PERMISSIONS` | 403 | User lacks permission: {permission} | No | Contact admin |

### 26.3 Error Response Model

```csharp
public class ErrorResponse
{
    public string ErrorCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string? CorrelationId { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}
```

---

## 27. Integration Contract Definitions

### 27.1 FPS API Contracts

```csharp
// GET /api/v1/projects/{projectCode}
public class ProjectResponse
{
    public string ParentProject { get; set; } = string.Empty;
    public string ProjectTitle { get; set; } = string.Empty;
    public string Program { get; set; } = string.Empty;
    public string Manager { get; set; } = string.Empty;
    public string ProjectStatus { get; set; } = string.Empty;
    public decimal CustIncome { get; set; }
    public decimal TransferIncome { get; set; }
    public DateTime DateCreated { get; set; }
    public ApiMeta Meta { get; set; } = new();
}

// POST /api/v1/projects/{projectCode}/year-end-transfer
public class YearEndTransferRequest
{
    [Required, Range(2000, 2100)]
    public int SourceYear { get; set; }

    [Required, Range(2000, 2100)]
    public int TargetYear { get; set; }

    [Required, EmailAddress]
    public string InitiatedBy { get; set; } = string.Empty;

    public bool ValidateOnly { get; set; }
}

public class YearEndTransferResponse
{
    public bool Success { get; set; }
    public string TransferId { get; set; } = string.Empty;
    public int ProjectsTransferred { get; set; }
    public decimal TotalAmountTransferred { get; set; }
    public List<ValidationError> Errors { get; set; } = new();
    public DateTime CompletedAtUtc { get; set; }
}
```

### 27.2 PACT API Contracts

```csharp
// GET /api/v1/animal-requirements/{jobCode}
public class AnimalRequirementResponse
{
    public string JobCode { get; set; } = string.Empty;
    public List<AnimalRequirement> Requirements { get; set; } = new();
    public decimal TotalCost { get; set; }
    public ApiMeta Meta { get; set; } = new();
}

public class AnimalRequirement
{
    public string AnimalType { get; set; } = string.Empty;
    public double NumberOfDays { get; set; }
    public double NumberOfAnimals { get; set; }
    public decimal DailyRate { get; set; }
    public decimal TotalCost { get; set; }
}

// POST /api/v1/animal-requirements
public class CreateAnimalRequirementRequest
{
    [Required]
    public string JobCode { get; set; } = string.Empty;

    [Required]
    public string AnimalType { get; set; } = string.Empty;

    [Required, Range(0.1, 365)]
    public double NumberOfDays { get; set; }

    [Required, Range(1, 10000)]
    public int NumberOfAnimals { get; set; }
}
```

---

## 28. Database Migration Scripts

### 28.1 Phase 1: Schema Creation

```sql
-- 001_CreateProjectsTable.sql
CREATE TABLE tlkp_project (
    parent_project VARCHAR(10) PRIMARY KEY,
    project_title VARCHAR(255) NOT NULL,
    program VARCHAR(10) NOT NULL,
    customer VARCHAR(100),
    manager VARCHAR(50) NOT NULL,
    transfer_income DECIMAL(18,2) DEFAULT 0,
    cust_income DECIMAL(18,2) DEFAULT 0,
    project_status VARCHAR(20) NOT NULL,
    date_created DATE NOT NULL,
    is_defra_project BOOLEAN DEFAULT FALSE,
    cost_centre NUMERIC,
    oracle_project_code VARCHAR(20),
    sub_account_code VARCHAR(10),
    project_group VARCHAR(50),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(100),
    updated_by VARCHAR(100),
    CONSTRAINT fk_project_program FOREIGN KEY (program) 
        REFERENCES tbl_programs(program_no),
    CONSTRAINT fk_project_manager FOREIGN KEY (manager) 
        REFERENCES tlkp_staff(staff_id),
    CONSTRAINT chk_project_status CHECK (
        project_status IN ('Active', 'Closed', 'On Hold', 'Cancelled')
    )
);

CREATE INDEX idx_project_program ON tlkp_project(program);
CREATE INDEX idx_project_status ON tlkp_project(project_status);
CREATE INDEX idx_project_manager ON tlkp_project(manager);
CREATE INDEX idx_project_date_created ON tlkp_project(date_created);

-- Add trigger for updated_at
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

CREATE TRIGGER update_project_updated_at BEFORE UPDATE ON tlkp_project
FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
```

### 28.2 Phase 2: Data Migration

```sql
-- 002_MigrateProjectData.sql
INSERT INTO tlkp_project (
    parent_project, project_title, program, customer, manager,
    transfer_income, cust_income, project_status, date_created,
    is_defra_project, cost_centre, oracle_project_code,
    sub_account_code, project_group, created_by
)
SELECT
    "ParentProject",
    "ProjectTitle",
    "Program",
    "Customer",
    "Manager",
    COALESCE("TransferIncome", 0),
    COALESCE("CustIncome", 0),
    "ProjectStatus",
    "DateCreated",
    CASE WHEN "IsDefraProject" = -1 THEN TRUE ELSE FALSE END,
    "CostCentre",
    "OracleProjectCode",
    "SubAccountCode",
    "ProjectGroup",
    'MIGRATION_SCRIPT'
FROM access_tlkp_project
ON CONFLICT (parent_project) DO NOTHING;

-- Log migration summary
DO $$
DECLARE
    source_count INTEGER;
    target_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO source_count FROM access_tlkp_project;
    SELECT COUNT(*) INTO target_count FROM tlkp_project;

    RAISE NOTICE 'Migration complete: % source records, % migrated', 
        source_count, target_count;
END $$;
```

### 28.3 Phase 3: Validation

```sql
-- 003_ValidateMigration.sql

-- Check record counts
SELECT
    'Access' AS source, COUNT(*) AS record_count
FROM access_tlkp_project
UNION ALL
SELECT
    'PostgreSQL' AS source, COUNT(*) AS record_count
FROM tlkp_project;

-- Check for data discrepancies
SELECT
    COALESCE(a."ParentProject", p.parent_project) AS project_code,
    a."ProjectTitle" AS access_title,
    p.project_title AS postgresql_title,
    CASE
        WHEN a."ParentProject" IS NULL THEN 'Missing in Access'
        WHEN p.parent_project IS NULL THEN 'Missing in PostgreSQL'
        WHEN a."ProjectTitle" <> p.project_title THEN 'Title mismatch'
        ELSE 'OK'
    END AS status
FROM access_tlkp_project a
FULL OUTER JOIN tlkp_project p ON a."ParentProject" = p.parent_project
WHERE a."ProjectTitle" <> p.project_title
   OR a."ParentProject" IS NULL
   OR p.parent_project IS NULL;

-- Verify referential integrity
SELECT 'Orphaned Projects' AS issue, COUNT(*) AS count
FROM tlkp_project p
LEFT JOIN tbl_programs prg ON p.program = prg.program_no
WHERE prg.program_no IS NULL
UNION ALL
SELECT 'Invalid Managers' AS issue, COUNT(*) AS count
FROM tlkp_project p
LEFT JOIN tlkp_staff s ON p.manager = s.staff_id
WHERE s.staff_id IS NULL;
```

---

## Appendix C: Validation Matrix

| Criterion | Knowledge Base | Application Code KB |
|-----------|----------------|---------------------|
| Contains actual project names | ❌ | ✅ |
| Contains actual code implementations | ❌ | ✅ |
| Contains database schemas | ❌ | ✅ |
| Contains configuration files | ❌ | ✅ |
| Contains business rules | ❌ | ✅ |
| Contains design patterns | ✅ | ❌ |
| Contains best practices | ✅ | ❌ |
| Contains technology standards | ✅ | ❌ |
| Reusable across projects | ✅ | ❌ |
| Project-specific | ❌ | ✅ |
| Contains stakeholder info | ❌ | ✅ |
| Contains compliance requirements | ❌ | ✅ |
| Contains error codes | ❌ | ✅ |
| Contains performance baselines | ❌ | ✅ |
| Contains migration scripts | ❌ | ✅ |
| Technology-agnostic guidance | ✅ | ❌ |

---

## Appendix D: Metadata Tags

### Knowledge Base Tags
```
#CleanArchitecture #EntityFrameworkCore #DependencyInjection
#RepositoryPattern #CQRS #APIDesign #Security #Performance
#Testing #DevOps #BestPractices #DesignPatterns #Microservices
#MessageQueues #BackgroundJobs #FileProcessing #NET10
#PostgreSQL #Serilog #AutoMapper #xUnit #Moq
```

### Application Code KB Tags
```
#AphaBatchJobs #DefraProject #DatabaseMigration #AccessToPostgreSQL
#YearEndTransfer #CrossDomain #FPS #PACT #PIMS #Costbook
#SQL2NET #BatchProcessing #OracleIntegration #HangfireJobs
#Defra #AnimalRequirements #ProjectManagement #FinancialSystem
```

---

## Appendix E: Application Code KB Folder Structure

```
/ApplicationCodeKB/
├── 01-ProjectOverview/
│   ├── ProjectCharter.pdf
│   ├── StakeholderMatrix.xlsx
│   └── CommunicationPlan.docx
├── 02-Analysis/
│   ├── CurrentStateAssessment.docx
│   ├── GapAnalysis.xlsx
│   ├── RiskRegister.xlsx
│   └── BusinessProcessMaps/
├── 03-Architecture/
│   ├── SystemArchitecture.pdf
│   ├── ComponentDiagram.vsdx
│   ├── SequenceDiagrams/
│   └── DeploymentArchitecture.pdf
├── 04-DatabaseDesign/
│   ├── CurrentSchema/
│   │   ├── AccessDatabase_ERD.pdf
│   │   ├── TableDefinitions.sql
│   │   ├── ViewDefinitions.sql
│   │   └── LinkedTableConfig.json
│   ├── TargetSchema/
│   │   ├── PostgreSQL_ERD.pdf
│   │   ├── TableCreationScripts/
│   │   ├── IndexDefinitions.sql
│   │   └── ConstraintDefinitions.sql
│   └── MigrationScripts/
│       ├── 001_CreateSchema.sql
│       ├── 002_MigrateData.sql
│       └── 003_ValidateData.sql
├── 05-ApplicationCode/
│   ├── Apha.BatchJobs.Console/
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── Jobs/
│   ├── Apha.FPS.Application/
│   ├── Apha.PACT.Application/
│   └── SharedKernel/
├── 06-DataMapping/
│   ├── FieldMappings.xlsx
│   ├── DataTypeConversions.xlsx
│   ├── BusinessRuleMappings.xlsx
│   └── ValidationRules.xlsx
├── 07-IntegrationContracts/
│   ├── APIEndpoints.md
│   ├── MessageSchemas/
│   ├── EventDefinitions.json
│   └── ExternalSystemAPIs/
├── 08-Configuration/
│   ├── appsettings.Development.json
│   ├── appsettings.Staging.json
│   ├── appsettings.Production.json (sanitized)
│   └── EnvironmentVariables.md
├── 09-Testing/
│   ├── TestStrategy.docx
│   ├── TestPlans/
│   ├── UnitTests/
│   ├── IntegrationTests/
│   ├── PerformanceTests/
│   └── TestData/
├── 10-Deployment/
│   ├── DeploymentGuide.md
│   ├── Dockerfile
│   ├── docker-compose.yml
│   ├── Kubernetes/
│   ├── CI-CD-Pipelines/
│   └── RollbackProcedures.md
├── 11-Operations/
│   ├── MonitoringSetup.md
│   ├── AlertingRules.json
│   ├── LoggingConfiguration.md
│   ├── BackupProcedures.md
│   └── DisasterRecovery.md
├── 12-Documentation/
│   ├── TechnicalSpecification.docx
│   ├── APIDocumentation.md
│   ├── UserGuides/
│   ├── AdminGuides/
│   └── Runbooks/
├── 13-Compliance/
│   ├── DataRetentionPolicy.pdf
│   ├── SecurityAssessment.docx
│   ├── GDPRChecklist.xlsx
│   └── AuditTrailRequirements.md
└── 14-ProjectManagement/
    ├── ProjectPlan.mpp
    ├── SprintBacklogs/
    ├── MeetingNotes/
    └── ChangeRequests/
```

---

## Appendix F: Enhanced Validation Checklist

### Before Submitting to Knowledge Base:
- [ ] Does it contain zero project-specific names/codes?
- [ ] Can it be understood without project context?
- [ ] Is it versioned (e.g., "EF Core 10.0 best practices")?
- [ ] Does it reference official documentation?
- [ ] Has it been peer-reviewed by senior developers?
- [ ] Is it applicable across multiple projects?
- [ ] Does it follow industry-standard patterns?
- [ ] Is it technology-agnostic where possible?

### Before Submitting to Application Code KB:
- [ ] Does it include version control information (Git commit hash)?
- [ ] Are all secrets/credentials sanitized?
- [ ] Does it include rollback procedures?
- [ ] Are all external dependencies documented with versions?
- [ ] Does it include contact information for support?
- [ ] Are all assumptions documented?
- [ ] Does it include troubleshooting guides?
- [ ] Is it linked to relevant user stories/tickets?
- [ ] Does it include test coverage information?
- [ ] Are deployment steps clearly documented?

---

## Document Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025 | GitHub Copilot | Initial creation from solution analysis |
| 1.1 | 2025 | GitHub Copilot | Added microservices patterns, message queues, background jobs, file processing |
| 1.2 | 2025 | GitHub Copilot | Added business rules catalog, data quality rules, performance baselines |
| 1.3 | 2025 | GitHub Copilot | Added error code registry, integration contracts, migration scripts |
| 1.4 | 2025 | GitHub Copilot | Added validation matrix, metadata tags, folder structure, enhanced checklists |

---

**End of Document**
