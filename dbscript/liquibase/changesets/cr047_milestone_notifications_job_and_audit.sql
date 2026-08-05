--liquibase formatted sql

--changeset repo-admin:CR047 labels:ddl context:all

-- A. Register the scheduled notification job and its non-approval lifecycle.
INSERT INTO fps.job_master
    (jobname, frequency, timetolive, created_at, updated_at)
SELECT
    'MilestoneUpdateNotifications',
    'Scheduled',
    20,
    NOW(),
    NOW()
WHERE NOT EXISTS (
    SELECT 1
    FROM fps.job_master
    WHERE jobname = 'MilestoneUpdateNotifications'
);

INSERT INTO fps.job_status (jobid, status)
SELECT jm.jobid, s.status
FROM fps.job_master jm
CROSS JOIN (
    VALUES ('Initiated'), ('Running'), ('Completed'), ('Failed')
) AS s(status)
WHERE jm.jobname = 'MilestoneUpdateNotifications'
AND NOT EXISTS (
    SELECT 1
    FROM fps.job_status js
    WHERE js.jobid = jm.jobid
      AND js.status = s.status
);

-- B. Recipient delivery audit.
CREATE TABLE IF NOT EXISTS fps.notification_delivery (
    notificationdeliveryid bigserial PRIMARY KEY,
    jobqueueid uuid NOT NULL,
    notificationtype varchar(50) NOT NULL,
    fpsyear integer NOT NULL,
    monthnumber integer NOT NULL,
    recipientid varchar(64) NOT NULL,
    recipientname text NULL,
    recipientemail text NOT NULL,
    delivery_status varchar(20) NOT NULL,
    attempted_at_utc timestamptz NOT NULL DEFAULT NOW(),
    sent_at_utc timestamptz NULL,
    failure_reason text NULL,
    CONSTRAINT fk_notification_delivery_jobqueue
        FOREIGN KEY (jobqueueid)
        REFERENCES fps.job_queue (jobqueueid)
);

CREATE INDEX IF NOT EXISTS idx_notification_delivery_business_key
    ON fps.notification_delivery
       (notificationtype, fpsyear, monthnumber, recipientid);

CREATE TABLE IF NOT EXISTS fps.notification_delivery_project (
    notificationdeliveryprojectid bigserial PRIMARY KEY,
    notificationdeliveryid bigint NOT NULL,
    projectcode text NOT NULL,
    projectname text NULL,
    CONSTRAINT fk_notification_delivery_project_parent
        FOREIGN KEY (notificationdeliveryid)
        REFERENCES fps.notification_delivery (notificationdeliveryid)
        ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_notification_delivery_project_parent
    ON fps.notification_delivery_project (notificationdeliveryid);

-- C. CAPS run-summary audit.
CREATE TABLE IF NOT EXISTS fps.notification_run_summary (
    notificationrunsummaryid bigserial PRIMARY KEY,
    jobqueueid uuid NOT NULL,
    fpsyear integer NOT NULL,
    monthnumber integer NOT NULL,
    candidateprojectcount integer NOT NULL DEFAULT 0,
    recipientcount integer NOT NULL DEFAULT 0,
    deliveryattemptcount integer NOT NULL DEFAULT 0,
    deliverysentcount integer NOT NULL DEFAULT 0,
    deliveryfailedcount integer NOT NULL DEFAULT 0,
    unresolvedrecipientcount integer NULL,
    unresolvedprojectcount integer NULL,
    caps_delivery_status varchar(20) NULL,
    caps_sent_at_utc timestamptz NULL,
    caps_failure_reason text NULL,
    created_at_utc timestamptz NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_notification_run_summary_jobqueue
        FOREIGN KEY (jobqueueid)
        REFERENCES fps.job_queue (jobqueueid)
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_notification_run_summary_jobqueue
    ON fps.notification_run_summary (jobqueueid);

--rollback DROP INDEX IF EXISTS fps.ux_notification_run_summary_jobqueue;
--rollback DROP TABLE IF EXISTS fps.notification_run_summary;
--rollback DROP INDEX IF EXISTS fps.idx_notification_delivery_project_parent;
--rollback DROP TABLE IF EXISTS fps.notification_delivery_project;
--rollback DROP INDEX IF EXISTS fps.idx_notification_delivery_business_key;
--rollback DROP TABLE IF EXISTS fps.notification_delivery;
--rollback DELETE FROM fps.job_status WHERE jobid IN (SELECT jobid FROM fps.job_master WHERE jobname = 'MilestoneUpdateNotifications');
--rollback DELETE FROM fps.job_master WHERE jobname = 'MilestoneUpdateNotifications';
