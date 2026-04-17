# FPS / MAB_Archive SQL Logic — Plain English Baseline for Conversion and Validation

## Document Purpose

This document explains the legacy SQL logic in plain English while preserving the original intent and behavior as closely as possible.

It is intended to serve as:

- the common understanding document for business and technical stakeholders
- the baseline interpretation of the legacy stored procedures
- the reference point for .NET conversion
- the reference point for validation and parity checking after conversion

This document is intentionally descriptive, not prescriptive.

It explains **what the SQL does today**.  
It does **not** redesign the logic, simplify the legacy behavior, or introduce improvements.

---

## Scope of This Baseline

This baseline is focused on the SQL procedures shared from:

- the **FPS yearly database** (example: `FPS2025`)
- the **MAB_Archive** database

The main logic covered here is the yearly data refresh and archive load flow driven by:

- `sp_LoadFromFPS`
- `sp_deleteFPSTotals`
- `sp_createFPSTotals`
- `sp_DeleteYearsFPSData`
- `sp_AddYearsFPSData`
- the `sp_AddMY_*` procedures called by that flow

This document should be used as the **logic baseline** for future conversion and validation.

---

## Golden Rule for Future Use

When using this document for conversion or validation:

- preserve the original SQL behavior first
- preserve side effects first
- preserve write targets first
- preserve execution sequence first
- only after parity is proven should improvements be considered

If any converted design differs from what is written here, that difference should be treated as:
- an enhancement
- a design change
- or a separately approved modernization decision

not as implicit legacy behavior.

---

# 1) System-Level Understanding

## 1.1 What problem this SQL is solving

The SQL logic is used to refresh and archive yearly FPS-related data.

At a high level, it performs these activities:

1. Build yearly totals inside an FPS database
2. Remove old archive data for a chosen year from MAB_Archive
3. Reload that year’s archive/reporting data into MAB_Archive
4. Conditionally handle current-year processing depending on the current month

This means the legacy system is not just calculating one totals table.  
It is refreshing a **whole year-based archive/reporting dataset**.

---

## 1.2 The two logical database roles

### FPS database
The FPS database for a year (for example `FPS2025`) is the operational source database for that year.

It contains source tables, source views, and the `FPSYearTotals` table.

### MAB_Archive database
The MAB_Archive database is the archive/reporting target.

It stores year-tagged copies of many business tables, such as:
- `MY_FPSYearTotals`
- `MY_MonthlyOutput`
- `MY_MonthlyTime`
- `MY_ProjectMonthFinal`
- `MY_tlkpProject`
- `MY_tlkpProject_all`
- and many others

So the overall logic is:

**build data in FPS → copy/archive it into MAB_Archive**

---

## 1.3 Main orchestration entry point

The master procedure is:

- `sp_LoadFromFPS`

This procedure decides:
- which year database to process
- whether to process only the previous year
- whether to also process the current year
- whether to run the full archive reload or only the project-all refresh

It is the single procedure that controls the yearly execution sequence.

---

# 2) Main Driver Procedure — `sp_LoadFromFPS`

## 2.1 Overall purpose

`sp_LoadFromFPS` is the main orchestration procedure.

It determines:
- the previous FPS year
- the current FPS year
- whether the current month is greater than 4
- which procedure sequence should run for each year

The behavior is month-sensitive.

---

## 2.2 Previous-year logic

The procedure first sets:

- `@FPSYear = current year - 1`
- `@cFPSVersion = 'FPS' + @FPSYear`

Example:
- if current year is 2026
- then first it targets `FPS2025`

It checks whether that database exists.

If it exists, it runs this sequence:

1. run `sp_deleteFPSTotals` inside the selected FPS database
2. run `sp_createFPSTotals` inside the selected FPS database
3. run `sp_DeleteYearsFPSData` in MAB_Archive for that year
4. run `sp_AddYearsFPSData` in MAB_Archive for that year

### Plain-English meaning
For the previous year, the procedure always does a full cycle:

- clear FPS totals
- rebuild FPS totals
- clear that year’s archive data in MAB_Archive
- reload that year’s archive data in MAB_Archive

So the previous year is always processed as a full refresh if the FPS database exists.

---

## 2.3 Current-year logic when month is greater than 4

If the current month is greater than 4, the procedure then switches to the current year:

- `@FPSYear = current year`
- `@cFPSVersion = 'FPS' + current year`

