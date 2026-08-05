--liquibase formatted sql

--changeset repo-admin:CR050 labels:ddl context:all runOnChange:true

-- View: mabarchive.vlastyearstests

CREATE OR REPLACE VIEW mabarchive.vlastyearstests AS
WITH latest AS (
    SELECT year, latestmonthreleased
    FROM mabarchive.tlkpyear
    WHERE year = (
        SELECT MAX(year)
        FROM mabarchive.tlkpyear
        WHERE latestmonthreleased IS NOT NULL
    )
)
SELECT
    mo.testcode,
    SUM(mo.volume) AS yeartotal
FROM mabarchive.my_monthlyoutput mo
CROSS JOIN latest
WHERE (mo.year::integer * 100 + mo.month::integer)
      BETWEEN CASE WHEN latest.latestmonthreleased = 12
                   THEN (latest.year * 100) + 1
                   ELSE ((latest.year - 1) * 100) + (latest.latestmonthreleased + 1)
              END
          AND (latest.year * 100) + latest.latestmonthreleased
GROUP BY mo.testcode;

--ROLLBACK
--DROP VIEW IF EXISTS mabarchive.vlastyearstests;
