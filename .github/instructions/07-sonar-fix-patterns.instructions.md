# Instruction: SonarCloud Issue Fix Patterns

> **Applies to:** `.NetCore-Sonar-Fix.prompt.md` only.
> These rules govern how the agent filters, processes, and fixes SonarCloud issues in C# source files.

> **Required output artefact:** `zPostRunValidationArtefacts/Sonar-Fix-Report.md` **must be published at Section 6** after all fixes are applied and the build verified. Do not consider the task complete without this file.

---

## 0 — Environment Prerequisites

Before downloading issues, verify the SonarCloud token is present as a persistent user environment variable.

> ⚠️ `$env:SONAR_TOKEN = "..."` is session-scoped — the agent runs in a separate process and will not see it.
> The token must be set at the user level: `[System.Environment]::SetEnvironmentVariable('SONAR_TOKEN', '<token>', 'User')`

Check it is available in the current process:

```powershell
$tokenLength = (Get-ChildItem env:SONAR_TOKEN -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Value).Length
[Console]::WriteLine("TokenLength=$tokenLength")
```

If the token is not set, stop and report:

```
❌ SONAR_TOKEN environment variable is not set.
Set it in the terminal before running this prompt:
  $env:SONAR_TOKEN = "<your-token>"
```

---

## 1 — Download Issues from SonarCloud

### 1a — Resolve git branch and output file path

Before running the script, resolve dynamic values in the terminal:

```powershell
# 1. Workspace root — derive from the location of this script's folder
$workspaceRoot = Resolve-Path "$PSScriptRoot\..\.."   # .github/instructions → workspace root
# Fallback if not running from a script context:
# $workspaceRoot = Resolve-Path "."   # run from workspace root in the terminal

# 2. Current git branch — read from the workspace, never hardcode
$branch = git -C $workspaceRoot rev-parse --abbrev-ref HEAD
[Console]::WriteLine("Branch: $branch")

# 3. Timestamped output file path — placed directly in zPostRunValidationArtefacts
$ts         = Get-Date -Format 'yyyyMMdd_HHmmss'
$outputDir  = Join-Path $workspaceRoot "zPostRunValidationArtefacts"
$outputFile = Join-Path $outputDir "sonar-report_$ts.json"
[Console]::WriteLine("Output: $outputFile")
```

> If `git rev-parse` fails (detached HEAD or no git repo), stop and report:
> ```
> ❌ Could not determine the current git branch.
> Ensure the workspace is a git repository and HEAD is not detached.
> ```

### 1b — Run sonar_extract.ps1

Run the script using a path relative to the workspace root:

```powershell
$scriptPath = Join-Path $workspaceRoot ".github\instructions\sonar_extract.ps1"
& $scriptPath `
  -SonarToken      $env:SONAR_TOKEN `
  -ProjectKey      "DEFRA_apha-fps-apps" `
  -Organization    "defra" `
  -BranchName      $branch `
  -OutputFile      $outputFile `
  -IncludeHotspots $true
```

### 1c — Verify output

```powershell
$r = Get-Content $outputFile | ConvertFrom-Json
Write-Host "Total: $($r.Count)  Issues: $(($r | Where type -eq 'ISSUE').Count)  Hotspots: $(($r | Where type -eq 'HOTSPOT').Count)"
```

If the output file does not exist or contains 0 records after the script runs, stop and report the error.

> **Note for subsequent phases:** store `$outputFile` as a variable throughout the session — all later steps that read `sonar-report.json` must use `$outputFile`, not a hardcoded path.

---

## 2 — Issue Filtering

Load the output file (`$outputFile`) and apply the following filters **before** processing any issue.

### 2a — Include only

| Condition | Value |
|---|---|
| `file` ends with | `.cs` or `Dockerfile` |
| `status` is | `OPEN`, `CONFIRMED`, or `TO_REVIEW` |
| `type` is | `ISSUE` or `HOTSPOT` |

### 2b — Exclude

| Condition |
|---|
| `file` ends with `.sql`, `.yaml`, `.yml`, `.json`, `.md`, or `.xml` |
| `file` path contains `dbscript/` |
| `file` path contains `.github/` |
| `status` is `CLOSED` |

### 2c — Path normalisation

