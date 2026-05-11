# RecreateSummaries Table and View Inventory

**Analysis date:** 2026-05-11
**Process analysed:** `sp_RecreateSummaries` / `.NET RecreateSummariesJobHandler`
**Purpose:** identify the tables and views that must exist for the RecreateSummaries process to run in PostgreSQL.

## Scope

This document combines three sources:

1. Reverse-engineered legacy dependency analysis in `dbscript/docs/recreatesummaries/`
2. The converted PostgreSQL SQL files under `src/Apha.BatchJobs/Apha.BatchJobs.Infrastructure/Sql/RecreateSummaries/`
3. Current PostgreSQL schema/view scripts under `dbscript/schemas/01fps/`

The inventory is split into:

- **Direct execution footprint**: objects referenced directly by the RecreateSummaries SQL steps
- **Expanded upstream footprint**: additional tables/views required because the direct views depend on them
- **Schema gaps / follow-up checks**: objects referenced by the process but not currently found in the active PostgreSQL schema scripts

## Executive Summary

### Direct execution footprint

- **25 direct tables** are referenced by the job SQL
- **14 direct views** are referenced by the job SQL
- **1 direct required view is missing from the current `01fps` schema scripts**: `fps.qryjobmonthmilestone`

### Expanded upstream footprint

To make the process actually runnable, the direct views pull in additional upstream objects:

- **11 additional tables** found through current view definitions
- **9 additional views** found through current view definitions

## 1. Direct Execution Footprint

These are the objects referenced directly by the RecreateSummaries steps themselves.

### 1.1 Direct Tables Required

| Table | Role in process | Step(s) |
|-------|-----------------|---------|
| `fps.fpsyeartotals` | Delete + rebuild year totals | 1, 2 |
| `fps.tlkpproject` | Source project metadata for totals, casework, refresh steps | 2, 3, 5, 15, 16, 17 |
| `fps.projectmonth` | Insert missing month rows, driver for month-level summaries | 3, 10 |
| `fps.timecostcalcs` | Delete + rebuild time cost calculations, source for month summaries and period refresh | 4, 5, 10, 17 |
| `fps.tblkpprofitcentre` | Source for time cost calc division | 5 |
| `fps.profitcentregrade` | Source for charge/pay/non-pay/overhead rates | 5 |
| `fps.workgroupgrade` | Source for workgroup/grade mapping | 5 |
| `fps.timecodevalid` | Source for valid time code/project/workgroup combinations | 5 |
| `fps.monthlytime` | Source for actual hours in time cost calculations | 5 |
| `fps.tlkpprogram` | Source for sector/charge logic | 5 |
| `fps.projectmonthcasework` | Delete + rebuild casework debit/credit, used in cumulative/final summaries | 6, 7, 12, 13 |
| `fps.projectmonthfinal` | Delete + rebuild final monthly summary output | 8, 13 |
| `fps.projectmonth2` | Delete + rebuild single-month summary dataset | 9, 10, 12, 13 |
| `fps.projectmonth3` | Delete + rebuild cumulative summary dataset | 11, 12, 13 |
| `fps.tblperiod` | Period lock check and period/month grouping driver | lock check, 12 |
| `fps.recreatesummaries_log` | Audit log target | 14 |
| `fps.period_monthlyoutput` | Conditional period snapshot target | 15 |
| `fps.costcentre` | Source for period refresh output fields | 15, 16, 17 |
| `fps.monthlyoutput` | Source for period monthly output refresh | 15 |
| `fps.workgroup` | Source for period monthly output and time cost refresh | 15, 17 |
| `fps.tlkptestreqmt` | Source for period monthly output and transfer-cost related calculations | 15 |
| `fps.period_proj_subcontract` | Conditional period subcontract snapshot target | 16 |
| `fps.proj_subcontract` | Source for subcontract and transfer-cost logic | 16 |
| `fps.period_timecostcalcs` | Conditional period time-cost snapshot target | 17 |
| `fps.tblwgemployee` | Source for period time-cost refresh | 17 |

