# FPS / MAB_Archive SQL Logic

Step-by-step illustrated walkthrough with hypothetical figures for conversion and validation baseline.

## Purpose

Explain the legacy SQL flow in simple English using worked examples and table-by-table start and end states.

## What this proves

How values are calculated, where data is deleted, where it is reloaded, and what the final tables look like after processing.

## Logic basis

This walkthrough follows the legacy stored procedure behavior in:

- `sp_LoadFromFPS`
- `sp_deleteFPSTotals`
- `sp_createFPSTotals`
- `sp_DeleteYearsFPSData`
- `sp_AddYearsFPSData`

---

## 1. Example Scenario

Assume today is 10 May 2026.

In legacy orchestration:
- previous year (2025) is always processed first,
- month is 5 (> 4), so current year (2026) is also fully processed.

So the legacy run executes a full yearly refresh for 2025 and then 2026.

| Field | Value |
|---|---|
| Run date | 10 May 2026 |
| Previous year | 2025 |
| Current year | 2026 |
| Current month | 5 |
| Legacy decision | month > 4, process both years fully |

---

## 2. Legacy Processing Flow in Plain English

For each processed year:

1. Inside FPS year DB, delete all rows from `FPSYearTotals`.
2. Rebuild `FPSYearTotals` from project and cost sources.
3. In MAB_Archive, delete that year slice across legacy archive/reporting tables.
4. Reload that year slice from FPS into MAB_Archive.

If month > 4, repeat the same full cycle for current year.

---

## 3. Starting Source Data for FPS2025

The following sample rows represent source data used to rebuild totals.

### 3.1 tlkpProject

| ParentProject | Program | CustIncome | TransferIncome | Budget_CVL | Profit | Manager | Customer | Status | PVSIncome | CaseworkDebit |
|---|---|---:|---:|---:|---:|---|---|---|---:|---:|
| P001 | PROG_A | 25000 | 15000 | 50000 | 2000 | John Doe | CUSTOMER_A | Active | 0 | 500 |
| P002 | PROG_B | 30000 | 20000 | 60000 | 3000 | Jane Smith | CUSTOMER_B | Active | 500 | 1000 |
| P003 | PROG_C | 28000 | 18000 | 55000 | 2500 | Bob Jones | CUSTOMER_C | Completed | 250 | 750 |

### 3.2 qryTotalAdditionalCosts

| JobCode | TotalAdditionalCosts |
|---|---:|
| P001 | 1000 |
| P002 | 2000 |
| P003 | 1500 |

### 3.3 qryTotalAnimalCosts

| JobCode | TotalAnimalCosts |
|---|---:|
| P001 | 5000 |
| P002 | 8000 |
| P003 | 6000 |

### 3.4 qryTotalStaffCosts

| JobCode | TotalStaffCosts | TotalPayCosts |
|---|---:|---:|
| P001 | 12000 | 12000 |
| P002 | 15000 | 15000 |
| P003 | 14000 | 14000 |

### 3.5 qryTotalTestCosts

| JobCode | TotalTestCosts |
|---|---:|
| P001 | 3000 |
| P002 | 4000 |
| P003 | 3500 |

---

## 4. Step 1 — sp_deleteFPSTotals

This procedure does one thing only:

- `DELETE FROM FPSYearTotals`

No year filter, no archive-before-delete.

### Example state before delete (FPS2025.dbo.FPSYearTotals)

| ParentProject | Program | TotalCosts | TotalIncome |
|---|---|---:|---:|
| OLD1 | OLD | 999 | 999 |
| OLD2 | OLD | 999 | 999 |

### State after delete

| ParentProject | Program | TotalCosts | TotalIncome |
|---|---|---:|---:|
| (no rows remain) |  |  |  |

---

## 5. Step 2 — sp_createFPSTotals

This procedure rebuilds `FPSYearTotals` by joining `tlkpProject` to cost summary sources.

- Missing cost fields are zeroed where SQL explicitly uses `CASE WHEN ... IS NULL THEN 0`.

Legacy formulas:

- `TotalCosts = Additional + Animal + Staff + Test + PlanCaseworkDebit`
- `TotalIncome = CustIncome + TransferIncome`

### Worked calculations for 2025

