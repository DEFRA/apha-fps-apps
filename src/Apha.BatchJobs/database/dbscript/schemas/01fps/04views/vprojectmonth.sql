-- View: fps.vprojectmonth

CREATE OR REPLACE VIEW fps.vprojectmonth AS
 SELECT projectmonth.project,
    projectmonth.monthno,
    projectmonth.costprofile
   FROM (fps.projectmonth
     JOIN fps.vtlkpproject ON (((projectmonth.project)::text = (vtlkpproject.parentproject)::text)));
