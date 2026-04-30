# Frontend — FPS ViewProjPlanActual_Tests

**Form**: `frmViewProjPlanActual_Tests` → **Route**: `/FPS/ProjectTestPlanActual`  
**Controller**: `ProjectTestPlanActualController`  
**View**: `Areas/FPS/Views/ProjectTestPlanActual/Index.cshtml`  
**ViewModel**: `ProjectTestPlanActualViewModel`

---

## Page layout

```
┌────────────────────────────────────────────────────────────────────────┐
│ Breadcrumb                          Project: [dropdown ▼]              │
├────────────────────────────────────────────────────────────────────────┤
│ Compare FPS Plan with PACT Actuals — Tests          [Back]             │
├────────────────────────────────────────────────────────────────────────┤
│ Project [____]  Description [________________________]                 │
│ Program [____]  Contract    [________________________]                 │
├───────────────────────────┬────────────────────────────────────────────┤
│  Planned Tests (PACT)     │  Actual Tests (FPS)                        │
│  [DataGrid fsubCompare1]  │  [DataGrid fsubCompareTests2]              │
│                           │                                            │
│  Total Planned Cost [£]   │  Totals: [Vol] [£Cost]                     │
│                           │  Percent of Plan: [%]                      │
└───────────────────────────┴────────────────────────────────────────────┘
```

---

## Grid 1 — Planned Tests (left)

| Property | Value |
|----------|-------|
| GridId | `testPlanGrid` |
| Item class | `TestPlanItem` (reused — existing) |
| AllowAdd | `true` |
| AllowEdit | `true` |
| AllowDelete | `true` |
| BindGridUrl | `/FPS/TestPlanJob/LoadTestPlanGrid?title=Planned+Tests+(PACT)` |
| ExtraFilterMethod | `getTestPlanExtraFilters()` → `{ jobCode: currentProjectCode }` |
| JS callbacks | `TestPlanJobConfig.onSaved/onUpdated/onDeleted` → reload grid + `refreshPlannedCost()` |

**Planned cost total** is loaded on page-load and project-change via `GET /FPS/ProjectTestPlanActual/GetTotalPlannedCost?projectCode=` which calls `ITestRequirementService.GetTotalTestPlanCostAsync` → PACT API `api/v1/testrequirement/totalcost/{parentProject}`.

---

## Grid 2 — Actual Tests (right)

| Property | Value |
|----------|-------|
| GridId | `compareTests2Grid` |
| Item class | `CompareTests2Item` (new) |
| AllowAdd | `false` |
| AllowEdit | `false` |
| AllowDelete | `true` |
| BindGridUrl | `/FPS/ProjectTestPlanActual/LoadCompareTests2Grid` |
| ExtraFilterMethod | `getCompareTests2ExtraFilters()` → `{ projectCode: currentProjectCode }` |
| DeleteFunction | `deleteCompareTests2(btn)` → `DELETE /FPS/ProjectTestPlanActual/DeleteMonthlyOutputCalcs` |
| KeyProperty | `RowKey` → `TestCode|Buyer|Month|WorkGroup` |

**Actual totals** refreshed after every grid mutation via `GET /FPS/ProjectTestPlanActual/GetTotalActualCost?projectCode=` which calls `IProjectTestPlanActualService.GetTotalActualByProjectAsync` → FPS API `api/v1/monthlyoutputcalcs/totals?projectCode=`.

---

## CompareTests2Item columns

| Property | GridColumn config |
|----------|-------------------|
| `Buyer` | `IsVisible = false` |
| `WorkGroup` | `IsVisible = false` |
| `TestCode` | `Width=120, ReadOnly, IsFilterable` |
| `Month` | `Width=60, ReadOnly` |
| `Volume` | `Width=80, ReadOnly` |
| `TestPrice` | `Width=100, GbpValue` |
| `Charge` | `Width=100, GbpValue` |
| `RowKey` | `IsVisible = false` — computed: `TestCode|Buyer|Month|WorkGroup` |

---

## ProjectTestPlanActualViewModel

| Property | Type | Source |
|----------|------|--------|
| `SelectedProjectCode` | `string` | route param / dropdown |
| `ProjectTitle` | `string` | `IProjectService.GetProjectByIdAsync` |
| `Program` | `string` | same |
| `Contract` | `string` | same |
| `TotalPlannedCost` | `decimal` | `ITestRequirementService.GetTotalTestPlanCostAsync` |
| `TotalActualVolume` | `double` | JS-refreshed from `GetTotalActualCost` |
| `TotalActualCost` | `double` | JS-refreshed from `GetTotalActualCost` |
| `PercentOfPlan` | `double` | JS-calculated: `(TotalActualCost / TotalPlannedCost) × 100` |
| `ProjectList` | `List<SelectListItem>` | `IProjectService.GetAllProjectsAsync` |
| `TestPlanGrid` | `DataGridConfig<TestPlanItem>` | configured in `Index()` |
| `CompareTests2Grid` | `DataGridConfig<CompareTests2Item>` | configured in `Index()` |

---

## Controller endpoints

| Method | Route | Action |
|--------|-------|--------|
| `GET` | `/FPS/ProjectTestPlanActual` | `Index(string? projectCode)` |
| `POST` | `/FPS/ProjectTestPlanActual/LoadCompareTests2Grid` | `LoadCompareTests2Grid(PaginationFilter, projectCode)` |
| `GET` | `/FPS/ProjectTestPlanActual/GetProjectInfo` | `GetProjectInfo(string projectCode)` |
| `GET` | `/FPS/ProjectTestPlanActual/GetTotalPlannedCost` | `GetTotalPlannedCost(string projectCode)` |
| `GET` | `/FPS/ProjectTestPlanActual/GetTotalActualCost` | `GetTotalActualCost(string projectCode)` |
| `DELETE` | `/FPS/ProjectTestPlanActual/DeleteMonthlyOutputCalcs` | `DeleteMonthlyOutputCalcs(string rowKey)` |

---

## Service-layer wiring

```
ProjectTestPlanActualController
  ├── IProjectTestPlanActualService  (FPSApps.Application)
  │     └── IFpsApiClient.FpsMonthlyOutputCalcs  (FpsMonthlyOutputCalcsApiClient)
  │           └── FPS API: api/v1/monthlyoutputcalcs
  └── ITestRequirementService  (FPSApps.Application — PACT side)
        └── IPactApiClient.PactTestRequirement  (PactTestRequirementApiClient)
              └── PACT API: api/v1/testrequirement/totalcost/{parentProject}
```

---

## AutoMapper registrations

| Mapper file | Mapping added |
|-------------|---------------|
| `FpsApiDtoMapper` | `MonthlyOutputCalcsViewDto ↔ MonthlyOutputCalcsViewRes` |
| `FpsApiDtoMapper` | `MonthlyOutputCalcsTotalsDto ↔ MonthlyOutputCalcsTotalsRes` |
| `FpsViewModelMapper` | `CompareTests2Item ↔ MonthlyOutputCalcsViewDto` |
