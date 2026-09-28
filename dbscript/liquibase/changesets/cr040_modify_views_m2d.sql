--liquibase formatted sql
--changeset repo-admin:CR040 labels:ddl context:all runOnChange:true
--comment Recreate FPS and mabarchive views for money->numeric(19,4) migration. Non-money double precision expressions (hours, quantities) are intentionally unchanged; money-derived expressions are cast to numeric so they no longer widen to double precision.
-- Source: pgAdmin schema diff output, adapted for Liquibase execution.
-- -----------------------------------------------------------------------------
-- View: fps.vtestssummary
-- -----------------------------------------------------------------------------
-- Note: View recreation may fail if dependent objects are not updated first.
DROP VIEW IF EXISTS fps.vtestssummary;
CREATE OR REPLACE VIEW fps.vtestssummary AS
SELECT tlkpprogram.programno,
    tlkpproject.parentproject,
    tlkptestreqmt.testcode,
    testorproduct.itemdescription,
    tlkptestreqmt.fpsyear,
    tlkptestreqmt.norequired * tlkptestreqmt.unitprice AS "planned test cost",
    tlkptestreqmt.norequired AS "planned test vol",
    sum(monthlyoutput.volume) AS "brought test volume",
    sum(monthlyoutput.volume * tlkptestreqmt.unitprice) AS "brought test cost"
FROM fps.tlkpprogram
    JOIN fps.tlkpproject ON tlkpprogram.programno::text = tlkpproject.program::text
    AND tlkpprogram.fpsyear = tlkpproject.fpsyear
    JOIN fps.tlkptestreqmt ON tlkpproject.parentproject::text = tlkptestreqmt.buyer::text
    AND tlkpproject.fpsyear = tlkptestreqmt.fpsyear
    LEFT JOIN fps.monthlyoutput ON tlkptestreqmt.buyer::text = monthlyoutput.buyer::text
    AND tlkptestreqmt.testcode::text = monthlyoutput.testcode::text
    AND tlkptestreqmt.fpsyear = monthlyoutput.fpsyear
    JOIN fps.testorproduct ON tlkptestreqmt.testcode::text = testorproduct.itemcode::text
    AND tlkptestreqmt.fpsyear = testorproduct.fpsyear
GROUP BY tlkpprogram.programno,
    tlkpproject.parentproject,
    tlkptestreqmt.testcode,
    testorproduct.itemdescription,
    tlkptestreqmt.fpsyear,
    (
        tlkptestreqmt.norequired * tlkptestreqmt.unitprice
    ),
    tlkptestreqmt.norequired;
-- -----------------------------------------------------------------------------
-- View: fps.qryfrmtimesellerpc_map
-- -----------------------------------------------------------------------------
-- Note: View recreation may fail if dependent objects are not updated first.
DROP VIEW IF EXISTS fps.qryfrmtimesellerpc_map;
CREATE OR REPLACE VIEW fps.qryfrmtimesellerpc_map AS
SELECT tblkpprofitcentre.conttarget,
    profitcentregrade.profitcentre AS sellingpc,
    profitcentregrade.chargerate,
    profitcentregrade.ohr,
    vqrytbidsum.sumofgenbid,
    workgroupgrade.workgroup,
    workgroupgrade.profitcentregrade AS profitcentregrade_col,
    workgroupgrade.wggrade,
    vapphours.sumofplannedhours AS apphours,
    sum(vstaffjobhours.plannedhours) AS hrs,
    sum(tblwgemployee.hrsavail) AS avhrs,
    sum(vstaffjobhours.plannedhours) * profitcentregrade.chargerate AS fec,
    vapphours.sumofplannedhours * profitcentregrade.chargerate AS appfec,
    profitcentregrade.ohr * sum(vstaffjobhours.plannedhours) AS contribution
FROM fps.vapphours
    RIGHT JOIN (
        fps.tblkpprofitcentre
        JOIN (
            fps.profitcentregrade
            LEFT JOIN fps.vqrytbidsum ON profitcentregrade.profitcentre::text = vqrytbidsum.profitcentre::text
        ) ON tblkpprofitcentre.profitcentre::text = profitcentregrade.profitcentre::text
        JOIN fps.workgroupgrade ON profitcentregrade.pcgrade::text = workgroupgrade.profitcentregrade::text
        JOIN fps.tblwgemployee ON workgroupgrade.wggrade::text = tblwgemployee.workgroupgrade::text
    ) ON vapphours.workgroupgrade::text = workgroupgrade.wggrade::text
    LEFT JOIN fps.vstaffjobhours ON tblwgemployee.pactid::text = vstaffjobhours.staffid::text
GROUP BY tblkpprofitcentre.conttarget,
    profitcentregrade.profitcentre,
    profitcentregrade.chargerate,
    profitcentregrade.ohr,
    vqrytbidsum.sumofgenbid,
    workgroupgrade.workgroup,
    workgroupgrade.profitcentregrade,
    workgroupgrade.wggrade,
    vapphours.sumofplannedhours;
