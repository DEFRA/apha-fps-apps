# Batch Jobs Learnings So Far

## Current Goal Context
- Modernize SQL-driven batch workflows into maintainable .NET batch jobs with behavior parity.
- Two canonical SQL input sources are currently parked for planning:
  - Scheduled track: `src/Apha.BatchJobs/ScheduledJobsTxt/tech-details.txt`
  - Adhoc track: `src/Apha.BatchJobs/KBUploads/ReArchitectFormInputs/adhocjobs.txt`

## What Has Worked
- Re-Imagine foundation v4 was the best baseline (clean architecture shape, usable structure, high efficacy).
- Wave 1 scheduled conversion path achieved accepted parity after targeted fixes.
- Manual deterministic patching proved reliable for missing orchestrator/DI generation gaps.
- Strict validation gates helped make quality measurable (coverage, markers, forbidden prose, required artifacts).

## What Did Not Work Reliably
- Re-Architect/AppMod outputs for Wave 2 adhoc were inconsistent across runs.
- Common failure pattern: helper services generated, but orchestrator and DI registration missing or malformed.
- Repeated prose contamination in C# output ("Key improvements made" sections appended to files).
- Output quality regressed across some reruns even with tighter prompt constraints.

## Wave 2 Adhoc Observations
- R1-R4 style runs repeatedly missed at least one of:
  - `AdhocRecreateSummariesJob`
  - DI registration for `IEnumerable<IAdhocJob>` discovery
  - Full procedure coverage (especially email procedures)
- Coverage improved incrementally in some runs, but deterministic completeness was not achieved.

## Core Lessons
- Prompt strictness alone is not sufficient for deterministic generation quality.
- Deterministic post-generation validation and repair is essential.
- A local, controlled workflow in-repo is more reliable than relying on external model behavior.
- Behavioral parity requirements must remain explicit: order, branch conditions, exit codes, timeout semantics, and side effects.

## Decision Taken
- AppMod is currently excluded from active execution.
- Work mode is now planning-first, with no automatic generation/validation runs unless explicitly requested.

## Planning Baseline (AppMod-Excluded)
- Build a local skill-based workflow that can:
  - ingest SQL objects,
  - map them to orchestration/service responsibilities,
  - generate or integrate C# deterministically,
  - validate against hard gates,
  - and optionally apply deterministic salvage patches.

## Non-Negotiable Quality Gates
- Required artifacts present (orchestrator + DI + contracts + mapped services).
- Full object/procedure coverage for requested scope.
- No prose contamination in generated code.
- Deterministic marker checks for orchestration and branch logic.
- Compile-ready output in target solution context.

## Next Planning Topics
1. SQL object taxonomy and mapping matrix (procedure/function/trigger role model).
2. Integration decision rules (new batch job vs plugin to existing code path).
3. Standard orchestration contract (timeouts, cancellation, logging, exit codes, branch handling).
4. Review and acceptance checklist for each conversion cycle.
5. Repo-native skill and optional custom-agent structure for controlled execution.
