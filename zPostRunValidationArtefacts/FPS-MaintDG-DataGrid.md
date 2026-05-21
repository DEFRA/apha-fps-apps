# DataGrid Analysis — FPS MaintDG

## Grid Operations Profile

| Property | Value | Source |
|---|---|---|
| `AllowAdd` | `true` | `.frm` — no `AllowAdditions = NotDefault` |
| `AllowEdit` | `true` | `.frm` — no `AllowEdits = NotDefault` |
| `AllowDelete` | `true` | `.frm` — no `AllowDeletions = NotDefault` |

## Editable Fields

All controls have default `Enabled` / `Locked` state (no `NotDefault` overrides in the `.frm`).

| Field | Column label | Edit in Add | Edit in Edit |
|---|---|---|---|
| `DivisionGradeCode` | Div. Grade | Text input (editable, becomes PK) | Hidden |
| `GradeCode` | GradeCode | `<select>` dropdown | `<select>` dropdown |
| `Division` | Division | `<select>` dropdown | `<select>` dropdown |
| `ChargeRate` | ChargeRate | Currency input | Currency input |
| `DirectRate` | DirectRate | Currency input | Currency input |
| `PayRate` | PayRate | Currency input | Currency input |
| `Npr` | NPR | Currency input | Currency input |
| `Ohr` | OHR | Currency input | Currency input |

## Read-only / Hidden Fields

| Field | Reason |
|---|---|
| `FpsYear` | Global query filter — set by DbContext; never shown in UI |

## Dropdown Sources

| Field | RowSource SQL | API endpoint |
|---|---|---|
| `GradeCode` | `SELECT [GradeCode] FROM [Grade]` (fps.grade) | `GET /api/v1/maintdg/grades` |
| `Division` | `SELECT [DivName] FROM [tlkpDivision]` (fps.tlkpdivision) | `GET /api/v1/division` (existing) |

## DataGridConfig Settings

```
GridId             = "divisionGradeGrid"
Title              = "Division Grade Maintenance"
ShowCheckboxColumn = false
ShowPagination     = true
KeyProperty        = "DivisionGradeCode"
AllowAdd           = true
AddFunction        = "addMaintDG"
AllowEdit          = true
EditFunction       = "editMaintDG"
AllowDelete        = true
DeleteFunction     = "deleteMaintDG"
BindGridUrl        = "/FPS/MaintDG/LoadMaintDGGrid"
```