- P001: `1000 + 5000 + 12000 + 3000 + 500 = 21500`; income `25000 + 15000 = 40000`
- P002: `2000 + 8000 + 15000 + 4000 + 1000 = 30000`; income `30000 + 20000 = 50000`
- P003: `1500 + 6000 + 14000 + 3500 + 750 = 25750`; income `28000 + 18000 = 46000`

### Final rebuilt FPS2025.dbo.FPSYearTotals

| ParentProject | Program | Addl | Animal | Staff | Test | TotalCosts | TotalIncome |
|---|---|---:|---:|---:|---:|---:|---:|
| P001 | PROG_A | 1000 | 5000 | 12000 | 3000 | 21500 | 40000 |
| P002 | PROG_B | 2000 | 8000 | 15000 | 4000 | 30000 | 50000 |
| P003 | PROG_C | 1500 | 6000 | 14000 | 3500 | 25750 | 46000 |

---

## 6. Step 3 — sp_DeleteYearsFPSData for 2025

This removes the 2025 archive/reporting slice from MAB_Archive.

It is a year-specific wipe, not retention cleanup.

### Example archive state before delete

#### MY_FPSYearTotals

| Year | ParentProject | TotalCosts | TotalIncome |
|---|---|---:|---:|
| 2025 | OLDP1 | 111 | 222 |
| 2025 | OLDP2 | 333 | 444 |

#### MY_tlkpProject_all

| Year | ParentProject | ProjectStatus |
|---|---|---|
| 2025 | OLDP1 | Active |
| 2025 | OLDP2 | Closed |

#### MY_MonthlyOutput

| Year | TestCode | Volume |
|---|---|---:|
| 2025 | T001 | 10 |
| 2025 | T002 | 20 |

After `sp_DeleteYearsFPSData @FPSYear = 2025`, all 2025 rows are removed for that archive slice.

---

## 7. Step 4 — sp_AddYearsFPSData for 2025

This reloads the yearly archive dataset by calling many `sp_AddMY_*` procedures.

### 7.1 sp_AddMY_FPSYearTotals

Source: `FPS2025.dbo.FPSYearTotals`

Target: `MAB_Archive.dbo.MY_FPSYearTotals` with `Year = 2025`

| Year | ParentProject | Program | TotalCosts | TotalIncome |
|---|---|---|---:|---:|
| 2025 | P001 | PROG_A | 21500 | 40000 |
| 2025 | P002 | PROG_B | 30000 | 50000 |
| 2025 | P003 | PROG_C | 25750 | 46000 |

### 7.2 sp_AddMY_tlkpProject_All

Source: `FPS2025.dbo.tlkpProject`

Target: `MAB_Archive.dbo.MY_tlkpProject_all` with `Year = 2025`

| Year | ParentProject | Program | Customer | Manager | ProjectStatus |
|---|---|---|---|---|---|
| 2025 | P001 | PROG_A | CUSTOMER_A | John Doe | Active |
| 2025 | P002 | PROG_B | CUSTOMER_B | Jane Smith | Active |
| 2025 | P003 | PROG_C | CUSTOMER_C | Bob Jones | Completed |

### 7.3 Other sp_AddMY_* procedures

Same pattern:

- read year-scoped rows from FPS2025,
- insert into corresponding `MY_*` archive tables with `Year = 2025`.

---

## 8. Proof of Previous-Year Start and End State (2025)

### Start values for 2025 source inputs

| Project | Addl | Animal | Staff | Test | Casework | CustIncome | TransferIncome |
|---|---:|---:|---:|---:|---:|---:|---:|
| P001 | 1000 | 5000 | 12000 | 3000 | 500 | 25000 | 15000 |
| P002 | 2000 | 8000 | 15000 | 4000 | 1000 | 30000 | 20000 |
| P003 | 1500 | 6000 | 14000 | 3500 | 750 | 28000 | 18000 |

### Final 2025 values after processing

| Project | Final TotalCosts | Final TotalIncome |
|---|---:|---:|
| P001 | 21500 | 40000 |
| P002 | 30000 | 50000 |
| P003 | 25750 | 46000 |

This proves 2025 final totals trace directly to starting values via legacy formulas.

---

## 9. Current-Year Full Processing Because Month = May (2026)

Because current month is 5 (> 4), orchestration also runs full cycle for 2026.

### 9.1 Starting source data for FPS2026

