-- Sync template from sink_curated into operational foundation tables.
-- Keep this deterministic and idempotent.

BEGIN;

-- 1) Upsert job definitions into operational.tbljobmaster.
-- INSERT INTO operational.tbljobmaster (jobname, frequency, note, timetolive)
-- SELECT c.job_name, c.frequency, c.note, c.time_to_live
-- FROM sink_curated.job_seed c
-- WHERE c.source_snapshot_id = :snapshot_id
-- ON CONFLICT (jobname)
-- DO UPDATE SET
--     frequency = EXCLUDED.frequency,
--     note = EXCLUDED.note,
--     timetolive = EXCLUDED.timetolive,
--     updated_at = NOW();

-- 2) Optional: seed expected statuses per job.
-- INSERT INTO operational.tbljobstatus (jobid, status)
-- SELECT jm.jobid, s.status_name
-- FROM operational.tbljobmaster jm
-- CROSS JOIN (VALUES ('Running'), ('Completed'), ('Failed'), ('Cancelled'), ('Skipped')) AS s(status_name)
-- ON CONFLICT (jobid, status) DO NOTHING;

COMMIT;
