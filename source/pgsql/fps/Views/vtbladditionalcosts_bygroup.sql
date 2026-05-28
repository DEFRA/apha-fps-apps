CREATE OR REPLACE VIEW fps.vtbladditionalcosts_bygroup AS
 SELECT ac.jobcode,
    ac.account,
    ac.description,
    ac.itemcost,
    ac.freq,
    ac.supplier,
    ac.fpsyear,
    p.user_id,
    p.dt2username,
    p.useremail
   FROM fps.tbladditionalcosts ac
     JOIN fps.vtlkpproject_bygroup p ON p.parentproject::text = ac.jobcode::text AND p.fpsyear = ac.fpsyear;
