# Instruction: File Placement Checklist

> **Lock file — phase start:** Before running the checklist, run `Get-Date -Format 'yyyy-MM-ddTHH:mm:ss'` and update `zPostRunValidationArtefacts/.codingagent-lock`:
> - Set `current-phase: Completion Check`
> - Add a row: `| Completion Check | <timestamp> | IN-PROGRESS | |`

> **Completion gate pre-check — run ALL three `Test-Path` commands immediately after updating the lock file:**
> ```powershell
> Test-Path "zPostRunValidationArtefacts/[App]-[FormName]-Backend.md"
> Test-Path "zPostRunValidationArtefacts/[App]-[FormName]-Frontend.md"
> Test-Path "zPostRunValidationArtefacts/[App]-[FormName]-Build.md"
> ```
> Expected results at this point:
> - `Backend.md` → `True` (must exist from Phase 1). If `False` — **stop**, create it now before continuing.
> - `Frontend.md` → `True` (must exist from Phase 2). If `False` — **stop**, create it now before continuing.
> - `Build.md` → `False` (not yet created — will be created after the build below). If already `True`, verify it is current.
>
> **Do not tick any checklist boxes or archive the lock file until all three gate files are in the correct state.**

> **Required output artefact — Completion gate:** `zPostRunValidationArtefacts/[App]-[FormName]-Build.md` **must be published after the build verification step** at the bottom of this file. Do not archive the lock file until this file exists.

After all phases are complete, confirm every file below has been created or updated.
Then run the **Build Verification** step at the bottom.

## Apha.Common
- [ ] `Apha.Common/Contracts/[App]/[Entity]Req.cs`
- [ ] `Apha.Common/Contracts/[App]/[Entity]Res.cs`
- [ ] `Apha.Common/Contracts/[App]/[EntityView]Res.cs`

## Apha.[App].Core
- [ ] `Apha.[App].Core/Entities/[Entity].cs`
- [ ] `Apha.[App].Core/Entities/[EntityView].cs`
- [ ] `Apha.[App].Core/Interfaces/I[Entity]Repository.cs`

## Apha.[App].Application
- [ ] `Apha.[App].Application/Dtos/[Entity]Dto.cs`
- [ ] `Apha.[App].Application/Dtos/[EntityView]Dto.cs`
- [ ] `Apha.[App].Application/Interfaces/I[FormName]Service.cs`
- [ ] `Apha.[App].Application/Services/[FormName]Service.cs`
- [ ] `Apha.[App].Application/Mappings/EntityMapper.cs` *(updated)*

## Apha.[App].DataAccess
- [ ] `Apha.[App].DataAccess/Repositories/[Entity]Repository.cs`
- [ ] `Apha.[App].DataAccess/Data/[Entity]Map.cs` *(new file — `IEntityTypeConfiguration<[Entity]>`)*
- [ ] `Apha.[App].DataAccess/Data/[App]DbContext.cs` *(updated — `DbSet<[Entity]>` + `ApplyConfiguration(new [Entity]Map(...))`)*

## Apha.[App].Api
- [ ] `Apha.[App].Api/Controllers/[FormName]Controller.cs`
- [ ] `Apha.[App].Api/Mappings/RequestMapper.cs` *(updated)*
- [ ] `Apha.[App].Api/Extensions/ServiceCollectionExtension.cs` *(updated)*

## Apha.FPSApps.Application
- [ ] `Apha.FPSApps.Application/Dtos/[App]/[Entity]Dto.cs`
- [ ] `Apha.FPSApps.Application/Dtos/[App]/[EntityView]Dto.cs`
- [ ] `Apha.FPSApps.Application/Interfaces/[App]ApiClients/I[App][FormName]ApiClient.cs`
- [ ] `Apha.FPSApps.Application/Interfaces/[App]ApiClients/I[App]ApiClient.cs` *(updated)*
- [ ] `Apha.FPSApps.Application/Interfaces/I[FormName]Service.cs`
- [ ] `Apha.FPSApps.Application/Services/[FormName]Service.cs`

## Apha.FPSApps.Infrastructure
- [ ] `Apha.FPSApps.Infrastructure/Integrations/[App]Apis/Clients/[App][FormName]ApiClient.cs`
- [ ] `Apha.FPSApps.Infrastructure/Integrations/[App]Apis/Clients/[App]ApiClient.cs` *(updated)*
- [ ] `Apha.FPSApps.Infrastructure/Mappings/ApiDtoMapper.cs` *(updated)*

