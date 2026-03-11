-- View: fps.pandoratimecostcalcs

CREATE OR REPLACE VIEW fps.pandoratimecostcalcs AS
 SELECT workgroup,
    jobcode,
    project,
    month,
    staffid,
    gradecode,
    name,
    chargerate,
    class,
    "time",
    cost,
    division,
    jobcodeold,
    pay,
    nonpay,
    overhead,
    fpsyear
   FROM fps.timecostcalcs
  WHERE ((workgroup)::text IN ( SELECT tbluser_workgroup.workgroup
           FROM fps.tbluser_workgroup
          WHERE (tbluser_workgroup.user_id IN ( SELECT tblusers.user_id
                   FROM fps.tblusers
                  WHERE ((tblusers.dt2username)::text = CURRENT_USER)))));
