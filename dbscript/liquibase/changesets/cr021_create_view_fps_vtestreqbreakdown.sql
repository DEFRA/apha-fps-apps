--liquibase formatted sql

--changeset repo-admin:CR021 labels:ddl context:all runOnChange:true

DROP VIEW IF EXISTS fps.vtestreqbreakdown;

CREATE VIEW fps.vtestreqbreakdown AS
WITH default_workgroup AS (
    SELECT
        tc.testcode,
        tc.fpsyear,
        MIN(tc.workgroup) AS wg
    FROM fps.tlkptestcapability tc
    GROUP BY tc.testcode, tc.fpsyear
),
project_lookup AS (
    SELECT DISTINCT ON (parentproject)
        parentproject,
        program
    FROM fps.tlkpproject
    ORDER BY parentproject, fpsyear DESC
),
test_description_lookup AS (
    SELECT DISTINCT ON (itemcode)
        itemcode,
        shortdescription
    FROM fps.testorproduct
    ORDER BY itemcode, fpsyear DESC
),
testrccost_lookup AS (
    SELECT DISTINCT ON (testcode, profitcentre)
        testcode,
        profitcentre,
        price
    FROM fps.tbltestrccost
    ORDER BY testcode, profitcentre, fpsyear DESC
),
workgroup_lookup AS (
    SELECT DISTINCT ON (workgroup)
        workgroup,
        profitcentre
    FROM fps.workgroup
    ORDER BY workgroup, fpsyear DESC
),
qry_test_req_breakdown AS (
    SELECT
        tr.testcode,
        td.shortdescription,
        p.program,
        tr.buyer                                                AS jobcode,
        COALESCE(trwg.workgroup, dw.wg)                        AS workg,
        trc.profitcentre,
        COALESCE(trc.price, tr.unitprice)                      AS wgprice,
        COALESCE(trwg.amount::double precision, tr.norequired) AS noreq,
        tr.fpsyear
    FROM fps.tlkptestreqmt tr
    INNER JOIN project_lookup p
        ON p.parentproject = tr.buyer
    INNER JOIN default_workgroup dw
        ON dw.testcode  = tr.testcode
       AND dw.fpsyear   = tr.fpsyear
    LEFT JOIN fps.tbltestreqwg trwg
        ON trwg.testcode  = tr.testcode
       AND trwg.buyer     = tr.buyer
       AND trwg.fpsyear   = tr.fpsyear
    LEFT JOIN testrccost_lookup trc
        ON trc.testcode = tr.testcode
    INNER JOIN test_description_lookup td
        ON td.itemcode = tr.testcode
)
SELECT
    q.testcode,
    q.shortdescription,
    q.program,
    q.jobcode                                          AS project,
    COALESCE(q.profitcentre, wg.profitcentre)          AS pc,
    q.workg,
    q.wgprice,
    (q.noreq::numeric * q.wgprice::numeric)::decimal(19,4) AS totalcost,
    q.fpsyear
FROM qry_test_req_breakdown q
LEFT JOIN workgroup_lookup wg
    ON wg.workgroup = q.workg;


--ROLLBACK
--Not Applicable