-- -----------------------------------------------------------------------------
-- View: fps.vprojectstaffplan
-- -----------------------------------------------------------------------------
-- Note: View recreation may fail if dependent objects are not updated first.
DROP VIEW IF EXISTS fps.vprojectstaffplan CASCADE;
CREATE OR REPLACE VIEW fps.vprojectstaffplan AS
SELECT tlkpproject.parentproject,
    tlkpprogram.programno,
    tlkpproject.contract,
    (
        COALESCE(tblemployee.lastname, ''::character varying)::text || ', '::text
    ) || COALESCE(tblemployee.firstname, ''::character varying)::text AS name,
    tblstaffjob.staffid,
    tblstaffjob.plannedhours,
    CASE
        tlkpproject.isdefraproject
        WHEN 0 THEN profitcentregrade.chargerate
        ELSE profitcentregrade.defrachargerate
    END AS chargerate,
    tblstaffjob.plannedhours::numeric * CASE
        lower(tlkpprogram.sector_name::text)
        WHEN 'charge'::text THEN 1::numeric
        ELSE 0::numeric
    END::numeric * CASE
        tlkpproject.isdefraproject
        WHEN 0 THEN profitcentregrade.chargerate
        ELSE profitcentregrade.defrachargerate
    END::numeric AS cost,
    tblstaffjob.plannedhours::numeric * CASE
        lower(tlkpprogram.sector_name::text)
        WHEN 'charge'::text THEN 1::numeric
        ELSE 0::numeric
    END::numeric * profitcentregrade.payrate::numeric AS paycost,
    profitcentregrade.profitcentre,
    workgroupgrade.workgroup,
    workgroupgrade.wggrade,
    profitcentregrade.pcgrade,
    workgroupgrade.gradecode,
    tblwgemployee.fpsyear
FROM fps.tblwgemployee
    JOIN fps.tblstaffjob ON tblwgemployee.pactid::text = tblstaffjob.staffid::text
    AND tblwgemployee.fpsyear = tblstaffjob.fpsyear
    JOIN fps.tblemployee ON tblwgemployee.spnumber::text = tblemployee.spnumber::text
    AND tblwgemployee.fpsyear = tblemployee.fpsyear
    JOIN fps.workgroupgrade ON tblwgemployee.workgroupgrade::text = workgroupgrade.wggrade::text
    AND tblwgemployee.fpsyear = workgroupgrade.fpsyear
    JOIN fps.profitcentregrade ON workgroupgrade.profitcentregrade::text = profitcentregrade.pcgrade::text
    AND workgroupgrade.fpsyear = profitcentregrade.fpsyear
    JOIN fps.tlkpproject ON tblstaffjob.jobcode::text = tlkpproject.parentproject::text
    AND tblstaffjob.fpsyear = tlkpproject.fpsyear
    JOIN fps.tlkpprogram ON tlkpproject.program::text = tlkpprogram.programno::text
    AND tlkpproject.fpsyear = tlkpprogram.fpsyear;

-- -----------------------------------------------------------------------------
-- View: fps.vplannedstaffcostspar1
-- -----------------------------------------------------------------------------
-- Recreate dependent view that was dropped with CASCADE
DROP VIEW IF EXISTS fps.vplannedstaffcostspar1;
CREATE OR REPLACE VIEW fps.vplannedstaffcostspar1 AS
SELECT vprojectstaffplan.parentproject,
    vprojectstaffplan.programno,
    vprojectstaffplan.fpsyear,
    sum(vprojectstaffplan.plannedhours) AS sumofplannedhours,
    sum(vprojectstaffplan.cost) AS sumofcost
FROM fps.vprojectstaffplan
    JOIN fps.tlkpproject ON (
        (
            (vprojectstaffplan.parentproject)::text = (tlkpproject.parentproject)::text
        )
        AND vprojectstaffplan.fpsyear = tlkpproject.fpsyear
    )
GROUP BY vprojectstaffplan.parentproject,
    vprojectstaffplan.programno,
    vprojectstaffplan.fpsyear;
-- -----------------------------------------------------------------------------
-- View: fps.vplannedstaffcostssummary
-- -----------------------------------------------------------------------------
-- Recreate dependent view that was dropped with CASCADE
DROP VIEW IF EXISTS fps.vplannedstaffcostssummary;
CREATE OR REPLACE VIEW fps.vplannedstaffcostssummary AS
SELECT workgroup.profitcentre,
    vprojectstaffplan.parentproject,
    vprojectstaffplan.fpsyear,
    sum(vprojectstaffplan.cost) AS sumofcost,
    sum(vprojectstaffplan.plannedhours) AS sumofplannedhours
FROM fps.vprojectstaffplan
    JOIN fps.workgroup ON (
        (
            (vprojectstaffplan.workgroup)::text = (workgroup.workgroup)::text
        )
        AND vprojectstaffplan.fpsyear = workgroup.fpsyear
    )
GROUP BY workgroup.profitcentre,
    vprojectstaffplan.parentproject,
    vprojectstaffplan.fpsyear;
