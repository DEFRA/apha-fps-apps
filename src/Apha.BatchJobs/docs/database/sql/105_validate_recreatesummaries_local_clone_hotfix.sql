-- 105_validate_recreatesummaries_local_clone_hotfix.sql
-- Purpose:
--   Post-check script for 104_recreatesummaries_local_clone_hotfix.sql.
--
-- Expected outcomes after successful local remediation:
--   projectmonth_null_fpsyear = 0
--   ut_projects_remaining = 0
--   over_len_program = 0
--   projectmonth_fpsyear_nullable = NO

SELECT 'projectmonth_null_fpsyear' AS check_name,
       COUNT(*)::text AS check_value
FROM fps.projectmonth
WHERE fpsyear IS NULL;

SELECT 'ut_projects_remaining' AS check_name,
       COUNT(*)::text AS check_value
FROM fps.tlkpproject
WHERE parentproject::text LIKE 'UT%';

SELECT 'over_len_program' AS check_name,
       COUNT(*)::text AS check_value
FROM fps.tlkpproject
WHERE length(program::text) > 10;

SELECT 'projectmonth_fpsyear_nullable' AS check_name,
       is_nullable AS check_value
FROM information_schema.columns
WHERE table_schema = 'fps'
  AND table_name = 'projectmonth'
  AND column_name = 'fpsyear';

-- Visibility queries for DBA review
SELECT parentproject::text, program::text, fpsyear
FROM fps.tlkpproject
WHERE length(program::text) > 10
ORDER BY fpsyear, parentproject
LIMIT 50;

SELECT project::text, COUNT(*) AS null_rows
FROM fps.projectmonth
WHERE fpsyear IS NULL
GROUP BY project
ORDER BY null_rows DESC, project
LIMIT 50;
