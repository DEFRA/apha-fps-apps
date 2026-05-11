-- View: fps.vtimecostcalcs_allstaff

CREATE OR REPLACE VIEW fps.vtimecostcalcs_allstaff AS
 SELECT timecostcalcs.workgroup,
    timecostcalcs.month,
    timecostcalcs.staffid,
    timecostcalcs.project,
    timecostcalcs.gradecode,
    timecostcalcs.name,
    timecostcalcs.class,
    timecostcalcs."time"
   FROM fps.timecostcalcs
UNION ALL
 SELECT workgroupgrade.workgroup,
    tblperiod.endperiod AS month,
    vtblstaff_general.staffid,
    ''::character varying AS project,
    workgroupgrade.gradecode,
    vtblstaff_general.name,
    ''::character varying AS class,
    0 AS "time"
   FROM ((fps.vtblstaff_general
     JOIN fps.workgroupgrade ON (((vtblstaff_general.workgroupgrade)::text = (workgroupgrade.wggrade)::text)))
     CROSS JOIN fps.tblperiod)
  WHERE ((tblperiod.finalsummariesrun = '-1'::integer) AND (vtblstaff_general.name !~~ '%general'::text));
