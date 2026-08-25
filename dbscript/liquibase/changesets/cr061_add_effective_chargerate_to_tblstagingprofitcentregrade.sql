--liquibase formatted sql

--changeset repo-admin:CR061 labels:ddl context:all

ALTER TABLE fps.tblstagingprofitcentregrade
    ADD COLUMN IF NOT EXISTS effective_chargerate numeric(19,4);

--ROLLBACK
--ALTER TABLE fps.tblstagingprofitcentregrade DROP COLUMN IF EXISTS effective_chargerate;