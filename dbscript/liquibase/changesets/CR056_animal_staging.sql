
--liquibase formatted sql

--changeset repo-admin:CR056 labels:AnimalStaging context:all


SET search_path TO fps;

-- CR056: Bring FEC Bulk Rates AGRUP/Staff/Animal staging up to parity with
-- what the shipped application code requires, finishing what CR044 started.
-- Uses numeric throughout (no money), matching every authoritative live
-- table already queried (testorproduct, tlkptestreqmt, profitcentregrade,
-- tblanimals all use numeric, never money).
--
-- Prerequisite: run corrected CR044 first. This script no longer touches
-- testcode/buyer/projectbuyercode/testbuyercode/testbuyerworkgroup/
-- pcgrade/animaltype -- CR044-corrected already retypes those on the same
-- tables this script alters.

DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'fps' AND table_name = 'tblstagingtlkptestreqmt'
          AND column_name = 'unitprice'
    ) THEN
        ALTER TABLE fps.tblstagingtlkptestreqmt RENAME COLUMN unitprice TO agrup;
    END IF;

    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'fps' AND table_name = 'tblstagingtlkptestreqmt'
          AND column_name = 'agrupnewrate'
    ) THEN
        ALTER TABLE fps.tblstagingtlkptestreqmt RENAME COLUMN agrupnewrate TO agrupnew;
    END IF;

    IF (
        SELECT data_type FROM information_schema.columns
        WHERE table_schema = 'fps' AND table_name = 'tblstagingtlkptestreqmt'
          AND column_name = 'active'
    ) = 'boolean' THEN
        ALTER TABLE fps.tblstagingtlkptestreqmt
            ALTER COLUMN active TYPE smallint
            USING (CASE WHEN active THEN 1 ELSE 0 END);
    END IF;
END $$;

ALTER TABLE fps.tblstagingtlkptestreqmt
    ADD COLUMN IF NOT EXISTS change numeric(19,4);

-- agrup/agrupnew/change may already exist as money rather than missing
-- entirely -- retype explicitly so this is correct either way.
ALTER TABLE fps.tblstagingtlkptestreqmt
    ALTER COLUMN agrup    TYPE numeric(19,4) USING agrup::numeric(19,4),
    ALTER COLUMN agrupnew TYPE numeric(19,4) USING agrupnew::numeric(19,4),
    ALTER COLUMN change   TYPE numeric(19,4) USING change::numeric(19,4);

ALTER TABLE fps.tblstagingprofitcentregrade
    DROP COLUMN IF EXISTS currentrate,
    DROP COLUMN IF EXISTS newrate;

ALTER TABLE fps.tblstagingprofitcentregrade
    ADD COLUMN IF NOT EXISTS payrate            numeric(19,4),
    ADD COLUMN IF NOT EXISTS npr                numeric(19,4),
    ADD COLUMN IF NOT EXISTS ohr                numeric(19,4),
    ADD COLUMN IF NOT EXISTS source_payrate     numeric(19,4),
    ADD COLUMN IF NOT EXISTS source_npr         numeric(19,4),
    ADD COLUMN IF NOT EXISTS source_ohr         numeric(19,4),
    ADD COLUMN IF NOT EXISTS effective_payrate  numeric(19,4),
    ADD COLUMN IF NOT EXISTS effective_npr      numeric(19,4),
    ADD COLUMN IF NOT EXISTS effective_ohr      numeric(19,4),
    ADD COLUMN IF NOT EXISTS calculated_action  character varying(30),
    ADD COLUMN IF NOT EXISTS validation_version integer;

-- payrate/npr/ohr may already exist as money -- same reasoning as AGRUP above.
ALTER TABLE fps.tblstagingprofitcentregrade
    ALTER COLUMN payrate TYPE numeric(19,4) USING payrate::numeric(19,4),
    ALTER COLUMN npr     TYPE numeric(19,4) USING npr::numeric(19,4),
    ALTER COLUMN ohr     TYPE numeric(19,4) USING ohr::numeric(19,4);

ALTER TABLE fps.tblstaginganimals
    DROP COLUMN IF EXISTS currentrate,
    DROP COLUMN IF EXISTS newrate;

