# Backend Analysis — FPS frmMaintAnimals

## Reference Map

| Referenced name | Type | Triggering event / context | Parameters / notes |
|---|---|---|---|
| `tblAnimals_MAP` | RecordSource (likely a view/alias) | Form `RecordSource` | Maps to `tblanimals` table — read/write |
| *(No MS Access queries found)* | — | — | — |
| *(No stored procedures found)* | — | — | — |
| *(No functions found)* | — | — | — |
| *(No triggers found)* | — | — | — |
| *(No BAS functions found)* | — | — | — |

## Artefact Detail

### Tables

**`tblanimals`** (`source/pgsql/fps/Tables/tblanimals.sql`)
- **Purpose:** Animal master table for animal types, rates, and configuration
- **Primary Key:** `(animaltype, fpsyear)`
- **Columns:** 
  - `animaltype` (varchar(50), NOT NULL) — Animal type identifier
  - `species` (varchar(50)) — Species classification
  - `security_level` (varchar(50)) — Security level designation
  - `dailyrate` (money) — Daily rate for costing
  - `planbyweek` (boolean, NOT NULL, default false) — Whether to plan in full weeks
  - `defradailyrate` (money) — DEFRA daily rate
  - `fpsyear` (integer, NOT NULL) — Fiscal year partition key
- **Usage in form:** CRUD operations on animal master data
- **LINQ implementation:** 
  - GetAll → `AsNoTracking().Where(year).ToListAsync()`
  - GetById → `AsNoTracking().Where(type, year).FirstOrDefaultAsync()`
  - Create → `Add()` + `SaveChangesAsync()`
  - Update → load, mutate, `SaveChangesAsync()`
  - Delete → load, `Remove()`, `SaveChangesAsync()`
- **Optimization notes:** Standard CRUD pipeline — `AsNoTracking()` for reads, tracked for writes

### Form VBA Analysis

**Form properties:**
- `RecordSource = "tblAnimals_MAP"` — likely maps to `tblanimals` table
- `DefaultView` = not specified (default 0 = Single Form)
- No `AllowAdditions`, `AllowEdits`, `AllowDeletions` specified = all enabled by default

**VBA code:**
- `Button17_Click()` — likely close/cancel button
- `Form_Close()` — cleanup on close
- No complex VBA logic, queries, or SP calls found

**HTML prototype analysis:**
- DataGrid table with columns: AnimalType, Species, Security_Level, DailyRate, DefraDailyRate, Plan Full Weeks
- Add button opens modal for new record
- Inline Edit/Delete actions in Actions column
- Modal has all fields editable for both Add and Edit modes

## C# Artefact Mapping

| MS Access artefact | C# implementation | Layer | File |
|---|---|---|---|
| `tblAnimals_MAP` RecordSource | `GetAllAnimalsAsync()` on `IAnimalRepository` | Data Access | `AnimalRepository.cs` |
| Add new animal | `AddAnimalAsync(Animal)` on `IAnimalRepository` | Data Access | `AnimalRepository.cs` |
| Update animal | `UpdateAnimalAsync(Animal)` on `IAnimalRepository` | Data Access | `AnimalRepository.cs` |
| Delete animal | `DeleteAnimalAsync(string, int)` on `IAnimalRepository` | Data Access | `AnimalRepository.cs` |
| Service orchestration | `IAnimalMaintenanceService` (new) or extend `IAnimalService` | Application | `AnimalService.cs` |
| API endpoints | `AnimalController` — add CRUD endpoints | API | `AnimalController.cs` |

**Existing vs. New:**
- ✅ `Animal` entity already exists
- ✅ `IAnimalRepository` and `AnimalRepository` exist — need to add CRUD methods
- ✅ `IAnimalService` and `AnimalService` exist — need to add CRUD methods
- ✅ `AnimalController` exists — need to add CRUD endpoints
- ❌ Frontend Animal maintenance controller — needs creation
- ❌ ViewModel and View — needs creation

## Raw SQL Decisions

*(None required — all operations are simple CRUD on indexed PK columns, fully expressible in LINQ)*

## File Changes — Phase 1 Backend

| # | Action | File path (relative to `src/`) | Reason |
|---|--------|-------------------------------|--------|
| 1 | UPDATE | `Apha.FPS.Core/Interfaces/IAnimalRepository.cs` | Add GetAllAnimalsAsync, GetAnimalByIdAsync, AddAnimalAsync, UpdateAnimalAsync, DeleteAnimalAsync methods |
| 2 | UPDATE | `Apha.FPS.DataAccess/Repositories/AnimalRepository.cs` | Implement CRUD methods for Animal master maintenance |
| 3 | UPDATE | `Apha.FPS.Application/Interfaces/IAnimalService.cs` | Add CRUD service methods for Animal maintenance |
| 4 | UPDATE | `Apha.FPS.Application/Services/AnimalService.cs` | Implement CRUD service methods |
| 5 | UPDATE | `Apha.Common/Contracts/FPS/AnimalReq.cs` | Verify exists or add for Animal maintenance request |
| 6 | UPDATE | `Apha.Common/Contracts/FPS/AnimalRes.cs` | Verify exists or add for Animal maintenance response |
| 7 | UPDATE | `Apha.FPS.Api/Controllers/AnimalController.cs` | Add CRUD endpoints for Animal maintenance |
| 8 | UPDATE | `Apha.FPS.Application/Mappings/EntityMapper.cs` | Add Animal maintenance mappings if needed |
| 9 | UPDATE | `Apha.FPS.Api/Mappings/RequestMapper.cs` | Add Animal maintenance request/response mappings if needed |
