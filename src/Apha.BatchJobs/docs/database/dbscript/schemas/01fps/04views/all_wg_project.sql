-- View: fps.all_wg_project

CREATE OR REPLACE VIEW fps.all_wg_project AS
 SELECT workgroup.workgroup,
    tlkpproject.parentproject AS project
   FROM fps.workgroup,
    fps.tlkpproject;
