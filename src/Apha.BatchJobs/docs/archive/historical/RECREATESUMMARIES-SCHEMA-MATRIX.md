# RecreateSummaries Schema Matrix

**Analysis date:** 2026-05-11
**Purpose:** actionable matrix for the PostgreSQL objects needed by the RecreateSummaries process.

**Cloud validation source:**

- `src/Apha.BatchJobs/docs/dbscript/CloudDump/tablesInCloud`
- `src/Apha.BatchJobs/docs/dbscript/CloudDump/viewsInCloud`
- `src/Apha.BatchJobs/docs/dbscript/CloudDump/constraintsInCloud`

## Status legend

- **Present**: object exists in `dbscript/schemas/01fps`
- **Missing**: object is required by the process but no active PostgreSQL schema script was found
- **Needs parity review**: object exists, but its current PostgreSQL definition appears to diverge from the legacy SQL Server behaviour and should be checked before relying on it for parity

## How to use this matrix

Start with rows where:

1. `Status = Missing`
2. `Need SQL from user? = Yes`
3. `Status = Needs parity review`

Those are the places where your SQL code or confirmation is most useful.

---

## 1. Direct Runtime Objects

These are the objects referenced directly by the RecreateSummaries process itself.

| Object | Type | Used by step/process | Status | Need SQL from user? | Notes |
|--------|------|----------------------|--------|---------------------|-------|
| `fps.fpsyeartotals` | Table | Steps 1, 2 | Present | No | Direct delete/rebuild target |
| `fps.tlkpproject` | Table | Steps 2, 3, 5, 15, 16, 17 | Present | No | Core source table |
| `fps.projectmonth` | Table | Steps 3, 10 | Present | No | Missing-month insert target and month-summary driver |
| `fps.timecostcalcs` | Table | Steps 4, 5, 10, 17 | Present | No | Direct delete/rebuild target |
| `fps.tblkpprofitcentre` | Table | Step 5 | Present | No | Time-cost source |
| `fps.profitcentregrade` | Table | Step 5 | Present | No | Time-cost rate source |
| `fps.workgroupgrade` | Table | Step 5 | Present | No | Time-cost mapping source |
| `fps.timecodevalid` | Table | Step 5 | Present | No | Valid workgroup/timecode/project combinations |
| `fps.monthlytime` | Table | Step 5 | Present | No | Actual hours source |
| `fps.tlkpprogram` | Table | Step 5 | Present | No | Sector/charge logic source |
| `fps.projectmonthcasework` | Table | Steps 6, 7, 12, 13 | Present | No | Direct delete/rebuild target |
| `fps.projectmonthfinal` | Table | Steps 8, 13 | Present | No | Final monthly summary target |
| `fps.projectmonth2` | Table | Steps 9, 10, 12, 13 | Present | No | Single-month summary target |
| `fps.projectmonth3` | Table | Steps 11, 12, 13 | Present | No | Cumulative summary target |
| `fps.tblperiod` | Table | Lock check, Step 12 | Present | No | Period lock + period grouping driver |
| `fps.recreatesummaries_log` | Table | Step 14 | Present | No | Audit log target |
| `fps.period_monthlyoutput` | Table | Step 15 | Present | No | Conditional refresh target |
| `fps.costcentre` | Table | Steps 15, 16, 17 | Present | No | Refresh source |
| `fps.monthlyoutput` | Table | Steps 15 and transfer-related view chains | Present | No | Refresh and transfer source |
| `fps.workgroup` | Table | Steps 15, 17 | Present | No | Refresh source |
| `fps.tlkptestreqmt` | Table | Step 15 and transfer/test-cost branches | Present | No | Refresh and view-chain source |
| `fps.period_proj_subcontract` | Table | Step 16 | Present | No | Conditional refresh target |
| `fps.proj_subcontract` | Table | Step 16 and month-summary view chain | Present | No | Refresh source and subcontract branch |
| `fps.period_timecostcalcs` | Table | Step 17 | Present | No | Conditional refresh target |
| `fps.tblwgemployee` | Table | Step 17 and staff/time view chains | Present | No | Refresh source |
| `fps.qrytotaladditionalcosts` | View | Step 2 | Needs parity review | Maybe | Current PG definition includes `fpsyear` grouping not present in legacy SP |
| `fps.qrytotalanimalcosts` | View | Step 2 | Needs parity review | Maybe | Current PG definition includes `fpsyear` grouping not present in legacy SP |
| `fps.qrytotalstaffcosts` | View | Step 2 | Needs parity review | Maybe | Current PG definition includes `fpsyear` grouping not present in legacy SP |
| `fps.qrytotaltestcosts` | View | Step 2 | Needs parity review | Maybe | Current PG definition includes `fpsyear` grouping not present in legacy SP |
| `fps.vpacttblstaff` | View | Step 5 | Present | No | Exists and looks structurally usable |
| `fps.qryprojectmonthcw` | View | Step 7 | Present | No | Exists and matches the expected role |
| `fps.qryjobmonth_subcontracts` | View | Step 10 | Present | No | Exists |
| `fps.qryjobmonth_time` | View | Step 10 | Present | No | Exists |
| `fps.qryjobmonthmilestone` | View | Step 10 | Present | No | Present in cloud (`viewsInCloud`); add local schema script under `dbscript/schemas/01fps/04views` to keep repo and cloud aligned |
| `fps.qryjobmonth_transferstotal` | View | Step 10 | Present | No | Exists |
| `fps.qryjobmonth_invoices` | View | Step 10 | Present | No | Exists |
| `fps.qryjobmonthportfoliosales` | View | Step 10 | Present | No | Exists |
| `fps.qryjobmonth_totprofile` | View | Step 10 | Present | No | Exists |
| `fps.tblkperiodmonth` | View | Step 12 | Present | No | Exists |

