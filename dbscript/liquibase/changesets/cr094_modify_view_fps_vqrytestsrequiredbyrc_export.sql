--liquibase formatted sql

--changeset vishal:CR094 labels:ddl context:all runOnChange:true splitStatements:false

-- View: fps.vqrytestsrequiredbyrc_export

CREATE OR REPLACE VIEW fps.vqrytestsrequiredbyrc_export AS
WITH latest_release AS (
    SELECT max(year) AS year
    FROM mabarchive.tlkpyear
    WHERE latestmonthreleased IS NOT NULL
)
SELECT w.profitcentre,
       w.testcode,
       w.itemdescription,
       sum(w.projectedtotal) AS projectedtotal,
       w.unitprice,
       lr.year AS fpsyear
FROM fps.vqrytestsrequiredbywg_export w
CROSS JOIN latest_release lr
GROUP BY w.profitcentre, w.testcode, w.itemdescription, w.unitprice, lr.year
ORDER BY w.profitcentre, w.testcode;

--rollback DROP VIEW IF EXISTS fps.vqrytestsrequiredbyrc_export;