-- -----------------------------------------------------------------------------
-- View: fps.qrytotalstaffcosts
-- -----------------------------------------------------------------------------
-- Recreate dependent view that was dropped with CASCADE.
-- Definition preserved from CR035 (COALESCE/numeric), not the older baseline.
DROP VIEW IF EXISTS fps.qrytotalstaffcosts;
CREATE OR REPLACE VIEW fps.qrytotalstaffcosts AS
SELECT parentproject AS jobcode,
    fpsyear,
    COALESCE(SUM(cost::numeric), 0) AS totalstaffcosts,
    COALESCE(SUM(paycost::numeric), 0) AS totalpaycosts
FROM fps.vprojectstaffplan
GROUP BY parentproject,
    fpsyear;
-- -----------------------------------------------------------------------------
-- View: fps.vtimerecordedrc_final
-- -----------------------------------------------------------------------------
-- Recreate dependent view that was dropped with CASCADE
DROP VIEW IF EXISTS fps.vtimerecordedrc_final;
CREATE OR REPLACE VIEW fps.vtimerecordedrc_final AS
SELECT vtimerecordedrc.project,
    vtimerecordedrc.profitcentre,
    vtimerecordedrc.fpsyear,
    COALESCE(
        vplannedstaffcostssummary.sumofplannedhours,
        (0)::double precision
    ) AS sumofplannedhours,
    COALESCE(
        (vplannedstaffcostssummary.sumofcost)::numeric,
        (0)::numeric
    ) AS sumofcost
FROM fps.vtimerecordedrc
    LEFT JOIN fps.vplannedstaffcostssummary ON (
        (
            (
                (vtimerecordedrc.profitcentre)::text = (vplannedstaffcostssummary.profitcentre)::text
            )
            AND (
                (vtimerecordedrc.project)::text = (vplannedstaffcostssummary.parentproject)::text
            )
            AND vtimerecordedrc.fpsyear = vplannedstaffcostssummary.fpsyear
        )
    )
GROUP BY vtimerecordedrc.project,
    vtimerecordedrc.profitcentre,
    vtimerecordedrc.fpsyear,
    vplannedstaffcostssummary.sumofplannedhours,
    vplannedstaffcostssummary.sumofcost;
-- -----------------------------------------------------------------------------
-- View: fps.qryjobmonth_tctransfers
-- -----------------------------------------------------------------------------
-- Note: Base view for qryjobmonth_transferunion. Recreated before its dependents.
DROP VIEW IF EXISTS fps.qryjobmonth_tctransfers CASCADE;
CREATE OR REPLACE VIEW fps.qryjobmonth_tctransfers AS
SELECT vpacttlkptestcapability.planportfolio AS project,
    monthlyoutput.month,
    monthlyoutput.testcode,
    monthlyoutput.volume,
    tlkptestreqmt.unitprice AS intunitprice,
    monthlyoutput.fpsyear,
    sum(monthlyoutput.volume * tlkptestreqmt.unitprice) AS transfercost
FROM fps.monthlyoutput
    JOIN fps.tlkptestreqmt ON monthlyoutput.testcode::text = tlkptestreqmt.testcode::text
    AND monthlyoutput.buyer::text = tlkptestreqmt.buyer::text
    AND monthlyoutput.fpsyear = tlkptestreqmt.fpsyear
    JOIN fps.vpacttlkptestcapability ON tlkptestreqmt.buyer::text = vpacttlkptestcapability.wgtestcode
    AND tlkptestreqmt.fpsyear = vpacttlkptestcapability.fpsyear
GROUP BY vpacttlkptestcapability.planportfolio,
    monthlyoutput.month,
    monthlyoutput.testcode,
    monthlyoutput.volume,
    tlkptestreqmt.unitprice,
    monthlyoutput.fpsyear;
-- -----------------------------------------------------------------------------
-- View: fps.qryjobmonth_transfers1
-- -----------------------------------------------------------------------------
-- Note: View recreation may fail if dependent objects are not updated first.
DROP VIEW IF EXISTS fps.qryjobmonth_transfers1 CASCADE;
CREATE OR REPLACE VIEW fps.qryjobmonth_transfers1 AS
SELECT DISTINCT monthlyoutput.buyer AS project,
    monthlyoutput.month,
    monthlyoutput.testcode,
    monthlyoutput.volume,
    tlkptestreqmt.unitprice AS intunitprice,
    monthlyoutput.fpsyear,
    sum(monthlyoutput.volume * tlkptestreqmt.unitprice) AS transfercost
FROM fps.testorproduct
    JOIN fps.tlkptestreqmt ON testorproduct.itemcode::text = tlkptestreqmt.testcode::text
    AND testorproduct.fpsyear = tlkptestreqmt.fpsyear
    JOIN fps.monthlyoutput ON tlkptestreqmt.buyer::text = monthlyoutput.buyer::text
    AND tlkptestreqmt.testcode::text = monthlyoutput.testcode::text
    AND tlkptestreqmt.fpsyear = monthlyoutput.fpsyear
