CREATE OR REPLACE VIEW fps.vtblstaffjob_rm AS
 SELECT staffid,
    jobcode,
    plannedhours
   FROM fps.tblstaffjob
  WHERE (staffid::text IN ( SELECT vtblwgemployee.pactid
           FROM fps.vtblwgemployee));
