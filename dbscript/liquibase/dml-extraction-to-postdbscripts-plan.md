# Extract DML Changesets into `postdbscripts` — Plan

## Goal

Move changesets from `changesets/` that contain **only** DML (INSERT/UPDATE/DELETE — no table/constraint changes) into `postdbscripts/`, since `postdbscripts` is applied as a separate step after all schema changes. Changesets that mix DDL and DML in the same file are left in `changesets/`, because their DML backfills a column/table created earlier in that same file — separating it would break the required execution order.

## Rule used to decide

- **Move**: the entire file is DML plus comments/rollback notes. No `CREATE`, `ALTER`, or `DROP`.
- **Keep in `changesets/`**: the file has DDL anywhere, even if most of it is DML (e.g. partition/backfill migrations where the DML depends on a column or shadow table created in the same changeset).

## Files reviewed

22 files in `changesets/` contained DML. Each was opened and checked in full.

| File | Content | Decision |
|---|---|---|
| `006_batchjobs_seed_data.sql` | Pure DML | Moved |
| `cr017_seed_data_fps_job_status.sql` | Pure DML | Moved |
| `cr019_seed_data_job_master.sql` | Pure DML | Moved |
| `cr031_delete_seeddata_jobstatus_jobmaster.sql` | Pure DML | Moved |
| `cr033_seed_tblyearmaster_data.sql` | Pure DML | Moved |
| `cr048_seed_yearend_datasetup_cutover_jobs.sql` | Pure DML | Moved |
| `cr075_seed_account_codes.sql` | Pure DML | Moved |
| `cr026_modify_ddl_batchjobs.sql` | Mixed — DML **partially** extracted | Split |
| `cr028_add_constraint_job_queue.sql` | Mixed — DML **partially** extracted | Split |
| `cr032_modify_fps_tblstagingmonthlytime.sql` | Mixed, backfill is order-dependent | Kept whole |
| `cr034_convert_money_columns_to_numeric_19_4.sql` | Mixed, procedural (not business DML) | Kept whole |
| `cr043_projectmonthcasework_fpsyear_and_indexes.sql` | Mixed, backfill is order-dependent | Kept whole |
| `cr044_bulk_rates_jobs_staging_and_validation.sql` | Mixed — DML **partially** extracted | Split |
| `cr045_job_queue_upload_metadata_and_bulk_rates_download.sql` | Mixed, DML migrates data inside the same `ALTER` | Kept whole |
| `cr047_milestone_notifications_job_and_audit.sql` | Mixed — DML **partially** extracted | Split |
| `cr071_partition_audit_tables.sql` | Mixed, data copy required before table swap | Kept whole |
| `cr073_create_masterlookup_and_directorate.sql` | Mixed — DML **partially** extracted | Split |
| `cr077_partition_job_queue_log.sql` | Mixed, data copy required before table swap | Kept whole |
| `cr082_partition_period_timecostcalcs.sql` | Mixed, data copy required before table swap | Kept whole |
| `cr083_partition_period_proj_subcontract.sql` | Mixed, data copy required before table swap | Kept whole |
| `cr084_partition_period_monthlyoutput.sql` | Mixed, data copy required before table swap | Kept whole |
| `cr085_partition_projectmonthcasework.sql` | Mixed, data copy required before table swap | Kept whole |

## Safety checks performed before moving

- **Target tables already exist by the time `postdbscripts` runs.** `tblyearmaster`, `tlkpaccountcode`, and `tlkpsubaccount` are created in the baseline (`baseline/001_fps_baseline_20260522.sql`); `job_master` and `job_status` are created in changeset `005_batchjobs_execution_control_objects.sql`. `postdbscripts` runs after the full `changesets` changelog, so all of these exist by then.
- **All 7 moved files are idempotent** — each uses `ON CONFLICT`, `WHERE NOT EXISTS`, or a condition that becomes a no-op once already applied. This matters because Liquibase identifies a changeset by its file path. On environments where these changesets already ran from `changesets/`, they will look like new/unrun changesets from their new location in `postdbscripts/` and may execute again — being idempotent makes that re-run harmless.
- **Relative execution order preserved.** Both folders use `includeAll` (alphabetical-by-filename). The moved files still sort as `006` → `cr017` → `cr019` → `cr031` → `cr033` → `cr048` → `cr075`, the same relative order as before.

## What was done

