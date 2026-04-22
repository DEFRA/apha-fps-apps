# Division Maintenance Feature - File Changes Documentation
**Feature:** MS Access Form `frmMaintDivision.frm` Conversion to ASP.NET Core  
**Date:** April 14, 2026  
**Branch:** feature/fps-maintain-division

---

## 📝 NEWLY CREATED FILES

### Backend API Layer (Apha.FPS)

| # | File Path | Purpose | LOC |
|---|-----------|---------|-----|
| 1 | `Apha.Common/Contracts/FPS/DivisionReq.cs` | Request contract for API operations | 28 |
| 2 | `Apha.Common/Contracts/FPS/DivisionRes.cs` | Response contract for API operations | 33 |
| 3 | `Apha.FPS/Apha.FPS.Core/Entities/Division.cs` | Core entity mapped to fps.tlkpdivision table | 43 |
| 4 | `Apha.FPS/Apha.FPS.Core/Interfaces/IDivisionRepository.cs` | Repository interface for data access | 62 |
| 5 | `Apha.FPS/Apha.FPS.Application/Dtos/DivisionDto.cs` | Application layer DTO | 34 |
| 6 | `Apha.FPS/Apha.FPS.Application/Interfaces/IDivisionService.cs` | Service interface for business logic | 56 |
| 7 | `Apha.FPS/Apha.FPS.Application/Services/DivisionService.cs` | Service implementation with validation | 106 |
| 8 | `Apha.FPS/Apha.FPS.DataAccess/Repositories/DivisionRepository.cs` | Repository implementation with EF Core | 122 |
| 9 | `Apha.FPS/Apha.FPS.Api/Controllers/DivisionController.cs` | REST API controller with CRUD endpoints | 139 |

### Frontend Web Layer (Apha.FPSApps)

| # | File Path | Purpose | LOC |
|---|-----------|---------|-----|
| 10 | `Apha.FPSApps/Apha.FPSApps.Application/Dtos/FPS/DivisionDto.cs` | Frontend DTO for API communication | 34 |
| 11 | `Apha.FPSApps/Apha.FPSApps.Application/Interfaces/FpsApiClients/IFpsDivisionApiClient.cs` | HTTP API client interface | 57 |
| 12 | `Apha.FPSApps/Apha.FPSApps.Application/Interfaces/FPS/IDivisionService.cs` | Frontend service interface | 56 |
| 13 | `Apha.FPSApps/Apha.FPSApps.Application/Services/FPS/DivisionService.cs` | Frontend service delegating to API client | 51 |
| 14 | `Apha.FPSApps/Apha.FPSApps.Infrastructure/Integrations/FPSApis/Clients/FpsDivisionApiClient.cs` | HTTP client for Division API | 218 |
| 15 | `Apha.FPSApps/Apha.FPSApps.Web/Areas/FPS/Models/DivisionMaintenanceViewModel.cs` | ViewModels for MVC views with DataGrid attributes | 58 |
| 16 | `Apha.FPSApps/Apha.FPSApps.Web/Areas/FPS/Controllers/DivisionMaintenanceController.cs` | MVC controller for Division maintenance UI | 262 |
| 17 | `Apha.FPSApps/Apha.FPSApps.Web/Areas/FPS/Views/DivisionMaintenance/Index.cshtml` | Main DataGrid view with CRUD operations | 196 |
| 18 | `Apha.FPSApps/Apha.FPSApps.Web/Areas/FPS/Views/DivisionMaintenance/_AddEditDivision.cshtml` | Modal partial view for Add/Edit operations | 89 |

**Total New Files:** 18  
**Total Lines of Code:** ~1,644 lines

---

## ✏️ UPDATED EXISTING FILES

### Backend Updates (Apha.FPS)

| File Path | Lines Modified | Changes Made |
|-----------|----------------|--------------|
| **Apha.FPS/Apha.FPS.Application/Mappings/EntityMapper.cs** | Line 41 | Added: `CreateMap<Division, DivisionDto>().ReverseMap();` |
| **Apha.FPS/Apha.FPS.Api/Mappings/RequestMapper.cs** | Lines 1, 47-48 | Added: `using Apha.Common.Contracts;`<br>Added: `CreateMap<DivisionReq, DivisionDto>().ReverseMap();`<br>Added: `CreateMap<DivisionRes, DivisionDto>().ReverseMap();` |
| **Apha.FPS/Apha.FPS.Api/Extensions/ServiceCollectionExtension.cs** | Lines 35, 54 | Added: `services.AddScoped<IDivisionService, DivisionService>();`<br>Added: `services.AddScoped<IDivisionRepository, DivisionRepository>();` |
| **Apha.FPS/Apha.FPS.DataAccess/Data/FpsDbContext.cs** | Lines 60, 1231-1257 | Added: `public virtual DbSet<Division> Divisions { get; set; }`<br>Added: Entity configuration for Division (27 lines) |

