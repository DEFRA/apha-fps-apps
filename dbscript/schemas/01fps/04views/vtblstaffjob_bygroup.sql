-- View: fps.vtblstaffjob_bygroup

CREATE OR REPLACE VIEW fps.vtblstaffjob_bygroup AS
 SELECT staffid,
    jobcode,
    plannedhours
   FROM fps.tblstaffjob
  WHERE ((jobcode)::text IN ( SELECT vtlkpproject_bygroup.parentproject
           FROM fps.vtlkpproject_bygroup));
