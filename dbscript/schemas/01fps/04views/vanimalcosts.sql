-- View: fps.vanimalcosts

CREATE OR REPLACE VIEW fps.vanimalcosts AS
 SELECT tblanimalreq.numberofdays,
    tblanimalreq.numberofanimals
   FROM (fps.tblanimals
     JOIN fps.tblanimalreq ON (((tblanimals.animaltype)::text = (tblanimalreq.animaltype)::text)));
