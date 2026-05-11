-- View: fps.qrytotalanimalcosts

CREATE OR REPLACE VIEW fps.qrytotalanimalcosts AS
 SELECT DISTINCT vprojectanimalplan.parentproject AS jobcode,
    sum(vprojectanimalplan.cost) AS totalanimalcosts,
    tlkpproject.fpsyear
    FROM (fps.vprojectanimalplan
       JOIN fps.tlkpproject ON (((vprojectanimalplan.parentproject)::text = (tlkpproject.parentproject)::text)))
  GROUP BY vprojectanimalplan.parentproject, tlkpproject.fpsyear;
