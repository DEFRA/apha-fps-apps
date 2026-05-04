-- View: fps.qrytotaladditionalcosts

CREATE OR REPLACE VIEW fps.qrytotaladditionalcosts AS
 SELECT DISTINCT tbladditionalcosts.jobcode,
   sum(tbladditionalcosts.itemcost) AS totaladditionalcosts,
   tbladditionalcosts.fpsyear
  FROM fps.tbladditionalcosts
  GROUP BY tbladditionalcosts.jobcode, tbladditionalcosts.fpsyear;