ALTER TABLE fps.tblstaginganimals
    ADD COLUMN IF NOT EXISTS species                    character varying(50),
    ADD COLUMN IF NOT EXISTS security_level              character varying(50),
    ADD COLUMN IF NOT EXISTS dailyrate                  numeric(19,4),
    ADD COLUMN IF NOT EXISTS defradailyrate             numeric(19,4),
    ADD COLUMN IF NOT EXISTS planbyweek                 boolean,
    ADD COLUMN IF NOT EXISTS source_dailyrate           numeric(19,4),
    ADD COLUMN IF NOT EXISTS source_defradailyrate      numeric(19,4),
    ADD COLUMN IF NOT EXISTS source_planbyweek          boolean,
    ADD COLUMN IF NOT EXISTS source_species             character varying(50),
    ADD COLUMN IF NOT EXISTS source_securitylevel       character varying(50),
    ADD COLUMN IF NOT EXISTS effective_dailyrate        numeric(19,4),
    ADD COLUMN IF NOT EXISTS effective_defradailyrate   numeric(19,4),
    ADD COLUMN IF NOT EXISTS effective_planbyweek       boolean,
    ADD COLUMN IF NOT EXISTS effective_species          character varying(50),
    ADD COLUMN IF NOT EXISTS effective_securitylevel    character varying(50),
    ADD COLUMN IF NOT EXISTS calculated_action          character varying(30),
    ADD COLUMN IF NOT EXISTS validation_version         integer;

-- dailyrate/defradailyrate may already exist as money -- same reasoning as
-- AGRUP/Staff above.
ALTER TABLE fps.tblstaginganimals
    ALTER COLUMN dailyrate      TYPE numeric(19,4) USING dailyrate::numeric(19,4),
    ALTER COLUMN defradailyrate TYPE numeric(19,4) USING defradailyrate::numeric(19,4);

CREATE TABLE IF NOT EXISTS fps.bulk_rates_staff_download_detail (
    id                 bigserial      NOT NULL,
    jobqueueid         uuid           NOT NULL,
    download_version   integer        NOT NULL,
    pcgrade            varchar(20)    NOT NULL,
    source_payrate     numeric(19,4),
    source_npr         numeric(19,4),
    source_ohr         numeric(19,4),
    downloaded_at_utc  timestamptz    NOT NULL DEFAULT now(),
    CONSTRAINT pk_bulk_rates_staff_download_detail
        PRIMARY KEY (id),
    CONSTRAINT fk_bulk_rates_staff_download_detail_download
        FOREIGN KEY (jobqueueid, download_version)
        REFERENCES fps.bulk_rates_download (jobqueueid, download_version)
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_bulk_rates_staff_download_detail_identity
    ON fps.bulk_rates_staff_download_detail (jobqueueid, download_version, pcgrade);

CREATE INDEX IF NOT EXISTS ix_bulk_rates_staff_download_detail_download
    ON fps.bulk_rates_staff_download_detail (jobqueueid, download_version);

CREATE TABLE IF NOT EXISTS fps.bulk_rates_animal_download_detail (
    id                      bigserial              NOT NULL,
    jobqueueid              uuid                   NOT NULL,
    download_version        integer                NOT NULL,
    animaltype              varchar(50)            NOT NULL,
    source_dailyrate        numeric(19,4),
    source_defradailyrate   numeric(19,4),
    source_planbyweek       boolean,
    source_species          character varying(50),
    source_securitylevel    character varying(50),
    downloaded_at_utc       timestamptz            NOT NULL DEFAULT now(),
    CONSTRAINT pk_bulk_rates_animal_download_detail
        PRIMARY KEY (id),
    CONSTRAINT fk_bulk_rates_animal_download_detail_download
        FOREIGN KEY (jobqueueid, download_version)
        REFERENCES fps.bulk_rates_download (jobqueueid, download_version)
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_bulk_rates_animal_download_detail_identity
    ON fps.bulk_rates_animal_download_detail (jobqueueid, download_version, animaltype);

CREATE INDEX IF NOT EXISTS ix_bulk_rates_animal_download_detail_download
    ON fps.bulk_rates_animal_download_detail (jobqueueid, download_version);
	
	
--ROLLBACK
