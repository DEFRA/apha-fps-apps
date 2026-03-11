-- View: fps.qryjobmonth_tctransfers

CREATE OR REPLACE VIEW fps.qryjobmonth_tctransfers AS
 SELECT vpacttlkptestcapability.planportfolio AS project,
    monthlyoutput.month,
    monthlyoutput.testcode,
    monthlyoutput.volume,
    tlkptestreqmt.unitprice AS intunitprice,
    sum((monthlyoutput.volume * tlkptestreqmt.unitprice)) AS transfercost
   FROM ((fps.monthlyoutput
     JOIN fps.tlkptestreqmt ON ((((monthlyoutput.testcode)::text = (tlkptestreqmt.testcode)::text) AND ((monthlyoutput.buyer)::text = (tlkptestreqmt.buyer)::text))))
     JOIN fps.vpacttlkptestcapability ON (((tlkptestreqmt.buyer)::text = vpacttlkptestcapability.wgtestcode)))
  GROUP BY vpacttlkptestcapability.planportfolio, monthlyoutput.month, monthlyoutput.testcode, monthlyoutput.volume, tlkptestreqmt.unitprice;
