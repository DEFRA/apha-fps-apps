CREATE TABLE IF NOT EXISTS fps.testreq_log (
    sequenceno integer NOT NULL,
    testcode character varying(20),
    buyer character varying(20),
    unitprice double precision,
    norequired integer,
    projectbuyercode character varying(50),
    testbuyercode character varying(50),
    active smallint,
    date_time timestamp without time zone,
    user_id character varying(255),
    insert_delete character(2),
    jobcode character varying(50),
    fpsyear integer NOT NULL,
    CONSTRAINT pk_testreq_log PRIMARY KEY (sequenceno, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.testreq_log_default PARTITION OF fps.testreq_log
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.testreq_log_y2016 PARTITION OF fps.testreq_log
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.testreq_log_y2017 PARTITION OF fps.testreq_log
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.testreq_log_y2018 PARTITION OF fps.testreq_log
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.testreq_log_y2019 PARTITION OF fps.testreq_log
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.testreq_log_y2020 PARTITION OF fps.testreq_log
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.testreq_log_y2021 PARTITION OF fps.testreq_log
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.testreq_log_y2022 PARTITION OF fps.testreq_log
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.testreq_log_y2023 PARTITION OF fps.testreq_log
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.testreq_log_y2024 PARTITION OF fps.testreq_log
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.testreq_log_y2025 PARTITION OF fps.testreq_log
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.testreq_log_y2026 PARTITION OF fps.testreq_log
    FOR VALUES IN (2026);
COMMENT ON COLUMN fps.testreq_log.jobcode IS 'Generated column based on projectbuyercode';

-- Foreign keys for fps.testreq_log
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_testreq_log_fpsyear'
          AND conrelid = 'fps.testreq_log'::regclass
    ) THEN
        ALTER TABLE fps.testreq_log
            ADD CONSTRAINT fk_testreq_log_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
