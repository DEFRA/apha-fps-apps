# Frontend Analysis — FPS frmProgrammeManager

## Field Mapping

This form has NO editable fields — it's a read-only navigation/selection interface.

### UI Elements

| HTML `id` / label text | Purpose | ViewModel property | Notes |
|---|---|---|---|
| `programmeDropdown` | Programme filter dropdown | `SelectedProgramNo` (string) | Bound to `IProgramService.GetAllProgramsAsync()` |
| `searchInput` | Client-side search box | *(JavaScript only)* | Filters table rows on client side, no server binding |
| Table columns: `Programme`, `Project Code` | Display project list | *(projected from service)* | Read from `IProjectService.GetProjectsByProgramAsync()` |
| "Plan" button (per row) | Navigate to project planning | — | `asp-action="Index"` `asp-controller="ProjectPlanning"` `asp-route-parentProject="@item.ParentProject"` |
| "Edit" button (per row) | Navigate to project edit | — | `asp-action="Details"` `asp-controller="ProjectMaintenance"` `asp-route-id="@item.ParentProject"` |

**Input count check:** 1 `<select>` (programme dropdown) + 1 `<input type="text">` (search box) = 2 total. Neither submits data—both are navigation aids only.

**No `asp-for` bindings needed** — form does not submit data. Dropdown change triggers page reload with query string; search is pure JavaScript.

## File Changes — Phase 2 Frontend

| # | Action | File path (relative to `src/`) | Reason |
|---|--------|-------------------------------|--------|
| 1 | CREATE | `Apha.FPSApps.Web/Areas/FPS/Models/ProgrammeManagerViewModel.cs` | ViewModel for read-only programme/project selection interface |
| 2 | CREATE | `Apha.FPSApps.Web/Areas/FPS/Controllers/ProgrammeManagerController.cs` | MVC controller — reuses existing `IProgramService` and `IProjectService` |
| 3 | CREATE | `Apha.FPSApps.Web/Areas/FPS/Views/ProgrammeManager/Index.cshtml` | Razor view with programme dropdown, client-side search, project list table |
