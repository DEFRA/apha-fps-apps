--liquibase formatted sql

--changeset repo-admin:CR044 labels:ddl context:all

CREATE EXTENSION IF NOT EXISTS citext WITH SCHEMA fps;

-- A. Register the three Bulk Rates jobs.
INSERT INTO fps.job_master
    (jobname, frequency, timetolive, created_at, updated_at)
SELECT v.jobname, 'Manual', 20, NOW(), NOW()
FROM (
    VALUES
        ('BulkTestRatesUpdate'),
        ('BulkStaffRatesUpdate'),
        ('BulkAnimalRatesUpdate')
) AS v(jobname)
WHERE NOT EXISTS (
    SELECT 1
    FROM fps.job_master jm
    WHERE jm.jobname = v.jobname
);

INSERT INTO fps.job_status (jobid, status)
SELECT jm.jobid, s.status
FROM fps.job_master jm
CROSS JOIN (
    VALUES
        ('Initiated'),
        ('ReleasedForApproval'),
        ('Approved'),
        ('Rejected'),
        ('Running'),
        ('Completed'),
        ('Failed'),
        ('Cancelled')
) AS s(status)
WHERE jm.jobname IN (
    'BulkTestRatesUpdate',
    'BulkStaffRatesUpdate',
    'BulkAnimalRatesUpdate'
)
AND NOT EXISTS (
    SELECT 1
    FROM fps.job_status js
    WHERE js.jobid = jm.jobid
      AND js.status = s.status
);

-- B. Create the final staging tables.
CREATE TABLE IF NOT EXISTS fps.tblstagingtestorproduct (
    jobqueueid uuid NOT NULL,
    testcode fps.citext NOT NULL,
    unitpricevla numeric NULL,
    defraunitprice numeric NULL,
    fecnewrate numeric NULL,
    change varchar(30) NULL,
    itemdescription text NULL,
    shortdescription text NULL,
    owner varchar(10) NULL,
    comments text NULL,
    validationcomments text NULL,
    calculated_action varchar(30) NULL,
    effective_new_rate numeric NULL,
    source_current_rate numeric NULL,
    validation_version integer NULL,
    CONSTRAINT pk_tblstagingtestorproduct
        PRIMARY KEY (jobqueueid, testcode),
    CONSTRAINT fk_tblstagingtestorproduct_jobqueue
        FOREIGN KEY (jobqueueid)
        REFERENCES fps.job_queue (jobqueueid),
    CONSTRAINT chk_tblstagingtestorproduct_owner
        CHECK (owner IS NULL OR owner IN ('PT', 'PA', 'SD', 'LT'))
);

CREATE TABLE IF NOT EXISTS fps.tblstagingtlkptestreqmt (
    jobqueueid uuid NOT NULL,
    buyer fps.citext NOT NULL,
    testcode fps.citext NOT NULL,
    unitprice numeric NULL,
    agrupnewrate numeric NULL,
    norequired numeric NULL,
    datecreated timestamp NULL,
    active boolean NULL,
    projectbuyercode fps.citext NULL,
    testbuyercode fps.citext NULL,
    testbuyerworkgroup fps.citext NULL,
    comments text NULL,
    validationcomments text NULL,
    calculated_action varchar(30) NULL,
    effective_new_rate numeric NULL,
    source_current_rate numeric NULL,
    validation_version integer NULL,
    CONSTRAINT pk_tblstagingtlkptestreqmt
        PRIMARY KEY (jobqueueid, buyer, testcode),
    CONSTRAINT fk_tblstagingtlkptestreqmt_jobqueue
        FOREIGN KEY (jobqueueid)
        REFERENCES fps.job_queue (jobqueueid)
);

CREATE TABLE IF NOT EXISTS fps.tblstagingprofitcentregrade (
    jobqueueid uuid NOT NULL,
    pcgrade fps.citext NOT NULL,
    currentrate numeric NULL,
    newrate numeric NULL,
    comments text NULL,
    validationcomments text NULL,
    CONSTRAINT pk_tblstagingprofitcentregrade
        PRIMARY KEY (jobqueueid, pcgrade),
    CONSTRAINT fk_tblstagingprofitcentregrade_jobqueue
        FOREIGN KEY (jobqueueid)
        REFERENCES fps.job_queue (jobqueueid)
);

