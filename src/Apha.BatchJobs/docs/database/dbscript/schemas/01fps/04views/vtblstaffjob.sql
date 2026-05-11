-- View: fps.vtblstaffjob

CREATE OR REPLACE VIEW fps.vtblstaffjob AS
 SELECT staffid,
    jobcode,
    plannedhours
   FROM fps.tblstaffjob
  WHERE ((jobcode)::text IN ( SELECT vtlkpproject.parentproject
           FROM fps.vtlkpproject));
