-- 07_create_project_month_casework.sql
-- Replaces: sp_CreateProjectMonthCasework
-- Syntax changes:
--   dbo.ProjectMonthCasework  -> fps.projectmonthcasework
--   dbo.qryProjectMonthCW     -> fps.qryprojectmonthcw

INSERT INTO fps.projectmonthcasework
SELECT DISTINCT
    qryprojectmonthcw.project,
    qryprojectmonthcw.monthno,
    qryprojectmonthcw.cwdebit::numeric::double precision,
    qryprojectmonthcw.cwcredit::numeric::double precision
FROM fps.qryprojectmonthcw;