| Project | Addl | Animal | Staff | Test | Casework | CustIncome | TransferIncome |
|---|---:|---:|---:|---:|---:|---:|---:|
| P001 | 1100 | 5500 | 12500 | 3200 | 550 | 26000 | 16000 |
| P002 | 2100 | 8500 | 15500 | 4200 | 1100 | 31000 | 21000 |
| P003 | 1600 | 6500 | 14500 | 3700 | 800 | 29000 | 19000 |

### 9.2 Worked calculations for 2026

- P001: `1100 + 5500 + 12500 + 3200 + 550 = 22850`; income `26000 + 16000 = 42000`
- P002: `2100 + 8500 + 15500 + 4200 + 1100 = 31400`; income `31000 + 21000 = 52000`
- P003: `1600 + 6500 + 14500 + 3700 + 800 = 27100`; income `29000 + 19000 = 48000`

### 9.3 Final FPS2026.dbo.FPSYearTotals

| ParentProject | Program | TotalCosts | TotalIncome |
|---|---|---:|---:|
| P001 | PROG_A | 22850 | 42000 |
| P002 | PROG_B | 31400 | 52000 |
| P003 | PROG_C | 27100 | 48000 |

---

## 10. Final Combined Archive State After Full Run

After both yearly cycles, archive contains refreshed rows for 2025 and 2026.

### 10.1 Final MAB_Archive.dbo.MY_FPSYearTotals

| Year | ParentProject | TotalCosts | TotalIncome |
|---|---|---:|---:|
| 2025 | P001 | 21500 | 40000 |
| 2025 | P002 | 30000 | 50000 |
| 2025 | P003 | 25750 | 46000 |
| 2026 | P001 | 22850 | 42000 |
| 2026 | P002 | 31400 | 52000 |
| 2026 | P003 | 27100 | 48000 |

### 10.2 Final MAB_Archive.dbo.MY_tlkpProject_all

| Year | ParentProject | ProjectStatus | Customer | Manager |
|---|---|---|---|---|
| 2025 | P001 | Active | CUSTOMER_A | John Doe |
| 2025 | P002 | Active | CUSTOMER_B | Jane Smith |
| 2025 | P003 | Completed | CUSTOMER_C | Bob Jones |
| 2026 | P001 | Active | CUSTOMER_A | John Doe |
| 2026 | P002 | Active | CUSTOMER_B | Jane Smith |
| 2026 | P003 | Active | CUSTOMER_C | Bob Jones |

---

## 11. Simple Proof Table — Start Values to Final Values

| Year | Project | Source Cost Inputs | Source Income Inputs | Formula Result | Final FPSYearTotals | Final Archive Row |
|---|---|---|---|---|---|---|
| 2025 | P001 | 1000+5000+12000+3000+500 | 25000+15000 | 21500 / 40000 | Present in FPS2025 | Present in MY_FPSYearTotals 2025 |
| 2025 | P002 | 2000+8000+15000+4000+1000 | 30000+20000 | 30000 / 50000 | Present in FPS2025 | Present in MY_FPSYearTotals 2025 |
| 2025 | P003 | 1500+6000+14000+3500+750 | 28000+18000 | 25750 / 46000 | Present in FPS2025 | Present in MY_FPSYearTotals 2025 |
| 2026 | P001 | 1100+5500+12500+3200+550 | 26000+16000 | 22850 / 42000 | Present in FPS2026 | Present in MY_FPSYearTotals 2026 |
| 2026 | P002 | 2100+8500+15500+4200+1100 | 31000+21000 | 31400 / 52000 | Present in FPS2026 | Present in MY_FPSYearTotals 2026 |
| 2026 | P003 | 1600+6500+14500+3700+800 | 29000+19000 | 27100 / 48000 | Present in FPS2026 | Present in MY_FPSYearTotals 2026 |

---

## 12. Final Plain-English Conclusion

The legacy SQL process is a year-based rebuild-and-reload flow.

For each processed year it:

1. clears `FPSYearTotals`,
2. recalculates totals from source data,
3. deletes that year archive slice from MAB_Archive,
4. reloads that year archive slice from the FPS database.

In this example, both 2025 and 2026 are fully processed because run month is May.
Final archive values are directly traceable to source numbers and legacy formulas.

---

## Strict parity caveats for implementation stories

This walkthrough is numeric and sequence-focused.
For strict SP-to-.NET parity controls (null caveats, branch caveats, table coverage controls, and sign-off checklist), use together with:

- `SCHEDULED-LOAD-FPS-DATA-FLOW-AND-CALCULATIONS.md`
