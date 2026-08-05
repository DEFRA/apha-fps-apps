--liquibase formatted sql

--changeset repo-admin:CR052 labels:ddl context:all runOnChange:true

-- View: fps.vqrytestsrequiredthisyear

CREATE OR REPLACE VIEW fps.vqrytestsrequiredthisyear AS
SELECT
    t.testcode,
    SUM(t.norequired)      AS norequired,
    MIN(p.itemdescription) AS itemdescription,
    MIN(p.unitpricevla)    AS unitpricevla
FROM fps.tlkptestreqmt t
LEFT JOIN fps.testorproduct p ON p.itemcode = t.testcode
GROUP BY t.testcode;

--ROLLBACK
--DROP VIEW IF EXISTS fps.vqrytestsrequiredthisyear;
