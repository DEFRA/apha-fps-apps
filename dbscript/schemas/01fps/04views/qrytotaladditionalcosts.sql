-- View: fps.qrytotaladditionalcosts

CREATE OR REPLACE VIEW fps.qrytotaladditionalcosts AS
 SELECT DISTINCT jobcode,
    sum(itemcost) AS totaladditionalcosts
   FROM fps.tbladditionalcosts
  GROUP BY jobcode;
