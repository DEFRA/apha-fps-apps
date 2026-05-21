CREATE TABLE IF NOT EXISTS fps.tblperiod (
    periodname character varying(50) NOT NULL,
    periodtype character varying(50),
    startperiod double precision,
    endperiod double precision,
    finalsummariesrun smallint,
    periodlocked smallint DEFAULT 0 NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tblperiod PRIMARY KEY (periodname, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.tblperiod_default PARTITION OF fps.tblperiod
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.tblperiod_y2016 PARTITION OF fps.tblperiod
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.tblperiod_y2017 PARTITION OF fps.tblperiod
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.tblperiod_y2018 PARTITION OF fps.tblperiod
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.tblperiod_y2019 PARTITION OF fps.tblperiod
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.tblperiod_y2020 PARTITION OF fps.tblperiod
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.tblperiod_y2021 PARTITION OF fps.tblperiod
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.tblperiod_y2022 PARTITION OF fps.tblperiod
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.tblperiod_y2023 PARTITION OF fps.tblperiod
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.tblperiod_y2024 PARTITION OF fps.tblperiod
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.tblperiod_y2025 PARTITION OF fps.tblperiod
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.tblperiod_y2026 PARTITION OF fps.tblperiod
    FOR VALUES IN (2026);

-- Foreign keys for fps.tblperiod
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblperiod_fpsyear'
          AND conrelid = 'fps.tblperiod'::regclass
    ) THEN
        ALTER TABLE fps.tblperiod
            ADD CONSTRAINT fk_tblperiod_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
