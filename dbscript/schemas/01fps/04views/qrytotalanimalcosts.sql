-- View: fps.qrytotalanimalcosts

CREATE OR REPLACE VIEW fps.qrytotalanimalcosts AS
 SELECT DISTINCT parentproject AS jobcode,
    sum(cost) AS totalanimalcosts
   FROM fps.vprojectanimalplan
  GROUP BY parentproject;
