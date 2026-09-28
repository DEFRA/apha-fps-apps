--liquibase formatted sql

--changeset repo-admin:CR074 labels:ddl context:all splitStatements:false

BEGIN;

-- ============================================================================
-- CR074
--
-- Purpose:
--   1. Correct fps.notification_run_summary to the current application schema.
--   2. Recreate fps.notification_delivery partitioned by LIST(fpsyear).
--   3. Recreate fps.notification_delivery_project partitioned by LIST(fpsyear).
--
-- Prerequisite:
--   CR071 must have completed successfully.
--
-- Design:
--   - fps.job_queue remains unpartitioned.
--   - fps.notification_run_summary is already partitioned by CR071.
--   - fps.notification_delivery is partitioned by LIST(fpsyear).
--   - fps.notification_delivery_project is partitioned by LIST(fpsyear).
-- ============================================================================


-- ============================================================================
-- 0. Preconditions
-- ============================================================================

DO $$
BEGIN
    IF to_regclass('fps.notification_run_summary') IS NULL THEN
        RAISE EXCEPTION
            'CR074 precondition failed: fps.notification_run_summary does not exist.';
    END IF;

    -- CR071 must already have converted notification_run_summary
    -- to a partitioned table.
    IF NOT EXISTS (
        SELECT 1
        FROM pg_partitioned_table
        WHERE partrelid = 'fps.notification_run_summary'::regclass
    ) THEN
        RAISE EXCEPTION
            'CR074 precondition failed: fps.notification_run_summary is not partitioned. CR071 must run first.';
    END IF;
END
$$;


-- Prevent concurrent writes while the table is corrected.
LOCK TABLE fps.notification_run_summary
    IN ACCESS EXCLUSIVE MODE;


-- notification_run_summary UUID conversion is only approved while empty.
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM fps.notification_run_summary
    ) THEN
        RAISE EXCEPTION
            'CR074 precondition failed: fps.notification_run_summary contains data. Reassess UUID conversion before execution.';
    END IF;
END
$$;


-- notification_delivery and notification_delivery_project will be dropped
-- and recreated. If they exist, lock them and verify that they are empty.
DO $$
DECLARE
    v_has_rows boolean;
BEGIN
    IF to_regclass('fps.notification_delivery_project') IS NOT NULL THEN

        EXECUTE
            'LOCK TABLE fps.notification_delivery_project IN ACCESS EXCLUSIVE MODE';

        EXECUTE
            'SELECT EXISTS (
                 SELECT 1
                 FROM fps.notification_delivery_project
                 LIMIT 1
             )'
        INTO v_has_rows;

        IF v_has_rows THEN
            RAISE EXCEPTION
                'CR074 precondition failed: fps.notification_delivery_project contains data.';
        END IF;
    END IF;


    IF to_regclass('fps.notification_delivery') IS NOT NULL THEN

        EXECUTE
            'LOCK TABLE fps.notification_delivery IN ACCESS EXCLUSIVE MODE';

        EXECUTE
            'SELECT EXISTS (
                 SELECT 1
                 FROM fps.notification_delivery
                 LIMIT 1
             )'
        INTO v_has_rows;

        IF v_has_rows THEN
            RAISE EXCEPTION
                'CR074 precondition failed: fps.notification_delivery contains data.';
        END IF;
    END IF;
END
$$;


-- ============================================================================
-- 1. fps.notification_run_summary
--    Already partitioned by CR071.
--    Correct schema to match the current application contract.
-- ============================================================================


-- Convert identifier from bigint identity/serial to UUID.
ALTER TABLE fps.notification_run_summary
    ALTER COLUMN notificationrunsummaryid DROP DEFAULT;


ALTER TABLE fps.notification_run_summary
    ALTER COLUMN notificationrunsummaryid
    TYPE uuid
    USING gen_random_uuid();


ALTER TABLE fps.notification_run_summary
    ALTER COLUMN notificationrunsummaryid
    SET DEFAULT gen_random_uuid();