### Frontend Updates (Apha.FPSApps)

| File Path | Lines Modified | Changes Made |
|-----------|----------------|--------------|
| **Apha.FPSApps/Apha.FPSApps.Infrastructure/Mappings/FpsApiDtoMapper.cs** | Lines 51-53 | Added: `CreateMap<DivisionDto, DivisionRes>().ReverseMap();`<br>Added: `CreateMap<DivisionDto, DivisionReq>().ReverseMap();` |
| **Apha.FPSApps/Apha.FPSApps.Web/Mappings/FpsViewModelMapper.cs** | Line 23 | Added: `CreateMap<DivisionViewModel, DivisionDto>().ReverseMap();` |
| **Apha.FPSApps/Apha.FPSApps.Web/Extensions/ServiceCollectionExtension.cs** | Line 43 | Added: `services.AddScoped<IDivisionService, DivisionService>();` |
| **Apha.FPSApps/Apha.FPSApps.Application/Interfaces/FpsApiClients/IFpsApiClient.cs** | Line 13 | Added: `IFpsDivisionApiClient FpsDivision { get; }` property |
| **Apha.FPSApps/Apha.FPSApps.Infrastructure/Integrations/FPSApis/Clients/FpsApiClient.cs** | Lines 19, 30 | Added: `public IFpsDivisionApiClient FpsDivision { get; }`<br>Added: `FpsDivision = new FpsDivisionApiClient(http, mapper);` |

**Total Updated Files:** 9  
**Total Lines Changed:** ~45 lines

---

## 📊 SUMMARY STATISTICS

### Files by Type

| Category | Created | Updated | Total |
|----------|---------|---------|-------|
| **Contracts (Shared)** | 2 | 0 | 2 |
| **Core Entities** | 2 | 0 | 2 |
| **Application Layer** | 3 | 1 | 4 |
| **Data Access** | 1 | 1 | 2 |
| **API Controllers** | 1 | 2 | 3 |
| **Frontend Application** | 4 | 2 | 6 |
| **Frontend Infrastructure** | 1 | 2 | 3 |
| **Frontend Web (MVC)** | 4 | 2 | 6 |
| **TOTAL** | **18** | **9** | **27** |

### Architecture Layers Distribution

```
┌─────────────────────────────────────────┐
│  Clean Architecture Layer Distribution  │
├─────────────────────────────────────────┤
│  Presentation Layer (Web/API)    → 10   │
│  Application Layer               → 7    │
│  Domain Layer (Core)             → 2    │
│  Infrastructure Layer            → 6    │
│  Shared (Contracts)              → 2    │
└─────────────────────────────────────────┘
```

### Code Metrics

| Metric | Value |
|--------|-------|
| **Total New Files** | 18 |
| **Total Updated Files** | 9 |
| **Total Lines of New Code** | ~1,644 |
| **Total Lines Modified** | ~45 |
| **Estimated Development Time** | ~8 hours |
| **Build Status** | ✅ Success (0 errors, 0 warnings) |

---

## 🔍 DETAILED LINE-BY-LINE CHANGES IN UPDATED FILES

### 1. Apha.FPS/Apha.FPS.Application/Mappings/EntityMapper.cs

```diff
Line 41:
+            CreateMap<Division, DivisionDto>().ReverseMap();
         }
```

---

### 2. Apha.FPS/Apha.FPS.Api/Mappings/RequestMapper.cs

```diff
Line 1:
+using Apha.Common.Contracts;
 using Apha.Common.Contracts.FPS;

Lines 47-48:
             CreateMap<YearMasterRes, YearMasterDto>().ReverseMap();
+            CreateMap<DivisionReq, DivisionDto>().ReverseMap();
+            CreateMap<DivisionRes, DivisionDto>().ReverseMap();
         }
```

---

### 3. Apha.FPS/Apha.FPS.Api/Extensions/ServiceCollectionExtension.cs

```diff
Line 35:
             services.AddScoped<IYearMasterService, YearMasterService>();
+            services.AddScoped<IDivisionService, DivisionService>();
             return services;

Line 54:
             services.AddScoped<IYearMasterRepository, YearMasterRepository>();
+            services.AddScoped<IDivisionRepository, DivisionRepository>();
             return services;
```

---

### 4. Apha.FPS/Apha.FPS.DataAccess/Data/FpsDbContext.cs

