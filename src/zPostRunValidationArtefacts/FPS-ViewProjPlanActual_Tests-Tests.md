# XUnit Test Report — FPS ViewProjPlanActual_Tests

**Date**: 2026-04-30  
**Build**: ✅ 0 C# errors (MSB file-lock warnings only — debug session running)  
**Solution**: `Apha.FPS.All.sln`

---

## Coverage Analysis

| Layer | Source Methods | Previously Covered | Added / Fixed | Final Coverage |
|-------|---------------|-------------------|---------------|----------------|
| Repository (`MonthlyOutputCalcsRepository`) | 3 | 0 | 13 new tests | ✅ All |
| API Service (`MonthlyOutputCalcsService`) | 3 | 3 (8 tests) | — | ✅ All |
| API Controller (`MonthlyOutputCalcsController`) | 3 | 3 (15 tests) | — | ✅ All |
| Infrastructure Client (`FpsMonthlyOutputCalcsApiClient`) | 3 | 3 (8 tests) | — | ✅ All |
| Web App Service (`ProjectTestPlanActualService`) | 4 | 3 (2 stale) | Fixed 2 stale + 4 new (`GetTotalPlannedCostAsync`) | ✅ All |
| Web MVC Controller (`ProjectTestPlanActualController`) | 7 | 5 | 6 new (`LoadTestPlanGrid` x3, `LoadCompareTests2Grid` x3) | ✅ All |

---

## Files Created / Modified

| File | Action | Tests Added |
|------|--------|-------------|
| `Apha.FPS.DataAccess.UnitTests/.../MonthlyOutputCalcsRepositoryTests.cs` | **CREATED** | 13 |
| `Apha.FPSApps.Application.UnitTests/.../ProjectTestPlanActualServiceTests.cs` | **REWRITTEN** | 16 total (4 new `GetTotalPlannedCostAsync`, 3 fixed `GetTotalActualByProjectAsync`, 1 new enrichment test) |
| `Apha.FPSApps.Web.UnitTests/.../ProjectTestPlanActualControllerTests.cs` | **UPDATED** | +6 (`LoadTestPlanGrid` x3, `LoadCompareTests2Grid` x3) |

---

## Key Changes from Original

| Change | Reason |
|--------|--------|
| `GetTotalActualByProjectAsync` tests rewritten | Service now enriches via PACT prices (FPS raw data + PACT `GetPagedTestReqmtbyProjectAsync`). Stale tests stubbed `_apiClient.GetTotalActualByProjectAsync` which is no longer called. |
| `GetTotalPlannedCostAsync` added (4 tests) | New method added in this session — was not covered. |
| `LoadTestPlanGrid` / `LoadCompareTests2Grid` added (6 tests) | New controller actions added in this session — were not covered. |
| Repository tests created (new file) | No test file existed for `MonthlyOutputCalcsRepository`. |
