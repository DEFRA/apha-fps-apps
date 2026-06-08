# RecreateSummaries Cloud Parity Review

Analysis date: 2026-05-11
Source: src/Apha.BatchJobs/docs/dbscript/CloudDump/viewsInCloud
Scope: High-risk view dependencies for RecreateSummaries parity

## Purpose

This document captures the exact cloud SQL definitions for the six parity-risk views and compares them to expected legacy behavior. Use it to decide what must be changed before final parity sign-off.

## Object 1: fps.qrymilestone1

Cloud SQL:

```sql
SELECT DISTINCT project,
       milestoneref,
       plandate,
       actualdate,
       monthnofin AS duemonth,
       CASE
           WHEN actualdate <= plandate THEN 1::numeric
           ELSE 0::numeric
       END AS ontimeflag,
       CASE
           WHEN actualdate IS NULL THEN 0
           ELSE 1
       END AS completeflag,
       year,
       fpsyear
FROM fps.milestone
WHERE year::text = '2003/2004'::text;
```

Expected legacy behavior:
- Output should not be hardcoded to one historical year.
- RecreateSummaries should evaluate milestones for the active processing year context.

Parity risk:
- High. Hardcoded filter can silently exclude current-year records.

Decision needed:
- Confirm whether the legacy logic was truly fixed to 2003/2004 or should be year-parameterized (or fpsyear-scoped).

## Object 2: fps.qrytotaladditionalcosts

Cloud SQL:

```sql
SELECT DISTINCT jobcode,
       fpsyear,
       sum(itemcost) AS totaladditionalcosts
FROM fps.tbladditionalcosts
GROUP BY jobcode, fpsyear;
```

Expected legacy behavior:
- Legacy dependency review flagged potential grouping mismatch (legacy flow reportedly grouped by jobcode only in this stage).
- If year-scoping is performed upstream, adding fpsyear here may alter join cardinality versus legacy.

Parity risk:
- High. Potential row-shape mismatch if consumers expect one row per jobcode.

Decision needed:
- Confirm whether Step 2 target query should be grouped by jobcode only or by jobcode plus fpsyear for parity.

## Object 3: fps.qrytotalanimalcosts

Cloud SQL:

```sql
SELECT DISTINCT parentproject AS jobcode,
       fpsyear,
       sum(cost) AS totalanimalcosts
FROM fps.vprojectanimalplan
GROUP BY parentproject, fpsyear;
```

Expected legacy behavior:
- Same parity question as additional costs: grouping dimensionality must match legacy output shape consumed by RecreateSummaries.

Parity risk:
- High. Potential duplication or join misses where downstream logic assumes jobcode-level totals only.

Decision needed:
- Confirm required grouping key set for parity: jobcode only vs jobcode plus fpsyear.

## Object 4: fps.qrytotalstaffcosts

Cloud SQL:

```sql
SELECT DISTINCT parentproject AS jobcode,
       fpsyear,
       sum(cost) AS totalstaffcosts,
       sum(paycost) AS totalpaycosts
FROM fps.vprojectstaffplan
GROUP BY parentproject, fpsyear;
```

Expected legacy behavior:
- Same grouping parity check as above, with additional risk around totalpaycosts alignment.

Parity risk:
- High. Aggregation grain mismatch can change both totalstaffcosts and totalpaycosts.

Decision needed:
- Confirm grouping keys and whether pay cost aggregation grain is expected to include fpsyear in this view.

## Object 5: fps.qrytotaltestcosts

Cloud SQL:

```sql
SELECT jobcode,
       sum(notests * testprice) AS totaltestcosts
FROM fps.vtbltestrequ
GROUP BY jobcode;
```

Expected legacy behavior:
- Totals should align to active-year scope used in the batch run.
- If vtbltestrequ exposes multi-year rows, grouping by jobcode only may blend years.

Parity risk:
- High. Risk depends on effective filters inherited from vtbltestrequ.

Decision needed:
- Confirm whether year scoping happens before this aggregation and whether explicit fpsyear grouping/filtering is required.

## Object 6: fps.vtbltestrequ

Cloud SQL:

```sql
SELECT DISTINCT tr.buyer AS jobcode,
       tr.testcode,
       tr.norequired AS notests,
       tr.unitprice AS testprice,
       tr.datecreated,
       tr.projectbuyercode,
       tr.fpsyear,
       u.user_id,
       u.dt2username,
       u.useremail
FROM fps.tlkptestreqmt tr
JOIN fps.tlkpproject pj ON tr.buyer::text = pj.parentproject::text
JOIN fps.tlkpprogram pg ON pj.program::text = pg.programno::text
JOIN fps.tbluser_program up ON pg.programno::text = up.programno::text
JOIN fps.tblusers u ON up.user_id = u.user_id;
```

Expected legacy behavior:
- Batch recomputation datasets typically should not be limited by per-user security mappings.
- This view introduces user/program security joins, potentially reducing or skewing rows for system/batch contexts.

Parity risk:
- High. Security-filtered data can undercount test costs.

Decision needed:
- Confirm whether RecreateSummaries should use a security-filtered dataset or a system-wide unfiltered requirement view.

## Consolidated Decisions Required

1. Remove or replace hardcoded year filter in qrymilestone1.
2. Confirm aggregation grain for qrytotaladditionalcosts, qrytotalanimalcosts, qrytotalstaffcosts.
3. Confirm year-scoping strategy for qrytotaltestcosts.
4. Confirm whether vtbltestrequ is valid for batch/system processing or needs an unfiltered alternative.

## Suggested Next Implementation Order

1. Resolve qrymilestone1 year filter.
2. Resolve vtbltestrequ filtering model.
3. Resolve qrytotaltestcosts year-scoping.
4. Resolve grouping grain for additional, animal, and staff totals.
