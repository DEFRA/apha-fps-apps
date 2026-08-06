--liquibase formatted sql

--changeset repo-admin:CR042 labels:ddl context:all runOnChange:true

-- View: fps.vpvtworkgroupstaffplan

CREATE OR REPLACE VIEW fps.vpvtworkgroupstaffplan AS
SELECT DISTINCT
    wgg.workgroup,
    wgg.gradecode,
    COALESCE(e.lastname, '') || ', ' || COALESCE(e.firstname, '') AS name,
    sj.fpsyear,
    p.manager,
    p.program,
    sj.jobcode,
    p.projectstatus,
    sj.plannedhours AS hrs,
    sj.plannedhours
        * pcg.chargerate
        * CASE
              WHEN LOWER(prog.sector_name) = 'charge' THEN 1::numeric
              ELSE 0::numeric
          END::double precision AS fee
FROM fps.tblstaffjob sj
JOIN fps.tblwgemployee wge
    ON  wge.pactid  = sj.staffid
    AND wge.fpsyear = sj.fpsyear
JOIN fps.tblemployee e
    ON  e.spnumber  = wge.spnumber
    AND e.fpsyear   = wge.fpsyear
JOIN fps.workgroupgrade wgg
    ON  wgg.wggrade = wge.workgroupgrade
    AND wgg.fpsyear = wge.fpsyear
JOIN fps.profitcentregrade pcg
    ON  pcg.pcgrade = wgg.profitcentregrade
    AND pcg.fpsyear = wgg.fpsyear
JOIN fps.tlkpproject p
    ON  p.parentproject = sj.jobcode
    AND p.fpsyear       = sj.fpsyear
JOIN fps.tlkpprogram prog
    ON  prog.programno = p.program
    AND prog.fpsyear   = p.fpsyear;

--ROLLBACK
--DROP VIEW IF EXISTS fps.vpvtworkgroupstaffplan;
