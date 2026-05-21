# SonarCloud Fix Report

**Generated:** 2026-05-21T12:14:12  
**Branch:** `feature/fps-Maintain-Division-Grade`  
**Project:** `DEFRA_apha-fps-apps`  
**Sonar Report Input:** `zPostRunValidationArtefacts/sonar-report_20260521_120317.json`

---

## Summary

| Metric                        | Value |
|-------------------------------|-------|
| Total issues downloaded       | 19    |
| After C# filter applied       | 1     |
| PL/SQL issues excluded        | 18    |
| Issues fixed                  | 1     |
| Issues skipped                | 0     |
| Build errors (before fix)     | 0     |
| Build errors (after fix)      | 0     |
| Build status                  | ✅ SUCCESS |

---

## Filtering Applied

| Filter                       | Rule                                                      |
|------------------------------|-----------------------------------------------------------|
| Language                     | C# only (`src/**/*.cs`)                                   |
| Status                       | OPEN / CONFIRMED                                          |
| Excluded                     | 18 PL/SQL issues (`dbscript/**/*.sql`) — not C# source    |

---

## Issues Fixed

| # | Key | Rule | Severity | File | Line | Message | Fix Applied |
|---|-----|------|----------|------|------|---------|-------------|
| 1 | `AZzHp-30l-vY2hRL2SWB` | `external_roslyn:CA1822` | INFO | `Apha.Common/Utilities/ExcelExport/ExcelExportService.cs` | 41 | Member 'ConvertExcelValue' does not access instance data and can be marked as static | Added `static` modifier to `private static object? ConvertExcelValue(...)` |

---

## Issues Skipped (Non-C# — Excluded by Language Filter)

| # | Rule | Language | File |
|---|------|----------|------|
| 1–17 | `plsql:CharVarchar`, `plsql:S1110`, `plsql:BooleanLiteralComparisonCheck` | PL/SQL | `dbscript/schemas/02mabarchive/**/*.sql` |
| 18 | `external_roslyn:CA1822` (duplicate key `AZ4rfUjF6hf2ZRDnzwMe`) | PL/SQL | `dbscript/**/*.sql` |

---

## Files Modified

| File | Rule Fixed | Change |
|------|------------|--------|
| `Apha.Common/Utilities/ExcelExport/ExcelExportService.cs` | `CA1822` | `private object? ConvertExcelValue` → `private static object? ConvertExcelValue` |

---

## Build Verification

```
dotnet build "Apha.FPS.All.sln"
Build succeeded.
0 Error(s)
0 Warning(s)
```

---

## Phase Execution Log

| Phase                                       | Start               | End                 | Status    |
|---------------------------------------------|---------------------|---------------------|-----------|
| Phase 1 — Download Issues from SonarCloud   | 2026-05-21T12:09:37 | 2026-05-21T12:11:03 | COMPLETED |
| Phase 2 — Fix C# Issues                     | 2026-05-21T12:11:03 | 2026-05-21T12:11:39 | COMPLETED |
| Phase 3 — Build Verification                | 2026-05-21T12:11:39 | 2026-05-21T12:14:12 | COMPLETED |
| Phase 4 — Publish Fix Report & Archive Lock | 2026-05-21T12:14:12 | 2026-05-21T12:14:20 | COMPLETED |
