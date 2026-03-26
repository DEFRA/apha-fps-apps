-- View: fps.qryjobmonth_transfers1

CREATE OR REPLACE VIEW fps.qryjobmonth_transfers1 AS
 SELECT DISTINCT monthlyoutput.buyer AS project,
    monthlyoutput.month,
    monthlyoutput.testcode,
    monthlyoutput.volume,
    tlkptestreqmt.unitprice AS intunitprice,
    sum((monthlyoutput.volume * tlkptestreqmt.unitprice)) AS transfercost
   FROM ((fps.testorproduct
     JOIN fps.tlkptestreqmt ON (((testorproduct.itemcode)::text = (tlkptestreqmt.testcode)::text)))
     JOIN fps.monthlyoutput ON ((((tlkptestreqmt.buyer)::text = (monthlyoutput.buyer)::text) AND ((tlkptestreqmt.testcode)::text = (monthlyoutput.testcode)::text))))
  GROUP BY monthlyoutput.buyer, monthlyoutput.month, monthlyoutput.testcode, monthlyoutput.volume, tlkptestreqmt.unitprice;