### 1.2 Direct Views Required

| View | Role in process | Step(s) | Current `01fps` script status |
|------|-----------------|---------|-------------------------------|
| `fps.qrytotaladditionalcosts` | Additional-cost totals for year totals rebuild | 2 | Present |
| `fps.qrytotalanimalcosts` | Animal-cost totals for year totals rebuild | 2 | Present |
| `fps.qrytotalstaffcosts` | Staff-cost totals for year totals rebuild | 2 | Present |
| `fps.qrytotaltestcosts` | Test-cost totals for year totals rebuild | 2 | Present |
| `fps.vpacttblstaff` | Staff/person lookup for time cost rebuild | 5 | Present |
| `fps.qryprojectmonthcw` | Derived casework debit/credit per project/month | 7 | Present |
| `fps.qryjobmonth_subcontracts` | Subcontract totals for month summary | 10 | Present |
| `fps.qryjobmonth_time` | Aggregated time/pay totals for month summary | 10 | Present |
| `fps.qryjobmonthmilestone` | Milestone counts and on-time metrics for month summary | 10 | **Missing from `dbscript/schemas/01fps/04views`** |
| `fps.qryjobmonth_transferstotal` | Transfer-cost totals for month summary | 10 | Present |
| `fps.qryjobmonth_invoices` | Invoice/COIW totals for month summary | 10 | Present |
| `fps.qryjobmonthportfoliosales` | Portfolio sales totals for month summary | 10 | Present |
| `fps.qryjobmonth_totprofile` | Total project cost profile for month summary | 10 | Present |
| `fps.tblkperiodmonth` | Period-to-month mapping for cumulative summary | 12 | Present |

## 2. Expanded Upstream Footprint

These objects are not referenced directly by the stored-procedure orchestration, but they are required because the direct views depend on them.

### 2.1 Additional Tables Required Through View Definitions

| Table | Needed via | Notes |
|-------|------------|-------|
| `fps.tbladditionalcosts` | `fps.qrytotaladditionalcosts` | Source of additional costs |
| `fps.tblanimalreq` | `fps.vprojectanimalplan` -> `fps.qrytotalanimalcosts` | Source of animal plan rows |
| `fps.tblanimals` | `fps.vprojectanimalplan` -> `fps.qrytotalanimalcosts` | Source of animal rate data |
| `fps.tblstaffjob` | `fps.vprojectstaffplan` -> `fps.qrytotalstaffcosts` | Source of planned staff hours/job rows |
| `fps.tblemployee` | `fps.vprojectstaffplan`, `fps.vpacttblstaff` | Employee name / identity source |
| `fps.tbluser_program` | `fps.vtbltestrequ` | Security-filtered program membership |
| `fps.tblusers` | `fps.vtbltestrequ` | Security-filtered user identity lookup |
| `fps.testorproduct` | `fps.qryjobmonth_transfers1` -> `fps.qryjobmonth_transferunion` -> `fps.qryjobmonth_transferstotal` | Transfer-cost branch |
| `fps.tblperiodmonth` | `fps.tblkperiodmonth` | End-month to month mapping |
| `fps.milestone` | `fps.qrymilestone1` -> `fps.qryjobmonthmilestone` | Milestone branch |
| `fps.tlkptestcapability` | `fps.vpacttlkptestcapability` -> `fps.qryjobmonth_tctransfers` | Transfer-cost branch |

### 2.2 Additional Views Required Through View Definitions

