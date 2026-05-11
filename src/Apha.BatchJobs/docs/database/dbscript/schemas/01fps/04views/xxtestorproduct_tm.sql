-- View: fps.xxtestorproduct_tm

CREATE OR REPLACE VIEW fps.xxtestorproduct_tm AS
 SELECT itemcode,
    itemdescription,
    testmanager,
    jobstatus,
    unitpricevla,
    priceahvg,
    owner,
    chargemethod,
    shortdescription,
    defraunitprice,
    fpsyear
   FROM fps.testorproduct
  WHERE ((owner)::text IN ( SELECT tbluser_testowner.test_owner
           FROM fps.tbluser_testowner
          WHERE (tbluser_testowner.user_id IN ( SELECT tblusers.user_id
                   FROM fps.tblusers
                  WHERE ((tblusers.username)::text = CURRENT_USER)))));