GROUP BY monthlyoutput.buyer,
    monthlyoutput.month,
    monthlyoutput.testcode,
    monthlyoutput.volume,
    tlkptestreqmt.unitprice,
    monthlyoutput.fpsyear;
-- -----------------------------------------------------------------------------
-- View: fps.qryjobmonth_transfers2
-- -----------------------------------------------------------------------------
-- Recreate dependent view that was dropped with CASCADE
DROP VIEW IF EXISTS fps.qryjobmonth_transfers2;
CREATE OR REPLACE VIEW fps.qryjobmonth_transfers2 AS
SELECT DISTINCT project,
    month,
    fpsyear,
    sum(transfercost) AS sumoftransfercost
FROM fps.qryjobmonth_transfers1
GROUP BY project,
    month,
    fpsyear;
-- -----------------------------------------------------------------------------
-- View: fps.qryjobmonth_transferunion
-- -----------------------------------------------------------------------------
-- Recreate dependent view that was dropped with CASCADE
DROP VIEW IF EXISTS fps.qryjobmonth_transferunion;
CREATE OR REPLACE VIEW fps.qryjobmonth_transferunion AS
SELECT qryjobmonth_tctransfers.project,
    qryjobmonth_tctransfers.month,
    qryjobmonth_tctransfers.fpsyear,
    qryjobmonth_tctransfers.transfercost
FROM fps.qryjobmonth_tctransfers
UNION ALL
SELECT qryjobmonth_transfers1.project,
    qryjobmonth_transfers1.month,
    qryjobmonth_transfers1.fpsyear,
    qryjobmonth_transfers1.transfercost
FROM fps.qryjobmonth_transfers1;
-- -----------------------------------------------------------------------------
-- View: fps.qryjobmonth_transferstotal
-- -----------------------------------------------------------------------------
-- Recreate dependent view that was dropped with CASCADE.
-- Definition preserved from CR036 (month::integer, COALESCE/numeric).
DROP VIEW IF EXISTS fps.qryjobmonth_transferstotal;
CREATE OR REPLACE VIEW fps.qryjobmonth_transferstotal AS
SELECT project,
    month::integer AS month,
    fpsyear,
    COALESCE(SUM(transfercost::numeric), 0) AS sumoftransfercost
FROM fps.qryjobmonth_transferunion
GROUP BY project,
    month,
    fpsyear;
-- -----------------------------------------------------------------------------
-- View: fps.qryprojectmonthcw
-- -----------------------------------------------------------------------------
-- Note: View recreation may fail if dependent objects are not updated first.
DROP VIEW IF EXISTS fps.qryprojectmonthcw;
CREATE OR REPLACE VIEW fps.qryprojectmonthcw AS
SELECT DISTINCT projectmonth.project,
    projectmonth.monthno,
    projectmonth.fpsyear,
    tlkpproject.plancaseworkdebit / 12 AS cwdebit,
    tlkpproject.transferincome * tlkpproject.caseworksub::numeric / 12 AS cwcredit
FROM fps.tlkpproject
    JOIN fps.projectmonth ON tlkpproject.parentproject::text = projectmonth.project::text
    AND tlkpproject.fpsyear = projectmonth.fpsyear;
-- -----------------------------------------------------------------------------
-- View: fps.vplantestcosts
-- -----------------------------------------------------------------------------
-- Note: View recreation may fail if dependent objects are not updated first.
DROP VIEW IF EXISTS fps.vplantestcosts;
CREATE OR REPLACE VIEW fps.vplantestcosts AS
SELECT buyer,
    fpsyear,
    sum(unitprice * norequired) AS testplancost
FROM fps.tlkptestreqmt
GROUP BY buyer,
    fpsyear;
-- -----------------------------------------------------------------------------
-- View: fps.qrytestspcostplan_xtab
-- -----------------------------------------------------------------------------
-- Note: Base view for vpostmort1. Dropped with CASCADE; dependents recreated later.
DROP VIEW IF EXISTS fps.qrytestspcostplan_xtab CASCADE;
CREATE OR REPLACE VIEW fps.qrytestspcostplan_xtab AS
SELECT testcode,
    sum(
        CASE
            lower(profitcentre::text)
            WHEN 'labt'::text THEN price::numeric(19, 4)
            ELSE 0::numeric(19, 4)
        END
    ) AS labt,
    sum(
        CASE
            lower(profitcentre::text)
            WHEN 'vsd gb'::text THEN price::numeric(19, 4)
            ELSE 0::numeric(19, 4)
        END
    ) AS vetr,
    sum(
        CASE
            lower(profitcentre::text)
            WHEN 'viro'::text THEN price::numeric(19, 4)
            ELSE 0::numeric(19, 4)
        END
    ) AS viro
