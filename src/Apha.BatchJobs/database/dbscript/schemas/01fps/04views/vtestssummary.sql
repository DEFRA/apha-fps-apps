-- View: fps.vtestssummary

CREATE OR REPLACE VIEW fps.vtestssummary AS
 SELECT tlkpprogram.programno,
    tlkpproject.parentproject,
    tlkptestreqmt.testcode,
    testorproduct.itemdescription,
    (tlkptestreqmt.norequired * tlkptestreqmt.unitprice) AS "planned test cost",
    tlkptestreqmt.norequired AS "planned test vol",
    sum(monthlyoutput.volume) AS "brought test volume",
    sum((monthlyoutput.volume * tlkptestreqmt.unitprice)) AS "brought test cost"
   FROM ((((fps.tlkpprogram
     JOIN fps.tlkpproject ON (((tlkpprogram.programno)::text = (tlkpproject.program)::text)))
     JOIN fps.tlkptestreqmt ON (((tlkpproject.parentproject)::text = (tlkptestreqmt.buyer)::text)))
     LEFT JOIN fps.monthlyoutput ON ((((tlkptestreqmt.buyer)::text = (monthlyoutput.buyer)::text) AND ((tlkptestreqmt.testcode)::text = (monthlyoutput.testcode)::text))))
     JOIN fps.testorproduct ON (((tlkptestreqmt.testcode)::text = (testorproduct.itemcode)::text)))
  GROUP BY tlkpprogram.programno, tlkpproject.parentproject, tlkptestreqmt.testcode, testorproduct.itemdescription, (tlkptestreqmt.norequired * tlkptestreqmt.unitprice), tlkptestreqmt.norequired;
