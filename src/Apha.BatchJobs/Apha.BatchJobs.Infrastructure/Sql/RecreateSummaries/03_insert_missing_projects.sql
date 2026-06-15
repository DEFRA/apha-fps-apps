-- 03_insert_missing_projects.sql
-- Replaces: sp_InsertMissingProjects (single loop body iteration)
-- Called once per month (1-12) by C# loop in InsertMissingProjectsStep.
-- @month is supplied as a parameter by the C# step for each iteration.
-- Syntax changes:
--   dbo.tlkpProject  -> fps.tlkpproject
--   dbo.ProjectMonth -> fps.projectmonth
--   [ColumnName]     -> columnname

INSERT INTO fps.projectmonth (project, monthno)
SELECT DISTINCT tlkpproject.parentproject,
                @month AS monthno
FROM fps.tlkpproject
LEFT JOIN fps.projectmonth
    ON  tlkpproject.parentproject = projectmonth.project
    AND @month = projectmonth.monthno
WHERE projectmonth.project IS NULL
ORDER BY parentproject;
