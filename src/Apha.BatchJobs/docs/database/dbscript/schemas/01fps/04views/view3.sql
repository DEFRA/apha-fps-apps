-- View: fps.view3

CREATE OR REPLACE VIEW fps.view3 AS
 SELECT timecostcalcs.project,
    timecostcalcs.jobcode,
    sum(timecostcalcs.cost) AS sumofcost,
        CASE workgroup.profitcentre
            WHEN 'Path'::text THEN 'Surveillance/Pathology'::text
            WHEN 'vetr'::text THEN 'Surveillance/Pathology'::text
            ELSE 'Laboratory Testing'::text
        END AS resourcecentre
   FROM (fps.timecostcalcs
     JOIN fps.workgroup ON (((timecostcalcs.workgroup)::text = (workgroup.workgroup)::text)))
  GROUP BY workgroup.profitcentre, timecostcalcs.project, timecostcalcs.jobcode,
        CASE workgroup.profitcentre
            WHEN 'Path'::text THEN 'Surveillance/Pathology'::text
            WHEN 'vetr'::text THEN 'Surveillance/Pathology'::text
            ELSE 'Laboratory Testing'::text
        END
 HAVING (((timecostcalcs.project)::text = 'TG0100'::text) AND (max(timecostcalcs.month) <= ( SELECT max(tblperiod.endperiod) AS endperiod
           FROM fps.tblperiod
          WHERE (tblperiod.finalsummariesrun = '-1'::integer))));
