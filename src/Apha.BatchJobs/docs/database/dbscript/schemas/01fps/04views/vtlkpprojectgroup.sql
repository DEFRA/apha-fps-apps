-- View: fps.vtlkpprojectgroup

CREATE OR REPLACE VIEW fps.vtlkpprojectgroup AS
 SELECT projectgroup
   FROM fps.tlkpprojectgroup
  WHERE ((projectgroup)::text IN ( SELECT tbluser_projectgroup.projectgroup
           FROM fps.tbluser_projectgroup
          WHERE (tbluser_projectgroup.user_id IN ( SELECT tblusers.user_id
                   FROM fps.tblusers
                  WHERE ((tblusers.dt2username)::text = CURRENT_USER)))));