FROM fps.tbltestrccost
GROUP BY testcode;
-- -----------------------------------------------------------------------------
-- View: fps.vpvtprojectgroupmgrplan
-- -----------------------------------------------------------------------------
-- Note: View recreation may fail if dependent objects are not updated first.
DROP VIEW IF EXISTS fps.vpvtprojectgroupmgrplan;
CREATE OR REPLACE VIEW fps.vpvtprojectgroupmgrplan AS
SELECT DISTINCT p.projectgroup,
    p.fpsyear,
    sj.useremail,
    pcg.profitcentre AS resourcecentre,
    wgg.workgroup,
    wgg.gradecode,
    (
        COALESCE(e.lastname, ''::character varying)::text || ', '::text
    ) || COALESCE(e.firstname, ''::character varying)::text AS name,
    p.manager,
    sj.jobcode,
    p.projectstatus,
    sj.plannedhours AS hrs,
    pcg.chargerate,
    sj.plannedhours::numeric * CASE
        WHEN lower(prog.sector_name::text) = 'charge'::text THEN 1::numeric
        ELSE 0::numeric
    END::numeric * pcg.chargerate::numeric AS fee
FROM fps.tlkpproject p
    JOIN fps.vtblstaffjob_bygroup sj ON sj.jobcode::text = p.parentproject::text
    AND sj.fpsyear = p.fpsyear
    JOIN fps.tblwgemployee wge ON wge.pactid::text = sj.staffid::text
    AND wge.fpsyear = sj.fpsyear
    JOIN fps.tblemployee e ON e.spnumber::text = wge.spnumber::text
    AND e.fpsyear = wge.fpsyear
    JOIN fps.workgroupgrade wgg ON wgg.wggrade::text = wge.workgroupgrade::text
    AND wgg.fpsyear = wge.fpsyear
    JOIN fps.profitcentregrade pcg ON pcg.pcgrade::text = wgg.profitcentregrade::text
    AND pcg.fpsyear = wgg.fpsyear
    JOIN fps.tlkpprogram prog ON prog.programno::text = p.program::text
    AND prog.fpsyear = p.fpsyear;
-- -----------------------------------------------------------------------------
-- View: fps.vtestreqbreakdown
-- -----------------------------------------------------------------------------
-- Note: View recreation may fail if dependent objects are not updated first.
DROP VIEW IF EXISTS fps.vtestreqbreakdown;
CREATE OR REPLACE VIEW fps.vtestreqbreakdown AS WITH default_workgroup AS (
        SELECT tc.testcode,
            tc.fpsyear,
            min(tc.workgroup::text) AS wg
        FROM fps.tlkptestcapability tc
        GROUP BY tc.testcode,
            tc.fpsyear
    ),
    project_lookup AS (
        SELECT DISTINCT ON (tlkpproject.parentproject) tlkpproject.parentproject,
            tlkpproject.program
        FROM fps.tlkpproject
        ORDER BY tlkpproject.parentproject,
            tlkpproject.fpsyear DESC
    ),
    test_description_lookup AS (
        SELECT DISTINCT ON (testorproduct.itemcode) testorproduct.itemcode,
            testorproduct.shortdescription
        FROM fps.testorproduct
        ORDER BY testorproduct.itemcode,
            testorproduct.fpsyear DESC
    ),
    testrccost_lookup AS (
        SELECT DISTINCT ON (
                tbltestrccost.testcode,
                tbltestrccost.profitcentre
            ) tbltestrccost.testcode,
            tbltestrccost.profitcentre,
            tbltestrccost.price
        FROM fps.tbltestrccost
        ORDER BY tbltestrccost.testcode,
            tbltestrccost.profitcentre,
            tbltestrccost.fpsyear DESC
    ),
    workgroup_lookup AS (
        SELECT DISTINCT ON (workgroup.workgroup) workgroup.workgroup,
            workgroup.profitcentre
        FROM fps.workgroup
        ORDER BY workgroup.workgroup,
            workgroup.fpsyear DESC
    ),
    qry_test_req_breakdown AS (
        SELECT tr.testcode,
            td.shortdescription,
            p.program,
            tr.buyer AS jobcode,
            COALESCE(trwg.workgroup, dw.wg::character varying) AS workg,
            trc.profitcentre,
            COALESCE(trc.price, tr.unitprice) AS wgprice,
            COALESCE(trwg.amount::double precision, tr.norequired) AS noreq,
            tr.fpsyear
        FROM fps.tlkptestreqmt tr
            JOIN project_lookup p ON p.parentproject::text = tr.buyer::text
            JOIN default_workgroup dw ON dw.testcode::text = tr.testcode::text
            AND dw.fpsyear = tr.fpsyear
            LEFT JOIN fps.tbltestreqwg trwg ON trwg.testcode::text = tr.testcode::text
            AND trwg.buyer::text = tr.buyer::text
            AND trwg.fpsyear = tr.fpsyear
            LEFT JOIN testrccost_lookup trc ON trc.testcode::text = tr.testcode::text
            JOIN test_description_lookup td ON td.itemcode::text = tr.testcode::text
    )
SELECT q.testcode,
    q.shortdescription,
    q.program,
    q.jobcode AS project,
    COALESCE(q.profitcentre, wg.profitcentre) AS pc,
    q.workg,
    q.wgprice,
    (q.noreq::numeric * q.wgprice::numeric)::numeric AS totalcost,
    q.fpsyear
