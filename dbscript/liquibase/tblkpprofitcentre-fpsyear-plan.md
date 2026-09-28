# `fps.tblkpprofitcentre.fpsyear` Change Plan

## Goal

Introduce FPS-year scoping for `fps.tblkpprofitcentre`, determine and update every affected database object, and support the approved yearly partition plan without losing existing data or breaking consumers.

## Current Baseline

- `fps.tblkpprofitcentre` is currently an unpartitioned table. Its primary key is `profitcentre` alone; it has a `division` index and a foreign key to `fps.tlkpdivision`.
- Five inbound foreign-key constraints currently reference `profitcentre`: one each from `costcentre`, `profitcentregrade`, `profitcentregrade_nondefra`, `tbltestrccost`, and `workgroup`.
- Existing profit-centre rows do not have an `fpsyear`; the intended model is multiple `(profitcentre, fpsyear)` rows, not assigning each existing row to one authoritative year. The year master and existing year-scoped relationships provide the candidate year set.
- The year-partition utility discovers tables partitioned by `LIST (fpsyear)` and creates a partition for a requested year. It does not convert an ordinary table into a partitioned table.
- The current working changeset, `cr089_add_fpsyear_to_tblkpprofitcentre.sql`, adds `fpsyear`, then (guarded by a precondition that the data-migration program has populated it) enforces `NOT NULL`, replaces the primary key with `(profitcentre, fpsyear)`, replaces the five inbound FKs plus adds the `tbluser_profitcentre` FK, and recreates the fpsyear-aware dependent views. It does not perform yearly partition conversion, which remains a separate later changeset.

## Repository Findings and Recommendations

- All five inbound-FK tables already have a non-null `fpsyear`: `costcentre`, `profitcentregrade`, `profitcentregrade_nondefra`, `tbltestrccost`, and `workgroup`. Their year-aware keys are already composite where applicable. No new year columns are needed on the referencing tables; replace all five current FKs with `(profitcentre, fpsyear)` FKs.
- `fps.tbluser_profitcentre` is already partitioned by `fpsyear` with primary key `(profitcentre, user_id, fpsyear)`. It is the clearest existing indication that user-to-profit-centre assignment is year-scoped, but it does not establish that the profit-centre definition and its attributes are year-specific.
- The baseline has no FK from `fps.tbluser_profitcentre` to `fps.tblkpprofitcentre`. Add a composite FK on `(profitcentre, fpsyear)`; its existing columns support it.
- The selected design changes the parent primary key from `profitcentre` to `(profitcentre, fpsyear)`. This permits the same profit-centre code in multiple years and replaces the existing global uniqueness guarantee with uniqueness within each year; partitioning by `fpsyear` requires the partition key in the parent primary/unique key.
- The composite parent key supports composite FKs from the five existing referencing tables and the new FK from `tbluser_profitcentre`. Add an FK from parent `fpsyear` to `tblyearmaster` for year validation.
- The view layer has year gaps to fix, not just opportunities to add a projected column. `cr024_modify_view_fps_vtblkpprofitcentre.sql` currently joins `tblkpprofitcentre` to `tbluser_profitcentre` on `profitcentre` only; it should also join on `fpsyear`. `cr025_modify_view_fps_vworkgroup.sql` already matches `workgroup.fpsyear` to the view's year, but depends on that upstream view being correctly scoped.
- Other direct view/query definitions to review for year-aware joins and projections are `vqrytbidsum`, `qryfrmtimesellerpc_map`, `vpacttblkpprofitcentre`, `vtblkpprofitcentre`, `vprofitcentregrade`, `vqryfrmtimesellerpc`, `vtblkpprofitcentre_general`, and `vtblpurchase`. In particular, the two seller-PC views join the parent to user assignments and grade rows without consistently matching `fpsyear`; `vtblpurchase` also joins workgroup, bid, profit centre, and user assignment on incomplete year keys. Check their downstream consumers and preserve output contracts when changing them.
- Existing partition-conversion changesets use explicit partitions for 2016 through 2027 plus a `DEFAULT` partition. Use this same range/default convention for the initial conversion and use `001_add_partition.sql` to add later years.
- Production was created using the Liquibase files in this workspace. Treat those files as the schema source of truth, while checking the deployed Liquibase status and any later environment changes before applying the new migration.
- Target Dev and SIT, using the PostgreSQL versions already installed in those environments. Record each version during deployment validation; no broader version matrix is in scope.

