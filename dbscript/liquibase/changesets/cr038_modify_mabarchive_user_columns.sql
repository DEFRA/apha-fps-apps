--liquibase formatted sql

--changeset repo-admin:CR038 labels:ddl context:all

-- Standardize user/audit columns to VARCHAR(255) for consistency across mabarchive tables.
ALTER TABLE mabarchive.tblcomments
ALTER COLUMN madeby TYPE VARCHAR(255);

ALTER TABLE mabarchive.tbllogmilestone
ALTER COLUMN changedby TYPE VARCHAR(255);

-- Add missing login email column to project manager table.
ALTER TABLE mabarchive.tblprojectmanager
ADD COLUMN IF NOT EXISTS loginemail VARCHAR(255);

--ROLLBACK
ALTER TABLE mabarchive.tblcomments
ALTER COLUMN madeby TYPE VARCHAR(50);

ALTER TABLE mabarchive.tbllogmilestone
ALTER COLUMN changedby TYPE VARCHAR(10);

ALTER TABLE mabarchive.tblprojectmanager
DROP COLUMN IF EXISTS loginemail;
