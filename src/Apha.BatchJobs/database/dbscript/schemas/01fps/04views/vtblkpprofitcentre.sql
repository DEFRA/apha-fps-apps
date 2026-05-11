-- View: fps.vtblkpprofitcentre

CREATE OR REPLACE VIEW fps.vtblkpprofitcentre AS
 SELECT profitcentre,
    profitcentrename,
    division,
    conttarget,
    profitcentrehead,
    divisionid,
    email_recipient,
    highlevelsummary
   FROM fps.tblkpprofitcentre
  WHERE ((profitcentre)::text IN ( SELECT tbluser_profitcentre.profitcentre
           FROM fps.tbluser_profitcentre
          WHERE (tbluser_profitcentre.user_id IN ( SELECT tblusers.user_id
                   FROM fps.tblusers
                  WHERE ((tblusers.dt2username)::text = CURRENT_USER)))));
