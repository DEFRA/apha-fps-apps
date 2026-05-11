-- View: fps.vplantestcosts

CREATE OR REPLACE VIEW fps.vplantestcosts AS
 SELECT buyer,
    sum((unitprice * norequired)) AS testplancost
   FROM fps.tlkptestreqmt
  GROUP BY buyer;
