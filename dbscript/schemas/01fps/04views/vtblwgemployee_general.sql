-- View: fps.vtblwgemployee_general

CREATE OR REPLACE VIEW fps.vtblwgemployee_general AS
 SELECT pactid,
    spnumber,
    workgroupgrade
   FROM fps.tblwgemployee;
