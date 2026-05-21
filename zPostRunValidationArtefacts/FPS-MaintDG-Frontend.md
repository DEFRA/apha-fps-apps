# Frontend Analysis — FPS MaintDG

## Field Mapping

| HTML id / label | Entity field | ViewModel property | `asp-for` value |
|---|---|---|---|
| `modal-dg-divgrade` / Div. Grade | `DivisionGradeCode` | `DivisionGradeCode` | `DivisionGradeCode` |
| `modal-dg-gradecode` / GradeCode | `GradeCode` | `GradeCode` | `GradeCode` |
| `modal-dg-division` / Division | `Division` | `Division` | `Division` |
| `modal-dg-chargerate` / ChargeRate | `ChargeRate` | `ChargeRate` | `ChargeRate` |
| `modal-dg-directrate` / DirectRate | `DirectRate` | `DirectRate` | `DirectRate` |
| `modal-dg-payrate` / PayRate | `PayRate` | `PayRate` | `PayRate` |
| `modal-dg-npr` / NPR | `Npr` | `Npr` | `Npr` |
| `modal-dg-ohr` / OHR | `Ohr` | `Ohr` | `Ohr` |

## File Changes — Phase 2 Frontend

| # | Action | File path (relative to `src/`) | Reason |
|---|--------|-------------------------------|--------|
| 1 | CREATE | `Apha.FPSApps/Apha.FPSApps.Application/Dtos/FPS/DivisionGradeDto.cs` | Frontend DTO |
| 2 | CREATE | `Apha.FPSApps/Apha.FPSApps.Application/Interfaces/FPSApiClients/IFpsMaintDGApiClient.cs` | API client interface |
| 3 | MODIFY | `Apha.FPSApps/Apha.FPSApps.Application/Interfaces/FPSApiClients/IFpsApiClient.cs` | Register new client |
| 4 | CREATE | `Apha.FPSApps/Apha.FPSApps.Application/Interfaces/FPS/IMaintDGService.cs` | Frontend service interface |
| 5 | CREATE | `Apha.FPSApps/Apha.FPSApps.Application/Services/FPS/MaintDGService.cs` | Frontend service implementation |
| 6 | CREATE | `Apha.FPSApps/Apha.FPSApps.Infrastructure/Integrations/FpsApis/Clients/FpsMaintDGApiClient.cs` | HTTP client implementation |
| 7 | MODIFY | `Apha.FPSApps/Apha.FPSApps.Infrastructure/Integrations/FpsApis/Clients/FpsApiClient.cs` | Wire new client |
| 8 | MODIFY | `Apha.FPSApps/Apha.FPSApps.Infrastructure/Mappings/FpsApiDtoMapper.cs` | Add DivisionGrade mappings |
| 9 | CREATE | `Apha.FPSApps/Apha.FPSApps.Web/Areas/FPS/Models/MaintDGViewModel.cs` | ViewModel + GridItem |
| 10 | CREATE | `Apha.FPSApps/Apha.FPSApps.Web/Areas/FPS/Controllers/MaintDGController.cs` | MVC controller |
| 11 | CREATE | `Apha.FPSApps/Apha.FPSApps.Web/Areas/FPS/Views/MaintDG/Index.cshtml` | Grid index view |
| 12 | CREATE | `Apha.FPSApps/Apha.FPSApps.Web/Areas/FPS/Views/MaintDG/_AddEditMaintDG.cshtml` | Modal partial |
| 13 | MODIFY | `Apha.FPSApps/Apha.FPSApps.Web/Mappings/FpsViewModelMapper.cs` | Add MaintDGItem mapping |
| 14 | MODIFY | `Apha.FPSApps/Apha.FPSApps.Web/Extensions/ServiceCollectionExtension.cs` | Register service |
