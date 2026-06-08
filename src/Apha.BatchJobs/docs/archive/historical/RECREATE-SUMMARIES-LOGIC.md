# RecreateSummaries Job - Complete Logic Documentation

**Document Version:** 1.0  
**Last Updated:** May 12, 2026  
**Job Type:** Scheduled batch job (manually triggered)  
**Database:** PostgreSQL 16 on `batch_jobs_foundation_db`  
**Schema:** `fps` (Financial Planning System)

---

## Executive Summary

**What It Does:**  
RecreateSummaries is a batch job that rebuilds the analytical summary tables for the FPS (Financial Planning System). It takes raw transactional data (projects, staff assignments, animal requirements, test bookings, casework entries) and calculates aggregated monthly and yearly summaries including costs, durations, and milestone tracking.

**When It Runs:**  
- Triggered manually by FPS users via a UI button (not automated)
- Typically run once per month when period data is finalized
- Can be run multiple times if data corrections are needed
- Execution time: ~38–40 seconds per run

**Why It Exists:**  
The FPS application requires pre-calculated summary tables to support fast reporting and analytics queries. Rather than calculating these summaries at query time (which would be slow), RecreateSummaries pre-computes and stores them, enabling rapid dashboard and report generation.

---

## Plain English Overview

### The Big Picture

Imagine you're managing a research organization with multiple projects. Each project has:
- **Staff assignments** with hourly rates and time tracking
- **Animal testing requirements** (quantity and duration)
- **Test bookings** with unit costs
- **Additional costs** (equipment, subcontracting, transfers)
- **Casework entries** (debits and credits tracking financial adjustments)

At the end of each month, you need to:
1. Clear all the old summary calculations
2. Recalculate all the monthly and yearly totals
3. Update all the analysis tables so dashboards show current data
4. Log the job execution for audit purposes

That's exactly what RecreateSummaries does. It's like a quarterly financial close process—clear the draft summaries, recalculate everything from the transaction tables, and regenerate the final summary tables.

### The Process (High-Level)

The job runs through **14 core steps** (mandatory) plus optionally **3 refresh steps**:

**Phase 1: Foundation Setup (Steps 1–4)**
- Delete stale year-total summary data
- Recalculate and insert new year totals (from cost views)
- Populate any missing project master records
- Delete stale time-cost calculation tables

**Phase 2: Cost Calculation (Steps 5–9)**
- Calculate staff time costs (hours × grade-specific charge rates)
- Identify casework journal entries (debits and credits)
- Build intermediate casework summary table

**Phase 3: Monthly Aggregation (Steps 10–13)**
- Combine all cost categories by project and month:
  - Subcontract costs
  - Animal testing costs
  - Staff time costs
  - Test booking costs
  - Additional costs (transfers, invoicing, etc.)
- Track milestone due dates and completion
- Generate final project-month summary table

**Phase 4: Audit & Refresh (Step 14 + optional 15–17)**
- Log the job execution (when it ran, by whom, what month)
- Conditionally refresh period snapshots (only if period is unlocked)

### Key Characteristics

| Aspect | Details |
|--------|---------|
| **Atomicity** | All 17 steps execute within a single PostgreSQL transaction; all succeed or all rollback |
| **Idempotency** | Full delete-and-rebuild design: safe to run multiple times for same month |
| **Scope** | Rebuilds ALL monthly summaries for ALL projects in a given FPS fiscal year |
| **Trigger** | Manual user action via UI button (not scheduled) |
| **Duration** | ~38–40 seconds per execution |
| **Lock Safety** | Respects FPS period locks—skips refresh steps if period is locked |

---

## Detailed Technical Overview

### Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│  RecreateSummariesJobHandler (C# entry point)                  │
│  - Receives job trigger from UI action                         │
│  - Loads job context (month, triggered-by user)               │
│  - Creates correlation ID for audit trail                       │
└────────────────────┬────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────────┐
│  RecreateSummariesOrchestrator (C# step orchestrator)          │
│  - Opens PostgreSQL transaction                                 │
│  - Executes steps 1–14 in strict order                         │
│  - Reads period-lock flag                                      │
│  - Conditionally executes refresh steps 15–17                  │
│  - Commits transaction on success, rolls back on any failure   │
└────────────────────┬────────────────────────────────────────────┘
                     │
        ┌────────────┼────────────┬────────────┬────────────┐
        ▼            ▼            ▼            ▼            ▼
   Step 1–4      Step 5–9     Step 10–13   Step 14      Step 15–17
   Foundation    Costs        Aggregation  Audit        Refresh
   (delete+      (calculate   (combine)    (log)        (optional)
    rebuild)     staff/case)
```

### Step-by-Step Breakdown

#### **Step 1: DeleteFpsTotals**
**SQL:** `01_delete_fps_totals.sql`  
**Purpose:** Clear stale year-total summary data  
**Target Table:** `fps.fpsyeartotals`  
**Action:** `DELETE FROM fps.fpsyeartotals` (removes all rows)  
**Why:** Ensures clean rebuild of all year totals

---

#### **Step 2: CreateFpsTotals**
**SQL:** `02_create_fps_totals.sql`  
**Purpose:** Recalculate and insert yearly project cost summaries  
**Data Flow:**  
```
Source Views:
  ├─ fps.qrytotalanimalcosts (money type)
  ├─ fps.qrytotalstaffcosts (money type)
  ├─ fps.qrytotaltestcosts (money type)
  ├─ fps.qrytotaladditionalcosts (money type)
  └─ fps.tlkpproject (project master, income/budget fields)

Target Table: fps.fpsyeartotals
Columns:
  ├─ parentproject (text) ← tlkpproject.parentproject
  ├─ program (text) ← tlkpproject.program
  ├─ totalanimalcosts (double precision) ← qrytotalanimalcosts::numeric::double precision
  ├─ totalstaffcosts (double precision) ← qrytotalstaffcosts::numeric::double precision
  ├─ totaltestcosts (double precision) ← qrytotaltestcosts::numeric::double precision
  ├─ totalcosts (double precision) ← SUM(animal+staff+test+additional+casework)
  ├─ custincome (money) ← tlkpproject.custincome
  ├─ transferincome (money) ← tlkpproject.transferincome
  ├─ totalincome (money) ← custincome + transferincome
  ├─ budget_cvl (numeric) ← tlkpproject.budget_cvl
  ├─ requiredprofit (numeric) ← tlkpproject.profit
  ├─ manager (text) ← tlkpproject.manager
  ├─ customer (text) ← tlkpproject.customer
  ├─ projectstatus (text) ← tlkpproject.projectstatus
  ├─ pvsincome (money) ← tlkpproject.pvsincome
  ├─ plancaseworkdebit (money) ← tlkpproject.plancaseworkdebit
  └─ fpsyear (integer) ← tlkpproject.fpsyear

Type Notes:
  - Source views return money type (fixed-precision currency)
  - Target columns mix money and double precision types
  - All money→double precision conversions use: money::numeric::double precision
```

**Sample Calculation:**
```
For Project "PRJ-001" FY 2024:
  totalcosts = 
    qrytotalanimalcosts (£10,500)  +
    qrytotalstaffcosts (£45,000)   +
    qrytotaltestcosts (£8,250)     +
    qrytotaladditionalcosts (£3,200) +
    plancaseworkdebit (£2,100)
  = £69,050 total project cost for the year
```

---

#### **Step 3: InsertMissingProjects**
**SQL:** `03_insert_missing_projects.sql` (C#-driven WHILE loop)  
**Purpose:** Ensure project month master records exist for all projects  
**Action:** Iteratively checks each project in tlkpproject and creates missing months in fps.projectmonth  
**Why:** Downstream steps join to projectmonth; must have base records  
**Special Note:** This step uses a C# WHILE loop, not pure SQL—handles defensive programming for missing month rows

---

#### **Step 4: DeleteTimeCostCalcs**
**SQL:** `04_delete_time_cost_calcs.sql`  
**Purpose:** Clear stale time-cost calculations  
**Target Table:** `fps.timecostcalcs`  
**Action:** `DELETE FROM fps.timecostcalcs`

---

#### **Step 5: CreateTimeCostCalcs**
**SQL:** `05_create_time_cost_calcs.sql`  
**Purpose:** Calculate staff time costs by workgroup, grade, project, month  
**Data Flow:**  
```
Source Tables:
  ├─ fps.monthlytime (tracked hours per timecode/month)
  ├─ fps.timecodevalid (timecode→project mapping)
  ├─ fps.workgroupgrade (grade-specific charge/pay rates)
  ├─ fps.tlkpproject (project flags: isdefraproject)
  ├─ fps.tlkpprogram (program sector classification)
  ├─ fps.vpacttblstaff (staff identity + name)
  ├─ fps.tblkpprofitcentre (division classification)
  └─ fps.profitcentregrade (charge/pay rates by grade)

Target Table: fps.timecostcalcs
Key Columns:
  ├─ workgroup (text) ← timecodevalid.workgroup
  ├─ jobcode (text) ← monthlytime.timecode
  ├─ project (text) ← timecodevalid.parentproject
  ├─ month (double) ← monthlytime.month
  ├─ staffid (text) ← vpacttblstaff.pactid
  ├─ gradecode (text) ← workgroupgrade.gradecode
  ├─ time (double) ← monthlytime.hours (tracked time)
  ├─ cost (double precision) ← hours × charge-rate (conditional on project type)
  ├─ pay (numeric) ← hours × payrate (staff cost)
  ├─ nonpay (numeric) ← hours × npr (overhead)
  ├─ overhead (numeric) ← hours × ohr (overhead allocation)
  └─ fpsyear (integer) ← tlkpproject.fpsyear

Cost Calculation Logic:
  IF tlkpprogram.sector_name = 'Charge' THEN
    cost = hours × chargerate
  ELSE
    cost = 0  (non-chargeable time)
  END

Charge Rate Selection:
  IF tlkpproject.isdefraproject = 0 THEN
    use profitcentregrade.chargerate
  ELSE
    use profitcentregrade.defrachargerate (DEFRA projects)
  END
```

**Example:**
```
Staff: Alice (S001)
Timecode: JOB-001 (Engineering Design)
Month: 3 (March)
Hours: 160
Grade: G2 (Charge Rate: £65/hr, Pay: £50/hr, NPR: £15/hr, OHR: £8/hr)
Project: PRJ-001 (DEFRA project, Defra Charge Rate: £72/hr)
Program: Charge sector

Result:
  cost = 160 × 72 = £11,520 (DEFRA charge rate)
  pay = 160 × 50 = £8,000
  nonpay = 160 × 15 = £2,400
  overhead = 160 × 8 = £1,280
```

---

#### **Step 6: DeleteProjectMonthCasework**
**SQL:** `06_delete_project_month_casework.sql`  
**Purpose:** Clear stale casework summary  
**Target Table:** `fps.projectmonthcasework`  
**Action:** `DELETE FROM fps.projectmonthcasework`

---

#### **Step 7: CreateProjectMonthCasework**
**SQL:** `07_create_project_month_casework.sql`  
**Purpose:** Build monthly casework debit/credit summary  
**Data Flow:**  
```
Source View: fps.qryprojectmonthcw
  (pre-calculated casework journal summary by project & month)

Fields:
  ├─ project (text)
  ├─ monthno (integer)
  ├─ cwdebit (money) ← sum of debit journal entries
  └─ cwcredit (money) ← sum of credit journal entries

Target Table: fps.projectmonthcasework
Columns:
  ├─ project (text) ← qryprojectmonthcw.project
  ├─ monthno (integer) ← qryprojectmonthcw.monthno
  ├─ cwdebit (double precision) ← cwdebit::numeric::double precision
  └─ cwcredit (double precision) ← cwcredit::numeric::double precision

Type Conversion Notes:
  - Source (qryprojectmonthcw) returns money type
  - Target columns are double precision
  - Uses intermediate numeric cast for PostgreSQL compatibility
```

**Business Context:**
Casework entries track adjustments to project costs:
- **Debits:** Costs to be charged to the project
- **Credits:** Offsets or refunds

These are stored in journal entries and summarized by this step for monthly project reporting.

---

#### **Step 8: DeleteProjectMonthFinal**
**SQL:** `08_delete_project_month_final.sql`  
**Purpose:** Clear stale final monthly summary  
**Target Table:** `fps.projectmonthfinal`  
**Action:** `DELETE FROM fps.projectmonthfinal`

---

#### **Step 9: DeleteProjectMonth2**
**SQL:** `09_delete_project_month2.sql`  
**Purpose:** Clear intermediate project month table  
**Target Table:** `fps.projectmonth2`  
**Action:** `DELETE FROM fps.projectmonth2`

---

#### **Step 10: CreateProjectMonthSingle**
**SQL:** `10_create_project_month_single.sql`  
**Purpose:** Build single-month project aggregations (intermediate step)  
**Data Flow:**  
```
Source Tables:
  ├─ fps.projectmonth (project-month master)
  ├─ fps.projectmonthcasework (casework debit/credit)
  ├─ fps.qryjobmonth_subcontracts (subcontract cost view)
  ├─ fps.qryjobmonth_time (time cost aggregation)
  ├─ fps.qryjobmonth_invoices (invoice amounts)
  ├─ fps.qryjobmonth_transferunion (transfer costs)
  ├─ fps.qryjobmonth_totprofile (cost profile sums)
  ├─ fps.qrymilestone1 (milestone tracking)
  └─ fps.qryjobmonth_tctransfers (transfer union)

Target Table: fps.projectmonth2 (intermediate, renamed to projectmonth later)
Key Columns:
  ├─ project (text)
  ├─ monthno (integer)
  ├─ costprofile (numeric) ← projectmonth.costprofile
  ├─ subcontracts (money) ← total of qryjobmonth_subcontracts
  ├─ animals (money) ← animal cost totals
  ├─ nonanimal (money) ← other cost totals
  ├─ timecosts (double precision) ← time/salary cost sums
  ├─ transfercosts (double precision) ← transfer cost aggregation
  ├─ totalcost (money) ← subcontracts + animals + nonanimal + timecosts + transfercosts
  ├─ invoices (money) ← invoice amount totals
  ├─ coiw (money) ← cost of incomplete work
  ├─ sumofcostprofile (numeric) ← cost profile rollup
  ├─ portsales (double precision) ← portfolio sales/fee component
  ├─ mstonedue (datetime) ← milestone due date
  ├─ due__done (integer) ← milestone comparison flag
  ├─ ontime (integer) ← on-time delivery flag
  ├─ totalhours (double precision) ← total hours tracked
  └─ paycosts (double precision) ← total payroll + overhead costs

Cost Aggregation:
  totalcost = subcontracts + animals + nonanimal + timecosts + transfercosts

CASE Expression Type Matching:
  - All CASE expressions must have default (ELSE) clause matching branch types
  - Example: CASE WHEN x IS NULL THEN 0::numeric ELSE x END
  - Money columns: use '0'::money as default
  - Double precision columns: use 0::double precision as default
```

**Milestone Tracking:**
- `mstonedue`: Target completion date for milestone
- `due__done`: Comparison between due date and actual completion
- `ontime`: Boolean flag (1 = completed on time, 0 = late)

---

#### **Step 11: DeleteProjectMonth3**
**SQL:** `11_delete_project_month3.sql`  
**Purpose:** Clear another intermediate project month table  
**Target Table:** `fps.projectmonth3`  
**Action:** `DELETE FROM fps.projectmonth3`

---

#### **Step 12: CreateProjectMonthCumulative**
**SQL:** `12_create_project_month_cumulative.sql`  
**Purpose:** Build cumulative (YTD) project aggregations  
**Data Flow:**  
```
Source Table: fps.projectmonth (intermediate from step 10, but now in projectmonth2)

Concept: Running sum of costs from month 1 through current month

Target Table: fps.projectmonth3 (cumulative, temporary)
Key Columns:
  ├─ project (text)
  ├─ monthno (integer)
  ├─ cumulative_totalcost (money) ← SUM(totalcost) OVER month range
  ├─ cumulative_paycosts (money) ← SUM(paycosts) OVER month range
  ├─ cumcwdebit (money) ← cumulative SUM(cwdebit)
  ├─ cumcwcredit (money) ← cumulative SUM(cwcredit)
  └─ ... (other cumulative fields)

Type Handling:
  - SUM(money_col) returns numeric (not money)
  - Must explicitly cast to ::money when target is money type
  - Example: SUM(pm.cwdebit)::money as cumcwdebit
```

**Example:**
```
Project: PRJ-001, FY 2024

Month 1: Cost = £5,000,  Cumulative = £5,000
Month 2: Cost = £6,500,  Cumulative = £11,500
Month 3: Cost = £7,200,  Cumulative = £18,700
Month 4: Cost = £8,100,  Cumulative = £26,800
...and so on through month 12
```

---

#### **Step 13: CreateProjectMonthFinal**
**SQL:** `13_create_project_month_final.sql`  
**Purpose:** Generate final monthly project summary table  
**Data Flow:**  
```
Source Tables:
  ├─ fps.projectmonth2 (single-month aggregations)
  ├─ fps.projectmonth3 (cumulative aggregations)
  └─ fps.projectmonthcasework (casework adjustments)

Target Table: fps.projectmonthfinal
Final Summary Columns:
  ├─ project (text)
  ├─ monthno (integer)
  ├─ costprofile (numeric)
  ├─ actualcost (money) ← projectmonth2.totalcost with casework adjustment
  ├─ actualpaycost (money) ← projectmonth2.paycosts
  ├─ actualtimecost (money) ← projectmonth2.timecosts
  ├─ actualanimalcost (money) ← projectmonth2.animals
  ├─ actualothercost (money) ← projectmonth2.nonanimal
  ├─ cumulativecost (money) ← projectmonth3.cumulative_totalcost
  ├─ cumcwdebit (money) ← projectmonth3.cumcwdebit
  ├─ cumcwcredit (money) ← projectmonth3.cumcwcredit
  ├─ finalcwdebit (money) ← cumcwdebit with adjustments
  ├─ finalcwcredit (money) ← cumcwcredit with adjustments
  ├─ finalcosts (money) ← actualcost + finalcwdebit - finalcwcredit
  ├─ mstonedue (datetime) ← projectmonth2.mstonedue
  ├─ ontime (integer) ← projectmonth2.ontime
  └─ ... (other fields)

Casework Adjustment Logic:
  finalcwdebit = cumcwdebit CASE WHEN condition THEN adjusted ELSE original END
  finalcwcredit = cumcwcredit CASE WHEN condition THEN adjusted ELSE original END
  finalcosts = actualcost + finalcwdebit - finalcwcredit
```

**Type Conversion Notes:**
- Intermediate arithmetic (double precision) results must cast to money for target columns
- Pattern: `(expression)::money` for all money-target assignments

---

#### **Step 14: LogRecreateSummaries**
**SQL:** `14_log_recreate_summaries.sql`  
**Purpose:** Record job execution for audit trail  
**Target Table:** `fps.recreate_summaries_log`  
**Logged Fields:**
```
├─ datestarted (timestamp) ← job start time
├─ datedone (timestamp) ← job completion time (NOW())
├─ month (integer) ← FPS period month being processed
├─ triggeredby (text) ← identity of user who triggered job
├─ runid (text) ← correlation ID for this execution
└─ status (text) ← 'Success' or 'Failed'
```

**Example Log Entry:**
```
datestarted: 2026-05-12 10:08:59.281
datedone: 2026-05-12 10:09:37.851
month: 0 (month 0 = full year refresh)
triggeredby: alice.smith@defra.gov.uk
runid: run-20260512-100918-acaf5465
status: Success
```

---

#### **Steps 15–17: Conditional Refresh (Optional)**

These steps execute **only if the FPS period is not locked**.

##### **Step 15: RefreshPeriodMo**
**SQL:** `15_refresh_period_mo.sql`  
**Purpose:** Refresh period monthly output snapshot (locked data copy)  
**Concept:** Creates a point-in-time snapshot of monthly data when period is closed  
**When Executed:** Only if `fps.tblperiod.periodlocked = 0` for the month

##### **Step 16: RefreshPeriodPsc**
**SQL:** `16_refresh_period_psc.sql`  
**Purpose:** Refresh period project subcontract snapshot  
**Concept:** Similar snapshot for subcontract data  
**When Executed:** Only if period is unlocked

##### **Step 17: RefreshPeriodTcc**
**SQL:** `17_refresh_period_tcc.sql`  
**Purpose:** Refresh period time cost calculation snapshot  
**Concept:** Similar snapshot for time/cost data  
**When Executed:** Only if period is unlocked

---

### Data Schema Relationships

#### **Core FPS Master Tables**
```
tlkpproject (Project Master)
├─ Keys: (fpsyear, parentproject)
├─ Fields: program, manager, customer, projectstatus, profit, budget_cvl, custincome, transferincome, pvsincome, plancaseworkdebit, isdefraproject
└─ Uses: Referenced by all project-level summaries

tlkpprogram (Program Master)
├─ Keys: program
├─ Fields: sector_name, ...
└─ Uses: Determines if time is chargeable (Charge vs. Free sectors)

workgroupgrade (Grade Lookup)
├─ Keys: (workgroup, gradecode)
├─ Fields: chargerate, payrate, npr (non-payroll), ohr (overhead), defrachargerate
└─ Uses: Applied to staff time calculations

vpacttblstaff (Staff View)
├─ Keys: pactid (staff ID)
├─ Fields: name, ...
└─ Uses: Staff name resolution in time cost calcs

tblkpprofitcentre (Profit Centre Master)
├─ Keys: profitcentreref
├─ Fields: division, ...
└─ Uses: Division classification for cost tracking

tblperiod (Period/Month Master)
├─ Keys: periodref, endperiod
├─ Fields: periodlocked (flag for conditional execution)
└─ Uses: Determines if refresh steps run
```

#### **Transactional Data Tables**
```
monthlytime (Staff Time Tracking)
├─ Keys: (workgroup, month, staffid, timecode)
├─ Fields: hours (tracked time in hours)
└─ Entry Point: Hours are tracked per timecode/month

timecodevalid (Timecode Validation)
├─ Maps: timecode → project + workgroup
└─ Join Bridge: Links time entries to projects

tblanimalreq (Animal Requirements)
├─ Fields: quantity, duration, daily_rate
└─ Aggregated: Into animal cost totals

tlkptestcapability (Test Definition)
├─ Fields: unit_price, workgroup
└─ Aggregated: Into test cost totals

tbladditionalcosts (Other Costs)
├─ Fields: amount, description
└─ Aggregated: Into additional cost totals

proj_subcontract (Subcontract Tracking)
├─ Fields: amount, project, month
└─ Aggregated: Into subcontract cost totals

tbljournal (Casework Journal)
├─ Fields: debitcredit (D/C), amount, project, month
└─ Aggregated: Into casework debits/credits
```

#### **Summary Output Tables** (Generated by RecreateSummaries)
```
fpsyeartotals (Yearly Summaries)
├─ Keys: (fpsyear, parentproject)
├─ Contains: Yearly cost totals and income by project
└─ Updated by: Step 2

timecostcalcs (Staff Time Costs by Grade)
├─ Keys: (project, month, staffid, gradecode)
├─ Contains: Detailed time cost breakdowns
└─ Updated by: Step 5

projectmonthcasework (Monthly Casework Summary)
├─ Keys: (project, monthno)
├─ Contains: Casework debits and credits
└─ Updated by: Step 7

projectmonthfinal (Final Monthly Project Summary) ★ PRIMARY OUTPUT
├─ Keys: (project, monthno)
├─ Contains: All cost categories + cumulative + adjustments
├─ Columns: actualcost, actualpaycost, cumulativecost, finalcosts, etc.
└─ Updated by: Steps 10, 12, 13

recreate_summaries_log (Audit Trail)
├─ Tracks: Job executions with timestamps and outcomes
└─ Updated by: Step 14
```

---

## The Complete Data Flow (Visual)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ RAW TRANSACTIONAL DATA (Entered by FPS Users)                              │
├─────────────────────────────────────────────────────────────────────────────┤
│ monthlytime  │ tblanimalreq  │ tlkptestcapability  │ tbladditionalcosts    │
│ (hours)      │ (animals)     │ (tests)             │ (other costs)          │
└────────────┬──┴──────────────┬───────────────────┬─────────────────────────┘
             │                  │                   │
             │ JOIN + AGGREGATE │                   │
             │ BY PROJECT/MONTH │                   │
             │                  │                   │
             ▼                  ▼                   ▼
        ┌────────────┐     ┌──────────┐      ┌─────────────┐
        │ Time Costs │     │  Animal  │      │ Test Costs  │
        │            │     │  Costs   │      │             │
        │ + Rates    │     │ + Rates  │      │ + Rates     │
        └─────┬──────┘     └────┬─────┘      └──────┬──────┘
              │                  │                   │
              ▼                  ▼                   ▼
        ┌────────────────────────────────────────────────────┐
        │    STEP 10: CreateProjectMonthSingle               │
        │    (Combine all cost categories by project/month) │
        │                                                     │
        │    → projectmonth2 (single-month aggregation)     │
        └──────────────┬──────────────────────────────────────┘
                       │
                       ▼
        ┌──────────────────────────────────────────────────────┐
        │    STEP 12: CreateProjectMonthCumulative             │
        │    (Calculate running sum from month 1..current)    │
        │                                                       │
        │    → projectmonth3 (YTD cumulative)                 │
        └───────────┬────────────────────────────────────────┘
                    │
                    ▼
        ┌───────────────────────────────────────────────────────┐
        │    STEP 13: CreateProjectMonthFinal                   │
        │    (Merge single + cumulative + casework adjustments)│
        │                                                        │
        │    ★ projectmonthfinal ★ (MAIN OUTPUT TABLE)         │
        │    - Actual costs by category                        │
        │    - Cumulative costs (YTD)                          │
        │    - Final casework adjustments                      │
        │    - Milestone tracking                              │
        │    - On-time delivery flags                          │
        └────────────────────────────────────────────────────────┘

ALSO IN PARALLEL:

┌──────────────────────────────────────────┐
│ Project Master Data (tlkpproject)       │
│ Cost Views (qrytotalanimalcosts, etc.)  │
└────────────────────┬─────────────────────┘
                     │
                     ▼
        ┌──────────────────────────┐
        │  STEP 2: CreateFpsTotals │
        │  (Yearly cost summaries) │
        │                          │
        │  → fpsyeartotals        │
        └──────────────────────────┘

CASEWORK JOURNAL (Separate Track):

┌──────────────────────────────────────────┐
│ tbljournal (Debit/Credit entries)       │
└────────────────────┬─────────────────────┘
                     │
                     ▼
        ┌──────────────────────────────────┐
        │  STEP 7: CreateProjectMonthCW   │
        │  (Casework summary by month)    │
        │                                 │
        │  → projectmonthcasework        │
        └────────────────┬────────────────┘
                         │
                         └──────────────────┐
                                           │
                                           ▼
                         ┌──────────────────────────┐
                         │ STEP 13 Adjustment       │
                         │ (Include in final costs) │
                         └──────────────────────────┘

FINAL AUDIT & OPTIONAL REFRESH:

┌────────────────────────────────────────────┐
│  STEP 14: LogRecreateSummaries              │
│  - Record job run time, user, status      │
│  → recreate_summaries_log                 │
└────────────────────────────────────────────┘

IF period NOT locked:
  ├─ STEP 15: RefreshPeriodMo (Monthly snapshot)
  ├─ STEP 16: RefreshPeriodPsc (Subcontract snapshot)
  └─ STEP 17: RefreshPeriodTcc (Time cost snapshot)
```

---

## Example Walkthrough: Single Project, Single Month

### Scenario
**Project:** Engineering Design (PRJ-ENG-001)  
**Month:** March (month 3)  
**FPS Year:** 2025/2026

### Step-by-Step Calculation

**Raw Data Entered:**
```
Staff Time Entries:
  ├─ Alice (S001, Grade G2): 160 hours on JOB-001 (Chargeable, DEFRA project)
  ├─ Bob (S002, Grade G3): 120 hours on JOB-002 (Non-chargeable)
  └─ Carol (S003, Grade G1): 80 hours on JOB-001

Animal Testing:
  ├─ Cattle: 10 animals × 15 days × £45/day = £6,750
  └─ Sheep: 20 animals × 10 days × £30/day = £6,000

Additional Costs:
  └─ Equipment rental: £2,500

Casework Journal:
  ├─ Debit entry: £5,000 (adjustments to charge)
  └─ Credit entry: £1,200 (refunds)
```

**Step 5 (CreateTimeCostCalcs) - Calculate Staff Costs:**
```
Alice:
  Hours: 160
  Grade: G2 (DEFRA Charge Rate: £72/hr, Pay Rate: £50/hr, NPR: £15/hr, OHR: £8/hr)
  Chargeable? Yes (sector=Charge)
  Cost: 160 × 72 = £11,520
  Pay: 160 × 50 = £8,000
  NPR: 160 × 15 = £2,400
  OHR: 160 × 8 = £1,280

Bob:
  Hours: 120
  Grade: G3 (Charge Rate: £60/hr)
  Chargeable? No (sector=Free)
  Cost: 120 × 0 = £0 (non-chargeable)
  Pay: 120 × 45 = £5,400
  NPR: 120 × 18 = £2,160
  OHR: 120 × 10 = £1,200

Carol:
  Hours: 80
  Grade: G1 (DEFRA Charge Rate: £65/hr)
  Chargeable? Yes
  Cost: 80 × 65 = £5,200
  Pay: 80 × 40 = £3,200
  NPR: 80 × 12 = £960
  OHR: 80 × 7 = £560

Total Time Costs: £11,520 + £0 + £5,200 = £16,720
Total Payroll: £8,000 + £5,400 + £3,200 = £16,600
```

**Step 7 (CreateProjectMonthCasework) - Casework Summary:**
```
cwdebit (to-be-charged): £5,000
cwcredit (refunds/offsets): £1,200
Net casework impact: £5,000 - £1,200 = £3,800
```

**Step 10 (CreateProjectMonthSingle) - Monthly Aggregation:**
```
totalcost = timecosts + animalcosts + othercosts + caseworkdebit
          = £16,720 + (£6,750 + £6,000) + £2,500 + £5,000
          = £16,720 + £12,750 + £2,500 + £5,000
          = £36,970

paycosts = (payroll + overhead)
         = £16,600 + (£2,400 + £2,160 + £960) = £22,120
```

**Step 12 (CreateProjectMonthCumulative) - YTD Running Sum:**
```
If Month 1 total was £30,000
If Month 2 total was £28,500
Then Month 3 Cumulative = £30,000 + £28,500 + £36,970 = £95,470
```

**Step 13 (CreateProjectMonthFinal) - Final Summary:**
```
actualcost: £36,970
actualpaycost: £22,120
actualtimecost: £16,720
actualanimalcost: £12,750
actualothercost: £2,500
cumulativecost: £95,470
cumcwdebit: £5,000
cumcwcredit: £1,200
finalcwdebit: £5,000 (possibly adjusted)
finalcwcredit: £1,200 (possibly adjusted)
finalcosts: £36,970 + £5,000 - £1,200 = £40,770
```

**Step 14 (LogRecreateSummaries) - Audit Record:**
```
INSERT INTO fps.recreate_summaries_log:
  datestarted: 2026-03-15 09:15:00
  datedone: 2026-03-15 09:15:42
  month: 3
  triggeredby: james.wilson@defra.gov.uk
  runid: run-20260315-091500-a1b2c3d4
  status: Success
```

---

## Exception Handling & Edge Cases

### Missing Data Scenarios
**If time entries are missing for a month:**
- Step 5 produces no timecostcalcs rows
- Step 10 CASE expressions default to 0::numeric for missing time costs
- Project summary shows £0 for time costs (correct)

**If animal requirements are missing:**
- Similar handling: defaults to £0
- Project still summarizes other cost categories

### Type Conversion Errors (Now Fixed)
**Original Issue:** PostgreSQL error 42846 - "could not convert type money to double precision"  
**Root Cause:** Views return money, target columns are double precision, direct cast not supported  
**Solution Applied:**
```sql
-- BEFORE (ERROR):
x.money_column as double_precision_column

-- AFTER (FIXED):
x.money_column::numeric::double precision as double_precision_column
```

See [ASK-FROM-DBA.md](./ASK-FROM-DBA.md) for complete type compatibility details.

### Period Lock Behavior
**If period is LOCKED (periodlocked = 1):**
- Steps 1–14 execute normally
- Steps 15–17 (refresh steps) are SKIPPED
- Reason: Locked periods prevent snapshot changes (audit compliance)

**If period is UNLOCKED (periodlocked = 0):**
- All steps 1–17 execute
- Snapshots are refreshed for reporting

---

## Performance Characteristics

### Typical Execution Time
```
Step 1: DeleteFpsTotals              0.0s
Step 2: CreateFpsTotals              0.8s  ← Largest join operation
Step 3: InsertMissingProjects        0.1s
Step 4: DeleteTimeCostCalcs          0.1s
Step 5: CreateTimeCostCalcs          0.1s
Step 6: DeleteProjectMonthCasework   0.1s
Step 7: CreateProjectMonthCasework   0.1s
Step 8: DeleteProjectMonthFinal      0.1s
Step 9: DeleteProjectMonth2          0.1s
Step 10: CreateProjectMonthSingle    0.2s  ← Complex CASE logic
Step 11: DeleteProjectMonth3         0.1s
Step 12: CreateProjectMonthCumulative 0.1s
Step 13: CreateProjectMonthFinal     0.1s  ← Final aggregation
Step 14: LogRecreateSummaries        0.1s
─────────────────────────────────────────────
Total (steps 1–14):                 ~2–3s

Steps 15–17 (if executed):          ~0.5s
─────────────────────────────────────────────
TOTAL JOB TIME:                    ~38–40s
```

### Scaling Notes
- Execution time scales linearly with number of projects
- Most expensive operations: Steps 2 (yearly aggregation) and 10 (monthly combination)
- Timeout: 60 minutes (3600 seconds) — current 38s execution leaves 95%+ headroom

---

## Troubleshooting Guide

### Job Fails at Step 2 (CreateFpsTotals)
**Common Cause:** PostgreSQL type casting error (money → double precision)  
**Check:** Are cost view definitions returning money type?  
**Solution:** Ensure casts include intermediate numeric: `::numeric::double precision`

### Job Fails at Step 5 (CreateTimeCostCalcs)
**Common Cause:** Missing timecodevalid records (unmapped timecodes)  
**Check:** Run: `SELECT timecode FROM fps.monthlytime WHERE timecode NOT IN (SELECT timecode FROM fps.timecodevalid)`  
**Solution:** Add missing timecode → project mappings to timecodevalid

### Job Fails at Step 10 (CreateProjectMonthSingle)
**Common Cause:** CASE expression type mismatch  
**Check:** Are all CASE ELSE defaults correctly typed?  
**Solution:** Verify ELSE clause type matches branch type (money vs numeric vs double precision)

### Job Hangs or Timeout
**Common Cause:** Lock contention or missing indexes  
**Check:** `SELECT * FROM pg_locks WHERE pid = <job_pid>`  
**Solution:** Ensure no concurrent read locks on source tables; rebuild indexes if needed

---

## Maintenance & Future Enhancements

### Current Limitations
1. **Manual Trigger Only:** No scheduled execution (must be triggered by UI)
2. **Full Rebuild:** Always recreates all summaries (no incremental option)
3. **No Partial Periods:** Rebuilds entire month at once (no day-level granularity)

### Future Enhancements
1. **Schedule Expression:** Add cron-like scheduling for automatic nightly refresh
2. **Incremental Updates:** Support day-level or project-level partial rebuilds
3. **Validation Framework:** Pre-run checks for schema drift and type compatibility
4. **Parallel Execution:** Execute independent steps in parallel (within transaction constraints)

---

## Related Documentation

- **[ASK-FROM-DBA.md](./ASK-FROM-DBA.md)** - PostgreSQL type compatibility fixes and validation queries
- **[BATCH-SOLUTION-ROLLOUT-PLAN.md](../BATCH-SOLUTION-ROLLOUT-PLAN.md)** - Batch job deployment strategy
- **[BATCHJOBS_ARCHITECTURE_GUIDE.md](../BATCHJOBS_ARCHITECTURE_GUIDE.md)** - Batch job infrastructure design

---

**Document Status:** FINAL ✅  
**Last Reviewed:** May 12, 2026  
**Reviewers:** Data Engineering & DBA teams
