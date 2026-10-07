--liquibase formatted sql

--changeset kenneth:CR092 labels:ddl context:all splitStatements:false

CREATE INDEX IF NOT EXISTS tblwgemployee_workgroupgrade_fpsyear_idx
    ON fps.tblwgemployee (workgroupgrade, fpsyear);
