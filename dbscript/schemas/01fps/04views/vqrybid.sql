-- View: fps.vqrybid

CREATE OR REPLACE VIEW fps.vqrybid AS
 SELECT DISTINCT tblkpaccountcategory.accshortname,
    tblbid.workgroup,
    tblbid.genbid,
    workgroup.profitcentre
   FROM ((fps.tblkpaccountcategory
     LEFT JOIN fps.tblbid ON (((tblkpaccountcategory.accshortname)::text = (tblbid.account)::text)))
     LEFT JOIN fps.workgroup ON (((tblbid.workgroup)::text = (workgroup.workgroup)::text)))
  GROUP BY tblkpaccountcategory.accshortname, tblbid.workgroup, tblbid.genbid, workgroup.profitcentre;
