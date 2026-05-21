# Build Issues — FPS-frmProgrammeManager

**Final status**: BUILD SUCCESS
**Errors**: 0 | **Warnings**: 42

| # | Category | Severity | File | Error message | Root cause | Fix applied |
|---|----------|----------|------|---------------|------------|-------------|
| 1 | COMPILATION | CRITICAL | ProgrammeManagerController.cs:18 | CS0246: The type or namespace name 'IProgramService' could not be found | Missing using directive | Added `using Apha.FPSApps.Application.Interfaces.FPS;` |