## Plan

### 1. Establish the Year Semantics and Data Mapping

- Treat `profitcentre` as a year-scoped identity: one code may have a row in each applicable FPS year, with `(profitcentre, fpsyear)` as the parent primary key.
- The separate data-migration program creates/populates the year-specific rows and values, using the FPS-year reference data and year-scoped relationships. All profit centres are expected to have year-specific references; validate this as a migration precondition.
- For future writes, require the caller/import process to provide `fpsyear` and validate it against `tblyearmaster`.

**Gate:** the separate data-migration program must produce a complete set of valid year-specific rows before cutover.

### 2. Inventory All Database Dependencies

- Search all baseline SQL, Liquibase changesets, partition scripts, and post-database scripts for direct references to `fps.tblkpprofitcentre`.
- Inspect the target database catalogs for inbound/outbound foreign keys, views and materialized views, functions/procedures, triggers, rules, indexes, constraints, grants, and dependencies not captured in the checked-in baseline.
- Traverse view dependencies recursively so views that consume another affected view are included.
- Record each object, whether it reads or writes the table, its join/key columns, whether it carries `fpsyear`, and the required action (no change, add year projection/filter, year-aware join, or rebuild).
- Compare the catalog inventory with checked-in SQL and include any environment-specific differences in the migration script.

**Known candidates from the checked-in baseline and changesets**

- Inbound FKs: `fps.costcentre`, `fps.profitcentregrade`, `fps.profitcentregrade_nondefra`, `fps.tbltestrccost`, and `fps.workgroup` (one constraint from each).
- Outbound FK: retain `fps.tblkpprofitcentre` to `fps.tlkpdivision`.
- Direct view/query consumers to inspect for year-aware behavior: `fps.vqrytbidsum`, `fps.qryfrmtimesellerpc_map`, `fps.vpacttblkpprofitcentre`, `fps.vtblkpprofitcentre`, `fps.vprofitcentregrade`, `fps.vqryfrmtimesellerpc`, `fps.vtblkpprofitcentre_general`, and `fps.vtblpurchase`.
- Indirect consumers include `fps.vworkgroup`, `fps.vprofitcentregrade_general`, and any other views discovered through the recursive dependency inventory. Check the current `cr024` and `cr040` definitions, not only the baseline snapshot, because later changesets replace some baseline views.
- Recreate the `division` index on the partitioned parent so its partitions receive matching indexes.

This is the starting inventory. Complete the repository and target-catalog search before finalizing the migration scripts.

### 3. Add and Populate `fpsyear`

- CR089 adds the nullable `fpsyear integer` column, preserving compatibility with existing writes while the data-migration program runs.
- Update all writers, imports, and maintenance jobs to supply `fpsyear` before the key/FK/view steps run.
- Run the separate data-migration program directly against `fps.tblkpprofitcentre`, populating one row per applicable `(profitcentre, fpsyear)` pair.
- CR089's precondition checks reject the migration if any row has a null `fpsyear`, a duplicate `(profitcentre, fpsyear)` pair, or a year absent from `tblyearmaster`.

### 4. Decide and Apply Primary-Key / Foreign-Key Changes