CREATE TABLE IF NOT EXISTS fps.tblstaginganimals (
    jobqueueid uuid NOT NULL,
    animaltype fps.citext NOT NULL,
    currentrate numeric NULL,
    newrate numeric NULL,
    comments text NULL,
    validationcomments text NULL,
    CONSTRAINT pk_tblstaginganimals
        PRIMARY KEY (jobqueueid, animaltype),
    CONSTRAINT fk_tblstaginganimals_jobqueue
        FOREIGN KEY (jobqueueid)
        REFERENCES fps.job_queue (jobqueueid)
);

-- The invalid cross-staging AGRUP-to-FEC FK is deliberately absent.
ALTER TABLE fps.tblstagingtlkptestreqmt
    DROP CONSTRAINT IF EXISTS fk_stagingtlkptestreqmt_test_parent;

-- C. Validation and permanent change audit.
CREATE TABLE IF NOT EXISTS fps.rate_change_history (
    ratechangehistoryid bigserial PRIMARY KEY,
    jobqueueid uuid NOT NULL,
    jobexecutionid uuid NULL,
    ratecategory varchar(20) NOT NULL,
    businesskey text NOT NULL,
    fieldname varchar(100) NOT NULL,
    operationtype varchar(20) NOT NULL,
    oldvalue text NULL,
    newvalue text NULL,
    changedby varchar(100) NULL,
    changedatutc timestamptz NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_rate_change_history_jobqueue
        FOREIGN KEY (jobqueueid)
        REFERENCES fps.job_queue (jobqueueid)
);

CREATE INDEX IF NOT EXISTS idx_rate_change_history_jobqueueid
    ON fps.rate_change_history (jobqueueid);

CREATE INDEX IF NOT EXISTS idx_rate_change_history_businesskey
    ON fps.rate_change_history (ratecategory, businesskey);

CREATE TABLE IF NOT EXISTS fps.staging_validation_error (
    validationerrorid bigserial PRIMARY KEY,
    jobqueueid uuid NOT NULL,
    upload_version integer NOT NULL,
    sheetname varchar(20) NULL,
    sourcerownumber integer NULL,
    testcode varchar(50) NULL,
    buyer varchar(50) NULL,
    fieldname varchar(100) NULL,
    validationcode varchar(100) NOT NULL,
    severity varchar(10) NOT NULL,
    validationmessage text NOT NULL,
    currentvalue text NULL,
    expectedvalue text NULL,
    is_request_level boolean NOT NULL DEFAULT false,
    created_at_utc timestamptz NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_staging_validation_error_jobqueue
        FOREIGN KEY (jobqueueid)
        REFERENCES fps.job_queue (jobqueueid),
    CONSTRAINT chk_staging_validation_error_severity
        CHECK (severity IN ('Error', 'Warning', 'Info'))
);

CREATE INDEX IF NOT EXISTS idx_staging_validation_error_jobqueue_upload
    ON fps.staging_validation_error (jobqueueid, upload_version);

CREATE INDEX IF NOT EXISTS idx_staging_validation_error_blocking
    ON fps.staging_validation_error (jobqueueid, severity)
    WHERE severity = 'Error';

--rollback DROP INDEX IF EXISTS fps.idx_staging_validation_error_blocking;
--rollback DROP INDEX IF EXISTS fps.idx_staging_validation_error_jobqueue_upload;
--rollback DROP TABLE IF EXISTS fps.staging_validation_error;
--rollback DROP INDEX IF EXISTS fps.idx_rate_change_history_businesskey;
--rollback DROP INDEX IF EXISTS fps.idx_rate_change_history_jobqueueid;
--rollback DROP TABLE IF EXISTS fps.rate_change_history;
--rollback DROP TABLE IF EXISTS fps.tblstaginganimals;
--rollback DROP TABLE IF EXISTS fps.tblstagingprofitcentregrade;
--rollback DROP TABLE IF EXISTS fps.tblstagingtlkptestreqmt;
--rollback DROP TABLE IF EXISTS fps.tblstagingtestorproduct;
--rollback DELETE FROM fps.job_status WHERE jobid IN (SELECT jobid FROM fps.job_master WHERE jobname IN ('BulkTestRatesUpdate', 'BulkStaffRatesUpdate', 'BulkAnimalRatesUpdate'));
--rollback DELETE FROM fps.job_master WHERE jobname IN ('BulkTestRatesUpdate', 'BulkStaffRatesUpdate', 'BulkAnimalRatesUpdate');
