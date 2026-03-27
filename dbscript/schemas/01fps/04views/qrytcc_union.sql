-- View: fps.qrytcc_union

CREATE OR REPLACE VIEW fps.qrytcc_union AS
 SELECT workgroup.workgroup,
    tlkpproject.parentproject AS project
   FROM fps.workgroup,
    fps.tlkpproject
  WHERE ((workgroup.workgroup)::text ~~ 'SV__'::text)
UNION
 SELECT timecostcalcs.workgroup,
    timecostcalcs.project
   FROM fps.timecostcalcs;