-- The old bigint sequence is no longer required.
-- CR071 provides:
--   12 yearly partitions (2016..2027)
--   + 1 DEFAULT partition
--   = 13 partitions.
DROP SEQUENCE IF EXISTS
    fps.notification_run_summary_notificationrunsummaryid_seq
CASCADE;


-- Add all columns required by the current application schema.
ALTER TABLE fps.notification_run_summary

    ADD COLUMN IF NOT EXISTS notificationtype
        varchar(64) NOT NULL DEFAULT 'MilestoneUpdate',

    ADD COLUMN IF NOT EXISTS candidatecount
        integer NOT NULL DEFAULT 0,

    ADD COLUMN IF NOT EXISTS identifiedrecipientcount
        integer NOT NULL DEFAULT 0,

    ADD COLUMN IF NOT EXISTS managerdeliverycount
        integer NOT NULL DEFAULT 0,

    ADD COLUMN IF NOT EXISTS manageremailattemptcount
        integer NOT NULL DEFAULT 0,

    ADD COLUMN IF NOT EXISTS manageremailsentcount
        integer NOT NULL DEFAULT 0,

    ADD COLUMN IF NOT EXISTS manageremailfailedcount
        integer NOT NULL DEFAULT 0,

    ADD COLUMN IF NOT EXISTS manageremailskippedcount
        integer NOT NULL DEFAULT 0,

    ADD COLUMN IF NOT EXISTS disabledrecipientcount
        integer NOT NULL DEFAULT 0,

    ADD COLUMN IF NOT EXISTS disabledprojectcount
        integer NOT NULL DEFAULT 0,

    ADD COLUMN IF NOT EXISTS missingemailrecipientcount
        integer NOT NULL DEFAULT 0,

    ADD COLUMN IF NOT EXISTS missingemailprojectcount
        integer NOT NULL DEFAULT 0,

    ADD COLUMN IF NOT EXISTS diagnosticavailable
        boolean NOT NULL DEFAULT true,

    ADD COLUMN IF NOT EXISTS outcomeunknownrecipientcount
        integer NOT NULL DEFAULT 0,

    ADD COLUMN IF NOT EXISTS duplicateskippedcount
        integer NOT NULL DEFAULT 0,

    ADD COLUMN IF NOT EXISTS capssummarystatus
        varchar(32),

    ADD COLUMN IF NOT EXISTS capssummarysentatutc
        timestamptz,

    ADD COLUMN IF NOT EXISTS capssummaryfailuremessage
        text,

    ADD COLUMN IF NOT EXISTS updatedatutc
        timestamptz NOT NULL DEFAULT now();


-- Required by the current application contract.
-- Safe because the table was verified empty above.
ALTER TABLE fps.notification_run_summary
    ALTER COLUMN capssummarystatus SET NOT NULL;


-- Rename legacy created timestamp if the old name is still present.
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'fps'
          AND table_name = 'notification_run_summary'
          AND column_name = 'created_at_utc'
    )
    AND NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'fps'
          AND table_name = 'notification_run_summary'
          AND column_name = 'createdatutc'
    ) THEN

        ALTER TABLE fps.notification_run_summary
            RENAME COLUMN created_at_utc
            TO createdatutc;

    END IF;
END
$$;


-- Remove columns from the stale draft schema.
ALTER TABLE fps.notification_run_summary
    DROP COLUMN IF EXISTS recipientcount,
    DROP COLUMN IF EXISTS deliveryattemptcount,
    DROP COLUMN IF EXISTS deliverysentcount,
    DROP COLUMN IF EXISTS deliveryfailedcount,
    DROP COLUMN IF EXISTS caps_delivery_status,
    DROP COLUMN IF EXISTS caps_sent_at_utc,
    DROP COLUMN IF EXISTS caps_failure_reason;


-- Application uses:
--
--     ON CONFLICT (jobqueueid, fpsyear)
--
-- CR071 should already have created this constraint.
-- Guard creation so CR074 works correctly in either case.
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conrelid =
              'fps.notification_run_summary'::regclass
          AND conname =
              'uq_notification_run_summary_jobqueueid_fpsyear'
    ) THEN

        ALTER TABLE fps.notification_run_summary
            ADD CONSTRAINT
                uq_notification_run_summary_jobqueueid_fpsyear
            UNIQUE (
                jobqueueid,
                fpsyear
            );

    END IF;
