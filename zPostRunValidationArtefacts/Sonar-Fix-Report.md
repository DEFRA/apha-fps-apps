# Sonar Fix Report

**Run date:** 2026-05-15  
**Branch:** feature/fps-EnterResource  
**Source file:** `zPostRunValidationArtefacts/sonar-report_20260515_190407.json`

---

## Summary

| Metric | Count |
|---|---|
| Total issues in output file | 2 |
| C# OPEN/CONFIRMED issues (after filter) | 2 |
| Issues fixed | 2 |
| Issues skipped (not C# / CLOSED) | 0 |
| Build errors introduced | 0 |

---

## Per-File Detail

| File | Rule | Line | Message | Fix Applied |
|---|---|---|---|---|
| `Apha.FPSApps.Web.UnitTests/.../ResourceSetUpControllerTests.cs` | `external_roslyn:CS8625` | 409 | Cannot convert null literal to non-nullable reference type | Changed `null` to `null!` (null-forgiving operator) |
| `Apha.FPSApps.Web/Extensions/AuthenticationExtension.cs` | `external_roslyn:CA1873` | 46 | Evaluation of this argument may be expensive and unnecessary if logging is disabled | Wrapped `LogInformation` call in `if (logger.IsEnabled(LogLevel.Information))` guard |

---

## Files Modified

| File | Changes |
|---|---|
| `src/Apha.FPSApps/Apha.FPSApps.Web.UnitTests/Controllers/FPS/ResourceSetUpControllerTest/ResourceSetUpControllerTests.cs` | Fixed CS8625 (line 409) |
| `src/Apha.FPSApps/Apha.FPSApps.Web/Extensions/AuthenticationExtension.cs` | Fixed CA1873 (line 46) |
