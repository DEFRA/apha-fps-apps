--liquibase formatted sql

--changeset repo-admin:CR045 labels:job_queue_upload context:all splitStatements:false


BEGIN;

-- A. Replace the generic configuration_json field with typed upload metadata.
ALTER TABLE fps.job_queue
    ADD COLUMN IF NOT EXISTS upload_filename text,
    ADD COLUMN IF NOT EXISTS upload_checksum_sha256 text,
    ADD COLUMN IF NOT EXISTS upload_version integer,
    ADD COLUMN IF NOT EXISTS upload_validated_at_utc timestamptz,
    ADD COLUMN IF NOT EXISTS upload_row_counts_json jsonb;

DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'fps'
          AND table_name = 'job_queue'
          AND column_name = 'configuration_json'
    ) THEN
        EXECUTE $sql$
            UPDATE fps.job_queue
            SET upload_filename = COALESCE(
                    upload_filename,
                    configuration_json ->> 'filename'
                ),
                upload_checksum_sha256 = COALESCE(
                    upload_checksum_sha256,
                    configuration_json ->> 'checksum_sha256'
                ),
                upload_version = COALESCE(
                    upload_version,
                    NULLIF(configuration_json ->> 'upload_version', '')::integer
                ),
                upload_validated_at_utc = COALESCE(
                    upload_validated_at_utc,
                    NULLIF(configuration_json ->> 'validation_completed_at_utc', '')::timestamptz
                ),
                upload_row_counts_json = COALESCE(
                    upload_row_counts_json,
                    configuration_json -> 'row_counts'
                )
            WHERE configuration_json IS NOT NULL
        $sql$;
    END IF;
END $$;

-- B. Create the immutable, versioned download snapshot model (varchar, not citext).
CREATE TABLE IF NOT EXISTS fps.bulk_rates_download (
    jobqueueid uuid NOT NULL,
    download_version integer NOT NULL,
    status varchar(20) NOT NULL DEFAULT 'Generating',
    created_at_utc timestamptz NOT NULL DEFAULT NOW(),
    ready_at_utc timestamptz NULL,
    CONSTRAINT pk_bulk_rates_download
        PRIMARY KEY (jobqueueid, download_version),
    CONSTRAINT fk_bulk_rates_download_jobqueue
        FOREIGN KEY (jobqueueid)
        REFERENCES fps.job_queue (jobqueueid),
    CONSTRAINT chk_bulk_rates_download_status
        CHECK (status IN ('Generating', 'Ready', 'Failed'))
);

CREATE TABLE IF NOT EXISTS fps.bulk_rates_downloaded_key (
    id bigserial PRIMARY KEY,
    jobqueueid uuid NOT NULL,
    download_version integer NOT NULL,
    sheetname varchar(20) NOT NULL,
    testcode varchar(20) NOT NULL,
    buyer varchar(20) NULL,
    source_rate numeric(19,4)  NULL,
    itemdescription text NULL,
    shortdescription text NULL,
    owner varchar(10) NULL,
    unitpricevla numeric(19,4)  NULL,
    norequired numeric(19,4)  NULL,
    datecreated timestamp NULL,
    active boolean NULL,
    projectbuyercode varchar(50) NULL,
    testbuyercode varchar(50) NULL,
    downloaded_at_utc timestamptz NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_bulk_rates_downloaded_key_header
        FOREIGN KEY (jobqueueid, download_version)
        REFERENCES fps.bulk_rates_download (jobqueueid, download_version),
    CONSTRAINT chk_bulk_rates_downloaded_key_sheetname
        CHECK (sheetname IN ('FEC', 'AGRUP', 'Staff', 'Animal'))
);

-- Drop first: the existing index's expression casts to fps.citext, which
-- would otherwise conflict with (or silently keep referencing) the old type
-- while the column beneath it is retyped.
DROP INDEX IF EXISTS fps.ux_bulk_rates_downloaded_key_identity;

ALTER TABLE fps.bulk_rates_downloaded_key
    ALTER COLUMN testcode         TYPE varchar(20) USING testcode::varchar(20),
    ALTER COLUMN buyer            TYPE varchar(20) USING buyer::varchar(20),
    ALTER COLUMN projectbuyercode TYPE varchar(50) USING projectbuyercode::varchar(50),
    ALTER COLUMN testbuyercode    TYPE varchar(50) USING testbuyercode::varchar(50);

CREATE UNIQUE INDEX IF NOT EXISTS ux_bulk_rates_downloaded_key_identity
    ON fps.bulk_rates_downloaded_key
       (jobqueueid, download_version, sheetname, testcode,
        COALESCE(buyer, ''::varchar(20)));

ALTER TABLE fps.job_queue
    ADD COLUMN IF NOT EXISTS active_download_version integer;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'fk_job_queue_active_bulk_rates_download'
          AND conrelid = 'fps.job_queue'::regclass
    ) THEN
        ALTER TABLE fps.job_queue
            ADD CONSTRAINT fk_job_queue_active_bulk_rates_download
            FOREIGN KEY (jobqueueid, active_download_version)
            REFERENCES fps.bulk_rates_download
                (jobqueueid, download_version);
    END IF;
END $$;

-- Drop the legacy generic JSON column only after the backfill.
ALTER TABLE fps.job_queue
    DROP COLUMN IF EXISTS configuration_json;

COMMIT;

--ROLLBACK