FROM qry_test_req_breakdown q
    LEFT JOIN workgroup_lookup wg ON wg.workgroup::text = q.workg::text;
-- -----------------------------------------------------------------------------
-- View: fps.vqryfrmtimesellerpc
-- -----------------------------------------------------------------------------
-- Note: View recreation may fail if dependent objects are not updated first.
DROP VIEW IF EXISTS fps.vqryfrmtimesellerpc;
CREATE OR REPLACE VIEW fps.vqryfrmtimesellerpc AS
SELECT pc.conttarget,
    pcg.profitcentre AS sellingpc,
    pcg.chargerate,
    pcg.ohr,
    bsum.sumofgenbid,
    wgg.workgroup,
    wgg.profitcentregrade,
    wgg.wggrade,
    ah.sumofplannedhours AS apphours,
    sum(sjh.plannedhours) AS hrs,
    sum(we.hrsavail) AS avhrs,
    (sum(sjh.plannedhours) * pcg.chargerate)::numeric AS fec,
    (ah.sumofplannedhours * pcg.chargerate)::numeric AS appfec,
    (pcg.ohr * sum(sjh.plannedhours))::numeric AS contribution,
    we.fpsyear,
    u.user_id,
    u.dt2username,
    u.useremail
FROM fps.tblkpprofitcentre pc
    JOIN fps.tbluser_profitcentre upc ON pc.profitcentre::text = upc.profitcentre::text
    JOIN fps.tblusers u ON upc.user_id = u.user_id
    JOIN fps.profitcentregrade pcg ON pc.profitcentre::text = pcg.profitcentre::text
    LEFT JOIN fps.vqrytbidsum bsum ON pcg.profitcentre::text = bsum.profitcentre::text
    AND pcg.fpsyear = bsum.fpsyear
    AND u.user_id = bsum.user_id
    JOIN fps.workgroupgrade wgg ON pcg.pcgrade::text = wgg.profitcentregrade::text
    AND pcg.fpsyear = wgg.fpsyear
    JOIN fps.tblwgemployee we ON wgg.wggrade::text = we.workgroupgrade::text
    AND wgg.fpsyear = we.fpsyear
    LEFT JOIN fps.vapphours ah ON wgg.wggrade::text = ah.workgroupgrade::text
    AND wgg.fpsyear = ah.fpsyear
    LEFT JOIN fps.vstaffjobhours sjh ON we.pactid::text = sjh.staffid::text
    AND we.fpsyear = sjh.fpsyear
GROUP BY pc.conttarget,
    pcg.profitcentre,
    pcg.chargerate,
    pcg.ohr,
    bsum.sumofgenbid,
    wgg.workgroup,
    wgg.profitcentregrade,
    wgg.wggrade,
    ah.sumofplannedhours,
    we.fpsyear,
    u.user_id,
    u.dt2username,
    u.useremail;
-- -----------------------------------------------------------------------------
-- View: fps.vpostmort1
-- -----------------------------------------------------------------------------
-- Note: View recreation may fail if dependent objects are not updated first.
DROP VIEW IF EXISTS fps.vpostmort1 CASCADE;
CREATE OR REPLACE VIEW fps.vpostmort1 AS
SELECT tlkptestcapability.planportfolio,
    monthlyoutput.testcode,
    testorproduct.shortdescription AS itemdescription,
    sum(monthlyoutput.volume) AS totvol,
    qrytestspcostplan_xtab.labt::numeric AS ltunitcharge,
    qrytestspcostplan_xtab.vetr::numeric AS sdunitcharge,
    qrytestspcostplan_xtab.labt::numeric * sum(monthlyoutput.volume) AS ltfee,
    qrytestspcostplan_xtab.vetr::numeric * sum(monthlyoutput.volume) AS sdfee,
    sum(monthlyoutput.volume) + qrytestspcostplan_xtab.vetr::numeric * sum(monthlyoutput.volume) AS totalfee,
    sum(
        vtbltestrequ.testprice::numeric * monthlyoutput.volume
    ) AS feecharged,
    sum(
        vtbltestrequ.testprice::numeric * monthlyoutput.volume
    ) - sum(monthlyoutput.volume) + qrytestspcostplan_xtab.vetr::numeric * sum(monthlyoutput.volume) AS profit_loss,
    monthlyoutput.workgroup
FROM fps.vtbltestrequ
    JOIN (
        fps.tlkptestcapability
        JOIN fps.monthlyoutput ON tlkptestcapability.workgroup::text = monthlyoutput.workgroup::text
        AND tlkptestcapability.testcode::text = monthlyoutput.testcode::text
    ) ON vtbltestrequ.testcode::text = monthlyoutput.testcode::text
    AND vtbltestrequ.jobcode::text = monthlyoutput.buyer::text
    JOIN fps.testorproduct ON monthlyoutput.testcode::text = testorproduct.itemcode::text
    LEFT JOIN fps.qrytestspcostplan_xtab ON testorproduct.itemcode::text = qrytestspcostplan_xtab.testcode::text
