-- View: fps.vprogramcosts

CREATE OR REPLACE VIEW fps.vprogramcosts AS
 SELECT tlkpprogram.programno,
    sum((projectmonthfinal.totalcost)::numeric) AS programcost
   FROM ((fps.tlkpprogram
     JOIN fps.tlkpproject ON (((tlkpprogram.programno)::text = (tlkpproject.program)::text)))
     JOIN fps.projectmonthfinal ON (((tlkpproject.parentproject)::text = (projectmonthfinal.project)::text)))
  GROUP BY tlkpprogram.programno;