If that current-year FPS database exists, it runs the same full sequence again:

1. run `sp_deleteFPSTotals`
2. run `sp_createFPSTotals`
3. run `sp_DeleteYearsFPSData`
4. run `sp_AddYearsFPSData`

### Plain-English meaning
From May onward, the current year is treated like a full yearly archive refresh as well.

That means both:
- previous year
- current year

can be fully processed in the same orchestration run.

---

## 2.4 Current-year logic when month is 4 or below

If the current month is 4 or below, the procedure does **not** run the full current-year archive refresh.

Instead it does only this for the current year:

1. delete rows from `MY_tlkpProject_all` for the current year
2. run `sp_AddMY_tlkpProject_All` for the current year

### Plain-English meaning
Before May, the system does not fully refresh the current-year archive/reporting dataset.

It only refreshes the project-all snapshot for that year.

This is very important and must not be lost in conversion.

---

## 2.5 Key orchestration rules that must not drift

The following are original legacy rules:

- previous year is always processed first
- current year is only fully processed if current month > 4
- before May, current year gets only project-all refresh
- the orchestration is year-based and branch-based
- the orchestration depends on database existence checks

This must remain visible in any baseline interpretation.

---

# 3) Procedure — `sp_deleteFPSTotals`

## 3.1 Purpose

This procedure deletes all data from the `FPSYearTotals` table.

## 3.2 Actual behavior

The procedure is simply:

- `DELETE from FPSYearTotals`

There is:
- no `WHERE` clause
- no archive logic
- no row filtering
- no condition
- no safety copy

## 3.3 Plain-English meaning

This is a full table wipe of `FPSYearTotals` in the selected FPS database.

It is not a partial delete.
It is not year-scoped.
It is not archive-before-delete.

## 3.4 Important preservation note

If any future .NET conversion does:
- archive first
- delete only one year
- keep some rows
- or add extra behavior

then that is **not** the original behavior of `sp_deleteFPSTotals`.

---

# 4) Procedure — `sp_createFPSTotals`

## 4.1 Purpose

This procedure rebuilds the `FPSYearTotals` table inside the FPS database.

It calculates totals per project by joining project records with multiple cost summary views/tables.

---

## 4.2 Inputs used by the procedure

It reads from:

- `tlkpProject`
- `qryTotalAdditionalCosts`
- `qryTotalAnimalCosts`
- `qryTotalStaffCosts`
- `qryTotalTestCosts`

The joins are based on:

- `tlkpProject.ParentProject`
- matching `JobCode` / `Jobcode`

So the project row is the central row, and the cost values are brought in by left joins.

---

## 4.3 Why LEFT JOIN matters

The procedure uses `LEFT JOIN`, which means:

- every project from `tlkpProject` is eligible to appear
- even if a cost source row does not exist
- missing cost rows will still allow a project row to be returned
- missing numeric values are then handled with null-to-zero logic

This is important because the totals logic is designed to survive missing cost data.

---

## 4.4 Output columns written to `FPSYearTotals`

For each distinct project row, the procedure inserts:

- `ParentProject`
- `Program`
- `TotalAdditionalCosts`
- `TotalAnimalCosts`
- `TotalStaffCosts`
- `TotalTestCosts`
- `TotalCosts`
- `CustIncome`
- `TransferIncome`
- `TotalIncome`
- `Budget_CVL`
- `RequiredProfit`
- `Manager`
- `Customer`
- `ProjectStatus`
- `PVSIncome`
- `PlanCaseworkDebit`
- `TotalPayCosts`

---

## 4.5 Null handling rules

The procedure converts nulls to zero for:

- `TotalAdditionalCosts`
- `TotalAnimalCosts`
- `TotalStaffCosts`
- `TotalTestCosts`
- `PVSIncome`
- `PlanCaseworkDebit`
- `TotalPayCosts`

This is done using `CASE WHEN ... IS NULL THEN 0 ELSE ... END`.

### Plain-English meaning
If a value is missing in the source, the totals logic does not leave it blank.  
It treats it as zero.

---

## 4.6 TotalCosts formula

`TotalCosts` is calculated as:

- additional costs
- plus animal costs
- plus staff costs
- plus test costs
- plus plan casework debit

In plain English:

> TotalCosts is the sum of the four cost components plus PlanCaseworkDebit, with any missing values treated as zero.

---

## 4.7 TotalIncome formula

