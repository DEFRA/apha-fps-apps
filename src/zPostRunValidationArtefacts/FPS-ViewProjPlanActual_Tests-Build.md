# Build Issues — FPS-ViewProjPlanActual_Tests

**Final status**: ✅ BUILD SUCCEEDED  
**Errors**: 0 | **Warnings**: 4 (pre-existing CS8625 in Apha.Costbook and Apha.PIMS — unrelated to this feature)  
**Unit tests**: 56 passed, 0 failed (across 5 new test classes)  
**Build date**: 2026-04-29  
**Solution**: `Apha.FPS.All.sln`

---

## Issues encountered during development

| # | File | Issue | Resolution |
|---|------|-------|------------|
| 1 | `MonthlyOutputCalcsRepository.cs` | `ApplySorting` was called but not defined (unlike `TimeCostCalcsRepository`, which defines it as a private static method) | Added `ApplySorting`, `ApplySortingByProperty`, and `ApplyOrder` private static methods following the `TimeCostCalcsRepository` pattern |
| 2 | `MonthlyOutputCalcsServiceTests.cs` | Test stubbed `GetTotalActualByProjectAsync` with a DTO, but the repository interface returns `Task<(double TotalVolume, double TotalCost)>` | Fixed stub to return the value-tuple `(TotalVolume: 10.0, TotalCost: 1200.0)` |

---

## Files created / modified

### Apha.Common
| File | Action |
|------|--------|
| `Contracts/FPS/MonthlyOutputCalcsViewRes.cs` | CREATED |
| `Contracts/FPS/MonthlyOutputCalcsTotalsRes.cs` | CREATED |
| `Contracts/FPS/MonthlyOutputCalcsReq.cs` | CREATED |
| `Constants/FpsApiEndpoints.cs` | UPDATED — added 3 MonthlyOutputCalcs endpoints |
| `Constants/PactApiEndpoints.cs` | UPDATED — added `GetTotalTestPlanCost` |

### Apha.FPS.Core
| File | Action |
|------|--------|
| `Entities/MonthlyOutputCalcsView.cs` | CREATED — keyless, maps to `fps.vmonthlyoutputcalcs` |
| `Entities/MonthlyOutput.cs` | CREATED — writable entity for delete |
| `Interfaces/IMonthlyOutputCalcsRepository.cs` | CREATED |

### Apha.FPS.Application
| File | Action |
|------|--------|
| `Dtos/MonthlyOutputCalcsViewDto.cs` | CREATED |
| `Dtos/MonthlyOutputCalcsTotalsDto.cs` | CREATED |
| `Interfaces/IMonthlyOutputCalcsService.cs` | CREATED |
| `Services/MonthlyOutputCalcsService.cs` | CREATED |
| `Mappings/EntityMapper.cs` | UPDATED — added `MonthlyOutputCalcsView ↔ Dto` mapping |

### Apha.FPS.DataAccess
| File | Action |
|------|--------|
| `Data/MonthlyOutputCalcsViewMap.cs` | CREATED — EF config, `ToView("vmonthlyoutputcalcs", "fps")` |
| `Data/MonthlyOutputMap.cs` | CREATED — EF config for writable `monthlyoutput` table |
| `Repositories/MonthlyOutputCalcsRepository.cs` | CREATED |
| `Data/FpsDbContext.cs` | UPDATED — added `MonthlyOutputCalcsViews` + `MonthlyOutputs` DbSets |

### Apha.FPS.Api
| File | Action |
|------|--------|
| `Controllers/MonthlyOutputCalcsController.cs` | CREATED |
| `Mappings/RequestMapper.cs` | UPDATED — added `MonthlyOutputCalcs Dto ↔ Res` |
| `Extensions/ServiceCollectionExtension.cs` | UPDATED — registered `IMonthlyOutputCalcsService` + repository |

### Apha.PACT.Core
| File | Action |
|------|--------|
| `Interfaces/ITestRequirementRepository.cs` | UPDATED — added `GetTotalTestPlanCostAsync` |

