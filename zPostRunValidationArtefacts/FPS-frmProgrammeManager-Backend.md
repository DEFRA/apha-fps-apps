# Backend Analysis — FPS frmProgrammeManager

## Reference Map

| Referenced name | Type | Triggering event / context | Parameters / notes |
|---|---|---|---|
| `tlkpProgram` | PostgreSQL table | `RowSource` on programme dropdown | Direct table reference — read-only |
| `tlkpProject` | PostgreSQL table | `RowSource` on project list | Filtered by `Program` field — read-only |
| *(No MS Access queries found)* | — | — | — |
| *(No stored procedures found)* | — | — | — |
| *(No functions found)* | — | — | — |
| *(No triggers - form is read-only)* | — | — | — |
| *(No BAS functions found)* | — | — | — |

## Artefact Detail

### Tables

**`tlkpProgram`** (`source/pgsql/fps/Tables/tlkpprogram.sql`)
- **Purpose:** Programme master table
- **Primary Key:** `(programno, fpsyear)`
- **Columns used:** `programno`, `programname`
- **Usage in form:** Populates the programme dropdown
- **LINQ implementation:** Read-only query via existing `IProgramService.GetAllProgramsAsync()`
- **Optimization notes:** Standard pipeline — `AsNoTracking().Where(year).Select()` materialised once

**`tlkpProject`** (`source/pgsql/fps/Tables/tlkpproject.sql`)
- **Purpose:** Project master table
- **Primary Key:** `(parentproject, fpsyear)`
- **Columns used:** `parentproject`, `program`
- **Usage in form:** Displays projects filtered by selected programme
- **LINQ implementation:** Read-only query via existing `IProjectService.GetProjectsByProgramAsync()`  
- **Optimization notes:** Standard pipeline — `AsNoTracking().Where(program, year).Select()` materialised once

### Form VBA Analysis

**Form properties:**
- `DefaultView = 0` (Single Form — NOT a DataGrid)
- `RecordSelectors = NotDefault`, `NavigationButtons = NotDefault`, `ScrollBars = 0`
- `ControlBox = NotDefault`

**Dropdowns:**
- `[cboProgram]` (assumed name from HTML): `RowSource = "SELECT tlkpProgram.ProgramNo FROM tlkpProgram;"`
- Triggers requery of project list on `AfterUpdate`

**Project list:**
- Inline SQL: `SELECT DISTINCTROW tlkpProject.ParentProject, tlkpProject.Program FROM tlkpProject WHERE tlkpProject.Program=[cboProgram];`
- Displayed in a listbox or subform (HTML shows table rows)

**Button click events:**
- `btnOpenProjectForm_Click()` — navigates to project planning form (`DoCmd.OpenForm` with filter)
- `btnOpenEditForm_Click()` — navigates to project edit form (`DoCmd.OpenForm` with filter)
- All other `Button*_Click()` subs appear to be navigation to other planning forms (Staff, Animal, Test, Additional Cost plans)

**No DML operations:** Form is entirely read-only — no `INSERT`, `UPDATE`, `DELETE` statements

## C# Artefact Mapping

| MS Access artefact | C# implementation | Layer | File |
|---|---|---|---|
| `tlkpProgram` query | Reuse `IProgramService.GetAllProgramsAsync()` | Application | `Apha.FPS.Application` |
| `tlkpProject` filtered query | Reuse `IProjectService.GetProjectsByProgramAsync()` | Application | `Apha.FPS.Application` |
| Button navigation | `asp-action` / `asp-route-*` | View | `Index.cshtml` |

**No new backend entities or services required.** Both `Program` and `Project` entities, repositories, and services already exist. Only frontend layer needs to be created.

## Raw SQL Decisions

*(None required — all queries are simple filters on indexed PK/FK columns, fully expressible in LINQ)*

## File Changes — Phase 1 Backend

| # | Action | File path (relative to `src/`) | Reason |
|---|--------|-------------------------------|--------|
| *(No backend changes)* | — | — | All required backend services already exist |

**Note:** This form reuses existing `IProgramService` and `IProjectService` from `Apha.FPS.Application`. No new repositories, entities, DTOs, or API endpoints are needed for the backend. Only frontend layers (Step 10–18) will be generated.
