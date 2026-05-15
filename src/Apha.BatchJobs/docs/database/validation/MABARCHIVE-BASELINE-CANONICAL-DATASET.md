# MABArchive Canonical Baseline Dataset

Status date: 2026-05-15
Scope: Baseline parity dataset and report format for MABArchive 24-loader migration.

## Canonical Environment

- Host: localhost
- Port: 5432
- Database: batch_jobs_foundation_db
- Source schema: fps
- Target schema: mabarchive

## Canonical Dataset Selection

- Baseline year: 2025
- Reason: Current local seed has non-empty rows across all 24 loader targets and includes key edge behaviors:
  - g_tlkpproject project-key delete scope
  - my_tbladditionalcosts ac_counter row numbering behavior
  - tlkpyear month lookup behavior
  - my_staff name composition behavior

## Snapshot Runner

- Script: docs/database/sql/validate-mabarchive-baseline.ps1
- Output path: docs/database/validation/mabarchive-baseline-<timestamp>.json
- Captured fields per loader:
  - sequence
  - loader
  - table
  - rowCount
  - rowHash

## Canonical Hash Format

- Row hash: md5(to_jsonb(row)::text)
- Table hash: sha256(string_agg(row_hash ordered by row_hash))
- JSON metadata value:
  - snapshotFormat = sha256(string_agg(md5(to_jsonb(row)::text) ordered by md5))

## Comparison Runner

- Script: docs/database/sql/compare-mabarchive-baseline.ps1
- Inputs:
  - BaselineJson
  - CandidateJson
- Pass criteria:
  - All 24 loaders present
  - rowCount match for each loader
  - rowHash match for each loader

## Initial Baseline Artifact

- docs/database/validation/mabarchive-baseline-20260515-142030.json
