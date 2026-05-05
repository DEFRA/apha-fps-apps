# MAB Archive Data Mapping (Source to Target)

## Purpose
This document shows exactly:
- where data is picked from (source tables/views in fps schema), and
- where data is written to (target tables in mabarchive schema).

It is based on the current implementation of the MAB Archive job.

## Big picture flow
1. Rebuild totals in source area:
   fps.fpsyeartotals is rebuilt from fps project and cost views.
2. Delete old archive rows for the target year.
3. Load 24 archive targets in fixed order.
4. In January to April, do an extra partial refresh for my_tlkpproject_all only (current year).

## Source of truth model (simple)

- During the day, business teams update operational data in fps source tables.
- At night (8 PM), the job recalculates totals and republishes clean reporting data.
- So operational truth is in fps, and reporting truth for consumers is in mabarchive after the nightly run.

## Story with 3 dates (simple lesson)

### Date 1: 2026-04-28 (start of day)

At the start of the day, assume both schemas are aligned.

Example for project P100 (year 2026):
- fps.fpsyeartotals totalcosts = 100000
- mabarchive.my_fpsyeartotals totalcosts = 100000

So morning state is: same totals in source totals and archive totals.

### During the day (what changed and where it was stored)

Now users work in source systems and rows change in fps tables such as:
- fps.tbladditionalcosts (extra cost lines)
- fps.tblanimalreq (animal requirement changes)
- fps.tblstaffjob and fps.timecostcalcs (staff effort and cost impact)
- fps.proj_invoice and fps.proj_subcontract (invoice/subcontract updates)
- fps.tlkpproject (project-level fields like income/status)

Example daytime changes for P100:
- Additional costs increase by 3000
- Staff costs increase by 5000
- Test costs decrease by 1000

Net effect expected in totals = +7000.

### Date 2: 2026-04-28 at 8:00 PM (nightly retotal and republish)

Now the nightly MAB Archive job runs.

Step 1 (inside fps):
- Pick data from fps.tlkpproject + qrytotal* views.
- Recalculate totals into fps.fpsyeartotals.
- For P100, totalcosts changes from 100000 to 107000.

Step 2 (refresh archive for year):
- Delete old year rows in mabarchive target tables.
- Insert revised rows from fps sources.
- For P100 in mabarchive.my_fpsyeartotals, totalcosts becomes 107000.

Why delete first?
- It removes stale rows from earlier runs.
- Then fresh insert gives a clean, consistent year slice.

### Date 3: 2026-04-29 (next morning)

By morning, downstream systems reading mabarchive see revised figures.

Example now visible to consumers:
- Previous reported totalcosts: 100000
- Revised reported totalcosts after nightly run: 107000

So the story is:
- Start of day: fps and mabarchive matched.
- Daytime: source rows changed in fps transactional/detail tables.
- Night: totals were recalculated, old year slice deleted, revised data reloaded.
- Next morning: mabarchive is updated and ready for reporting consumers.

## What is picked, totalled, and aggregated

- Picked (examples): project, cost, staff, test, invoice, subcontract, monthly output/time data from fps tables.
- Totalled: fps.fpsyeartotals is recomputed from tlkpproject + qrytotaladditionalcosts + qrytotalanimalcosts + qrytotalstaffcosts + qrytotaltestcosts.
- Aggregated/published for reporting: loaded into mabarchive.my_fpsyeartotals and other my_* archive tables (24 loaders in total).

## A. Source totals rebuild (inside fps schema)

| Step | Pick data from | Put data into | Year filter |
|---|---|---|---|
| Rebuild totals | fps.tlkpproject + fps.qrytotaladditionalcosts + fps.qrytotalanimalcosts + fps.qrytotalstaffcosts + fps.qrytotaltestcosts | fps.fpsyeartotals | yes (fpsyear = selected year) |

Important:
- The job clears fps.fpsyeartotals first, then inserts rebuilt rows.
- This is source-to-source preparation before archive copy.

## B. Yearly delete scope (archive cleanup before load)

Delete condition for most tables: year = selected year.

