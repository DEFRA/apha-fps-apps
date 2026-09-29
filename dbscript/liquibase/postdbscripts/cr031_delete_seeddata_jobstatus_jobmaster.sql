--liquibase formatted sql

--changeset repo-admin:CR031 labels:seed context:all

DELETE FROM fps.job_status js
USING fps.job_master jm
WHERE js.jobid = jm.jobid
    AND jm.jobname = 'YearEndProcess';

DELETE FROM fps.job_master jm
WHERE jm.jobname = 'YearEndProcess'
    AND NOT EXISTS (
            SELECT 1
            FROM fps.job_queue jq
            WHERE jq.jobid = jm.jobid
    );

--ROLLBACK
--Not Applicable