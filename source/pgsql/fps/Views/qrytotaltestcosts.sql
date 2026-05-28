CREATE OR REPLACE VIEW fps.qrytotaltestcosts AS
 SELECT jobcode,
    sum(notests * testprice) AS totaltestcosts
   FROM fps.vtbltestrequ
  GROUP BY jobcode;
