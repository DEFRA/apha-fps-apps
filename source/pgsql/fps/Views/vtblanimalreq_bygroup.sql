CREATE OR REPLACE VIEW fps.vtblanimalreq_bygroup AS
 SELECT ar.jobcode,
    ar.animaltype,
    ar.numberofdays,
    ar.numberofanimals,
    ar.indcounter,
    ar.fpsyear,
    p.user_id,
    p.dt2username,
    p.useremail
   FROM fps.tblanimalreq ar
     JOIN fps.vtlkpproject_bygroup p ON p.parentproject::text = ar.jobcode::text AND p.fpsyear = ar.fpsyear;
