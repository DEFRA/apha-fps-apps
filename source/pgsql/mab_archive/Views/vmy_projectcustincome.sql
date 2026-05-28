CREATE OR REPLACE VIEW mabarchive.vmy_projectcustincome AS
 SELECT COALESCE(pims.year, fps.year) AS year,
    COALESCE(pims.project, fps.parentproject) AS project,
    COALESCE(pims.pybudget, fps.custincome) AS custinc
   FROM mabarchive.my_fpsyeartotals fps
     FULL JOIN mabarchive.my_tlkpprojectradtrackdata pims ON fps.year = pims.year AND fps.parentproject::text = pims.project::text;