## Apha.FPSApps.Web
- [ ] `Apha.FPSApps.Web/Areas/[App]/Models/[FormName]ViewModel.cs`
- [ ] `Apha.FPSApps.Web/Areas/[App]/Models/[FormName]Item.cs` *(DataGrid forms only — see 03b)*
- [ ] `Apha.FPSApps.Web/Areas/[App]/Controllers/[FormName]Controller.cs`
- [ ] `Apha.FPSApps.Web/Areas/[App]/Views/[FormName]/Index.cshtml`
- [ ] `Apha.FPSApps.Web/Areas/[App]/Views/[FormName]/_AddEdit[FormName].cshtml` *(DataGrid forms only, when AllowAdd or AllowEdit: true — see 03b)*
- [ ] `Apha.FPSApps.Web/Areas/[App]/Views/[FormName]/_Delete[FormName].cshtml` *(DataGrid forms only, when AllowDelete: true — see 03b)*
- [ ] `Apha.FPSApps.Web/Mappings/ViewModelMapper.cs` *(updated)*
- [ ] `Apha.FPSApps.Web/Extensions/ServiceCollectionExtension.cs` *(updated)*

---

## Subform-derived artefacts

For each `Begin Subform` in the main `.frm` whose subform `.frm` has its own `RecordSource`, confirm the following additional files have been created or updated. The subform entity shares the same API and MVC **controllers** as the main form — only the lower layers are new.

### Apha.Common
- [ ] `Apha.Common/Contracts/[App]/[SubEntity]Res.cs` *(if the subform exposes a distinct response shape)*

### Apha.[App].Core
- [ ] `Apha.[App].Core/Entities/[SubEntity].cs` *(or `[SubEntityView].cs` if the subform `RecordSource` is a query/view)*
- [ ] `Apha.[App].Core/Interfaces/I[SubEntity]Repository.cs`

### Apha.[App].Application
- [ ] `Apha.[App].Application/Dtos/[SubEntity]Dto.cs`
- [ ] `Apha.[App].Application/Interfaces/I[FormName]Service.cs` *(updated — subform query method added)*
- [ ] `Apha.[App].Application/Services/[FormName]Service.cs` *(updated — subform query method added)*
- [ ] `Apha.[App].Application/Mappings/EntityMapper.cs` *(updated)*

### Apha.[App].DataAccess
- [ ] `Apha.[App].DataAccess/Repositories/[SubEntity]Repository.cs`
- [ ] `Apha.[App].DataAccess/Data/[SubEntity]Map.cs` *(new file — `IEntityTypeConfiguration<[SubEntity]>`)*
- [ ] `Apha.[App].DataAccess/Data/[App]DbContext.cs` *(updated — `DbSet<[SubEntity]>` + `ApplyConfiguration(new [SubEntity]Map(...))`)*

### Apha.[App].Api
- [ ] `Apha.[App].Api/Controllers/[FormName]Controller.cs` *(updated — subform endpoint added)*

### Apha.FPSApps.Application
- [ ] `Apha.FPSApps.Application/Dtos/[App]/[SubEntity]Dto.cs`
- [ ] `Apha.FPSApps.Application/Interfaces/[App]ApiClients/I[App][FormName]ApiClient.cs` *(updated — subform method added)*
- [ ] `Apha.FPSApps.Application/Interfaces/I[FormName]Service.cs` *(updated — subform method added)*
- [ ] `Apha.FPSApps.Application/Services/[FormName]Service.cs` *(updated — subform method added)*

### Apha.FPSApps.Infrastructure
- [ ] `Apha.FPSApps.Infrastructure/Integrations/[App]Apis/Clients/[App][FormName]ApiClient.cs` *(updated — subform method added)*
- [ ] `Apha.FPSApps.Infrastructure/Mappings/ApiDtoMapper.cs` *(updated)*

### Apha.FPSApps.Web
- [ ] `Apha.FPSApps.Web/Areas/[App]/Models/[SubFormName]Item.cs` *(grid item model for the subform table)*
- [ ] `Apha.FPSApps.Web/Areas/[App]/Models/[FormName]ViewModel.cs` *(updated — `DataGridConfig<[SubFormName]Item>` property added)*
- [ ] `Apha.FPSApps.Web/Areas/[App]/Controllers/[FormName]Controller.cs` *(updated — `Load[SubFormName]Grid` action added)*
- [ ] `Apha.FPSApps.Web/Mappings/ViewModelMapper.cs` *(updated)*

---

## Build Verification — MANDATORY before marking the task done

