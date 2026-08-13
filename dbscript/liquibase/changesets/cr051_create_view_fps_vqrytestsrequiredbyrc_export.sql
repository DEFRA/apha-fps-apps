--liquibase formatted sql

--changeset repo-admin:CR051 labels:ddl context:all runOnChange:true

-- View: fps.vqrytestsrequiredbyrc_export

CREATE OR REPLACE VIEW fps.vqrytestsrequiredbyrc_export AS
WITH

-- Replaces DECLARE @from_ym / @latest_ym variables (not allowed in a view)
latest_release AS (
    SELECT
        year,
        latestmonthreleased,
        (year * 100) + latestmonthreleased                   AS latest_ym,
        CASE WHEN latestmonthreleased = 12
             THEN (year * 100) + 1
             ELSE ((year - 1) * 100) + (latestmonthreleased + 1)
        END                                                  AS from_ym
    FROM mabarchive.tlkpyear
    WHERE year = (
        SELECT MAX(year)
        FROM   mabarchive.tlkpyear
        WHERE  latestmonthreleased IS NOT NULL
    )
),

-- qryLastYearsTests: SUM(Volume) per TestCode for the rolling 12-month window
last_year_tests AS (
    SELECT
        UPPER(mo.testcode) as testcode,
        SUM(mo.volume) AS yeartotal
    FROM mabarchive.my_monthlyoutput mo
    CROSS JOIN latest_release lr
    WHERE (mo.year::integer * 100 + mo.month::integer)
          BETWEEN lr.from_ym AND lr.latest_ym
    GROUP BY UPPER(mo.testcode)
),

-- qryLastYearsWGTests: SUM(Volume) per TestCode + WorkGroup for the rolling 12-month window
last_year_rc_tests AS (
    SELECT
        mo.testcode as testcode,
        UPPER(mo.workgroup) as workgroup,
        SUM(mo.volume) AS testsbywg
    FROM mabarchive.my_monthlyoutput mo
    CROSS JOIN latest_release lr
    WHERE (mo.year::integer * 100 + mo.month::integer)
          BETWEEN lr.from_ym AND lr.latest_ym
    GROUP BY testcode, UPPER(mo.workgroup)
),

-- Replaces DLookUp correlated subquery; rn=1 picks first alphabetical workgroup per test
default_workgroup AS (
    SELECT
        UPPER(testcode) as testcode,
        workgroup,
        ROW_NUMBER() OVER (PARTITION BY UPPER(testcode) ORDER BY workgroup) AS rn
    FROM fps.tlkptestcapability tc
    INNER JOIN latest_release lr ON tc.fpsyear = lr.year
),

-- qryTestsRequiredThisYear: plan totals per TestCode; early filter reduces downstream rows
tests_required AS (
    SELECT
        t.testcode,
        SUM(t.norequired)      AS norequired,
        MIN(p.itemdescription) AS itemdescription,
        MIN(p.unitpricevla)    AS unitpricevla,
        lr.year
    FROM fps.tlkptestreqmt t
    CROSS JOIN latest_release lr
    LEFT JOIN fps.testorproduct p ON (p.itemcode)::text = (t.testcode)::text
    AND p.fpsyear = t.fpsyear
    WHERE t.norequired <> 0
    AND t.fpsyear = lr.year
    GROUP BY t.testcode, lr.year
),

-- qryTestsRequiredByWG_RCCost: RC price expanded to workgroup via profitcentre
rc_cost AS (
    SELECT
        rc.testcode,
        wg.workgroup,
        rc.price
    FROM fps.tbltestrccost rc
    INNER JOIN fps.workgroup wg ON (wg.profitcentre)::text = (rc.profitcentre)::text
    INNER JOIN latest_release lr ON rc.fpsyear = lr.year and wg.fpsyear = lr.year
),

-- qryTestsRequiredByWG: projected volume per workgroup
tests_by_rc AS (
    SELECT
        COALESCE(lwg.workgroup, dwg.workgroup)          AS wg,
        tr.testcode,
        tr.itemdescription,
        COALESCE(rc.price, tr.unitpricevla)             AS unitprice,
        -- NULLIF guards division by zero; COALESCE handles NULL TestsByWG
        CASE
            WHEN COALESCE(lyt.yeartotal, 0) = 0
                THEN ROUND(tr.norequired::numeric)::integer
            ELSE
                ROUND((tr.norequired * COALESCE(lwg.testsbywg, 0::double precision)
                       / NULLIF(lyt.yeartotal, 0) + 0.49)::numeric)::integer
        END                                             AS projectedtotal
    FROM tests_required tr
    LEFT JOIN last_year_tests    lyt ON (lyt.testcode)::text  = (tr.testcode)::text
    LEFT JOIN last_year_rc_tests lwg ON (lwg.testcode)::text  = (tr.testcode)::text
    LEFT JOIN rc_cost             rc  ON (rc.testcode)::text   = (lwg.testcode)::text
                                    AND (rc.workgroup)::text   = (lwg.workgroup)::text
    LEFT JOIN default_workgroup  dwg  ON (dwg.testcode)::text  = (tr.testcode)::text
                                    AND dwg.rn = 1
)

-- Collapse workgroups: SUM projectedtotal per (profitcentre, testcode)
-- HAVING on non-aggregated column converted to WHERE at query time
SELECT
    wg.profitcentre,
    t.testcode,
    t.itemdescription,
    SUM(t.projectedtotal) as projectedtotal ,
    t.unitprice
FROM tests_by_rc t
INNER JOIN fps.workgroup wg ON (wg.workgroup)::text = (t.wg)::text
INNER JOIN latest_release lr ON lr.year = wg.fpsyear
GROUP BY wg.profitcentre, t.testcode, t.itemdescription , t.unitprice
ORDER BY wg.profitcentre, t.testcode;

--ROLLBACK
--DROP VIEW IF EXISTS fps.vqrytestsrequiredbyrc_export;
