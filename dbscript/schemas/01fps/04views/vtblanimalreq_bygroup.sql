-- View: fps.vtblanimalreq_bygroup

CREATE OR REPLACE VIEW fps.vtblanimalreq_bygroup AS
 SELECT jobcode,
    animaltype,
    numberofdays,
    numberofanimals,
    indcounter,
    fpsyear
   FROM fps.tblanimalreq
  WHERE ((jobcode)::text IN ( SELECT vtlkpproject_bygroup.parentproject
           FROM fps.vtlkpproject_bygroup));
