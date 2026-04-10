# AppMod Re-Imagine Learnings

Captures insights from iterative Re-Imagine runs to produce the AphaBatchJobs foundation. Also documents the full AppMod pipeline: what Re-Imagine and Re-Architect do, how the Knowledge Bases feed them, and how the two modes work together as a controlled modernisation strategy.

---

## Run History

| Run | Zip Size | Files | Build | .NET Target | Prose Leak | Efficacy |
|-----|----------|-------|-------|-------------|------------|----------|
| v1 | 39 KB | 17 | No | net8.0 | No | 55% |
| v2 | 1.3 KB | 3 | No | n/a | No | 15% — intermittent partial download |
| v3 | ~30 KB | 20 | No | net8.0 | No | 62% |
| v4 | 31 KB | 22 | Yes | net8.0 | No | 90% — best output, narrative prompt |
| v5 | 47 KB | 29 | No | net8.0 | Yes | 72% — wrong TFM, csproj XML broken |
| v6 | 32 KB | 26 | No | net10.0 | Yes | 72% — correct TFM, prose in all files |

---

## What Worked

### Narrative-style user story (v4 — 90%)
Describing each class, its constructor parameters, method signatures, and config file keys as prose produced the cleanest output. AppMod treats narrative as a spec to implement rather than instructions to explain.

Key characteristics of the winning prompt:
- Written as a developer story starting with "As a platform developer I want..."
- Each project section describes exact file names and what they contain
- Method signatures described in plain English without code syntax
- NuGet package names and exact versions stated in prose
- Acceptance criteria listed at the end as plain sentences
- No bullet lists of constraints
- No imperative commands like "MUST" or "ONLY GENERATE"

### Separate user-story.zip for upload
The upload zip must contain only user-story.txt at the root. Including KB context files in the upload zip caused AppMod to treat them as source rather than guidance.

### Leaving Additional Info blank
Populating the Additional Info field caused output regression in multiple runs. AppMod appeared to merge Additional Info with the story in ways that triggered explanatory prose generation. Leaving it blank let the uploaded file drive the generation cleanly.

---

## What Did Not Work

### Command-style prompt (v6 — 72% but prose leakage)
Using imperative instructions like "ONLY GENERATE CODE. NO COMMENTARY." caused AppMod to append review text and "Key Changes Made" markdown blocks after every file terminator. The instruction itself triggered the behaviour it was trying to prevent.

### Bullet list constraints
Bullet lists of constraints without narrative context produced either minimal output (1-3 files) or contaminated output. AppMod needs descriptive prose, not a checklist.

### Specifying .NET 10 in the User Story field
The User Story input field has a character limit and does not accept version numbers well in some runs. Putting .NET 10 only in the uploaded file is more reliable than the form field.

### Wildcard package versions
Specifying `Version="8.0.*"` across all projects produced wildcard references in the output. Stating exact versions in the narrative prose (e.g. "version 9.0.0") resulted in exact versions being used.

### Intermittent partial downloads
Some AppMod runs produced a very small zip (1-3 KB) which turned out to be a mid-generation download. The full output zip was always larger and available separately. Always check zip size before analysing output. Anything under 5 KB is likely a partial capture.

---

## AppMod Behaviour Patterns Observed

| Behaviour | Trigger | Mitigation |
|-----------|---------|------------|
| Appends review prose after file terminators | Imperative command-style prompt | Use narrative prose instead |
| Downgrades to net8.0 with "not yet released" comment | Stating version number in form User Story field | Put TFM in the uploaded file only |
| Generates 1-3 files only | Very short or list-only prompt | Narrative with method-level descriptions drives more files |
| Wildcard package versions | Not specifying versions explicitly | State "version X.Y.Z" in the narrative |
| Application layer missing | Prompt only describes Host and Infrastructure | Explicitly describe Application project with its interfaces and services |
| No Program.cs or sln generated | Prompt lists architecture files but not runtime entrypoint | Describe Program.cs content explicitly including CLI arg handling and host builder pattern |

---

## Recommended Prompt Structure for Re-Imagine

1. Opening As a user sentence (short, no special characters, under 50 words)
2. Background paragraph explaining domain context
3. Solution Structure section naming the solution file and all projects
4. Per-project sections each covering:
   - csproj target framework and output type
   - Project references
   - NuGet packages with exact versions
   - Each class or interface with its members described in prose
