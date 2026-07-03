-- CR-015: Fix Year-Scoped Aggregation Views
-- Applied: 2026-06-29

CREATE OR REPLACE VIEW fps.qrytotaladditionalcosts AS
 SELECT jobcode,
    fpsyear,
    sum(itemcost) AS totaladditionalcosts
   FROM fps.tbladditionalcosts
  GROUP BY jobcode, fpsyear;

CREATE OR REPLACE VIEW fps.qrytotalanimalcosts AS
 SELECT parentproject AS jobcode,
    fpsyear,
    sum(cost) AS totalanimalcosts
   FROM fps.vprojectanimalplan
  GROUP BY parentproject, fpsyear;

CREATE OR REPLACE VIEW fps.qrytotalstaffcosts AS
 SELECT parentproject AS jobcode,
    fpsyear,
    sum(cost) AS totalstaffcosts,
    sum(paycost) AS totalpaycosts
   FROM fps.vprojectstaffplan
  GROUP BY parentproject, fpsyear;

-- Verify views updated
SELECT table_name, column_name 
FROM information_schema.columns
WHERE table_schema = 'fps' 
  AND table_name IN ('qrytotaladditionalcosts', 'qrytotalanimalcosts', 'qrytotalstaffcosts')
ORDER BY table_name, ordinal_position;