END
$$;


-- ============================================================================
-- 2. fps.notification_delivery
--    Durable recipient-delivery audit partitioned by FPS year.
-- ============================================================================

DROP TABLE IF EXISTS fps.notification_delivery_project;
DROP TABLE IF EXISTS fps.notification_delivery;


CREATE TABLE fps.notification_delivery
(
    notificationdeliveryid uuid
        NOT NULL
        DEFAULT gen_random_uuid(),

    jobqueueid uuid
        NOT NULL,

    notificationtype varchar(64)
        NOT NULL
        DEFAULT 'MilestoneUpdate',

    fpsyear integer
        NOT NULL,

    monthnumber integer
        NOT NULL,

    recipientid varchar(64)
        NOT NULL,

    durablepersonid varchar(64),

    recipientname varchar(128)
        NOT NULL,

    recipientemail varchar(320),

    deliverystatus varchar(32)
        NOT NULL,

    outcomereason varchar(32),

    isforceresend boolean
        NOT NULL
        DEFAULT false,

    sentatutc timestamptz,

    failuremessage text,

    templateversion varchar(32)
        NOT NULL,

    createdatutc timestamptz
        NOT NULL
        DEFAULT now(),

    CONSTRAINT notification_delivery_pkey
        PRIMARY KEY (
            notificationdeliveryid,
            fpsyear
        ),

    CONSTRAINT fk_notification_delivery_jobqueue
        FOREIGN KEY (jobqueueid)
        REFERENCES fps.job_queue (jobqueueid)
)
PARTITION BY LIST (fpsyear);


-- Create yearly partitions 2016..2027.
DO $$
DECLARE
    y integer;
BEGIN
    FOR y IN 2016..2027 LOOP

        EXECUTE format(
            'CREATE TABLE fps.notification_delivery_y%s
             PARTITION OF fps.notification_delivery
             FOR VALUES IN (%s)',
            y,
            y
        );

    END LOOP;
END
$$;


-- Safety partition for a year not yet explicitly provisioned.
CREATE TABLE fps.notification_delivery_default
    PARTITION OF fps.notification_delivery
    DEFAULT;


CREATE INDEX idx_notification_delivery_business_key
    ON fps.notification_delivery
    (
        notificationtype,
        fpsyear,
        monthnumber,
        recipientid,
        deliverystatus
    );


CREATE INDEX idx_notification_delivery_jobqueueid
    ON fps.notification_delivery (jobqueueid);


-- ============================================================================
-- 3. fps.notification_delivery_project
--    Durable project-level audit partitioned by the same FPS-year key.
-- ============================================================================

CREATE TABLE fps.notification_delivery_project
(
    notificationdeliveryprojectid uuid
        NOT NULL
        DEFAULT gen_random_uuid(),

    notificationdeliveryid uuid
        NOT NULL,

    fpsyear integer
        NOT NULL,

    projectcode varchar(20)
        NOT NULL,

    deliverystatus varchar(32)
        NOT NULL,

    outcomereason varchar(32),

    CONSTRAINT notification_delivery_project_pkey
        PRIMARY KEY (
            notificationdeliveryprojectid,
            fpsyear
        ),

    CONSTRAINT fk_notification_delivery_project_parent
        FOREIGN KEY (
            notificationdeliveryid,
            fpsyear
        )
        REFERENCES fps.notification_delivery
        (
            notificationdeliveryid,
            fpsyear
        )
        ON DELETE CASCADE
)
PARTITION BY LIST (fpsyear);


DO $$
DECLARE
    y integer;
BEGIN
    FOR y IN 2016..2027 LOOP

        EXECUTE format(
            'CREATE TABLE fps.notification_delivery_project_y%s
             PARTITION OF fps.notification_delivery_project
             FOR VALUES IN (%s)',
            y,
            y
        );

    END LOOP;
END
$$;


