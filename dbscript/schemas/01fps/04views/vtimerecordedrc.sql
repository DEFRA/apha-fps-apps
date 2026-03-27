-- View: fps.vtimerecordedrc

CREATE OR REPLACE VIEW fps.vtimerecordedrc AS
 SELECT timecostcalcs.project,
    workgroup.profitcentre
   FROM (fps.workgroup
     JOIN fps.timecostcalcs ON (((workgroup.workgroup)::text = (timecostcalcs.workgroup)::text)))
  GROUP BY timecostcalcs.project, workgroup.profitcentre;
