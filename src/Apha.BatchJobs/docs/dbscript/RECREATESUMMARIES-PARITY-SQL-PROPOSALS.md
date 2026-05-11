# RecreateSummaries Parity SQL Proposals

Analysis date: 2026-05-11
Scope: Proposed replacements for parity-risk views used by RecreateSummaries
Status: Proposal only (not applied)

## Recommendation

Use Proposal A (strict legacy parity baseline) first, because RecreateSummaries step SQL still joins many inputs by project/month without fpsyear keys.

After parity baseline is validated, consider Proposal B (year-safe hardening) as a second controlled change.

## Why Proposal A is recommended now

- `sp_createFPSTotals` joins total-cost views by `ParentProject = JobCode` only.
- Current PostgreSQL total-cost views include `fpsyear`, which can alter join shape versus legacy.
- `10_create_project_month_single.sql` joins monthly views by project+month (no fpsyear join), so introducing year-grain changes in upstream views can create behavior drift.

---

## Proposal A: Strict Legacy Parity Baseline

### 1) fps.qrymilestone1

```sql
CREATE OR REPLACE VIEW fps.qrymilestone1 AS
SELECT DISTINCT
    m.project,
    m.milestoneref,
    m.plandate,
    m.actualdate,
    m.monthnofin AS duemonth,
    CASE WHEN m.actualdate <= m.plandate THEN 1::numeric ELSE 0::numeric END AS ontimeflag,
    CASE WHEN m.actualdate IS NULL THEN 0 ELSE 1 END AS completeflag,
    m.year,
    m.fpsyear
FROM fps.milestone AS m;
```

Notes:
- Removes hardcoded `year = '2003/2004'` filter.
- Keeps `year` and `fpsyear` available for downstream hardening, without changing current `qryjobmonthmilestone` shape.

### 2) fps.qrytotaladditionalcosts

```sql
CREATE OR REPLACE VIEW fps.qrytotaladditionalcosts AS
SELECT
    a.jobcode,
    sum(a.itemcost) AS totaladditionalcosts
FROM fps.tbladditionalcosts AS a
GROUP BY a.jobcode;
```

Notes:
- Matches legacy aggregation grain used by `sp_createFPSTotals` join pattern.

### 3) fps.qrytotalanimalcosts

```sql
CREATE OR REPLACE VIEW fps.qrytotalanimalcosts AS
SELECT
    v.parentproject AS jobcode,
    sum(v.cost) AS totalanimalcosts
FROM fps.vprojectanimalplan AS v
GROUP BY v.parentproject;
```

Notes:
- Removes extra join to `tlkpproject` only used to add `fpsyear`.

### 4) fps.qrytotalstaffcosts

```sql
CREATE OR REPLACE VIEW fps.qrytotalstaffcosts AS
SELECT
    v.parentproject AS jobcode,
    sum(v.cost) AS totalstaffcosts,
    sum(v.paycost) AS totalpaycosts
FROM fps.vprojectstaffplan AS v
GROUP BY v.parentproject;
```

Notes:
- Keeps expected `totalpaycosts` output while restoring legacy-like grain.

### 5) fps.vtbltestrequ

```sql
CREATE OR REPLACE VIEW fps.vtbltestrequ AS
SELECT DISTINCT
    tr.buyer AS jobcode,
    tr.testcode,
    tr.norequired AS notests,
    tr.unitprice AS testprice,
    tr.datecreated,
    tr.projectbuyercode,
    tr.fpsyear
FROM fps.tlkptestreqmt AS tr;
```

Notes:
- Removes user-context security filter (`CURRENT_USER`) and avoids user/program join amplification.
- Produces stable batch/system dataset.

### 6) fps.qrytotaltestcosts

```sql
CREATE OR REPLACE VIEW fps.qrytotaltestcosts AS
SELECT
    v.jobcode,
    sum(v.notests * v.testprice) AS totaltestcosts
FROM fps.vtbltestrequ AS v
GROUP BY v.jobcode;
```

Notes:
- Aligns with legacy `sp_createFPSTotals` join grain.

---