CREATE TABLE fps.notification_delivery_project_default
    PARTITION OF fps.notification_delivery_project
    DEFAULT;


CREATE INDEX idx_notification_delivery_project_parent
    ON fps.notification_delivery_project
    (
        notificationdeliveryid,
        fpsyear
    );


-- ============================================================================
-- 4. Postconditions
-- ============================================================================

DO $$
DECLARE
    v_delivery_partition_count integer;
    v_project_partition_count integer;
BEGIN

    -- notification_run_summary must still be partitioned.
    IF NOT EXISTS (
        SELECT 1
        FROM pg_partitioned_table
        WHERE partrelid =
              'fps.notification_run_summary'::regclass
    ) THEN
        RAISE EXCEPTION
            'CR074 postcondition failed: notification_run_summary is not partitioned.';
    END IF;


    -- notification_delivery must be partitioned.
    IF NOT EXISTS (
        SELECT 1
        FROM pg_partitioned_table
        WHERE partrelid =
              'fps.notification_delivery'::regclass
    ) THEN
        RAISE EXCEPTION
            'CR074 postcondition failed: notification_delivery is not partitioned.';
    END IF;


    -- notification_delivery_project must be partitioned.
    IF NOT EXISTS (
        SELECT 1
        FROM pg_partitioned_table
        WHERE partrelid =
              'fps.notification_delivery_project'::regclass
    ) THEN
        RAISE EXCEPTION
            'CR074 postcondition failed: notification_delivery_project is not partitioned.';
    END IF;


    -- notification_run_summary identifier must now be UUID.
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'fps'
          AND table_name = 'notification_run_summary'
          AND column_name = 'notificationrunsummaryid'
          AND data_type = 'uuid'
    ) THEN
        RAISE EXCEPTION
            'CR074 postcondition failed: notification_run_summary.notificationrunsummaryid is not uuid.';
    END IF;


    -- Composite delivery -> project FK must exist.
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conrelid =
              'fps.notification_delivery_project'::regclass
          AND conname =
              'fk_notification_delivery_project_parent'
    ) THEN
        RAISE EXCEPTION
            'CR074 postcondition failed: notification_delivery_project parent FK is missing.';
    END IF;


    -- Each newly created delivery table should have:
    -- 12 yearly partitions + DEFAULT = 13.
    SELECT COUNT(*)
    INTO v_delivery_partition_count
    FROM pg_inherits
    WHERE inhparent =
          'fps.notification_delivery'::regclass;


    IF v_delivery_partition_count <> 13 THEN
        RAISE EXCEPTION
            'CR074 postcondition failed: expected 13 notification_delivery partitions, found %.',
            v_delivery_partition_count;
    END IF;


    SELECT COUNT(*)
    INTO v_project_partition_count
    FROM pg_inherits
    WHERE inhparent =
          'fps.notification_delivery_project'::regclass;


    IF v_project_partition_count <> 13 THEN
        RAISE EXCEPTION
            'CR074 postcondition failed: expected 13 notification_delivery_project partitions, found %.',
            v_project_partition_count;
    END IF;

END
$$;


