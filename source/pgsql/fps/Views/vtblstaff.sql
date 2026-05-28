CREATE OR REPLACE VIEW fps.vtblstaff AS
 SELECT tblwgemployee.pactid AS staffid,
    (COALESCE(tblemployee.lastname, ''::character varying)::text || ', '::text) || COALESCE(tblemployee.firstname, ''::character varying)::text AS name,
    tblwgemployee.workgroupgrade,
    tblemployee.title,
    tblwgemployee.personstatus,
    tblwgemployee.personclass,
    tblwgemployee.hrspaid,
    tblwgemployee.leave,
    tblwgemployee.sickspecial,
    tblwgemployee.hrsavail,
    tblwgemployee.makeavailable,
    tblwgemployee.fpsyear,
    wgg.user_id,
    wgg.dt2username,
    wgg.useremail
   FROM fps.tblwgemployee
     JOIN fps.tblemployee ON tblemployee.spnumber::text = tblwgemployee.spnumber::text
     JOIN fps.vworkgroupgrade wgg ON wgg.wggrade::text = tblwgemployee.workgroupgrade::text AND wgg.fpsyear = tblwgemployee.fpsyear;