- CR089 replaces the single-column primary key with `(profitcentre, fpsyear)` in place, once the precondition checks pass. This permits one profit-centre code in different years; all consumers must use year-qualified lookups.
- CR089 drops the five old single-column FKs and recreates them on `(profitcentre, fpsyear)`, and adds the missing FK from `tbluser_profitcentre` on the same pair. It preserves the outbound `division` FK and adds an FPS-year FK to `tblyearmaster`.
- CR089 recreates the affected views in the same changeset so they join on `fpsyear` and are never left pointing at the pre-migration join logic.
- Yearly partition conversion (`PARTITION BY LIST (fpsyear)`) is deliberately out of scope for CR089 and remains a separate later changeset using the shadow-table pattern already established for other partitioned tables in this repository.

### 5. Update and Optimize Views / Queries

- In every affected view/query, include `fpsyear` in joins, filters, and grouping wherever the related data is year-scoped. Apply year predicates at the earliest correct relation to prevent cross-year matches and row multiplication.
- Keep API/reporting view output contracts stable. Do not rely on ordinal column positions or `SELECT *`; expose `fpsyear` explicitly where consumers need it.
- Recreate dependent views in dependency order, with explicit column lists and stable output contracts where practical.
- Compare query plans on representative workloads before and after changes. Add supporting indexes for measured year-scoped joins and filters, including `(profitcentre, fpsyear)` where needed.
- Compare result sets and row counts by FPS year, including boundary cases where identifiers recur across years.

### 6. Convert to Yearly Partitions

- Record the PostgreSQL version in Dev and SIT and run the migration against those versions.
- Partition the shadow table before cutover, preserving columns, defaults, indexes, and grants. Pause writes only for the controlled cutover in Dev and SIT; no separate downtime requirement is in scope.
- Use the established initial partition range (2016–2027 plus `DEFAULT`) and `partitions/001_add_partition.sql` for later years.
- Ensure every unique/primary-key constraint on the partitioned parent includes `fpsyear`; ensure all foreign keys point to a valid unique key.
- Test inserts for current, future, and unsupported years, plus behavior when a default partition exists.
- Swap the table only after dependent views, FKs, permissions, and application consumers are included in the tested migration sequence.

### 7. Update Writers and Consumers Outside the Database

- Search application repositories, data import/export jobs, reporting integrations, and operational scripts for reads/writes using profit-centre identifiers.
- Update every create/update/delete and lookup path to use the year-scoped key. Include `fpsyear` in API/DTO contracts that identify or select a profit-centre row.
- Verify seed/reference-data loading and year-rollover workflows create the right profit-centre rows before dependent data is loaded.
- Coordinate deployment order so schema, database objects, and application versions remain compatible during rollout.

### 8. Test, Deploy, and Operate

- Add migration checks for preconditions, approved backfill completeness, duplicate keys, dependent FKs, and row-count parity.
- Test in Dev and SIT, including rollback/recovery and concurrent-write behavior.
- Validate all changed views and write paths, query plans, permissions, and partition creation for multiple years.
- Document deployment ordering, lock behavior, monitoring, and recovery steps. Deploy the nullable column, create and populate the shadow table, validate it, and then perform the key/FK/view cutover.

## Deliverables

1. Separate data-migration program output containing validated year-scoped rows and values.
2. Complete repository plus live-catalog dependency inventory with an action for every dependent object.
3. Separate reviewed changesets for column addition, backfill/constraints, dependent keys and views, and partition conversion as needed.
4. Updated yearly partition operations and application writers for year-qualified profit-centre access.
5. Migration validation, deployment, and recovery instructions.

## Data Migration Responsibility

- A separate data-migration program determines and populates the values for each profit-centre year. Liquibase establishes the schema and validates the resulting rows; it does not choose or copy the year-specific values.

## Confirmed Scope

- No profit centres are expected to be missing year-specific references; enforce this as a precondition before backfilling.
- The work targets Dev and SIT, using the PostgreSQL versions installed there. No wider compatibility matrix or special downtime requirement is in scope.
- Production was created from this workspace's Liquibase files. Check the deployed Liquibase status before rollout to confirm which changes have already been applied.
- Production Liquibase status and catalog checks are execution validations, not design decisions.