**Before running the first build**, create the `Build.md` skeleton:

```markdown
# Build Issues — [App]-[FormName]

**Final status**: *(fill in after final build)*
**Errors**: *(fill in)* | **Warnings**: *(fill in)*

| # | Category | Severity | File | Error message | Root cause | Fix applied |
|---|----------|----------|------|---------------|------------|-------------|
```

Then run the build:

```powershell
# Terminal cwd must be: src/
dotnet build "Apha.FPS.All.sln" 2>&1 | Select-Object -Last 8
```

**Expected:** `Build succeeded.` with `0 Error(s)`.

**For each build error encountered:**
1. Read the error line — file path, line number, Roslyn error code (e.g. `CS0246`, `CS0103`, `CS1061`)
2. Fix the error in the reported file
3. **→ Build.md:** Append a row for this error immediately — before re-running the build
4. Re-run the build command
5. Repeat until `0 Error(s)` — do not mark the task complete while any error remains

If the build succeeds on the **first attempt** with no errors, append a single sentinel row: `— | — | — | — | No build issues | — | —`

Common errors and fixes:

| Error | Typical cause | Fix |
|---|---|---|
| `CS0246` type/namespace not found | Missing `using` directive | Add the correct `using` — e.g. `using Apha.Common.Contracts;` for `PaginationReq<T>` **or** `PaginationRes<T>` (both live in `Apha.Common.Contracts`, not `Apha.Common.Contracts.[App]`) |
| `CS0103` name does not exist | `ApplySorting` called but not defined | Add the three private `ApplySorting`/`ApplySortingByProperty`/`ApplyOrder` methods to the repository (see 02-backend Step 7) |
| `CS1061` does not contain a definition | Wrong property name on a DTO or entity | Cross-check property names between the DTO, entity, and mapper — they must match exactly |
| `CS0117` does not contain a definition for | Incorrect `DbSet` or entity name in DbContext | Verify the `DbSet<T>` property name and the entity class name match |

---

### Finalise Build Verification Output

Once the build reaches `0 Error(s)`, update the header of `[App]-[FormName]-Build.md` with the final status and counts:

```markdown
**Final status**: BUILD SUCCESS
**Errors**: 0 | **Warnings**: <count>
```

Rows are already present from the progressive step above — this is a header-fill-in only, not a new publish.

> **Row rules (applied at each fix step):**
> - **One row per build error encountered**, including errors fixed in earlier iterations — do not omit resolved errors
> - **Category**: `COMPILATION` | `MISSING_DEPENDENCY` | `TEST_FAILURE` | `CONFIGURATION`
> - **Severity**: `CRITICAL` (blocks build) | `MAJOR` (test failure / runtime risk) | `MINOR` (warning)
> - **File**: filename and line number (e.g. `XxxController.cs:14`)
> - **Fix applied**: the actual code or config change — not a description of the problem

---

> ⚠️ **DO NOT archive the lock file until `zPostRunValidationArtefacts/[App]-[FormName]-Build.md` has been published.** Publishing this file is a mandatory step — it is not complete to rename the lock without it.

> **Build.md gate check — run this command before touching the lock file:**
> ```powershell
> Test-Path "zPostRunValidationArtefacts/[App]-[FormName]-Build.md"
> ```
> - If `False` → the Build.md has NOT been published yet. Go back and publish it now. **Do not proceed to the lock-file steps below until this returns `True`.**
> - If `True` → confirmed published. Proceed to the lock-file steps below.

> **Lock file — phase end / session complete:** These three steps MUST be executed in the same agent turn — do not stop after any one of them:
> 1. **Confirm** `Test-Path "zPostRunValidationArtefacts/[App]-[FormName]-Build.md"` returns `True` (see gate check above).
> 2. Run `Get-Date -Format 'yyyy-MM-ddTHH:mm:ss'` and update `zPostRunValidationArtefacts/.codingagent-lock`: set `Status = COMPLETED` and fill in the `End` timestamp on the `Completion Check` row.
> 3. **Immediately after** (same agent turn), archive the lock by running the rename in the terminal — this is MANDATORY and is NOT complete until the `Rename-Item` command has been executed:
>   ```powershell
>   Rename-Item `
>     -Path  "zPostRunValidationArtefacts/.codingagent-lock" `
>     -NewName "codingagentrun_$(Get-Date -Format 'yyyyMMdd_HHmmss').log"
>   ```
>   Verify with `Get-ChildItem "zPostRunValidationArtefacts"` that no file named `.codingagent-lock` remains. The workspace is now free for the next run.