| View | Required by | Notes |
|------|-------------|-------|
| `fps.vprojectanimalplan` | `fps.qrytotalanimalcosts` | Expanded animal-cost source |
| `fps.vprojectstaffplan` | `fps.qrytotalstaffcosts` | Expanded staff-cost source |
| `fps.vtbltestrequ` | `fps.qrytotaltestcosts` | Expanded test-cost source |
| `fps.qryjobmonth_subcontracts1` | `fps.qryjobmonth_subcontracts` | Subcontract split by account code |
| `fps.qryjobmonth_transferunion` | `fps.qryjobmonth_transferstotal` | Union of two transfer-cost branches |
| `fps.qryjobmonth_tctransfers` | `fps.qryjobmonth_transferunion` | Transfer-cost branch based on capability portfolio |
| `fps.qryjobmonth_transfers1` | `fps.qryjobmonth_transferunion` | Transfer-cost branch based on buyer |
| `fps.qrymilestone1` | `fps.qryjobmonthmilestone` | Milestone aggregation source |
| `fps.vpacttlkptestcapability` | `fps.qryjobmonth_tctransfers` | Simple capability wrapper over `tlkptestcapability` |

## 3. View-Chain Notes By Process Area

### 3.1 Totals rebuild (`CreateFpsTotals`)

Direct dependencies:

- `fps.fpsyeartotals`
- `fps.tlkpproject`
- `fps.qrytotaladditionalcosts`
- `fps.qrytotalanimalcosts`
- `fps.qrytotalstaffcosts`
- `fps.qrytotaltestcosts`

Expanded view chain:

- `fps.qrytotaladditionalcosts` -> `fps.tbladditionalcosts`
- `fps.qrytotalanimalcosts` -> `fps.vprojectanimalplan` -> `fps.tblanimalreq`, `fps.tblanimals`, `fps.tlkpproject`
- `fps.qrytotalstaffcosts` -> `fps.vprojectstaffplan` -> `fps.tblwgemployee`, `fps.tblstaffjob`, `fps.tblemployee`, `fps.workgroupgrade`, `fps.profitcentregrade`, `fps.tlkpproject`, `fps.tlkpprogram`
- `fps.qrytotaltestcosts` -> `fps.vtbltestrequ` -> `fps.tlkptestreqmt`, `fps.tlkpproject`, `fps.tlkpprogram`, `fps.tbluser_program`, `fps.tblusers`

### 3.2 Time cost rebuild (`CreateTimeCostCalcs`)

Direct dependencies:

- `fps.timecostcalcs`
- `fps.tblkpprofitcentre`
- `fps.profitcentregrade`
- `fps.workgroupgrade`
- `fps.timecodevalid`
- `fps.vpacttblstaff`
- `fps.monthlytime`
- `fps.tlkpproject`
- `fps.tlkpprogram`

Expanded view chain:

- `fps.vpacttblstaff` -> `fps.tblemployee`, `fps.tblwgemployee`

### 3.3 Month summary build (`CreateProjectMonthSingle`)

Direct dependencies:

- `fps.projectmonth`
- `fps.projectmonth2`
- `fps.qryjobmonth_subcontracts`
- `fps.qryjobmonth_time`
- `fps.qryjobmonthmilestone`
- `fps.qryjobmonth_transferstotal`
- `fps.qryjobmonth_invoices`
- `fps.qryjobmonthportfoliosales`
- `fps.qryjobmonth_totprofile`

Expanded view chain:

- `fps.qryjobmonth_subcontracts` -> `fps.qryjobmonth_subcontracts1` -> `fps.proj_subcontract`
- `fps.qryjobmonth_time` -> `fps.timecostcalcs`
- `fps.qryjobmonthmilestone` -> `fps.qrymilestone1` -> `fps.milestone`
- `fps.qryjobmonth_transferstotal` -> `fps.qryjobmonth_transferunion`
- `fps.qryjobmonth_transferunion` -> `fps.qryjobmonth_tctransfers`, `fps.qryjobmonth_transfers1`
- `fps.qryjobmonth_tctransfers` -> `fps.monthlyoutput`, `fps.tlkptestreqmt`, `fps.vpacttlkptestcapability` -> `fps.tlkptestcapability`
- `fps.qryjobmonth_transfers1` -> `fps.testorproduct`, `fps.tlkptestreqmt`, `fps.monthlyoutput`
- `fps.qryjobmonth_invoices` -> `fps.proj_invoice`
- `fps.qryjobmonthportfoliosales` -> `fps.tlkptestreqmt`, `fps.tlkptestcapability`, `fps.monthlyoutput`
- `fps.qryjobmonth_totprofile` -> `fps.projectmonth`