Deleted targets:
- mabarchive.my_timecostcalcs
- mabarchive.my_monthlyoutput
- mabarchive.my_monthlytime
- mabarchive.my_projectmonthfinal
- mabarchive.my_proj_invoice
- mabarchive.my_proj_subcontract
- mabarchive.my_tbladditionalcosts
- mabarchive.my_tblanimalreq
- mabarchive.my_tblcontract
- mabarchive.my_tblstaffjob
- mabarchive.my_tlkptestreqmt
- mabarchive.my_testorproduct
- mabarchive.my_staff
- mabarchive.my_workgroup
- mabarchive.my_tblprofitcentre
- mabarchive.my_profitcentregrade
- mabarchive.my_workgroupgrade
- mabarchive.my_tblanimals
- mabarchive.my_tlkpprogram
- mabarchive.my_tlkpproject
- mabarchive.my_tlkpproject_all
- mabarchive.my_fpsyeartotals
- mabarchive.tlkpyear

Special delete rule:
- mabarchive.g_tlkpproject is deleted by parentproject list coming from fps.tlkpproject for selected fpsyear.
- It is project-based delete, not direct year column delete.

## C. Full yearly load map (24 loaders)

| # | Pick data from (source) | Put data into (target) | Year filter rule |
|---|---|---|---|
| 1 | fps.tlkpprogram | mabarchive.my_tlkpprogram | fpsyear = year |
| 2 | fps.tlkpproject | mabarchive.g_tlkpproject | fpsyear = year (grouped) |
| 3 | fps.tlkpproject | mabarchive.my_tlkpproject | fpsyear = year |
| 4 | fps.fpsyeartotals | mabarchive.my_fpsyeartotals | fpsyear = year |
| 5 | fps.monthlyoutput | mabarchive.my_monthlyoutput | fpsyear = year |
| 6 | fps.monthlytime | mabarchive.my_monthlytime | fpsyear = year |
| 7 | fps.proj_invoice | mabarchive.my_proj_invoice | fpsyear = year |
| 8 | fps.proj_subcontract | mabarchive.my_proj_subcontract | fpsyear = year |
| 9 | fps.projectmonthfinal | mabarchive.my_projectmonthfinal | fpsyear = year |
| 10 | fps.tbladditionalcosts | mabarchive.my_tbladditionalcosts | fpsyear = year |
| 11 | fps.tblanimalreq | mabarchive.my_tblanimalreq | fpsyear = year |
| 12 | fps.tblcontract | mabarchive.my_tblcontract | fpsyear = year |
| 13 | fps.tblstaffjob | mabarchive.my_tblstaffjob | fpsyear = year |
| 14 | fps.timecostcalcs | mabarchive.my_timecostcalcs | fpsyear = year |
| 15 | fps.tlkptestreqmt | mabarchive.my_tlkptestreqmt | fpsyear = year |
| 16 | fps.tbldb_variables | mabarchive.tlkpyear | no fpsyear in source; row where db_var_name = month |
| 17 | fps.workgroupgrade | mabarchive.my_workgroupgrade | fpsyear = year |
| 18 | fps.profitcentregrade | mabarchive.my_profitcentregrade | fpsyear = year |
| 19 | fps.tblkpprofitcentre | mabarchive.my_tblprofitcentre | no fpsyear in source; all rows copied with selected year stamped |
| 20 | fps.testorproduct | mabarchive.my_testorproduct | fpsyear = year |
| 21 | fps.tblwgemployee join fps.tblemployee | mabarchive.my_staff | fps.tblwgemployee.fpsyear = year |
| 22 | fps.workgroup | mabarchive.my_workgroup | fpsyear = year |
| 23 | fps.tblanimals | mabarchive.my_tblanimals | fpsyear = year |
| 24 | fps.tlkpproject | mabarchive.my_tlkpproject_all | fpsyear = year |

## D. Partial refresh map (January to April behavior)

| Step | Pick data from | Put data into | Rule |
|---|---|---|---|
| Partial refresh only | fps.tlkpproject | mabarchive.my_tlkpproject_all | delete target year rows then reload for current year |

Only this one table is refreshed in the partial cycle.

## E. Year availability check

Before year processing, the job checks:
- source table: fps.tblyearmaster
- condition: fpsyear = selected year exists

If not found, that year is skipped.

## F. Quick clarity points

- Most source tables are year-based by fpsyear.
- Most target archive tables are year-based by year.
- Two source tables are shared (no fpsyear filter in query):
  - fps.tbldb_variables
  - fps.tblkpprofitcentre
- One target table uses project-based handling:
  - mabarchive.g_tlkpproject

## Implementation references
- src/Apha.BatchJobs/Apha.BatchJobs.Infrastructure/Repositories/MabArchive/MyFpsYearlyDataService.cs
- src/Apha.BatchJobs/Apha.BatchJobs.Infrastructure/Repositories/MabArchive/ReloadFpsTotalsService.cs
