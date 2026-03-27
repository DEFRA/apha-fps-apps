-- View: fps.vcontractstaffjob

CREATE OR REPLACE VIEW fps.vcontractstaffjob AS
 SELECT staffid,
    jobcode,
    plannedhours,
    fpsyear
   FROM fps.tblstaffjob
  WHERE ((jobcode)::text IN ( SELECT vcontractproject.parentproject
           FROM fps.vcontractproject));
