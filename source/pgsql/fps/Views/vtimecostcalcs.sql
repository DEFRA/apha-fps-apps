CREATE OR REPLACE VIEW fps.vtimecostcalcs AS
 SELECT tc.workgroup,
    tc.jobcode,
    tc.project,
    tc.month,
    tc.staffid,
    tc.gradecode,
    tc.name,
    tc.chargerate,
    tc.class,
    tc."time",
    tc.cost,
    tc.division,
    tc.jobcodeold,
    tc.fpsyear,
    p.user_id,
    p.dt2username,
    p.useremail
   FROM fps.timecostcalcs tc
     JOIN fps.vtlkpproject p ON tc.project::text = p.parentproject::text AND p.fpsyear = tc.fpsyear;