`TotalIncome` is calculated as:

- `CustIncome + TransferIncome`

### Important note on null behavior

The SQL expression for TotalIncome is:

- `custincome + Transferincome`

There is no explicit null-to-zero handling applied to either `CustIncome` or `TransferIncome` in this expression.

This means:
- if `CustIncome` is NULL → result is NULL
- if `TransferIncome` is NULL → result is NULL
- if both are NULL → result is NULL

### Plain-English implication

Unlike cost fields (which are explicitly defaulted to 0 via `CASE WHEN`), `TotalIncome` may evaluate to NULL if either input value is NULL.

### Conversion preservation rule

Any .NET conversion must preserve this behavior unless explicitly agreed otherwise.

Replacing this with a null-safe calculation (for example, treating NULL as 0) would change the original business logic.

---

## 4.8 RequiredProfit mapping

The procedure maps:

- `tlkpProject.Profit as RequiredProfit`

This means the output field `RequiredProfit` comes from the source field `Profit`.

That mapping should be preserved exactly.

---

## 4.9 PVSIncome handling

The procedure maps `PVSIncome` with null-to-zero handling:

- if null → 0
- else actual value

This must be preserved as-is.

---

## 4.10 TotalPayCosts handling

The procedure maps `TotalPayCosts` from `qryTotalStaffCosts.TotalPayCosts` with null-to-zero handling.

This means staff cost output includes:
- total staff costs
- plus a separate pay cost field

Both belong to the baseline behavior.

---

## 4.11 Use of `SELECT DISTINCT`

The insert uses `SELECT DISTINCT`.

### Plain-English meaning
If the joins would otherwise create duplicate output rows, the procedure suppresses duplicates before inserting into `FPSYearTotals`.

This is an important behavior detail and should not be ignored.

---

## 4.12 Plain-English summary of `sp_createFPSTotals`

This procedure rebuilds `FPSYearTotals` by:

- taking project rows from `tlkpProject`
- joining in additional, animal, staff, and test cost totals
- defaulting missing numeric values to zero where coded
- calculating TotalCosts and TotalIncome
- inserting the resulting distinct project rows into `FPSYearTotals`

This is the main source of the yearly totals logic.

---

# 5) Procedure — `sp_DeleteYearsFPSData`

## 5.1 Purpose

This procedure deletes archive/reporting data for one specific year from MAB_Archive.

Its purpose is to clear out that year’s existing archive rows so that they can be reloaded fresh.

---

## 5.2 Parameters

It accepts:

- `@cFPSVersion`
- `@FPSYear`

The year parameter is the key driver for the deletes.

---

## 5.3 First delete action — `G_tlkpProject`

Before the year-based deletes, it performs a dynamic delete from `G_tlkpProject`.

It deletes rows where `ParentProject` is found in the selected FPS database’s `tlkpProject`.

### Plain-English meaning
It removes project reference rows from `G_tlkpProject` that correspond to projects in the target FPS database.

This is not a simple year-filter delete; it is project-based.

---

## 5.4 Year-based delete coverage

It then deletes rows for the target year from all of these tables:

- `MY_FPSYearTotals`
- `MY_MonthlyOutput`
- `MY_MonthlyTime`
- `MY_Proj_Invoice`
- `MY_Proj_SubContract`
- `MY_ProjectMonthFinal`
- `MY_tblAdditionalCosts`
- `MY_tblAnimalReq`
- `MY_tblContract`
- `MY_tblStaffJob`
- `MY_TimeCostCalcs`
- `MY_tlkpTestReqmt`
- `MY_tlkpProject`
- `MY_tlkpProgram`
- `tlkpYear`
- `MY_ProfitCentreGrade`
- `MY_WorkGroupGrade`
- `MY_tblProfitCentre`
- `MY_TestOrProduct`
- `MY_Staff`
- `MY_Workgroup`
- `MY_tblAnimals`
- `MY_tlkpProject_all`

---

## 5.5 Plain-English meaning

This is not a retention cleanup.

It is not a “delete anything older than X” rule.

It is a **year-specific wipe** of the archive dataset for the chosen year.

The goal is:
- remove existing archive rows for that year
- then reload the same year fresh

That distinction is critical.

---

## 5.6 What must not drift

If future documentation or conversion describes this as:
- retention logic
- stale-data cleanup
- delete old years only
- archive pruning

then that would drift from the original logic.

