-- View: fps.vtblanimalreq

CREATE OR REPLACE VIEW fps.vtblanimalreq AS
 SELECT jobcode,
    animaltype,
    numberofdays,
    numberofanimals,
    indcounter,
    fpsyear
   FROM fps.tblanimalreq
  WHERE ((jobcode)::text IN ( SELECT vtlkpproject.parentproject
           FROM fps.vtlkpproject));
