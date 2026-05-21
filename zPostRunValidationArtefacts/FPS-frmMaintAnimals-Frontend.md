# Frontend Analysis — FPS frmMaintAnimals

## HTML Prototype Structure

**Source:** `source/ui/fps/frmMaintAnimals.html`

**Pattern:** Single-file modal-based CRUD interface (no separate -add/-edit/-delete variants)

## Field Mapping

| HTML field name | HTML field type | Entity property | Entity type | Required | Notes |
|---|---|---|---|---|---|
| `animalType` | text | `AnimalType` | string | YES | Primary key component |
| `species` | text | `Species` | string? | NO | Optional classification |
| `securityLevel` | text | `SecurityLevel` | string? | NO | Security level designation |
| `dailyRate` | money (£ prefix) | `DailyRate` | decimal? | NO | Display with currency formatting |
| `defraDailyRate` | money (£ prefix) | `DefraDailyRate` | decimal? | NO | Display with currency formatting |
| `planFullWeeks` | checkbox | `PlanByWeek` | bool | NO | Checkbox state → bool |

## User Interface Elements

### DataGrid Table
- **Columns:**
  - AnimalType (string)
  - Species (string)
  - Security_Level (string)
  - DailyRate (money, £ prefix)
  - DefraDailyRate (money, £ prefix)
  - Plan Full Weeks (checkbox/boolean display)
  - Actions (Edit/Delete buttons)
- **Features:**
  - Pagination (with itemsPerPage selector)
  - Search filter
  - Sort by column headers

### Modal Dialog
- **Title:** "Add Animal" or "Edit Animal"
- **Fields:** All 6 fields listed in field mapping above
- **Actions:** Save, Cancel
- **Validation:** AnimalType required (HTML5 `required` attribute)

### Primary Actions
- **Add:** Opens modal in Add mode with empty fields
- **Edit:** Opens modal in Edit mode pre-populated with existing data
- **Delete:** Confirms deletion and removes record

## API Client Requirements

### Interface: `IFpsAnimalMaintenanceApiClient`
Methods needed:
- `Task<PaginationRes<AnimalRes>> GetAnimalsAsync(PaginationReq<string> query, int fpsYear)`
- `Task<AnimalRes?> GetAnimalByIdAsync(string animalType, int fpsYear)`
- `Task<AnimalRes> AddAnimalAsync(AnimalReq request)`
- `Task<AnimalRes> UpdateAnimalAsync(AnimalReq request)`
- `Task<bool> DeleteAnimalAsync(string animalType, int fpsYear)`

### Implementation: `FpsAnimalMaintenanceApiClient`
Base route: `api/v{version}/animal/maintenance` (or reuse existing `/animal` with new endpoints)

## Application Service Requirements

### Interface: `IAnimalMaintenanceService`
Methods:
- `Task<PaginationRes<AnimalRes>> GetAnimalsAsync(PaginationReq<string> query, int fpsYear)`
- `Task<AnimalRes?> GetAnimalByIdAsync(string animalType, int fpsYear)`
- `Task<AnimalRes> AddAnimalAsync(AnimalReq request)`
- `Task<AnimalRes> UpdateAnimalAsync(AnimalReq request)`
- `Task<bool> DeleteAnimalAsync(string animalType, int fpsYear)`

### Implementation: `AnimalMaintenanceService`
Wraps API client, adds error handling/logging

## ViewModel Requirements

### `AnimalMaintenanceViewModel`
Properties:
- `string AnimalType` (required)
- `string? Species`
- `string? SecurityLevel`
- `decimal? DailyRate`
- `decimal? DefraDailyRate`
- `bool PlanByWeek`
- `int FpsYear` (from context)

### Validation attributes:
- `[Required]` on `AnimalType`
- `[StringLength(50)]` on string fields
- `[Range]` on decimal fields if needed

## MVC Controller Requirements

### `AnimalMaintenanceController` in `Areas/FPS/Controllers`
Actions:
- `[HttpGet] Index()` — returns view with table
- `[HttpGet] GetAnimals(PaginationReq<string> query, int fpsYear)` — returns JSON for DataGrid
- `[HttpGet] GetAnimalById(string animalType, int fpsYear)` — returns JSON for Edit modal
- `[HttpPost] AddAnimal(AnimalMaintenanceViewModel model)` — creates new animal
- `[HttpPut] UpdateAnimal(AnimalMaintenanceViewModel model)` — updates existing animal
- `[HttpDelete] DeleteAnimal(string animalType, int fpsYear)` — deletes animal

## View Requirements

### `Views/FPS/AnimalMaintenance/Index.cshtml`
Structure:
- Page header with title "Animal Maintenance"
- Add button (opens modal)
- DataGrid table (client-side rendered via JavaScript)
- Pagination controls
- Modal dialog with form (Add/Edit mode)
- JavaScript file reference: `fps_animal_maintenance.js`

### GOV.UK Frontend Components:
- Button (primary for Save, secondary for Cancel/Add)
- Form Group (for each field)
- Text Input
- Checkboxes
- Error Summary (validation errors)
- Modal Dialog (custom or Bootstrap-based)

## File Changes — Phase 2 Frontend

| # | Action | File path (relative to `src/`) | Reason |
|---|--------|-------------------------------|--------|
| 1 | CREATE | `Apha.FPSApps.Infrastructure/Integrations/FPSApis/Clients/FpsAnimalMaintenanceApiClient.cs` | API client implementation |
| 2 | CREATE | `Apha.FPSApps.Application/Interfaces/FpsApiClients/IFpsAnimalMaintenanceApiClient.cs` | API client interface |
| 3 | CREATE | `Apha.FPSApps.Application/Services/FPS/AnimalMaintenanceService.cs` | Service implementation |
| 4 | CREATE | `Apha.FPSApps.Application/Interfaces/IAnimalMaintenanceService.cs` | Service interface |
| 5 | CREATE | `Apha.FPSApps.Web/Areas/FPS/Models/AnimalMaintenanceViewModel.cs` | ViewModel for form |
| 6 | CREATE | `Apha.FPSApps.Web/Areas/FPS/Controllers/AnimalMaintenanceController.cs` | MVC controller |
| 7 | CREATE | `Apha.FPSApps.Web/Areas/FPS/Views/AnimalMaintenance/Index.cshtml` | Razor view |
| 8 | CREATE | `Apha.FPSApps.Web/wwwroot/js/fps/fps_animal_maintenance.js` | Client-side JavaScript |
| 9 | UPDATE | `Apha.FPSApps.Application/Interfaces/IAnimalMaintenanceService.cs` | Register service in DI container |

## Dependencies

- Existing `Animal` entity in `Apha.FPS.Core`
- Existing `AnimalRes` and `AnimalReq` contracts (verify they support maintenance fields)
- GOV.UK Frontend styles and components
- Bootstrap modal (if used)
- DataTables or custom grid implementation