The original behavior is:

> delete the full archive dataset for the supplied year

---

# 6) Procedure — `sp_AddYearsFPSData`

## 6.1 Purpose

This procedure reloads a full year of archive/reporting data into MAB_Archive.

It does this by calling a chain of procedures that each copy one logical table/domain from FPS into MAB_Archive.

---

## 6.2 Parameters

It accepts:

- `@cFPSVersion`
- `@vcFPSYear`

---

## 6.3 Procedure chain executed

It executes these procedures in order:

1. `sp_AddMY_tlkpProgram`
2. `sp_AddG_tlkpProject`
3. `sp_AddMY_tlkpProject`
4. `sp_AddMY_FPSYearTotals`
5. `sp_AddMY_MonthlyOutput`
6. `sp_AddMY_MonthlyTime`
7. `sp_AddMY_Proj_Invoice`
8. `sp_AddMY_Proj_SubContract`
9. `sp_AddMY_ProjectMonthFinal`
10. `sp_AddMY_tblAdditionalCosts`
11. `sp_AddMY_tblAnimalReq`
12. `sp_AddMY_tblContract`
13. `sp_AddMY_tblStaffJob`
14. `sp_AddMY_TimeCostCalcs`
15. `sp_AddMY_tlkpTestReqmt`
16. `sp_addMY_YearDetails`
17. `sp_addMY_WorkGroupGrade`
18. `sp_addMY_ProfitCentreGrade`
19. `sp_AddMY_tblProfitCentre`
20. `sp_AddMY_TestOrProduct`
21. `sp_AddMY_Staff`
22. `sp_AddMY_Workgroup`
23. `sp_AddMY_tblAnimals`
24. `sp_AddMY_tlkpProject_All`

---

## 6.4 Plain-English meaning

This is a broad fan-out yearly archive load.

It is not just loading:
- totals
- or project metadata

It reloads a wide reporting/archive dataset for that year, covering:
- projects
- programs
- monthly output
- time
- invoices
- subcontracts
- contracts
- animal data
- staff data
- workgroup/profit centre data
- project-all data
- and more

This broadness is part of the original logic.

---

## 6.5 What must not drift

If future documentation says this procedure is only about:
- totals
- or a small set of archive tables

that would be incorrect.

The original procedure is a **full archive-year rebuild**, not a narrow yearly totals step.

---

# 7) Procedure — `sp_AddMY_FPSYearTotals`

## 7.1 Purpose

This procedure archives yearly totals from the FPS database into MAB_Archive.

## 7.2 Behavior

It inserts into:

- `MY_FPSYearTotals`

It selects from:

- `<FPSVersion>.dbo.FPSYearTotals`

It prefixes each inserted row with the supplied FPS year.

## 7.3 Plain-English meaning

Once `FPSYearTotals` has been rebuilt in the FPS database, this procedure copies those totals into the archive database as year-tagged archive rows.

This is the archive target for yearly totals.

---

# 8) Procedure — `sp_AddMY_tlkpProject_All`

## 8.1 Purpose

This procedure copies yearly project-all data from the FPS database into the MAB_Archive table `MY_tlkpProject_all`.

## 8.2 Behavior

It inserts year-tagged rows into:

- `MY_tlkpProject_all`

by selecting from:

- `<FPSVersion>.dbo.tlkpProject`

The output includes business/project fields such as:
- ParentProject
- Program
- Customer
- Manager
- TransferIncome
- CustIncome
- WIP fields
- ProjectStatus
- Profit
- Budget_CVL
- PVSIncome
- PlanCaseworkDebit
- Disease
- Contract
- and other project attributes

## 8.3 Plain-English meaning

This is the archive/project snapshot table for the year.

It stores a year-tagged copy of project master-style data in MAB_Archive.

---

# 9) Procedure — `sp_AddG_tlkpProject`

## 9.1 Purpose

This procedure loads project reference data into `G_tlkpProject`.

## 9.2 Behavior

It inserts:

- ParentProject
- ProjectTitle
- CostBookNo
- Disease
- Contract
- ShortTitle
- ProjectStatus

from the selected FPS database’s `tlkpProject`, using `GROUP BY`.

## 9.3 Plain-English meaning

This is a project reference load used as part of the yearly archive/reporting refresh.

---

# 10) Other `sp_AddMY_*` Procedures

## 10.1 General pattern

All of these procedures follow a similar pattern:

