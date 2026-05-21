CREATE OR REPLACE VIEW fps.vtblstaffjob_bygroup AS
 SELECT sj.staffid,
    sj.jobcode,
    sj.plannedhours,
    sj.fpsyear,
    p.user_id,
    p.dt2username,
    p.useremail
   FROM fps.tblstaffjob sj
     JOIN fps.vtlkpproject_bygroup p ON p.parentproject::text = sj.jobcode::text AND p.fpsyear = sj.fpsyear;