For each issue:
1. Strip any project prefix before `:` (e.g. `DEFRA_apha-fps-apps:src/...` → `src/...`)
2. Map the relative `src/` path to the absolute workspace root: `d:\FPS\apha-fps-apps_fps-wgstaff\src\`

### 2d — Grouping

- Sort filtered issues by `file`
- Process **one file at a time**
- Apply **all fixes for a file in a single edit pass** — do not make a separate edit call per issue

---

## 3 — Fix Patterns by Category

### Exception handling

| Sonar rule | Fix |
|---|---|
| **S112** Generic exception types | Replace `throw new Exception(...)` with a specific type: `InvalidOperationException`, `ArgumentException`, `ArgumentNullException`, etc. — choose based on context |
| **S2139** Exception not logged and re-thrown | Either log before re-throwing, or remove the catch and let it propagate — never catch and silently swallow |
| **S3776** Cognitive complexity | Extract nested blocks into private helper methods; reduce nesting depth |

### Null safety

| Sonar rule | Fix |
|---|---|
| **S8714** Nullable reference types | Add `?` to optional reference parameters and return types; use `?.` for conditional member access; use `??` for fallbacks |
| **S2259** Null dereference | Add null guard before use; use `ArgumentNullException.ThrowIfNull()` (.NET 6+) at method entry |
| **S1155** Use `Any()` not `Count()` | Replace `collection.Count() > 0` with `collection.Any()` and `collection.Count() == 0` with `!collection.Any()` |

### Async

| Sonar rule | Fix |
|---|---|
| **S4462** Blocking async | Replace `.Result` and `.Wait()` with `await`; mark the containing method `async Task<T>` |
| **S6966** `await` inside loop | Materialise async results before the loop using `Task.WhenAll` or a pre-fetched list; do not `await` inside `foreach` |
| **S4457** Sync validation before `await` | Move parameter null/range checks to before the first `await` statement |

### Code smells

| Sonar rule | Fix |
|---|---|
| **S1192** Magic strings | Extract repeated string literals to `private const string` |
| **S109** Magic numbers | Extract repeated numeric literals to `private const int` / `private const double` |
| **S1481** Unused local variable | Remove the variable; if the return value must be discarded, use `_` |
| **S1172** Unused parameter | Remove parameter if not part of an interface; if required by interface, use discard `_` in the parameter name |
| **S1128** Unused `using` | Remove the directive |
| **S2933** Non-readonly field | Add `readonly` to any `private` field assigned only in the constructor |
| **S107** Too many parameters | Introduce a parameter object (record/class) grouping related parameters; do not exceed 7 constructor or method parameters |

### Security

| Sonar rule | Fix |
|---|---|
| **S2068** Hardcoded credentials | Replace string literal with `IConfiguration` injection or environment variable access — never commit credentials |
| **S2076** SQL injection | Use `NpgsqlParameter` (PostgreSQL) or `SqlParameter` (SQL Server) for all user-supplied values in `FromSqlRaw` / `ExecuteSqlRawAsync` |
| **S5445** Insecure temporary file | Use `Path.GetTempFileName()` or `Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())` |

### Maintainability

| Sonar rule | Fix |
|---|---|
| **S3358** Nested ternary | Replace with `if`/`else` or a switch expression |
| **S2971** Use `AnyAsync` | Replace `CountAsync() > 0` with `AnyAsync()` in EF queries |
| **S4144** Intentional delegation | If two methods share identical bodies, extract the shared logic into a private helper and delegate |
| **S2325** Missing XML doc | Add `/// <summary>` on all `public` controller actions that lack one |
| **S1135** Track TODO | `// TODO` comments must reference a ticket or have an owner — never commit anonymous TODOs |

---

## 4 — Fix Rules

| Rule | Detail |
|---|---|
| **C# and Dockerfile only** | Skip any issue whose `file` does not end with `.cs` or `Dockerfile` |
| **OPEN/CONFIRMED only** | Skip `CLOSED` issues — already resolved in SonarCloud |
| **No behaviour change** | Do not alter business logic unless the fix directly requires it |
| **No removals** | Do not remove public API surface, methods, or classes unless the rule explicitly requires it |
| **Minimal edits** | Change only the lines required to fix the reported issue |
| **Existing style** | Match indentation, braces, and naming conventions of the surrounding code |
| **Clean Architecture** | Keep fixes within the correct layer — do not move logic across layers |
| **One pass per file** | Apply all fixes for a file in a single edit call; do not make repeated small edits to the same file |

---

## 5 — Build Verification

After all edits:

```powershell
# Run from: src/
dotnet build "Apha.FPS.All.sln" 2>&1 | Select-Object -Last 8
```

Expected: `Build succeeded. 0 Error(s)`.

If the build fails:
1. Read the full error line — file path, line number, error code (e.g. `CS0246`)
2. Fix the reported error
3. Re-run until 0 errors
4. Do **not** proceed to the output report while any compile error remains

---

## 6 — Output Report Format

Publish `zPostRunValidationArtefacts/Sonar-Fix-Report.md` containing:

**Summary table:**

| Metric | Count |
|---|---|
| Total issues in output file (`$outputFile`) | N |
| C# OPEN/CONFIRMED issues (after filter) | N |
| Issues fixed | N |
| Issues skipped (not C# / CLOSED) | N |
| Build errors introduced | 0 |

**Per-file detail table:**

| File | Rule | Line | Message | Fix Applied |
|---|---|---|---|---|
| `XxxService.cs` | `S112` | 42 | Generic exception thrown | Replaced with `InvalidOperationException` |

**Files Modified table:**

| File | Changes |
|---|---|
| `src/.../XxxService.cs` | Fixed S112 (line 42), S2933 (line 8) |

If no issues were found after filtering, publish the report with a single data row:
`— | — | — | No C# OPEN/CONFIRMED issues found after filtering | —`