5. Acceptance criteria as plain sentences

Keep the prompt in the uploaded user-story.txt file. Use the form User Story field for a short plain-text summary only.

---

## Form Field Guidance

| Field | Guidance |
|-------|----------|
| User Story | Short plain text, under 50 words, no special characters, no version numbers |
| Package Name | Use versioned name e.g. AphaBatchJobsFoundationV7 to distinguish runs |
| Tech Stack | Plain words e.g. Dotnet10 PostgreSQL AWS |
| Additional Info | Leave blank — populating this caused output regression in multiple runs |
| Upload File | user-story.zip containing only user-story.txt at the root |

---

## Current State

- **v4 (90%):** 22 files, net8.0, builds (after prose cleanup + Serilog fix), clean generated code structure. **CHOSEN AS FOUNDATION** but will upgrade to .NET 10 post-ReArchitect
- v6 (72%): 26 files, net10.0, breaks on Core.csproj line 46 due to prose injection, but has correct package versions for .NET 10 transition
- v7 (68%): regression, mixed frameworks, broken build

**Plan:**
1. Accept v4 net8.0 foundation as golden baseline once build issues resolved
2. Run Re-Architect on v4 foundation to generate procedure implementations  
3. After Re-Architect complete and tested: upgrade entire solution from net8.0 to net10.0 (csproj TargetFramework updates only, no code changes needed)
4. Tag v0.2.0-foundation-net10 after successful upgrade and validation
5. Continue to v1.0.0-batchjobs-ga

Active baseline: `ReImagineAnalysis_v4/AphaBatchJobsFoundationV3.*` (pending fix)
Re-Architect foundation: same as above after build validation
Scheduled upgrade: net8.0 → net10.0 (post-ReArchitect completion)

---

## Understanding AppMod: Re-Imagine and Re-Architect

AppMod is a generative AI tool that modernises legacy code. It operates in two distinct modes: Re-Imagine and Re-Architect. They solve different problems and must be run in sequence, not simultaneously.

### Re-Imagine

Re-Imagine generates a greenfield modernised foundation from a user story. It does not look at legacy code. It reads the story, the Application Code KB, and the Code Best Practices KB, and produces a compile-ready project skeleton that follows the target architecture patterns.

**What it is for:**
Establishing the target solution shape. Re-Imagine is used once per workstream to produce the host, project files, DI wiring, interfaces, base services, configuration, Dockerfile, and runtime entrypoint. It is the starting block that all subsequent Re-Architect output plugs into.

**What it does not do:**
It does not convert stored procedures. It has no knowledge of legacy SQL logic. It cannot produce business logic implementations without being told what they are.

**When to use it:**
Before any Re-Architect runs. The Re-Imagine output must be accepted and committed as the foundation baseline before Re-Architect begins.

**Input it uses:**
- User Story field in the form (short plain text)
- Uploaded user-story.txt inside user-story.zip (detailed narrative spec)
- Application Code KB (ReImagine KB, uploaded separately)
- Code Best Practices KB (shared, uploaded separately)

### Re-Architect

Re-Architect converts existing code into the modernised target. It reads the legacy source (stored procedures, SQL logic, old service files) and the Code KB, and produces equivalent dotnet implementations that maintain behavioural parity with the original.

**What it is for:**
Migrating each stored procedure or legacy unit into a concrete dotnet class that implements the appropriate interface from the Re-Imagine foundation. Re-Architect is run per wave: orchestrator first, then dependent procedures in dependency order.

**What it does not do:**
It does not generate project structure, DI wiring, or runtime entrypoints. It assumes the Re-Imagine foundation already exists and generates classes that slot into it.

**When to use it:**
After the Re-Imagine foundation is committed and validated. Each Re-Architect wave produces a set of concrete job implementations that are added on top of the foundation.

**Input it uses:**
- User Story field in the form describing the conversion scope for this wave
- Uploaded tech-details.txt inside a zip containing the legacy SQL source for this wave
- Application Code KB (ReArchitect KB, uploaded separately)
- Code Best Practices KB (shared, uploaded separately)

---

## Knowledge Bases Used

AppMod uses two types of Knowledge Base: Application Code KB and Code Best Practices KB. Each is uploaded separately in the AppMod interface. They persist across multiple runs and provide AppMod with context it cannot derive from the user story alone.

