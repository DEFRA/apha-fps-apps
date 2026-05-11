-- View: fps.vtblcontract

CREATE OR REPLACE VIEW fps.vtblcontract AS
 SELECT contractno,
    category,
    manager,
    customer,
    title,
    registereddate,
    startdate,
    enddate,
    contractdoc,
    duration,
    fpsyear
   FROM fps.tblcontract
  WHERE ((category)::text IN ( SELECT tbluser_category.category
           FROM fps.tbluser_category
          WHERE (tbluser_category.user_id IN ( SELECT tblusers.user_id
                   FROM fps.tblusers
                  WHERE ((tblusers.dt2username)::text = CURRENT_USER)))));