- take an FPS database name
- take a target year
- read source rows from a table in the FPS database
- insert them into a MAB_Archive table
- add the supplied year value into the archive target where needed

## 10.2 What kinds of data they copy

These procedures archive the following kinds of information:

- monthly output
- monthly time
- invoices
- subcontract records
- project month final records
- additional costs
- animal requirements
- animals
- contracts
- profit centre information
- staff-job links
- program master data
- project master data
- test requirements
- year details
- workgroup and workgroup grade
- staff
- test/product

## 10.3 Plain-English meaning

Together these procedures rebuild the archive/reporting database for one year.

---

# 11) Procedure — `sp_addMY_YearDetails`

## 11.1 Purpose

This procedure adds a year-level detail row into `tlkpYear`.

## 11.2 Behavior

It inserts:
- the supplied year
- plus `db_var_value`

from:
- `<FPSVersion>.dbo.tblDB_Variables`

where:
- `db_var_name = 'month'`

## 11.3 Plain-English meaning

This stores a year-specific metadata/detail row based on the FPS database’s stored month configuration.

---

# 12) Legacy Logic as a Single End-to-End Story

## 12.1 Previous-year cycle

The system always starts with the previous year.

For that previous year it:

1. clears `FPSYearTotals`
2. rebuilds `FPSYearTotals`
3. deletes the archive/reporting data for that year from MAB_Archive
4. reloads the archive/reporting data for that year into MAB_Archive

## 12.2 Current-year cycle after April

If the current month is greater than 4, it then repeats the same full cycle for the current year.

## 12.3 Current-year behavior before May

If the current month is 4 or below, it does not run the full current-year archive refresh.

Instead it only refreshes:
- `MY_tlkpProject_all`

for the current year.

---

# 13) Rules That Must Not Drift

These are the most important originality-preserving rules.

## Rule 1 — `sp_deleteFPSTotals` is delete-only
It does not archive first.
It does not filter by year.
It simply deletes all rows from `FPSYearTotals`.

## Rule 2 — `sp_createFPSTotals` defines the totals logic
Its formulas, joins, null handling, and output fields define the original totals behavior.

## Rule 3 — `sp_DeleteYearsFPSData` is a year-specific archive wipe
It is not retention logic.
It is not cleanup of “old” data in general.
It is a full delete for the supplied year.

## Rule 4 — `sp_AddYearsFPSData` is a broad archive reload
It is not just a totals reload.
It reloads a large multi-table yearly archive/reporting dataset.

## Rule 5 — `sp_LoadFromFPS` contains important branching logic
It always processes the previous year.
It only fully processes current year after April.
Before May it only refreshes project-all for the current year.

## Rule 6 — Write targets matter
Archive/reporting outputs are written into MAB_Archive tables.
This is part of the original behavior.

## Rule 7 — FPS database existence check must be preserved

The procedure `sp_LoadFromFPS` checks whether the target FPS database exists before executing any processing steps:

- It queries `master.dbo.sysdatabases` using the constructed database name (for example, `FPS2025`)
- Processing for that year only proceeds if the database exists

Conversion implication:
- missing-year database must result in skipping that year's cycle,
- conversion must not assume all FPS year databases exist.

---

# 14) Conversion Baseline Use

This plain-English baseline should be used to answer the following questions during conversion:

- What is the original sequence?
- What exactly is being deleted?
- What exactly is being recalculated?
- Which tables are being loaded?
- Which outputs go to MAB_Archive?
- Which branch runs before May?
- Which branch runs after April?
- Which logic is formula logic?
- Which logic is orchestration logic?
- Which logic is archive refresh logic?

If a converted .NET design does not match this baseline, the difference should be explicitly called out.

---

# 15) Validation Baseline Use

This document should also be used for validation.

Validation should verify at least:

- previous year is always processed
- current year is only fully processed when month > 4
- before May only project-all refresh occurs for current year
- `FPSYearTotals` is deleted and rebuilt in the source FPS database
- target year archive rows are deleted from all relevant archive tables
- all expected `MY_*` loads are executed for the selected year
- totals formulas match `sp_createFPSTotals`
- null handling matches original behavior
- output targets match original archive tables

---

# 16) Final Baseline Summary

The legacy SQL logic is a year-based archive/reporting refresh process.

It always processes the previous year by:
- deleting FPS totals
- recreating FPS totals
- deleting that year’s archive rows
- reloading that year’s archive/reporting dataset

