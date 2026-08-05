--liquibase formatted sql

--changeset repo-admin:CR053 labels:ddl context:all runOnChange:true

-- View: fps.vqrytestsrequiredbywg_rccost

CREATE OR REPLACE VIEW fps.vqrytestsrequiredbywg_rccost AS
SELECT
    rc.testcode,
    wg.workgroup,
    rc.price
FROM fps.tbltestrccost rc
INNER JOIN fps.vworkgroup_general wg ON wg.profitcentre = rc.profitcentre;

--ROLLBACK
--DROP VIEW IF EXISTS fps.vqrytestsrequiredbywg_rccost;
