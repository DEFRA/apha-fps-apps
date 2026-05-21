CREATE TABLE IF NOT EXISTS fps.tlkpmonthhours (
    year smallint NOT NULL,
    month smallint NOT NULL,
    days numeric(5,1),
    cvlhours numeric(5,1),
    vidhours numeric(5,1),
    fmonth smallint,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tlkpmonthhours PRIMARY KEY (year, month, fpsyear),
    CONSTRAINT tlkpmonthhours_pk UNIQUE (year, month, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.tlkpmonthhours_default PARTITION OF fps.tlkpmonthhours
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.tlkpmonthhours_y2016 PARTITION OF fps.tlkpmonthhours
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.tlkpmonthhours_y2017 PARTITION OF fps.tlkpmonthhours
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.tlkpmonthhours_y2018 PARTITION OF fps.tlkpmonthhours
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.tlkpmonthhours_y2019 PARTITION OF fps.tlkpmonthhours
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.tlkpmonthhours_y2020 PARTITION OF fps.tlkpmonthhours
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.tlkpmonthhours_y2021 PARTITION OF fps.tlkpmonthhours
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.tlkpmonthhours_y2022 PARTITION OF fps.tlkpmonthhours
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.tlkpmonthhours_y2023 PARTITION OF fps.tlkpmonthhours
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.tlkpmonthhours_y2024 PARTITION OF fps.tlkpmonthhours
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.tlkpmonthhours_y2025 PARTITION OF fps.tlkpmonthhours
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.tlkpmonthhours_y2026 PARTITION OF fps.tlkpmonthhours
    FOR VALUES IN (2026);

-- Foreign keys for fps.tlkpmonthhours
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpmonthhours_fpsyear'
          AND conrelid = 'fps.tlkpmonthhours'::regclass
    ) THEN
        ALTER TABLE fps.tlkpmonthhours
            ADD CONSTRAINT fk_tlkpmonthhours_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
