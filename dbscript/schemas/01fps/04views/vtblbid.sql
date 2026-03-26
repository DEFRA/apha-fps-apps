-- View: fps.vtblbid

CREATE OR REPLACE VIEW fps.vtblbid AS
 SELECT workgroup,
    account,
    genbid
   FROM fps.tblbid
  WHERE ((workgroup)::text IN ( SELECT vworkgroup.workgroup
           FROM fps.vworkgroup));
