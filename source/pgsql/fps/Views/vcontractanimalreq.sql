CREATE OR REPLACE VIEW fps.vcontractanimalreq AS
 SELECT jobcode,
    animaltype,
    numberofdays,
    numberofanimals,
    indcounter,
    fpsyear
   FROM fps.tblanimalreq
  WHERE (jobcode::text IN ( SELECT vcontractproject.parentproject
           FROM fps.vcontractproject));
