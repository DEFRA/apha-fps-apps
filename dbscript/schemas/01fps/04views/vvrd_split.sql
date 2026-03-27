-- View: fps.vvrd_split

CREATE OR REPLACE VIEW fps.vvrd_split AS
 SELECT qvrd_splitlocationmonthly.location,
    sum((qvrd_splitlocationmonthly.labltsplitfee / qvrd_splitmonthly.totalltsplitfee)) AS split
   FROM (fps.qvrd_splitmonthly
     JOIN fps.qvrd_splitlocationmonthly ON ((qvrd_splitmonthly.month = qvrd_splitlocationmonthly.month)))
  GROUP BY qvrd_splitlocationmonthly.location
  ORDER BY qvrd_splitlocationmonthly.location;
