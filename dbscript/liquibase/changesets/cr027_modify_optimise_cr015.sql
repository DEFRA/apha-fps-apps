--liquibase formatted sql

--changeset repo-admin:CR027 labels:validation context:all

WITH base AS (
    SELECT jobcode,
        fpsyear,
        totaltestcosts
    FROM fps.qrytotaltestcosts
),
refined AS (
    SELECT
        tr.jobcode,
        tr.fpsyear,
        SUM(tr.notests * tr.testprice) AS totaltestcosts
    FROM fps.vtbltestrequ tr
    JOIN fps.tlkpproject p
        ON p.parentproject = tr.jobcode
       AND p.fpsyear = tr.fpsyear
    GROUP BY tr.jobcode, tr.fpsyear
)
SELECT EXISTS (
    SELECT 1
    FROM base b
        FULL OUTER JOIN refined r ON b.jobcode = r.jobcode
        AND b.fpsyear = r.fpsyear
    WHERE COALESCE(b.totaltestcosts::numeric, 0::numeric)
        <> COALESCE(r.totaltestcosts::numeric, 0::numeric)
) AS has_mismatch;

--ROLLBACK
--Not Applicable