WHERE monthlyoutput.month <= (
        (
            SELECT max(tblperiod.endperiod) AS endperiod
            FROM fps.tblperiod
            WHERE tblperiod.finalsummariesrun = '-1'::integer
        )
    )
GROUP BY tlkptestcapability.planportfolio,
    monthlyoutput.testcode,
    testorproduct.shortdescription,
    qrytestspcostplan_xtab.labt,
    qrytestspcostplan_xtab.vetr,
    monthlyoutput.workgroup
HAVING tlkptestcapability.planportfolio::text ~~* 'tg0100'::text;
-- -----------------------------------------------------------------------------
-- View: fps.vprojectanimalplan
-- -----------------------------------------------------------------------------
-- Note: View recreation may fail if dependent objects are not updated first.
DROP VIEW IF EXISTS fps.vprojectanimalplan CASCADE;
CREATE OR REPLACE VIEW fps.vprojectanimalplan AS
SELECT tlkpproject.parentproject,
    tlkpproject.program,
    tblanimalreq.animaltype,
    tblanimalreq.numberofdays,
    tblanimalreq.numberofanimals,
    CASE
        tlkpproject.isdefraproject
        WHEN 0 THEN tblanimals.dailyrate
        ELSE tblanimals.defradailyrate
    END AS dailyrate,
    tblanimalreq.numberofanimals * tblanimalreq.numberofdays * CASE
        tlkpproject.isdefraproject
        WHEN 0 THEN tblanimals.dailyrate
        ELSE tblanimals.defradailyrate
    END AS cost,
    tblanimals.species,
    tblanimals.security_level,
    tblanimalreq.indcounter,
    tblanimalreq.fpsyear
FROM fps.tlkpproject
    JOIN fps.tblanimalreq ON tlkpproject.parentproject::text = tblanimalreq.jobcode::text
    AND tlkpproject.fpsyear = tblanimalreq.fpsyear
    JOIN fps.tblanimals ON tblanimalreq.animaltype::text = tblanimals.animaltype::text
    AND tblanimalreq.fpsyear = tblanimals.fpsyear;
-- -----------------------------------------------------------------------------
-- View: fps.qrytotalanimalcosts
-- -----------------------------------------------------------------------------
-- Recreate dependent view that was dropped with CASCADE.
-- Definition preserved from CR035 (COALESCE/numeric), not the older baseline.
DROP VIEW IF EXISTS fps.qrytotalanimalcosts;
CREATE OR REPLACE VIEW fps.qrytotalanimalcosts AS
SELECT parentproject AS jobcode,
    fpsyear,
    COALESCE(SUM(cost::numeric), 0) AS totalanimalcosts
FROM fps.vprojectanimalplan
GROUP BY parentproject,
    fpsyear;
-- -----------------------------------------------------------------------------
-- View: fps.vpostmortem1report_obsolete
-- -----------------------------------------------------------------------------
-- Recreate dependent view that was dropped with CASCADE
DROP VIEW IF EXISTS fps.vpostmortem1report_obsolete;
CREATE OR REPLACE VIEW fps.vpostmortem1report_obsolete AS
SELECT testcode,
    itemdescription,
    totvol,
    ltunitcharge,
    sdunitcharge,
    (round((ltfee)::numeric))::integer AS ltfee,
    (round((sdfee)::numeric))::integer AS sdfee,
    (
        (round((ltfee)::numeric))::integer + (round((sdfee)::numeric))::integer
    ) AS total_fee,
    (round((feecharged)::numeric))::integer AS fee_charged,
    (round((((feecharged - ltfee) - sdfee))::numeric))::integer AS profit_loss,
    workgroup
FROM fps.vpostmort1;
-- -----------------------------------------------------------------------------
-- View: mabarchive.vmy_projectanimalplan
-- -----------------------------------------------------------------------------
-- Note: View recreation may fail if dependent objects are not updated first.
DROP VIEW IF EXISTS mabarchive.vmy_projectanimalplan;
CREATE OR REPLACE VIEW mabarchive.vmy_projectanimalplan AS
SELECT my_tlkpproject.year,
    my_tlkpproject.parentproject,
    my_tblanimalreq.animaltype,
    my_tblanimalreq.numberofdays,
    my_tblanimalreq.numberofanimals,
    CASE
        WHEN my_tlkpproject.isdefraproject <> 0
        AND my_tlkpproject.year >= 2013 THEN my_tblanimals.defradailyrate
        ELSE my_tblanimals.dailyrate
    END AS rate,
    CASE
        WHEN my_tlkpproject.isdefraproject <> 0
        AND my_tlkpproject.year >= 2013 THEN my_tblanimals.defradailyrate
        ELSE my_tblanimals.dailyrate
    END * my_tblanimalreq.numberofdays * my_tblanimalreq.numberofanimals AS cost
