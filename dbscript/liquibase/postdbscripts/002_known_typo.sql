--liquibase formatted sql

--changeset migration-fix:002-known-typo labels:dml context:all

UPDATE fps.tlkpproject
SET subaccountcode = '1020405'
WHERE subaccountcode = '10200405';