--rollback -- CR074 is destructive by design and intentionally drops/recreates the notification audit tables.
--rollback -- The rollback below is a best-effort cleanup only; use a database backup or data restoration
--rollback -- if the original table contents must be recovered.
--rollback DROP TABLE IF EXISTS fps.notification_delivery_project_default;
--rollback DROP TABLE IF EXISTS fps.notification_delivery_project;
--rollback DROP TABLE IF EXISTS fps.notification_delivery_default;
--rollback DROP TABLE IF EXISTS fps.notification_delivery;
--rollback ALTER TABLE fps.notification_run_summary DROP CONSTRAINT IF EXISTS uq_notification_run_summary_jobqueueid_fpsyear;
--rollback ALTER TABLE fps.notification_run_summary DROP COLUMN IF EXISTS updatedatutc;
--rollback ALTER TABLE fps.notification_run_summary DROP COLUMN IF EXISTS capssummaryfailuremessage;
--rollback ALTER TABLE fps.notification_run_summary DROP COLUMN IF EXISTS capssummarysentatutc;
--rollback ALTER TABLE fps.notification_run_summary DROP COLUMN IF EXISTS capssummarystatus;
--rollback ALTER TABLE fps.notification_run_summary DROP COLUMN IF EXISTS duplicateskippedcount;
--rollback ALTER TABLE fps.notification_run_summary DROP COLUMN IF EXISTS outcomeunknownrecipientcount;
--rollback ALTER TABLE fps.notification_run_summary DROP COLUMN IF EXISTS diagnosticavailable;
--rollback ALTER TABLE fps.notification_run_summary DROP COLUMN IF EXISTS missingemailprojectcount;
--rollback ALTER TABLE fps.notification_run_summary DROP COLUMN IF EXISTS missingemailrecipientcount;
--rollback ALTER TABLE fps.notification_run_summary DROP COLUMN IF EXISTS disabledprojectcount;
--rollback ALTER TABLE fps.notification_run_summary DROP COLUMN IF EXISTS disabledrecipientcount;
--rollback ALTER TABLE fps.notification_run_summary DROP COLUMN IF EXISTS manageremailskippedcount;
--rollback ALTER TABLE fps.notification_run_summary DROP COLUMN IF EXISTS manageremailfailedcount;
--rollback ALTER TABLE fps.notification_run_summary DROP COLUMN IF EXISTS manageremailsentcount;
--rollback ALTER TABLE fps.notification_run_summary DROP COLUMN IF EXISTS manageremailattemptcount;
--rollback ALTER TABLE fps.notification_run_summary DROP COLUMN IF EXISTS managerdeliverycount;
--rollback ALTER TABLE fps.notification_run_summary DROP COLUMN IF EXISTS identifiedrecipientcount;
--rollback ALTER TABLE fps.notification_run_summary DROP COLUMN IF EXISTS candidatecount;
--rollback ALTER TABLE fps.notification_run_summary DROP COLUMN IF EXISTS notificationtype;
--rollback ALTER TABLE fps.notification_run_summary DROP COLUMN IF EXISTS recipientcount;
--rollback ALTER TABLE fps.notification_run_summary DROP COLUMN IF EXISTS deliveryattemptcount;
--rollback ALTER TABLE fps.notification_run_summary DROP COLUMN IF EXISTS deliverysentcount;
--rollback ALTER TABLE fps.notification_run_summary DROP COLUMN IF EXISTS deliveryfailedcount;
--rollback ALTER TABLE fps.notification_run_summary DROP COLUMN IF EXISTS caps_delivery_status;
--rollback ALTER TABLE fps.notification_run_summary DROP COLUMN IF EXISTS caps_sent_at_utc;
--rollback ALTER TABLE fps.notification_run_summary DROP COLUMN IF EXISTS caps_failure_reason;
--rollback DO $$
--rollback BEGIN
--rollback     IF EXISTS (
--rollback         SELECT 1
--rollback         FROM information_schema.columns
--rollback         WHERE table_schema = 'fps'
--rollback           AND table_name = 'notification_run_summary'
--rollback           AND column_name = 'createdatutc'
--rollback     )
--rollback     AND NOT EXISTS (
--rollback         SELECT 1
--rollback         FROM information_schema.columns
--rollback         WHERE table_schema = 'fps'
--rollback           AND table_name = 'notification_run_summary'
--rollback           AND column_name = 'created_at_utc'
--rollback     ) THEN
--rollback         ALTER TABLE fps.notification_run_summary
--rollback             RENAME COLUMN createdatutc TO created_at_utc;
--rollback     END IF;
--rollback END
--rollback $$;
--rollback ALTER TABLE fps.notification_run_summary ALTER COLUMN notificationrunsummaryid DROP DEFAULT;
--rollback ALTER TABLE fps.notification_run_summary ALTER COLUMN notificationrunsummaryid TYPE bigint USING NULL;
--rollback DROP SEQUENCE IF EXISTS fps.notification_run_summary_notificationrunsummaryid_seq CASCADE;


COMMIT;