FROM mabarchive.my_tlkpproject
    JOIN mabarchive.my_tblanimalreq ON my_tlkpproject.year = my_tblanimalreq.year
    AND my_tlkpproject.parentproject::text = my_tblanimalreq.jobcode::text
    JOIN mabarchive.my_tblanimals ON my_tblanimalreq.year = my_tblanimals.year
    AND my_tblanimalreq.animaltype::text = my_tblanimals.animaltype::text;
-- -----------------------------------------------------------------------------
-- View: mabarchive.vmy_projectstaffplan
-- -----------------------------------------------------------------------------
-- Note: View recreation may fail if dependent objects are not updated first.
DROP VIEW IF EXISTS mabarchive.vmy_projectstaffplan;
CREATE OR REPLACE VIEW mabarchive.vmy_projectstaffplan AS
SELECT my_tlkpproject.year,
    my_tlkpproject.parentproject,
    my_profitcentregrade.pcgrade,
    my_staff.workgroupgrade,
    my_staff.name,
    my_tblstaffjob.plannedhours,
    CASE
        WHEN my_tlkpproject.isdefraproject <> 0
        AND my_tlkpproject.year >= 2013 THEN my_profitcentregrade.npr + my_profitcentregrade.payrate
        ELSE my_profitcentregrade.chargerate
    END AS rate,
    CASE
        WHEN my_tlkpproject.isdefraproject <> 0
        AND my_tlkpproject.year >= 2013 THEN my_tblstaffjob.plannedhours * (
            my_profitcentregrade.npr + my_profitcentregrade.payrate
        )
        ELSE my_tblstaffjob.plannedhours * my_profitcentregrade.chargerate
    END :: numeric AS cost
FROM mabarchive.my_tlkpproject
    JOIN mabarchive.my_tblstaffjob ON my_tlkpproject.year = my_tblstaffjob.year
    AND my_tlkpproject.parentproject::text = my_tblstaffjob.jobcode::text
    JOIN mabarchive.my_staff ON my_tblstaffjob.year = my_staff.year
    AND my_tblstaffjob.staffid::text = my_staff.staffid::text
    JOIN mabarchive.my_workgroupgrade ON my_staff.year = my_workgroupgrade.year
    AND my_staff.workgroupgrade::text = my_workgroupgrade.wggrade::text
    JOIN mabarchive.my_profitcentregrade ON my_workgroupgrade.year = my_profitcentregrade.year
    AND my_workgroupgrade.profitcentregrade::text = my_profitcentregrade.pcgrade::text;

-- -----------------------------------------------------------------------------
-- View: fps.vqrytestsactualbreakdown
-- -----------------------------------------------------------------------------
-- Note: New view, no prior definition in baseline/changesets.
DROP VIEW IF EXISTS fps.vqrytestsactualbreakdown;
CREATE OR REPLACE VIEW fps.vqrytestsactualbreakdown
 AS
 SELECT DISTINCT vtlkpproject_general.program,
    monthlyoutput.buyer,
    tlkptestcapability.planportfolio AS portfolio,
    tlkptestcapability.workgroup,
    tlkptestcapability.testcode,
    testorproduct.shortdescription,
    monthlyoutput.month,
    monthlyoutput.fpsyear,
    COALESCE(tbltestrccost.price, tlkptestreqmt.unitprice)::numeric AS pcprice,
    monthlyoutput.volume::numeric * COALESCE(tbltestrccost.price, tlkptestreqmt.unitprice)::numeric AS pccost,
        CASE
            WHEN monthlyoutput.workgroup::text = 'LTLA'::text AND tbltestrccost.profitcentre::text = 'VetR'::text THEN 'Path'::character varying
            WHEN tbltestrccost.profitcentre IS NULL THEN vworkgroup_general.profitcentre
            ELSE tbltestrccost.profitcentre
        END AS profitcentre
   FROM fps.tlkptestreqmt
     JOIN fps.tlkptestcapability ON tlkptestcapability.testcode::text = tlkptestreqmt.testcode::text AND tlkptestcapability.fpsyear = tlkptestreqmt.fpsyear
     JOIN fps.monthlyoutput ON tlkptestcapability.testcode::text = monthlyoutput.testcode::text AND tlkptestcapability.workgroup::text = monthlyoutput.workgroup::text AND tlkptestreqmt.buyer::text = monthlyoutput.buyer::text AND monthlyoutput.fpsyear = tlkptestreqmt.fpsyear
     JOIN fps.vworkgroup_general ON monthlyoutput.workgroup::text = vworkgroup_general.workgroup::text AND vworkgroup_general.fpsyear = monthlyoutput.fpsyear
     JOIN fps.testorproduct ON tlkptestreqmt.testcode::text = testorproduct.itemcode::text AND testorproduct.fpsyear = tlkptestreqmt.fpsyear
     JOIN fps.vtlkpproject_general ON tlkptestreqmt.buyer::text = vtlkpproject_general.parentproject::text AND vtlkpproject_general.fpsyear = tlkptestreqmt.fpsyear
     LEFT JOIN fps.tbltestrccost ON testorproduct.itemcode::text = tbltestrccost.testcode::text AND tbltestrccost.fpsyear = tlkptestreqmt.fpsyear;




-- End of changeset