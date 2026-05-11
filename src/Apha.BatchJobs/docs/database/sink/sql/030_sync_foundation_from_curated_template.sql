-- Sync curated records into fps foundation tables.
-- Requires psql variable: snapshot_id (UUID string)
-- Example:
-- psql "$SINK_CONN" -v ON_ERROR_STOP=1 -v snapshot_id="7e4b9f7c-7f0d-4ecf-ab26-7482f2d8cebb" -f database/sink/sql/030_sync_foundation_from_curated_template.sql

BEGIN;

WITH run_row AS (
	INSERT INTO sink_meta.pipeline_run (
		run_id,
		snapshot_id,
		stage,
		status
	)
	VALUES (
		gen_random_uuid(),
		:'snapshot_id'::UUID,
		'foundation_sync',
		'started'
	)
	RETURNING run_id
),
upsert_jobmaster AS (
	INSERT INTO fps.job_master (
		jobname,
		frequency,
		note,
		timetolive
	)
	SELECT
		c.job_name,
		c.frequency,
		c.note,
		c.time_to_live
	FROM sink_curated.job_seed c
	WHERE c.source_snapshot_id = :'snapshot_id'::UUID
	ON CONFLICT (jobname)
	DO UPDATE SET
		frequency = EXCLUDED.frequency,
		note = EXCLUDED.note,
		timetolive = EXCLUDED.timetolive,
		updated_at = NOW()
	RETURNING jobid
),
insert_statuses AS (
	INSERT INTO fps.job_status (jobid, status)
	SELECT jm.jobid, s.status_name
	FROM fps.job_master jm
	INNER JOIN sink_curated.job_seed c
		ON c.job_name = jm.jobname
	CROSS JOIN (VALUES
		('Running'),
		('Completed'),
		('Failed'),
		('Cancelled'),
		('Skipped')
	) AS s(status_name)
	WHERE c.source_snapshot_id = :'snapshot_id'::UUID
	ON CONFLICT (jobid, status)
	DO NOTHING
	RETURNING 1
)
UPDATE sink_meta.pipeline_run pr
SET
	completed_at = NOW(),
	status = 'succeeded',
	rows_read = (SELECT COUNT(*) FROM sink_curated.job_seed c WHERE c.source_snapshot_id = :'snapshot_id'::UUID),
	rows_written = (SELECT COALESCE((SELECT COUNT(*) FROM upsert_jobmaster), 0) + COALESCE((SELECT COUNT(*) FROM insert_statuses), 0))
WHERE pr.run_id = (SELECT run_id FROM run_row);

COMMIT;
