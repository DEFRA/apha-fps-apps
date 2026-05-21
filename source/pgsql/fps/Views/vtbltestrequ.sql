CREATE OR REPLACE VIEW fps.vtbltestrequ AS
 SELECT DISTINCT tr.buyer AS jobcode,
    tr.testcode,
    tr.norequired AS notests,
    tr.unitprice AS testprice,
    tr.datecreated,
    tr.projectbuyercode,
    tr.fpsyear,
    u.user_id,
    u.dt2username,
    u.useremail
   FROM fps.tlkptestreqmt tr
     JOIN fps.tlkpproject pj ON tr.buyer::text = pj.parentproject::text
     JOIN fps.tlkpprogram pg ON pj.program::text = pg.programno::text
     JOIN fps.tbluser_program up ON pg.programno::text = up.programno::text
     JOIN fps.tblusers u ON up.user_id = u.user_id;
