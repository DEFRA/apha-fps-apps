-- View: fps.qryjobmonthportfoliosales

CREATE OR REPLACE VIEW fps.qryjobmonthportfoliosales AS
 SELECT DISTINCT tlkptestcapability.planportfolio,
    monthlyoutput.month,
    sum((tlkptestreqmt.unitprice * monthlyoutput.volume)) AS fee
   FROM (fps.tlkptestreqmt
     JOIN (fps.tlkptestcapability
     JOIN fps.monthlyoutput ON ((((tlkptestcapability.workgroup)::text = (monthlyoutput.workgroup)::text) AND ((tlkptestcapability.testcode)::text = (monthlyoutput.testcode)::text)))) ON ((((tlkptestreqmt.buyer)::text = (monthlyoutput.buyer)::text) AND ((tlkptestreqmt.testcode)::text = (monthlyoutput.testcode)::text))))
  GROUP BY tlkptestcapability.planportfolio, monthlyoutput.month;
