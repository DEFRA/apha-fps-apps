-- View: fps.vtblstaff

CREATE OR REPLACE VIEW fps.vtblstaff AS
 SELECT tblwgemployee.pactid AS staffid,
    (((COALESCE(tblemployee.lastname, ''::character varying))::text || ', '::text) || (COALESCE(tblemployee.firstname, ''::character varying))::text) AS name,
    tblwgemployee.workgroupgrade,
    tblemployee.title,
    tblwgemployee.personstatus,
    tblwgemployee.personclass,
    tblwgemployee.hrspaid,
    tblwgemployee.leave,
    tblwgemployee.sickspecial,
    tblwgemployee.hrsavail,
    tblwgemployee.makeavailable
   FROM fps.tblwgemployee,
    fps.tblemployee
  WHERE (((tblwgemployee.spnumber)::text = (tblemployee.spnumber)::text) AND ((tblwgemployee.workgroupgrade)::text IN ( SELECT vworkgroupgrade.wggrade
           FROM fps.vworkgroupgrade)));