After April, it does the same for the current year.

Before May, it does not run the full current-year cycle.  
Instead it refreshes only the current-year `MY_tlkpProject_all` dataset.

The totals logic comes from `sp_createFPSTotals`.  
The archive delete logic comes from `sp_DeleteYearsFPSData`.  
The archive reload logic comes from `sp_AddYearsFPSData` and its called procedures.  
The orchestration logic comes from `sp_LoadFromFPS`.

---

# 17) One-Line Understanding

This legacy SQL is a **year-based totals rebuild and archive refresh pipeline** that always processes the previous year, conditionally processes the current year, and reloads a broad MAB_Archive yearly dataset from the selected FPS database.

# 18) Subtle SQL Behaviors Worth Preserving

This section captures behavior details that commonly drift during conversion.
For sign-off use, pair this section with the executable checks in Section 19.

## 18.1 TotalIncome can become NULL

In `sp_createFPSTotals`, `TotalIncome` is calculated as `custincome + Transferincome` with no null wrapper around either input.

Why it matters:
- if either input is NULL, the result can become NULL,
- this differs from cost fields that are explicitly defaulted to zero.

## 18.2 SELECT DISTINCT is part of the logic

`sp_createFPSTotals` uses `SELECT DISTINCT` before inserting into `FPSYearTotals`.

Why it matters:
- duplicate joined rows are suppressed by SQL itself,
- conversions that do not preserve de-duplication may drift in row counts and totals.

## 18.3 sp_deleteFPSTotals is a full wipe, not a year delete

The legacy procedure is `DELETE from FPSYearTotals` with no `WHERE` clause.

Why it matters:
- year-filtered delete, archive-before-delete, or selective delete behavior is not original behavior.

## 18.4 sp_LoadFromFPS is guarded by database existence checks

Before running each yearly cycle, `sp_LoadFromFPS` checks `master.dbo.sysdatabases` for the target FPS database.

Why it matters:
- if the database does not exist, that year's cycle is skipped,
- conversion must preserve skip-if-missing behavior and avoid assuming all year databases exist.

## 18.5 Orchestration branch semantics must stay intact

`sp_LoadFromFPS` orchestration has three core properties:
- previous year is attempted first,
- current-year full cycle runs only when `DATEPART(month, GETDATE()) > 4`,
- before May, current year only refreshes `MY_tlkpProject_all`.

Why it matters:
- changing order or branch behavior changes legacy orchestration semantics.

## 18.6 sp_DeleteYearsFPSData is a broad year-specific wipe

Legacy delete-years logic removes the supplied year across many MY_* tables plus `tlkpYear`, and removes matching projects from `G_tlkpProject`.

Why it matters:
- this is year-specific wipe behavior, not retention cleanup,
- totals-only delete coverage is under-implementation.

## 18.7 sp_AddYearsFPSData is a broad fan-out loader

Legacy add-years logic runs a long chain across many MY_* and G_* loaders.

Why it matters:
- totals-only year-add behavior is not functionally equivalent.

## 18.8 Dynamic SQL and dynamic DB naming are part of orchestration behavior

Many loaders build SQL strings with `@cFPSVersion` and read from `<FPSYear>.dbo.<table>`. `sp_LoadFromFPS` also builds procedure names dynamically.

Why it matters:
- conversion must preserve selected-year database behavior conceptually, even if implementation differs.

## 18.9 Archive targets matter

Legacy loaders write business output into MAB_Archive targets such as:
- `MY_FPSYearTotals`,
- `MY_tlkpProject_all`,
- and many other MY_* tables.

Why it matters:
- changing output targets is a design change, not a language conversion.

## 18.10 sp_AddG_tlkpProject uses GROUP BY rather than a simple copy

`sp_AddG_tlkpProject` uses grouped project reference columns.

Why it matters:
- row-for-row copying may not preserve effective grouped output behavior.

## 18.11 sp_AddMY_tlkpProject_All is insert-only

Legacy behavior is `INSERT INTO MY_tlkpProject_all SELECT ...` with no merge/upsert logic.

Why it matters:
- upsert can be valid enhancement, but is not strict parity by default.

## 18.12 sp_AddMY_FPSYearTotals loads from FPSYearTotals, not raw source tables

This procedure copies from the already-built `FPSYearTotals` table and does not recalculate totals.