### 3.4 Cumulative / final summary build

Direct dependencies:

- `fps.projectmonth3`
- `fps.tblperiod`
- `fps.tblkperiodmonth`
- `fps.projectmonth2`
- `fps.projectmonthcasework`
- `fps.projectmonthfinal`

Expanded view chain:

- `fps.tblkperiodmonth` -> `fps.tblperiodmonth`, `fps.tblperiod`

### 3.5 Conditional period refresh

Direct dependencies:

- `fps.period_monthlyoutput`, `fps.period_proj_subcontract`, `fps.period_timecostcalcs`
- `fps.tlkpproject`
- `fps.costcentre`
- `fps.monthlyoutput`
- `fps.workgroup`
- `fps.tlkptestreqmt`
- `fps.proj_subcontract`
- `fps.tblwgemployee`
- `fps.timecostcalcs`

No additional second-level views are required for these refresh statements; they read base tables directly.

## 4. Required-But-Missing or Needs-Verification

### 4.1 Missing in active PostgreSQL schema scripts

| Object | Type | Status | Evidence |
|--------|------|--------|----------|
| `fps.qryjobmonthmilestone` | View | Missing from `dbscript/schemas/01fps/04views` | Present only in reverse-engineered docs under `dbscript/docs/recreatesummaries/sp_RecreateSummaries_details.md` |

Reverse-engineered legacy SQL for the missing view:

```sql
CREATE VIEW [dbo].[qryJobMonthMilestone]
AS
SELECT DISTINCT
    Project,
    DueMonth,
    COUNT(MilestoneRef) AS MstoneDue,
    SUM(CompleteFlag) AS Due__Done,
    SUM(OnTimeFlag) AS OnTime
FROM qryMilestone1
GROUP BY Project, DueMonth
```

### 4.2 Semantics to verify during parity testing

These objects exist, but their PostgreSQL definitions already introduce context or filtering that may matter for strict parity:

| Object | Observation |
|--------|-------------|
| `fps.qrytotaladditionalcosts`, `fps.qrytotalanimalcosts`, `fps.qrytotalstaffcosts`, `fps.qrytotaltestcosts` | Current PostgreSQL versions carry `fpsyear` columns/groups not present in the original SQL Server procedure text |
| `fps.vtbltestrequ` | Current PostgreSQL view applies user/program security filtering through `tbluser_program` and `tblusers` |
| `fps.qrymilestone1` | Current PostgreSQL view hardcodes `WHERE year = '2003/2004'` |

These do not change the inventory, but they are important parity risks.

## 5. Minimum Provisioning Recommendation

If the goal is to make RecreateSummaries runnable in PostgreSQL, the minimum object set should be provisioned in this order:

1. All **25 direct tables**
2. All **14 direct views**
3. The **missing direct view** `fps.qryjobmonthmilestone`
4. All **upstream views and tables** used by those direct views
5. Any user/security support tables used by filtered views (`fps.tbluser_program`, `fps.tblusers`)

## 6. Bottom Line

For RecreateSummaries, the process depends on more than the obvious output tables.

- The **core runtime footprint** is 25 tables + 14 views
- The **expanded practical footprint** includes additional source tables/views behind totals, milestone, transfer, and test-cost calculations
- The **main schema gap currently visible** is `fps.qryjobmonthmilestone`

If you want the next step, the clean follow-on is to convert this inventory into a **checklist against `dbscript/schemas/01fps`** with `Present / Missing / Needs parity review` for each object.