## Proposal B: Year-Safe Hardening (Phase 2)

Use this only after baseline parity is proven. This requires coordinated updates to downstream joins and RecreateSummaries SQL so all joins include `fpsyear` where data can span years.

### B1) fps.qrytotaladditionalcosts

```sql
CREATE OR REPLACE VIEW fps.qrytotaladditionalcosts AS
SELECT
    a.jobcode,
    a.fpsyear,
    sum(a.itemcost) AS totaladditionalcosts
FROM fps.tbladditionalcosts AS a
GROUP BY a.jobcode, a.fpsyear;
```

### B2) fps.qrytotalanimalcosts

```sql
CREATE OR REPLACE VIEW fps.qrytotalanimalcosts AS
SELECT
    v.parentproject AS jobcode,
    p.fpsyear,
    sum(v.cost) AS totalanimalcosts
FROM fps.vprojectanimalplan AS v
JOIN fps.tlkpproject AS p
  ON p.parentproject::text = v.parentproject::text
GROUP BY v.parentproject, p.fpsyear;
```

### B3) fps.qrytotalstaffcosts

```sql
CREATE OR REPLACE VIEW fps.qrytotalstaffcosts AS
SELECT
    v.parentproject AS jobcode,
    p.fpsyear,
    sum(v.cost) AS totalstaffcosts,
    sum(v.paycost) AS totalpaycosts
FROM fps.vprojectstaffplan AS v
JOIN fps.tlkpproject AS p
  ON p.parentproject::text = v.parentproject::text
GROUP BY v.parentproject, p.fpsyear;
```

### B4) fps.qrytotaltestcosts

```sql
CREATE OR REPLACE VIEW fps.qrytotaltestcosts AS
SELECT
    v.jobcode,
    v.fpsyear,
    sum(v.notests * v.testprice) AS totaltestcosts
FROM fps.vtbltestrequ AS v
GROUP BY v.jobcode, v.fpsyear;
```

---

## Verification SQL (run after applying Proposal A)

```sql
-- 1) Check views compile and are readable
SELECT schemaname, viewname
FROM pg_views
WHERE schemaname = 'fps'
  AND viewname IN (
    'qrymilestone1',
    'qrytotaladditionalcosts',
    'qrytotalanimalcosts',
    'qrytotalstaffcosts',
    'vtbltestrequ',
    'qrytotaltestcosts'
  )
ORDER BY viewname;

-- 2) Smoke row-counts
SELECT 'qrymilestone1' AS obj, count(*) FROM fps.qrymilestone1
UNION ALL SELECT 'qrytotaladditionalcosts', count(*) FROM fps.qrytotaladditionalcosts
UNION ALL SELECT 'qrytotalanimalcosts', count(*) FROM fps.qrytotalanimalcosts
UNION ALL SELECT 'qrytotalstaffcosts', count(*) FROM fps.qrytotalstaffcosts
UNION ALL SELECT 'vtbltestrequ', count(*) FROM fps.vtbltestrequ
UNION ALL SELECT 'qrytotaltestcosts', count(*) FROM fps.qrytotaltestcosts;

-- 3) Ensure one row per jobcode in totals (Proposal A expectation)
SELECT jobcode, count(*)
FROM fps.qrytotaladditionalcosts
GROUP BY jobcode
HAVING count(*) > 1;

SELECT jobcode, count(*)
FROM fps.qrytotalanimalcosts
GROUP BY jobcode
HAVING count(*) > 1;

SELECT jobcode, count(*)
FROM fps.qrytotalstaffcosts
GROUP BY jobcode
HAVING count(*) > 1;

SELECT jobcode, count(*)
FROM fps.qrytotaltestcosts
GROUP BY jobcode
HAVING count(*) > 1;
```

## Rollout order

1. Apply `vtbltestrequ`.
2. Apply `qrytotaltestcosts`.
3. Apply `qrytotaladditionalcosts`, `qrytotalanimalcosts`, `qrytotalstaffcosts`.
4. Apply `qrymilestone1`.
5. Run verification SQL.
6. Re-run RecreateSummaries and compare sample outputs against known-good baseline.
