-- Validate required ScheduledLoadFromFps table footprint in current Postgres design.
-- Design target: single database, two schemas (fps, mabarchive).
-- This script is read-only and raises an exception when required objects are missing.

BEGIN;

DO $$
DECLARE
    missing_tables TEXT[] := ARRAY[]::TEXT[];
    rec RECORD;
BEGIN
    -- Required source and archive tables for ScheduledLoadFromFps baseline.
    FOR rec IN
        SELECT *
        FROM (VALUES
            ('fps', 'fpsyeartotals'),
            ('fps', 'tlkpproject'),
            ('mabarchive', 'my_fpsyeartotals'),
            ('mabarchive', 'my_tlkpproject_all'),
            ('mabarchive', 'my_monthlyoutput'),
            ('mabarchive', 'my_monthlytime'),
            ('mabarchive', 'my_projectmonthfinal'),
            ('mabarchive', 'my_proj_invoice'),
            ('mabarchive', 'my_proj_subcontract'),
            ('mabarchive', 'my_profitcentregrade'),
            ('mabarchive', 'my_staff'),
            ('mabarchive', 'my_tbladditionalcosts'),
            ('mabarchive', 'my_tblanimalreq'),
            ('mabarchive', 'my_tblanimals'),
            ('mabarchive', 'my_tblcontract'),
            ('mabarchive', 'my_tblprofitcentre'),
            ('mabarchive', 'my_tblstaffjob'),
            ('mabarchive', 'my_testorproduct'),
            ('mabarchive', 'my_timecostcalcs'),
            ('mabarchive', 'my_tlkpprogram'),
            ('mabarchive', 'my_tlkpproject'),
            ('mabarchive', 'my_tlkpproject_all'),
            ('mabarchive', 'my_tlkptestreqmt'),
            ('mabarchive', 'my_workgroup'),
            ('mabarchive', 'my_workgroupgrade'),
            ('mabarchive', 'g_tlkpproject'),
            ('mabarchive', 'tlkpyear')
        ) AS t(schema_name, table_name)
    LOOP
        IF NOT EXISTS (
            SELECT 1
            FROM information_schema.tables it
            WHERE it.table_schema = rec.schema_name
              AND it.table_name = rec.table_name
        ) THEN
            missing_tables := array_append(missing_tables, rec.schema_name || '.' || rec.table_name);
        END IF;
    END LOOP;

    IF array_length(missing_tables, 1) IS NOT NULL THEN
        RAISE EXCEPTION 'ScheduledLoad validation failed. Missing tables: %', array_to_string(missing_tables, ', ');
    END IF;
END $$;

-- Enforce key parity for core year-scoped archive tables.
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu
          ON tc.constraint_name = kcu.constraint_name
         AND tc.table_schema = kcu.table_schema
         AND tc.table_name = kcu.table_name
        WHERE tc.constraint_type = 'PRIMARY KEY'
          AND tc.table_schema = 'mabarchive'
          AND tc.table_name = 'my_fpsyeartotals'
        GROUP BY tc.constraint_name
        HAVING COUNT(*) = 2
           AND SUM(CASE WHEN kcu.column_name = 'year' THEN 1 ELSE 0 END) = 1
           AND SUM(CASE WHEN kcu.column_name = 'parentproject' THEN 1 ELSE 0 END) = 1
    ) THEN
        RAISE EXCEPTION 'Expected composite PK (year, parentproject) on mabarchive.my_fpsyeartotals.';
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu
          ON tc.constraint_name = kcu.constraint_name
         AND tc.table_schema = kcu.table_schema
         AND tc.table_name = kcu.table_name
        WHERE tc.constraint_type = 'PRIMARY KEY'
          AND tc.table_schema = 'mabarchive'
          AND tc.table_name = 'my_tlkpproject_all'
        GROUP BY tc.constraint_name
        HAVING COUNT(*) = 2
           AND SUM(CASE WHEN kcu.column_name = 'year' THEN 1 ELSE 0 END) = 1
           AND SUM(CASE WHEN kcu.column_name = 'parentproject' THEN 1 ELSE 0 END) = 1
    ) THEN
        RAISE EXCEPTION 'Expected composite PK (year, parentproject) on mabarchive.my_tlkpproject_all.';
    END IF;
END $$;

-- Positive evidence for logs/automation.
SELECT 'ScheduledLoad required table validation passed.' AS validation_status;

COMMIT;
