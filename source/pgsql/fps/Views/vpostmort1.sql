CREATE OR REPLACE VIEW fps.vpostmort1 AS
 SELECT tlkptestcapability.planportfolio,
    monthlyoutput.testcode,
    testorproduct.shortdescription AS itemdescription,
    sum(monthlyoutput.volume) AS totvol,
    qrytestspcostplan_xtab.labt::numeric AS ltunitcharge,
    qrytestspcostplan_xtab.vetr::numeric AS sdunitcharge,
    qrytestspcostplan_xtab.labt::numeric::double precision * sum(monthlyoutput.volume) AS ltfee,
    qrytestspcostplan_xtab.vetr::numeric::double precision * sum(monthlyoutput.volume) AS sdfee,
    sum(monthlyoutput.volume) + qrytestspcostplan_xtab.vetr::numeric::double precision * sum(monthlyoutput.volume) AS totalfee,
    sum(vtbltestrequ.testprice::numeric::double precision * monthlyoutput.volume) AS feecharged,
    sum(vtbltestrequ.testprice::numeric::double precision * monthlyoutput.volume) - sum(monthlyoutput.volume) + qrytestspcostplan_xtab.vetr::numeric::double precision * sum(monthlyoutput.volume) AS profit_loss,
    monthlyoutput.workgroup
   FROM fps.vtbltestrequ
     JOIN (fps.tlkptestcapability
     JOIN fps.monthlyoutput ON tlkptestcapability.workgroup::text = monthlyoutput.workgroup::text AND tlkptestcapability.testcode::text = monthlyoutput.testcode::text) ON vtbltestrequ.testcode::text = monthlyoutput.testcode::text AND vtbltestrequ.jobcode::text = monthlyoutput.buyer::text
     JOIN fps.testorproduct ON monthlyoutput.testcode::text = testorproduct.itemcode::text
     LEFT JOIN fps.qrytestspcostplan_xtab ON testorproduct.itemcode::text = qrytestspcostplan_xtab.testcode::text
  WHERE monthlyoutput.month <= (( SELECT max(tblperiod.endperiod) AS endperiod
           FROM fps.tblperiod
          WHERE tblperiod.finalsummariesrun = '-1'::integer))
  GROUP BY tlkptestcapability.planportfolio, monthlyoutput.testcode, testorproduct.shortdescription, qrytestspcostplan_xtab.labt, qrytestspcostplan_xtab.vetr, monthlyoutput.workgroup
 HAVING tlkptestcapability.planportfolio::text ~~* 'tg0100'::text;
