--liquibase formatted sql

--changeset repo-admin:CR054 labels:ddl context:all runOnChange:true

-- View: fps.vqrytestsrequiredbywg

CREATE OR REPLACE VIEW fps.vqrytestsrequiredbywg AS
SELECT
    COALESCE(
        wgtests.workgroup,
        (SELECT (tc.workgroup)::text
         FROM   fps.tlkptestcapability tc
         WHERE  (tc.testcode)::text = (thisyear.testcode)::text
         LIMIT  1)
    ) AS wg,
    thisyear.testcode,
    thisyear.norequired,
    lyt.yeartotal,
    wgtests.testsbywg,
    CASE
        WHEN lyt.yeartotal IS NULL
            THEN ROUND((thisyear.norequired)::numeric)::integer
        ELSE
            ROUND(((thisyear.norequired * wgtests.testsbywg / lyt.yeartotal) + 0.49)::numeric)::integer
    END                                                    AS projectedtotal,
    thisyear.itemdescription,
    COALESCE(rc.price, thisyear.unitpricevla)              AS unitprice,
    thisyear.unitpricevla,
    rc.price
FROM fps.vqrytestsrequiredthisyear thisyear
LEFT JOIN mabarchive.vlastyearstests lyt
    ON  (lyt.testcode)::text = (thisyear.testcode)::text
LEFT JOIN mabarchive.vlastyearswgtests wgtests
    ON  (wgtests.testcode)::text = (thisyear.testcode)::text
LEFT JOIN fps.vqrytestsrequiredbywg_rccost rc
    ON  (rc.testcode)::text  = (wgtests.testcode)::text
    AND (rc.workgroup)::text = (wgtests.workgroup)::text
WHERE thisyear.norequired <> 0;

--ROLLBACK
--DROP VIEW IF EXISTS fps.vqrytestsrequiredbywg;
