--liquibase formatted sql

--changeset repo-admin:CR019 labels:ddl context:all

UPDATE fps.job_master SET jobname='RecreateSummary', updated_at=NOW() WHERE jobname='RecreateSummaries';

--ROLLBACK
--Not Applicable