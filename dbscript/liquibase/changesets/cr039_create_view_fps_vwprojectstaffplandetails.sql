--liquibase formatted sql

--changeset repo-admin:CR039 labels:ddl context:all runOnChange:true

-- View: fps.vwprojectstaffplandetails

CREATE OR REPLACE VIEW fps.vwprojectstaffplandetails AS
SELECT
    vprojectstaffplan.profitcentre,
    vprojectstaffplan.workgroup,
    vprojectstaffplan.gradecode,
    vprojectstaffplan.name,
    vtlkpproject_general.manager,
    vtlkpproject_general.program,
    vtlkpproject_general.projectstatus,
    vprojectstaffplan.plannedhours,
    vprojectstaffplan.chargerate,
    vprojectstaffplan.cost,
    vprojectstaffplan.fpsyear
FROM fps.vtlkpproject_general vtlkpproject_general
INNER JOIN fps.vprojectstaffplan vprojectstaffplan
    ON vtlkpproject_general.parentproject = vprojectstaffplan.parentproject
   AND vtlkpproject_general.fpsyear = vprojectstaffplan.fpsyear;

COMMENT ON VIEW fps.vwprojectstaffplandetails
    IS 'view for Staff Plan Pivot Screen. ';

--ROLLBACK
--DROP VIEW IF EXISTS fps.vwprojectstaffplandetails;