Why it matters:
- independent recalculation in conversion may alter execution semantics.

## 18.13 sp_AddMY_YearDetails depends on a specific DB variable name

It reads `tblDB_Variables` where `db_var_name = 'month'` and inserts into `tlkpYear`.

Why it matters:
- this specific metadata dependency should not be generalized away.

---

# 19) Conversion Risk Checklist

Use this checklist during .NET conversion and parity validation.
Each item below highlights where legacy behavior often drifts.

## 19.1 Formula and null-handling risks

- [ ] `TotalCosts` preserves exact legacy formula: Additional + Animal + Staff + Test + PlanCaseworkDebit.
- [ ] Null-to-zero behavior is preserved for TotalAdditionalCosts, TotalAnimalCosts, TotalStaffCosts, TotalTestCosts, PVSIncome, PlanCaseworkDebit, and TotalPayCosts.
- [ ] `TotalIncome = CustIncome + TransferIncome` preserves legacy null behavior exactly.
- [ ] Conversion does not silently replace `NULL + value` with zero-safe logic unless explicitly approved.
- [ ] `RequiredProfit` continues to map from `Profit`.

## 19.2 Source-read risks

- [ ] `sp_createFPSTotals` source-read behavior is preserved from `tlkpProject`, `qryTotalAdditionalCosts`, `qryTotalAnimalCosts`, `qryTotalStaffCosts`, and `qryTotalTestCosts`.
- [ ] LEFT JOIN semantics are preserved conceptually.
- [ ] `SELECT DISTINCT` de-duplication behavior is preserved.
- [ ] Any denormalized source table used in conversion is validated against original joined-source behavior.

## 19.3 Delete behavior risks

- [ ] `sp_deleteFPSTotals` is treated as delete-only.
- [ ] No archive-before-delete behavior is added unless tracked as approved enhancement.
- [ ] No year filter is introduced into the `sp_deleteFPSTotals` equivalent unless explicitly approved.

## 19.4 Archive delete scope risks

- [ ] `sp_DeleteYearsFPSData` is treated as a year-specific wipe, not retention cleanup.
- [ ] Full delete coverage across legacy archive/reporting tables is preserved.
- [ ] `G_tlkpProject` delete logic is covered.
- [ ] `tlkpYear` delete logic is covered.
- [ ] Validation/audit cleanup is not misrepresented as original legacy behavior.

## 19.5 Archive load scope risks

- [ ] `sp_AddYearsFPSData` is treated as a broad fan-out yearly load.
- [ ] All called legacy sub-procedures are mapped or explicitly deferred.
- [ ] Conversion does not reduce add-years logic to totals-only behavior without approval.
- [ ] `MY_FPSYearTotals` target load is preserved.
- [ ] `MY_tlkpProject_all` target load is preserved.
- [ ] Other MY_* archive tables are accounted for in parity scope.

## 19.6 Write-target risks

- [ ] Legacy business outputs that went to MAB_Archive still go to MAB_Archive.
- [ ] Output targets are not relocated to alternative schemas without approval.
- [ ] `MY_FPSYearTotals` remains the yearly totals archive target.
- [ ] `MY_tlkpProject_all` remains the project-all archive target.

## 19.7 Orchestration risks

- [ ] Previous year is always processed first.
- [ ] Current year full cycle runs only when month > 4.
- [ ] Before May, current year only refreshes `MY_tlkpProject_all`.
- [ ] Database existence checks are preserved before each yearly cycle.
- [ ] Missing FPS year database causes that cycle to be skipped, not treated as fatal by default.
- [ ] Dynamic year-based database selection behavior is preserved conceptually.

## 19.8 Insert vs upsert risks

- [ ] Insert-only legacy procedures are not silently converted to upsert without explicit review.
- [ ] If upsert is introduced, it is documented as enhancement and validated for behavior equivalence.

## 19.9 Validation risks

- [ ] Formula validation is separate from orchestration validation.
- [ ] Archive table count validation includes more than totals.
- [ ] Branch behavior before/after April is explicitly tested.
- [ ] Year-scoped delete and reload behavior is explicitly tested.
- [ ] Parity tests confirm both business calculations and side effects/table coverage.

## 19.10 Sign-off rule

- [ ] Do not claim strict SP-to-.NET parity unless formula behavior, null behavior, delete scope, archive load scope, output targets, and orchestration branch behavior all match legacy.