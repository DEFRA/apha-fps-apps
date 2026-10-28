--liquibase formatted sql

--changeset repo-admin:CR055 labels:ddl context:all

-- Standardize user/audit columns to VARCHAR(255) for consistency across mabarchive tables.

-- Add missing login email column to tblaccessusers table.
ALTER TABLE mabarchive.tblaccessusers
ADD COLUMN IF NOT EXISTS useremail VARCHAR(255);

--rollback ALTER TABLE mabarchive.tblaccessusers DROP COLUMN IF EXISTS useremail;
