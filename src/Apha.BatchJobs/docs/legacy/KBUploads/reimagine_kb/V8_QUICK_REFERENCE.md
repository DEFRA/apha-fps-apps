# v8 Refined Prompt Improvements Checklist

## 7 Key Changes from v4 → v8

| # | Issue in v4 | v8 Refinement | Impact |
|---|---|---|---|
| 1 | Serilog.Enrichers.Environment 3.1.0 not in NuGet | **Explicit prose:** "version 3.0.1" everywhere this package mentioned | Eliminates NU1102 restore error |
| 2 | Missing `using Quartz.Extensions.DependencyInjection;` in InfrastructureDependencyInjection.cs | **Explicit description:** "calls AddQuartz passing a configuration action" + list Quartz.Extensions.DependencyInjection in packages + explicitly name extension methods | AppMod generates correct imports |
| 3 | Missing `using Microsoft.Extensions.Configuration.Binder;` in InfrastructureDependencyInjection.cs | **Explicit description:** "configuration binder" + list Configuration.Binder in packages + explicitly mention GetValue extension in ServiceCollectionExtensions narrative | AppMod generates correct imports |
| 4 | Project references inferred, sometimes wrong | **Explicit statement per project:** "AphaBatchJobs.Host.csproj... references AphaBatchJobs.Application and AphaBatchJobs.Infrastructure" | AppMod generates correct ProjectReference XML |
| 5 | Packages ambiguously distributed across projects | **Packages listed by project:** Each project section has its own "NuGet packages for [ProjectName]:" table | No cross-project package confusion |
| 6 | Prose contamination after file terminators | **Leave Additional Info blank** + highly specific user story + explicit file descriptions | Reduces AppMod's inference need, less review commentary appended |
| 7 | DI registration order matters, not specified | **Explicit sequence:** "binds DatabaseOptions... binds JobOptions... adds DbContext... registers CorrelationIdService... calls AddQuartz..." | AppMod generates ServiceCollectionExtensions in correct initialization order |

---

## Form Submission Quick Reference

- **User Story Field:** Paste entire user-story-v8-refined.txt (1800+ words)
- **Package Name:** `AphaBatchJobsFoundationV8`
- **Tech Stack:** `Dotnet8 PostgreSQL AWS` (not .NET 10)
- **Additional Info:** **[LEAVE BLANK]**
- **Upload File:** `user-story-v8.zip` (contains only user-story-v8-refined.txt)

---

## Expected Outcomes

✅ **File Count:** 22-24 (same as v4)  
✅ **Prose Leakage:** Near-zero (cleanable)  
✅ **Build Status:** Should pass `dotnet build AphaBatchJobs.sln` without manual fixes  
✅ **Package Resolution:** All versions should exist in NuGet, zero NU1102 errors  
✅ **Efficacy Target:** 92-95% (baseline was v4's 90%)  

---

## If Build Fails After Generation

1. **Check .csproj versions** — especially Serilog.Enrichers.Environment (must be 3.0.1)
2. **Check using statements** — especially Quartz.Extensions.DependencyInjection and Configuration.Binder
3. **Check for prose** — delete any `**` or markdown after method/file closing
4. **If still fails** — one-line targeted fix usually resolves (similar to v4 cleanup)

---

## File References

- **Refined User Story:** `src/Apha.BatchJobs/KBUploads/reimagine_kb/user-story-v8-refined.txt`
- **Form Guide:** `src/Apha.BatchJobs/KBUploads/reimagine_kb/V8_REFINED_FORM_SUBMISSION.md` (this file)
- **Learnings Reference:** `src/Apha.BatchJobs/APPMOD-REIMAGINE-LEARNINGS.md` (full context on why v8 works)

---

## Next Steps After Generation

1. Run `dotnet build AphaBatchJobs.sln` on output
2. If build passes → **commit as `v0.1.0-foundation`** + proceed to Re-Architect Wave 1
3. If build fails → apply same targeted fixes as v4 (versions, usings, prose cleanup), then commit

