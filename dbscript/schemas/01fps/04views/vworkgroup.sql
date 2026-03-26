-- View: fps.vworkgroup

CREATE OR REPLACE VIEW fps.vworkgroup AS
 SELECT workgroup,
    profitcentre,
    costcentre,
    owner,
    description,
    centraloverhead,
    sendemail,
    cos90,
    costcentreold,
    email_recipient,
    fpsyear
   FROM fps.workgroup
  WHERE ((profitcentre)::text IN ( SELECT vtblkpprofitcentre.profitcentre
           FROM fps.vtblkpprofitcentre));
