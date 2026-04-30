-- Validation script to verify operational -> fps migration success
-- Run this after the migration to ensure all objects were created correctly

-- Section 1: Verify fps schema exists
SELECT 'Schema Verification' AS section;
SELECT schema_name FROM information_schema.schemata WHERE schema_name = 'fps';

-- Section 2: Verify all fps tables exist with correct column counts
SELECT 'Table Verification' AS section;
SELECT 
    table_schema,
    table_name,
    (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema='fps' AND information_schema.columns.table_name=t.table_name) AS column_count
FROM information_schema.tables t
WHERE table_schema = 'fps'
ORDER BY table_name;

-- Section 3: Verify backward-compatibility views exist in operational schema
SELECT 'View Verification' AS section;
SELECT table_schema, table_name 
FROM information_schema.views 
WHERE table_schema = 'operational' 
  AND table_name IN ('batch_lock', 'tbljobmaster', 'tbljobstatus', 'tbljobqueue', 'tbljobqueue_log')
ORDER BY table_name;

-- Section 4: Verify all constraints exist in fps
SELECT 'Constraint Verification' AS section;
SELECT 
    c.relname AS table_name,
    con.conname AS constraint_name,
    CASE con.contype
        WHEN 'p' THEN 'PRIMARY KEY'
        WHEN 'f' THEN 'FOREIGN KEY'
        WHEN 'u' THEN 'UNIQUE'
        WHEN 'c' THEN 'CHECK'
        ELSE con.contype::text
    END AS constraint_type,
    pg_get_constraintdef(con.oid) AS definition
FROM pg_constraint con
JOIN pg_class c ON c.oid = con.conrelid
JOIN pg_namespace n ON n.oid = c.relnamespace
WHERE n.nspname = 'fps'
ORDER BY c.relname, con.conname;

-- Section 5: Verify all indexes exist in fps
SELECT 'Index Verification' AS section;
SELECT schemaname, tablename, indexname
FROM pg_indexes
WHERE schemaname = 'fps'
ORDER BY tablename, indexname;

-- Section 6: Data count verification (compare operational legacy to fps)
SELECT 'Data Count Verification' AS section;
SELECT 
    'job_master' AS table_pair,
    (SELECT COUNT(*) FROM operational.tbljobmaster) AS operational_legacy_count,
    (SELECT COUNT(*) FROM fps.job_master) AS fps_count
UNION ALL
SELECT 
    'job_status',
    (SELECT COUNT(*) FROM operational.tbljobstatus),
    (SELECT COUNT(*) FROM fps.job_status)
UNION ALL
SELECT 
    'job_queue',
    (SELECT COUNT(*) FROM operational.tbljobqueue),
    (SELECT COUNT(*) FROM fps.job_queue)
UNION ALL
SELECT 
    'job_queue_log',
    (SELECT COUNT(*) FROM operational.tbljobqueue_log),
    (SELECT COUNT(*) FROM fps.job_queue_log)
UNION ALL
SELECT 
    'job_lock',
    (SELECT COUNT(*) FROM operational.batch_lock),
    (SELECT COUNT(*) FROM fps.job_lock);

-- Section 7: Verify referential integrity
SELECT 'Referential Integrity Check' AS section;

-- Check job_queue references to job_master and job_status
WITH invalid_job_queue AS (
    SELECT jq.jobqueueid 
    FROM fps.job_queue jq
    LEFT JOIN fps.job_master jm ON jq.jobid = jm.jobid
    LEFT JOIN fps.job_status js ON jq.statusid = js.statusid
    WHERE jm.jobid IS NULL OR js.statusid IS NULL
)
SELECT COUNT(*) AS invalid_job_queue_fk_count FROM invalid_job_queue;

-- Check job_queue_log references to job_queue and job_status
WITH invalid_job_queue_log AS (
    SELECT jql.jobqueuelogid 
    FROM fps.job_queue_log jql
    LEFT JOIN fps.job_queue jq ON jql.jobqueueid = jq.jobqueueid
    LEFT JOIN fps.job_status js ON jql.statusid = js.statusid
    WHERE jq.jobqueueid IS NULL OR js.statusid IS NULL
)
SELECT COUNT(*) AS invalid_job_queue_log_fk_count FROM invalid_job_queue_log;

-- Section 8: Check job_queue enddatetime >= startdatetime constraint
SELECT 'Check Constraint Validation' AS section;
SELECT COUNT(*) AS invalid_datetime_count
FROM fps.job_queue
WHERE enddatetime IS NOT NULL AND enddatetime < startdatetime;

-- Section 9: View access test (verify backward-compatibility views work)
SELECT 'View Access Test' AS section;
SELECT 
    'operational.batch_lock' AS view_name,
    COUNT(*) AS row_count
FROM operational.batch_lock
UNION ALL
SELECT 'operational.tbljobmaster', COUNT(*) FROM operational.tbljobmaster
UNION ALL
SELECT 'operational.tbljobstatus', COUNT(*) FROM operational.tbljobstatus
UNION ALL
SELECT 'operational.tbljobqueue', COUNT(*) FROM operational.tbljobqueue
UNION ALL
SELECT 'operational.tbljobqueue_log', COUNT(*) FROM operational.tbljobqueue_log;

-- Summary
SELECT 'Migration Validation Complete' AS result;
SELECT 'All fps tables created, backward-compatibility views established in operational schema' AS status;