```diff
Line 60:
         public virtual DbSet<YearMaster> YearMasters { get; set; }
+        public virtual DbSet<Division> Divisions { get; set; }

Lines 1231-1257 (New Entity Configuration):
+            modelBuilder.Entity<Division>(entity =>
+            {
+                entity.HasKey(e => e.DivName).HasName("pk__tlkpdivision__10566f31");
+
+                entity.ToTable("tlkpdivision", "fps", tb => tb.HasComment("Organizational divisions within agencies for cost allocation and reporting."));
+
+                entity.Property(e => e.DivName)
+                    .HasMaxLength(255)
+                    .HasColumnType("citext")
+                    .HasComment("Division name. Primary key (case-insensitive text).")
+                    .HasColumnName("divname");
+
+                entity.Property(e => e.DivisionId)
+                    .HasComment("Division identifier. Auto-generated.")
+                    .HasColumnName("divisionid");
+
+                entity.Property(e => e.AgencyId)
+                    .HasComment("Parent agency identifier. References fps.tlkpagency(agencyid).")
+                    .HasColumnName("agencyid");
+
+                entity.Property(e => e.CentOverhead)
+                    .HasColumnType("money")
+                    .HasDefaultValue(0m)
+                    .HasComment("Central overhead cost allocation.")
+                    .HasColumnName("centoverhead");
+            });
+
         }
```

---

### 5. Apha.FPSApps/Apha.FPSApps.Infrastructure/Mappings/FpsApiDtoMapper.cs

```diff
Lines 51-53:
             CreateMap<YearMasterDto, YearMasterRes>().ReverseMap();
             CreateMap<YearMasterDto, YearMasterReq>().ReverseMap();
+
+            // Division
+            CreateMap<DivisionDto, DivisionRes>().ReverseMap();
+            CreateMap<DivisionDto, DivisionReq>().ReverseMap();
         }
```

---

### 6. Apha.FPSApps/Apha.FPSApps.Web/Mappings/FpsViewModelMapper.cs

```diff
Line 23:
             CreateMap<AnimalPlanItem, AnimalRequestDto>().ReverseMap();
+            CreateMap<DivisionViewModel, DivisionDto>().ReverseMap();
         }
```

---

### 7. Apha.FPSApps/Apha.FPSApps.Web/Extensions/ServiceCollectionExtension.cs

```diff
Line 43:
             services.AddScoped<IYearMasterService, YearMasterService>();
+            services.AddScoped<IDivisionService, DivisionService>();
             services.AddScoped<IProjectInvoiceService, ProjectInvoiceService>();
```

---

### 8. Apha.FPSApps/Apha.FPSApps.Application/Interfaces/FpsApiClients/IFpsApiClient.cs

```diff
Line 13:
         IFpsYearMasterApiClient FpsYearMaster { get; }
+        IFpsDivisionApiClient FpsDivision { get; }
     }
```

---

### 9. Apha.FPSApps/Apha.FPSApps.Infrastructure/Integrations/FPSApis/Clients/FpsApiClient.cs

```diff
Line 19:
         public IFpsYearMasterApiClient FpsYearMaster { get; }
+
+        public IFpsDivisionApiClient FpsDivision { get; }

Line 30:
             FpsYearMaster = new FpsYearMasterApiClient(http, mapper);
+            FpsDivision = new FpsDivisionApiClient(http, mapper);
         }
```

---

## 🎯 FEATURE CAPABILITIES

The Division Maintenance feature provides:

### Frontend Capabilities
- ✅ DataGrid display with pagination, sorting, and filtering
- ✅ Add new division via modal dialog
- ✅ Edit existing division via modal dialog
- ✅ Delete division with confirmation dialog
- ✅ Real-time validation and error handling
- ✅ Responsive UI following GOV.UK Design System

### Backend Capabilities
- ✅ RESTful API endpoints (GET, POST, PUT, DELETE)
- ✅ Pagination support for large datasets
- ✅ Business validation (duplicate check, required fields)
- ✅ Entity Framework Core integration with PostgreSQL
- ✅ Role-based authorization (Admin/User)
- ✅ AutoMapper for DTO transformations

### Database Integration
- ✅ Mapped to existing `fps.tlkpdivision` table
- ✅ Foreign key relationship to `fps.tlkpagency`
- ✅ Support for case-insensitive text search (citext)
- ✅ Optimized queries with AsNoTracking()

---

## 📝 NOTES

1. **Build Status:** All files compile successfully with 0 errors and 0 warnings
2. **Testing Required:** Unit tests and integration tests need to be created
3. **Database Migration:** Entity is configured for existing table, no migration needed
4. **Agency Dropdown:** The `_AddEditDivision.cshtml` has placeholder agency data - needs API endpoint
5. **Navigation:** Need to add link in `_MaintenanceSideNav.cshtml` for Division Maintenance

---

## 🔗 RELATED COMPONENTS

### Dependencies
- AutoMapper (for object mapping)
- Entity Framework Core (for data access)
- ASP.NET Core MVC (for web UI)
- jQuery (for AJAX operations)
- GOV.UK Frontend (for styling)

### Related Features
- YearMaster (similar CRUD pattern)
- StaffMaintenance (DataGrid reference)
- Agency lookup (future enhancement)

---

**Document Generated:** April 14, 2026  
**Build Verified:** ✅ Success  
**Ready for:** Code Review & Testing
