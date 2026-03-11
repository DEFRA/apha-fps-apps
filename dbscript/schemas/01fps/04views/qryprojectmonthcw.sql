-- View: fps.qryprojectmonthcw

CREATE OR REPLACE VIEW fps.qryprojectmonthcw AS
 SELECT DISTINCT projectmonth.project,
    projectmonth.monthno,
    (tlkpproject.plancaseworkdebit / 12) AS cwdebit,
    ((tlkpproject.transferincome * (tlkpproject.caseworksub)::double precision) / 12) AS cwcredit
   FROM (fps.tlkpproject
     JOIN fps.projectmonth ON (((tlkpproject.parentproject)::text = (projectmonth.project)::text)));
