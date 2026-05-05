-- Seed: deterministic baseline run + step audit + validation expectation rows for ScheduledLoadFromFps.
-- Depends on: 001_seed_scheduled_job_master.sql
-- Safe to re-run: uses deterministic run_id and ON CONFLICT guards.

BEGIN;

WITH seed_run AS (
    INSERT INTO fps.scheduled_load_run (
        run_id,
        job_name,
        fps_year,
        job_started_at,
        job_completed_at,
        final_status,
        correlation_id,
        created_at
    )
    VALUES (
        '00000000-0000-0000-0000-000000000001',
        'ScheduledLoadFromFps',
        2025,
        NOW(),
        NOW(),
        'Success',
        'baseline-seed',
        NOW()
    )
    ON CONFLICT (run_id)
    DO UPDATE SET
        job_name = EXCLUDED.job_name,
        fps_year = EXCLUDED.fps_year,
        job_completed_at = EXCLUDED.job_completed_at,
        final_status = EXCLUDED.final_status,
        correlation_id = EXCLUDED.correlation_id
    RETURNING run_id
),
resolved_run AS (
    SELECT run_id FROM seed_run
    UNION ALL
    SELECT run_id
    FROM fps.scheduled_load_run
    WHERE run_id = '00000000-0000-0000-0000-000000000001'
    LIMIT 1
)
INSERT INTO fps.scheduled_load_step_run (
    step_run_id,
    run_id,
    step_name,
    step_sequence,
    started_at,
    completed_at,
    step_status,
    error_message,
    rows_affected,
    created_at
)
VALUES
    ('00000000-0000-0000-0000-000000000101', (SELECT run_id FROM resolved_run), 'ProcessPreviousYearTotals', 1, NOW(), NOW(), 'Completed', NULL, 3, NOW()),
    ('00000000-0000-0000-0000-000000000102', (SELECT run_id FROM resolved_run), 'ProcessCurrentYearTotals', 2, NOW(), NOW(), 'Completed', NULL, 3, NOW()),
    ('00000000-0000-0000-0000-000000000103', (SELECT run_id FROM resolved_run), 'DeleteYearsFpsData', 3, NOW(), NOW(), 'Completed', NULL, 24, NOW()),
    ('00000000-0000-0000-0000-000000000104', (SELECT run_id FROM resolved_run), 'AddYearsFpsData', 4, NOW(), NOW(), 'Completed', NULL, 24, NOW()),
    ('00000000-0000-0000-0000-000000000105', (SELECT run_id FROM resolved_run), 'HandleCurrentYearProjectAll', 5, NOW(), NOW(), 'Completed', NULL, 3, NOW())
ON CONFLICT (step_run_id)
DO UPDATE SET
    run_id = EXCLUDED.run_id,
    step_name = EXCLUDED.step_name,
    step_sequence = EXCLUDED.step_sequence,
    started_at = EXCLUDED.started_at,
    completed_at = EXCLUDED.completed_at,
    step_status = EXCLUDED.step_status,
    error_message = EXCLUDED.error_message,
    rows_affected = EXCLUDED.rows_affected,
    created_at = EXCLUDED.created_at;

WITH resolved_run AS (
    SELECT run_id
    FROM fps.scheduled_load_run
    WHERE run_id = '00000000-0000-0000-0000-000000000001'
)
INSERT INTO fps.scheduled_load_validation_result (
    validation_id,
    run_id,
    assertion_code,
    assertion_description,
    expected_value,
    actual_value,
    passed,
    checked_at,
    created_at
)
VALUES
    (gen_random_uuid(), (SELECT run_id FROM resolved_run), 'BASELINE_001', 'Total archived projects in mabarchive.my_fpsyeartotals for 2025 should be 3', 3, NULL, FALSE, NOW(), NOW()),
    (gen_random_uuid(), (SELECT run_id FROM resolved_run), 'BASELINE_002', 'Sum of totalcosts in mabarchive.my_fpsyeartotals for 2025 should equal 77250.00', 77250, NULL, FALSE, NOW(), NOW()),
    (gen_random_uuid(), (SELECT run_id FROM resolved_run), 'BASELINE_003', 'Sum of totalincome in mabarchive.my_fpsyeartotals for 2025 should equal 136000.00', 136000, NULL, FALSE, NOW(), NOW())
ON CONFLICT (run_id, assertion_code)
DO UPDATE SET
    assertion_description = EXCLUDED.assertion_description,
    expected_value = EXCLUDED.expected_value,
    checked_at = EXCLUDED.checked_at;

COMMIT;