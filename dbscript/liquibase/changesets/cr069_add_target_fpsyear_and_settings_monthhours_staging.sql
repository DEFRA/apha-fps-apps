--liquibase formatted sql

--changeset repo-admin:CR069 labels:ddl context:all splitStatements:false

-- CR069: Add target_fpsyear to job_queue and create staging tables for settings and month hours

ALTER TABLE fps.job_queue
    ADD COLUMN IF NOT EXISTS target_fpsyear integer;

CREATE TABLE IF NOT EXISTS fps.tblsettings_staging (
    id          varchar(50)   NOT NULL,
    setting     varchar(255),
    notes       varchar(255),
    fpsyear     integer       NOT NULL,
    updated_by  varchar(100),
    updated_at  timestamp     NOT NULL DEFAULT now(),
    CONSTRAINT pk_tblsettings_staging
        PRIMARY KEY (id, fpsyear)
);

CREATE TABLE IF NOT EXISTS fps.tlkpmonthhours_staging (
    year        smallint      NOT NULL,
    month       smallint      NOT NULL,
    fmonth      smallint      NOT NULL,
    days        numeric(5,1),
    cvlhours    numeric(5,1),
    vidhours    numeric(5,1),
    fpsyear     integer       NOT NULL,
    CONSTRAINT pk_tlkpmonthhours_staging
        PRIMARY KEY (year, month, fpsyear)
);

--ROLLBACK
--DROP TABLE IF EXISTS fps.tlkpmonthhours_staging;
--DROP TABLE IF EXISTS fps.tblsettings_staging;
--ALTER TABLE fps.job_queue DROP COLUMN IF EXISTS target_fpsyear;
