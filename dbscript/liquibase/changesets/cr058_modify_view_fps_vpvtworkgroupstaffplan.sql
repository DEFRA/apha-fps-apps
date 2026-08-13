--liquibase formatted sql

--changeset repo-admin:CR058 labels:ddl context:all runOnChange:true

-- View: fps.vpvtworkgroupstaffplan
-- CR058: fee cast to numeric (was double precision) to match CR034's decimal(19,4) money conversion.

CREATE OR REPLACE VIEW fps.vpvtworkgroupstaffplan
 AS
 SELECT DISTINCT wgg.workgroup,
    wgg.gradecode,
    (COALESCE(e.lastname, ''::character varying)::text || ', '::text) || COALESCE(e.firstname, ''::character varying)::text AS name,
    sj.fpsyear,
    p.manager,
    p.program,
    sj.jobcode,
    p.projectstatus,
    sj.plannedhours AS hrs,
    sj.plannedhours::numeric * pcg.chargerate::numeric *
        CASE
            WHEN lower(prog.sector_name::text) = 'charge'::text THEN 1::numeric
            ELSE 0::numeric
        END::numeric AS fee
   FROM fps.tblstaffjob sj
     JOIN fps.tblwgemployee wge ON wge.pactid::text = sj.staffid::text AND wge.fpsyear = sj.fpsyear
     JOIN fps.tblemployee e ON e.spnumber::text = wge.spnumber::text AND e.fpsyear = wge.fpsyear
     JOIN fps.workgroupgrade wgg ON wgg.wggrade::text = wge.workgroupgrade::text AND wgg.fpsyear = wge.fpsyear
     JOIN fps.profitcentregrade pcg ON pcg.pcgrade::text = wgg.profitcentregrade::text AND pcg.fpsyear = wgg.fpsyear
     JOIN fps.tlkpproject p ON p.parentproject::text = sj.jobcode::text AND p.fpsyear = sj.fpsyear
     JOIN fps.tlkpprogram prog ON prog.programno::text = p.program::text AND prog.fpsyear = p.fpsyear;

--ROLLBACK
--Not Applicable
