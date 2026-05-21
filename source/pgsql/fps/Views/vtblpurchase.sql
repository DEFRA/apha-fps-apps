CREATE OR REPLACE VIEW fps.vtblpurchase AS
 SELECT DISTINCT tp.workgroup,
    tp.account,
    tp.itemdescription,
    tp.amount,
    tp.fpsyear,
    u.user_id,
    u.dt2username,
    u.useremail
   FROM fps.tblpurchase tp
     JOIN fps.tblbid b ON tp.workgroup::text = b.workgroup::text
     JOIN fps.workgroup w ON b.workgroup::text = w.workgroup::text
     JOIN fps.tblkpprofitcentre pc ON w.profitcentre::text = pc.profitcentre::text
     JOIN fps.tbluser_profitcentre upc ON pc.profitcentre::text = upc.profitcentre::text
     JOIN fps.tblusers u ON upc.user_id = u.user_id
  WHERE (tp.account::text IN ( SELECT b2.account
           FROM fps.tblbid b2
             JOIN fps.workgroup w2 ON b2.workgroup::text = w2.workgroup::text
             JOIN fps.tblkpprofitcentre pc2 ON w2.profitcentre::text = pc2.profitcentre::text
             JOIN fps.tbluser_profitcentre upc2 ON pc2.profitcentre::text = upc2.profitcentre::text
          WHERE upc2.user_id = u.user_id));
