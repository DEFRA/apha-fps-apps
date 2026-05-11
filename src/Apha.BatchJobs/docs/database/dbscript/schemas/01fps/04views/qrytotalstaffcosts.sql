-- View: fps.qrytotalstaffcosts

CREATE OR REPLACE VIEW fps.qrytotalstaffcosts AS
 SELECT DISTINCT vprojectstaffplan.parentproject AS jobcode,
    sum(vprojectstaffplan.cost) AS totalstaffcosts,
    sum(vprojectstaffplan.paycost) AS totalpaycosts,
    tlkpproject.fpsyear
    FROM (fps.vprojectstaffplan
       JOIN fps.tlkpproject ON (((vprojectstaffplan.parentproject)::text = (tlkpproject.parentproject)::text)))
  GROUP BY vprojectstaffplan.parentproject, tlkpproject.fpsyear;
