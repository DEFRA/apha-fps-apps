--liquibase formatted sql

--changeset repo-admin:CR057 labels:ddl context:all splitStatements:false

-- CR057: Add job/audit tracking columns to fps.rate_change_history (jobid, fpsyear,
-- changetype, requestedby, approvedby, appliedatutc), link jobid to fps.job_master,
-- retype businesskey from text (CR044) to jsonb per CR045 worker spec, and relax the
-- legacy CR044 NOT NULL constraints on operationtype/changedatutc that the worker
-- never populates.

-- Step 1: new job/audit columns.
ALTER TABLE fps.rate_change_history
    ADD COLUMN IF NOT EXISTS jobid integer,
    ADD COLUMN IF NOT EXISTS fpsyear integer,
    ADD COLUMN IF NOT EXISTS changetype varchar(20),
    ADD COLUMN IF NOT EXISTS requestedby varchar(100),
    ADD COLUMN IF NOT EXISTS approvedby varchar(100),
    ADD COLUMN IF NOT EXISTS appliedatutc timestamptz DEFAULT now();

-- Step 2: FK and indexes for jobid / fpsyear.
ALTER TABLE fps.rate_change_history
    DROP CONSTRAINT IF EXISTS fk_rate_change_history_job_master;

ALTER TABLE fps.rate_change_history
    ADD CONSTRAINT fk_rate_change_history_job_master
        FOREIGN KEY (jobid)
        REFERENCES fps.job_master (jobid);

CREATE INDEX IF NOT EXISTS idx_rate_change_history_jobid
    ON fps.rate_change_history (jobid);

CREATE INDEX IF NOT EXISTS idx_rate_change_history_fpsyear
    ON fps.rate_change_history (fpsyear);

-- Step 3: businesskey was created as text in CR044 but CR045 spec and worker use jsonb.
-- Safe to run only when table has 0 rows; otherwise cast may fail on non-JSON text.
ALTER TABLE fps.rate_change_history
    ALTER COLUMN businesskey TYPE jsonb USING businesskey::jsonb;

-- Step 4: legacy CR044 columns (operationtype, changedatutc) are NOT NULL but the
-- worker never populates them; drop constraints so they coexist as nullable leftovers.
ALTER TABLE fps.rate_change_history ALTER COLUMN operationtype DROP NOT NULL;
ALTER TABLE fps.rate_change_history ALTER COLUMN changedatutc DROP NOT NULL;

--rollback ALTER TABLE fps.rate_change_history ALTER COLUMN changedatutc SET NOT NULL;
--rollback ALTER TABLE fps.rate_change_history ALTER COLUMN operationtype SET NOT NULL;
--rollback ALTER TABLE fps.rate_change_history ALTER COLUMN businesskey TYPE text USING businesskey::text;
--rollback DROP INDEX IF EXISTS fps.idx_rate_change_history_fpsyear;
--rollback DROP INDEX IF EXISTS fps.idx_rate_change_history_jobid;
--rollback ALTER TABLE fps.rate_change_history DROP CONSTRAINT IF EXISTS fk_rate_change_history_job_master;
--rollback ALTER TABLE fps.rate_change_history DROP COLUMN IF EXISTS appliedatutc;
--rollback ALTER TABLE fps.rate_change_history DROP COLUMN IF EXISTS approvedby;
--rollback ALTER TABLE fps.rate_change_history DROP COLUMN IF EXISTS requestedby;
--rollback ALTER TABLE fps.rate_change_history DROP COLUMN IF EXISTS changetype;
--rollback ALTER TABLE fps.rate_change_history DROP COLUMN IF EXISTS fpsyear;
--rollback ALTER TABLE fps.rate_change_history DROP COLUMN IF EXISTS jobid;
