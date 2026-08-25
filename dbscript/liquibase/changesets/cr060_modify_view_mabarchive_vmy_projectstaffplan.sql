--liquibase formatted sql

--changeset repo-admin:CR060 labels:ddl context:all runOnChange:true

-- View: mabarchive.vmy_projectstaffplan

DROP VIEW IF EXISTS mabarchive.vmy_projectstaffplan;

CREATE OR REPLACE VIEW mabarchive.vmy_projectstaffplan
 AS
 SELECT my_tlkpproject.year,
    my_tlkpproject.parentproject,
    my_profitcentregrade.pcgrade,
    my_staff.workgroupgrade,
    my_staff.name,
    my_tblstaffjob.plannedhours,
        CASE
            WHEN my_tlkpproject.isdefraproject <> 0 AND my_tlkpproject.year >= 2013 THEN my_profitcentregrade.npr + my_profitcentregrade.payrate
            ELSE my_profitcentregrade.chargerate
        END AS rate,
        CASE
            WHEN my_tlkpproject.isdefraproject <> 0 AND my_tlkpproject.year >= 2013 THEN my_tblstaffjob.plannedhours * (my_profitcentregrade.npr + my_profitcentregrade.payrate)::double precision
            ELSE my_tblstaffjob.plannedhours * my_profitcentregrade.chargerate::double precision
        END::numeric AS cost
   FROM mabarchive.my_tlkpproject
     JOIN mabarchive.my_tblstaffjob ON my_tlkpproject.year = my_tblstaffjob.year AND my_tlkpproject.parentproject::text = my_tblstaffjob.jobcode::text
     JOIN mabarchive.my_staff ON my_tblstaffjob.year = my_staff.year AND my_tblstaffjob.staffid::text = my_staff.staffid::text
     JOIN mabarchive.my_workgroupgrade ON my_staff.year = my_workgroupgrade.year AND my_staff.workgroupgrade::text = my_workgroupgrade.wggrade::text
     JOIN mabarchive.my_profitcentregrade ON my_workgroupgrade.year = my_profitcentregrade.year AND my_workgroupgrade.profitcentregrade::text = my_profitcentregrade.pcgrade::text;

--ROLLBACK
--DROP VIEW IF EXISTS mabarchive.vmy_projectstaffplan;