### Apha.PACT.Application
| File | Action |
|------|--------|
| `Interfaces/ITestRequirementService.cs` | UPDATED — added `GetTotalTestPlanCostAsync` |
| `Services/TestRequirementService.cs` | UPDATED — delegation added |

### Apha.PACT.DataAccess
| File | Action |
|------|--------|
| `Repository/TestRequirementRepository.cs` | UPDATED — LINQ sum implementation |

### Apha.PACT.Api
| File | Action |
|------|--------|
| `Controllers/TestRequirementController.cs` | UPDATED — added `GET totalcost/{parentProject}` endpoint |

### Apha.FPSApps.Application
| File | Action |
|------|--------|
| `Dtos/FPS/MonthlyOutputCalcsViewDto.cs` | CREATED |
| `Dtos/FPS/MonthlyOutputCalcsTotalsDto.cs` | CREATED |
| `Interfaces/FpsApiClients/IFpsMonthlyOutputCalcsApiClient.cs` | CREATED |
| `Interfaces/FpsApiClients/IFpsApiClient.cs` | UPDATED — added `FpsMonthlyOutputCalcs` property |
| `Interfaces/FPS/IProjectTestPlanActualService.cs` | CREATED |
| `Services/FPS/ProjectTestPlanActualService.cs` | CREATED |
| `Interfaces/PactApiClients/IPactTestRequirementApiClient.cs` | UPDATED — added `GetTotalTestPlanCostAsync` |
| `Interfaces/PACT/ITestRequirementService.cs` | UPDATED — added `GetTotalTestPlanCostAsync` |
| `Services/PACT/TestRequirementService.cs` | UPDATED — delegation added |

### Apha.FPSApps.Infrastructure
| File | Action |
|------|--------|
| `Integrations/FPSApis/Clients/FpsMonthlyOutputCalcsApiClient.cs` | CREATED |
| `Integrations/FPSApis/Clients/FpsApiClient.cs` | UPDATED — added `FpsMonthlyOutputCalcs` |
| `Integrations/PACTApis/Clients/PactTestRequirementApiClient.cs` | UPDATED — added `GetTotalTestPlanCostAsync` |
| `Mappings/FpsApiDtoMapper.cs` | UPDATED — added `MonthlyOutputCalcs` mappings |

### Apha.FPSApps.Web
| File | Action |
|------|--------|
| `Areas/FPS/Models/CompareTests2Item.cs` | CREATED |
| `Areas/FPS/Models/ProjectTestPlanActualViewModel.cs` | CREATED |
| `Areas/FPS/Controllers/ProjectTestPlanActualController.cs` | CREATED |
| `Areas/FPS/Views/ProjectTestPlanActual/Index.cshtml` | CREATED |
| `Mappings/FpsViewModelMapper.cs` | UPDATED — added `CompareTests2Item ↔ MonthlyOutputCalcsViewDto` |
| `Extensions/ServiceCollectionExtension.cs` | UPDATED — registered `IProjectTestPlanActualService` |

### Unit Tests
| File | Tests |
|------|-------|
| `Apha.FPS.Application.UnitTests/.../MonthlyOutputCalcsServiceTests.cs` | 8 |
| `Apha.FPS.Api.UnitTests/.../MonthlyOutputCalcsControllerTests.cs` | 15 |
| `Apha.FPSApps.Application.UnitTests/.../ProjectTestPlanActualServiceTests.cs` | 10 |
| `Apha.FPSApps.Infrastructure.UnitTests/.../FpsMonthlyOutputCalcsApiClientTests.cs` | 8 |
| `Apha.FPSApps.Web.UnitTests/.../ProjectTestPlanActualControllerTests.cs` | 15 |
| **Total** | **56** |

| # | Category | Severity | File | Error message | Root cause | Fix applied |
|---|----------|----------|------|---------------|------------|-------------|