### Application Code KB — Re-Imagine (reimagine_kb.zip)

**Purpose:** Defines the target architecture and generation scope for the foundation run. This KB tells AppMod what kind of solution to produce, what layers to include, what naming conventions to follow, and what the definition of done looks like.

**Why it matters:** Without this KB, AppMod has no understanding of the Apha solution conventions or what a batch job foundation should look like in this codebase. It would generate a generic dotnet console app with no domain-specific structure. The KB constrains generation toward the target patterns.

**Files included:**

| File | Content | Purpose |
|------|---------|---------|
| `kb_overview.txt` | Purpose, scope, target outcome, architecture rules, naming rules, definition of done | Sets the overall generation contract |
| `target_structure.txt` | Folder layout, project names, conventions (nullable, implicit usings, async/await) | Tells AppMod exactly how to name and structure folders |
| `input_contract.txt` | What to do and not do in this run: foundation only, no stubs, no procedure conversion | Prevents AppMod from generating outside scope |
| `user-story.txt` | Full narrative spec per project, per class, per method | Primary generation driver (also uploaded separately as user-story.zip) |

**Key rules it enforces:**
- Clean layered architecture: Host, Application, Core, Infrastructure separation
- Interface-based design throughout
- Scoped lifetime for services, repositories and DbContext
- Infrastructure concerns must not appear in Application orchestration layer
- Async/await for all I/O and database operations
- Structured logging with correlation ID
- No placeholder-only implementations

### Application Code KB — Re-Architect (rearchitect_kb.zip)

**Purpose:** Defines the conversion rules, procedure inventory, and orchestrator dependency chain for the stored procedure migration. This KB tells AppMod how to translate SQL logic into dotnet, what behavioural parity to preserve, and in what execution order procedures must run.

**Why it matters:** Stored procedure conversion is not straightforward. Procedures have implicit dependencies, conditional execution branches, transaction scopes, dynamic SQL, and notification side effects. Without this KB, AppMod would generate flat independent classes that break the execution order and lose the parity guarantees.

**Files included:**

| File | Content | Purpose |
|------|---------|---------|
| `kb_overview.txt` | Conversion rules, parity rules, definition of done | Sets the conversion contract |
| `scheduled_inventory.txt` | All 32 scheduled procedures by name | Gives AppMod the full scope of Scheduled track |
| `adhoc_inventory.txt` | All 24 adhoc procedures by name | Gives AppMod the full scope of Adhoc track |
| `orchestrator_dependencies.txt` | Call chains for `sp_LoadFromFPS` and `sp_RecreateSummaries` with dependent procedure order | Ensures AppMod preserves orchestration sequence |

**Key rules it enforces:**
- Maintain input/output behavioural parity with original SQL
- Preserve null handling and default behaviour
- Preserve transaction intent
- Preserve orchestrator execution order (critical for `sp_LoadFromFPS` and `sp_RecreateSummaries` chains)
- Map dynamic SQL safely and explicitly
- No placeholder-only implementations

**Orchestrator dependency context captured:**

Scheduled master: `sp_LoadFromFPS` calls `sp_deleteFPSTotals`, `sp_createFPSTotals`, `sp_DeleteYearsFPSData`, `sp_AddYearsFPSData` in fixed order across both source fps database and target archive database.

Adhoc master: `sp_RecreateSummaries` is the primary orchestrator. `spSendReportEmails_Manual` group handles notification side effects.

### Code Best Practices KB (BATCHJOBS_ARCHITECTURE_GUIDE.md)

**Purpose:** Provides AppMod with Apha-specific coding standards extracted from the existing FPS solution. This KB is shared across both Re-Imagine and Re-Architect runs. It teaches AppMod the house style so generated code is consistent with the rest of the codebase.

**Why it matters:** A modernised batch solution that does not match the patterns of the surrounding FPS, PACT, PIMS, and Costbook solutions creates a maintenance burden. The best practices KB ensures naming, DI registration style, logging format, error handling, and repository patterns are identical to existing services.

**What it covers:**
- Clean architecture layer boundaries and BatchJobs-specific adaptations
- Project structure and naming conventions
- Dependency injection patterns and lifetime rules
- Logging standards including correlation ID propagation
- Error handling and exception patterns
- Repository pattern for data access
- Database patterns with Npgsql and EF Core
- AutoMapper usage patterns
- Authentication and authorisation conventions
- Testing patterns
- Configuration management
- C# coding standards (nullable, implicit usings, async, records)
- Correlation ID pattern