---

## 2. Upstream Objects Required By Direct Views

These are not referenced directly by the orchestration SQL, but the direct views depend on them.

| Object | Type | Required via | Status | Need SQL from user? | Notes |
|--------|------|--------------|--------|---------------------|-------|
| `fps.tbladditionalcosts` | Table | `fps.qrytotaladditionalcosts` | Present | No | Source for additional-cost totals |
| `fps.tblanimalreq` | Table | `fps.vprojectanimalplan` | Present | No | Animal plan source |
| `fps.tblanimals` | Table | `fps.vprojectanimalplan` | Present | No | Animal rate source |
| `fps.tblstaffjob` | Table | `fps.vprojectstaffplan` | Present | No | Planned staff/job hours source |
| `fps.tblemployee` | Table | `fps.vprojectstaffplan`, `fps.vpacttblstaff` | Present | No | Employee name/identity source |
| `fps.tbluser_program` | Table | `fps.vtbltestrequ` | Present | No | Security filter input |
| `fps.tblusers` | Table | `fps.vtbltestrequ` | Present | No | Security filter input |
| `fps.testorproduct` | Table | `fps.qryjobmonth_transfers1` | Present | No | Transfer-cost branch source |
| `fps.tblperiodmonth` | Table | `fps.tblkperiodmonth` | Present | No | Period-month mapping source |
| `fps.milestone` | Table | `fps.qrymilestone1` | Present | No | Milestone branch source |
| `fps.tlkptestcapability` | Table | `fps.vpacttlkptestcapability` | Present | No | Transfer-cost branch source |
| `fps.vprojectanimalplan` | View | `fps.qrytotalanimalcosts` | Present | No | Exists |
| `fps.vprojectstaffplan` | View | `fps.qrytotalstaffcosts` | Present | No | Exists |
| `fps.vtbltestrequ` | View | `fps.qrytotaltestcosts` | Needs parity review | Maybe | Applies user/program security filtering via `tbluser_program` + `tblusers` |
| `fps.qryjobmonth_subcontracts1` | View | `fps.qryjobmonth_subcontracts` | Present | No | Exists |
| `fps.qryjobmonth_transferunion` | View | `fps.qryjobmonth_transferstotal` | Present | No | Exists |
| `fps.qryjobmonth_tctransfers` | View | `fps.qryjobmonth_transferunion` | Present | No | Exists |
| `fps.qryjobmonth_transfers1` | View | `fps.qryjobmonth_transferunion` | Present | No | Exists |
| `fps.qrymilestone1` | View | `fps.qryjobmonthmilestone` | Needs parity review | Maybe | Hardcoded `WHERE year = '2003/2004'` in current PG definition |
| `fps.vpacttlkptestcapability` | View | `fps.qryjobmonth_tctransfers` | Present | No | Exists |

---

## 3. Priority Matrix For SQL Help

These are the highest-value items for you to help with first.

| Priority | Object | Why it matters | What I need from you |
|----------|--------|----------------|----------------------|
| High | `fps.qrymilestone1` | Present, but current PG version hardcodes `year = '2003/2004'`, which is a parity risk | Confirm whether that filter is intentional, temporary, or should be replaced |
| High | `fps.qrytotaladditionalcosts` | Present, but PG version groups by `fpsyear` unlike the legacy procedure | Confirm expected PostgreSQL definition for parity target |
| High | `fps.qrytotalanimalcosts` | Same issue as above | Confirm expected PostgreSQL definition for parity target |
| High | `fps.qrytotalstaffcosts` | Same issue as above | Confirm expected PostgreSQL definition for parity target |
| High | `fps.qrytotaltestcosts` | Same issue as above | Confirm expected PostgreSQL definition for parity target |
| Medium | `fps.vtbltestrequ` | Present, but applies user-security filtering that may change parity output | Confirm whether RecreateSummaries should use filtered or unfiltered test requirement data |
| Medium | `fps.qryjobmonthmilestone` | Cloud has it, but repo script is missing; this can cause environment drift | Provide cloud DDL so we can add `dbscript/schemas/01fps/04views/qryjobmonthmilestone.sql` |

---

## 4. Suggested Working Sequence

Use this order when gathering SQL code / confirming parity:

1. `fps.qrymilestone1`
2. `fps.qrytotaladditionalcosts`
3. `fps.qrytotalanimalcosts`
4. `fps.qrytotalstaffcosts`
5. `fps.qrytotaltestcosts`
6. `fps.vtbltestrequ`
7. `fps.qryjobmonthmilestone` (for repo/cloud alignment)

---

## 5. Bottom Line

### Safe to treat as already provisioned

Most base tables and most direct views are present in `dbscript/schemas/01fps`.

### Immediate gap

No missing direct dependency was found in the cloud export.

One repository alignment gap remains:

- `fps.qryjobmonthmilestone` exists in cloud but has no corresponding script file under `dbscript/schemas/01fps/04views`.

### Immediate parity-risk objects

The clearest existing-but-review-needed dependencies are:

- `fps.qrymilestone1`
- `fps.qrytotaladditionalcosts`
- `fps.qrytotalanimalcosts`
- `fps.qrytotalstaffcosts`
- `fps.qrytotaltestcosts`
- `fps.vtbltestrequ`

These are the best places for you to help me with the SQL code next.