Used `git mv` to relocate the 7 files listed above from [changesets](changesets) to [postdbscripts](postdbscripts), preserving git history. No file content was changed. No changelog YAML edits were needed, since both `db.changelog-master.yaml` and `db.changelog-postdbscripts.yaml` use `includeAll` on their respective folders.

## Follow-up for deployment owners

Because these changesets already ran in Dev/SIT/Production from their old path, Liquibase will likely attempt to run them again (from the new path) the next time `db.changelog-postdbscripts.yaml` is applied there. This is safe because all 7 files are idempotent, but deployment owners should be aware this will show up as new changesets being applied.

## Part 2 — Extracting safe DML out of the mixed DDL+DML files

The 15 mixed files were reviewed statement-by-statement to find DML that is **not** order-dependent on the DDL around it (i.e. it doesn't backfill a column before a `NOT NULL`/constraint is added, and it doesn't feed a table-swap/rename). Only that kind of DML is safe to move, because `postdbscripts` runs strictly after every changeset finishes.

### Extracted (safe, no dependency on same-changeset DDL)

| Source changeset | What was extracted | Why it's safe | New file |
|---|---|---|---|
| `cr026_modify_ddl_batchjobs.sql` | Trailing stale-job/stale-lock reconciliation (`UPDATE`/`DELETE`) | Runs after all new columns already exist in that file; nothing later depends on it | `postdbscripts/cr026_cleanup_stale_jobs_and_locks.sql` |
| `cr028_add_constraint_job_queue.sql` | The already-separate `CR028_cleanup` changeset (stale lock `DELETE`) | It was already isolated as its own changeset id, only touches time-based cleanup | `postdbscripts/cr028_cleanup_job_lock.sql` |
| `cr044_bulk_rates_jobs_staging_and_validation.sql` | Leading `job_master`/`job_status` seed rows for the 3 Bulk Rates jobs | Seeds pre-existing tables; positioned before the staging tables it has no relation to | `postdbscripts/cr044_seed_bulk_rates_jobs.sql` |
| `cr047_milestone_notifications_job_and_audit.sql` | Leading `job_master`/`job_status` seed row for `MilestoneUpdateNotifications` | Same reasoning as cr044 | `postdbscripts/cr047_seed_milestone_notifications_job.sql` |
| `cr073_create_masterlookup_and_directorate.sql` | `tblmasterlookup` seed (`DELETE` + 3 `INSERT`s) | Unrelated to the `tbldirectorate` FK work later in the file | `postdbscripts/cr073_seed_masterlookup.sql` |

Each new file keeps the same changeset author, gets a `_cleanup`/`_seed` suffix on the id (matching the existing `CR028_cleanup` convention already in the repo) so it can't collide with the original changeset id, and is idempotent (`NOT EXISTS`/`ON CONFLICT`/date-based conditions), so it's safe if it also still ran previously from its old location.

The original changeset files were edited to remove only the extracted lines, replaced with a one-line comment pointing at the new location. All surrounding DDL, `BEGIN`/`COMMIT`, and rollback comments were left untouched.

### Left fully in place (not safe to split)

| Changeset | Why the DML can't be separated |
|---|---|
| `cr032_modify_fps_tblstagingmonthlytime.sql` | Backfills the new `id` column before it's made `NOT NULL` and turned into the primary key |
| `cr034_convert_money_columns_to_numeric_19_4.sql` | The "DML" is procedural bookkeeping (temp tables tracking view dependencies) for an in-place type-conversion algorithm, not business data |
| `cr043_projectmonthcasework_fpsyear_and_indexes.sql` | Backfills `fpsyear` before it's made `NOT NULL` and the primary key is replaced |
| `cr045_job_queue_upload_metadata_and_bulk_rates_download.sql` | Migrates data from an old JSON column into new columns inside the same `ALTER TABLE` block |
| `cr073_create_masterlookup_and_directorate.sql` (directorate part only) | The `tbldirectorate` seed `INSERT` must run before the `ADD CONSTRAINT` FK later in the file, otherwise the FK creation fails against an empty table |
| `cr071`, `cr077`, `cr082`, `cr083`, `cr084`, `cr085` (all partition conversions) | Each copies data into a new partitioned/shadow table, verifies row-count parity, then renames tables — the copy must happen between create and swap, in the same changeset |

### Result

`postdbscripts/` now contains 5 new extracted CRs in addition to the 7 whole files moved in Part 1. The remaining 10 mixed files (and the directorate half of cr073) stay untouched in `changesets/` because splitting them would change execution order and could break the migration.