---

## How Re-Imagine and Re-Architect Work Together

The two modes are designed as a pipeline, not as alternatives. Re-Imagine produces the skeleton and Re-Architect fills it with implementations. Neither is complete without the other.

```
Legacy SQL Stored Procedures (56 total)
         |
         v
   Re-Architect KB
   (conversion rules + inventory + orchestrator deps)
         |
         v
  Re-Architect runs (per wave)
  - Wave 1: Scheduled orchestrator (sp_LoadFromFPS chain)
  - Wave 2: Scheduled data loaders (sp_AddMY_* group)
  - Wave 3: Adhoc orchestrator (sp_RecreateSummaries chain)
  - Wave 4: Adhoc notifications (spSend* group)
         |
         v
  Concrete job implementations
  (IScheduledJob / IAdhocJob implementations)
         |
         | plugs into
         v
  Re-Imagine Foundation          <--- built first
  (Host, Application, Core, Infrastructure)
  AphaBatchJobs.sln
  Program.cs + CLI triggers
  DI wiring
  IScheduledJob / IAdhocJob interfaces
  PostgreSQL / Npgsql setup
  Correlation ID + logging
  Exit code mapping
  Dockerfile
         ^
         |
   Re-Imagine KB
   (foundation spec + architecture rules)
         |
  User story narrative (user-story.txt)
  Code Best Practices KB (shared)
```

**Delivery sequence:**
1. Re-Imagine produces the foundation → commit as `v0.1.0-foundation`
2. Re-Architect Wave 1 (scheduled orchestrator) → commit and validate parity
3. Re-Architect Wave 2 (scheduled data loaders) → commit and validate
4. Re-Architect Wave 3 (adhoc orchestrator) → commit and validate
5. Re-Architect Wave 4 (adhoc notifications) → commit and validate
6. Full solution release → `v1.0.0-batchjobs-ga`

**Why this separation is important:**
Running Re-Architect without a foundation means generated procedure classes have nowhere to slot in. They would be free-floating files with unresolved interface references. By committing the foundation first and validating it compiles, every subsequent Re-Architect wave has a stable compile target and the incremental changes can be tested in isolation.

---

## Source Inventory Summary

| Track | Total Procedures | Master Orchestrator | Estimated Effort |
|-------|-----------------|---------------------|-----------------|
| Scheduled | 32 | `sp_LoadFromFPS` | 316 hours |
| Adhoc | 24 | `sp_RecreateSummaries` | 276 hours |
| **Total** | **56** | — | **592 hours** |

AppMod Re-Architect is expected to reduce the manual conversion effort substantially by generating behavioural parity implementations from the SQL source, with human review focused on correctness rather than translation.

---

## Final Decision: Pragmatic Path Forward

**Key insight:** Asking AppMod (an LLM) to generate patterns it hasn't been trained on (.NET 10, released post-cutoff) results in either refusal or downgrade behavior. AppMod reliably generates .NET 8 and will self-correct .NET 10 requests back to 8 with "doesn't exist yet" comments.

**Chosen approach:**
- Accept v4 (net8.0, 90% quality, clean structure) as the stable foundation
- Plan the .NET 10 upgrade as a *post-Re-Architect* step, not a generation-time decision  
- This avoids fighting AppMod's knowledge boundaries and focuses generation effort on code structure + procedure conversion

**Why this works:**
- Re-Imagine produces a solid net8.0 skeleton with clean layering, DI wiring, Dockerfile, and runtime entrypoints
- Re-Architect fills that skeleton with procedure implementations on top of the net8.0 base
- After Re-Architect is complete and tested, a single coordinated global `net8.0 → net10.0` upgrade via csproj changes can be applied to the full tree with one validation pass
- This avoids iterating against AppMod's generation weaknesses and instead uses its strengths (architecture design, code structure) with manual control over version targeting

**v4 Foundation refinements needed:**
1. Fix Serilog.Enrichers.Environment version constraint (3.1.0 → 3.0.1) ✓ Done
2. Remove prose commentary from 12 files ✓ Done
3. Add missing `using` statements for configuration binder and Quartz DI extensions (in progress)

Once v4 compiles cleanly, commit it and lock it as the foundation baseline for all Re-Architect waves.
