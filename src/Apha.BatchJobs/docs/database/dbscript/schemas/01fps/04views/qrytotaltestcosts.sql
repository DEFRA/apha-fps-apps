-- View: fps.qrytotaltestcosts

CREATE OR REPLACE VIEW fps.qrytotaltestcosts AS
 SELECT DISTINCT vtbltestrequ.jobcode,
    sum((vtbltestrequ.notests * vtbltestrequ.testprice)) AS totaltestcosts,
    tlkpproject.fpsyear
    FROM (fps.vtbltestrequ
       JOIN fps.tlkpproject ON (((vtbltestrequ.jobcode)::text = (tlkpproject.parentproject)::text)))
  GROUP BY vtbltestrequ.jobcode, tlkpproject.fpsyear;
