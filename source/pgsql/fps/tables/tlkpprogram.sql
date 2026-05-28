CREATE TABLE IF NOT EXISTS fps.tlkpprogram (
    programno character varying(10) NOT NULL,
    programname character varying(80),
    directorate character varying(15),
    minim character varying(7),
    sector_name character varying(50) DEFAULT 'Charge'::character varying,
    customer character varying(50),
    target money DEFAULT 0,
    manager character varying(50),
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tlkpprogram PRIMARY KEY (programno, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.tlkpprogram_default PARTITION OF fps.tlkpprogram
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.tlkpprogram_y2016 PARTITION OF fps.tlkpprogram
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.tlkpprogram_y2017 PARTITION OF fps.tlkpprogram
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.tlkpprogram_y2018 PARTITION OF fps.tlkpprogram
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.tlkpprogram_y2019 PARTITION OF fps.tlkpprogram
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.tlkpprogram_y2020 PARTITION OF fps.tlkpprogram
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.tlkpprogram_y2021 PARTITION OF fps.tlkpprogram
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.tlkpprogram_y2022 PARTITION OF fps.tlkpprogram
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.tlkpprogram_y2023 PARTITION OF fps.tlkpprogram
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.tlkpprogram_y2024 PARTITION OF fps.tlkpprogram
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.tlkpprogram_y2025 PARTITION OF fps.tlkpprogram
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.tlkpprogram_y2026 PARTITION OF fps.tlkpprogram
    FOR VALUES IN (2026);

-- Foreign keys for fps.tlkpprogram
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpprogram_fpsyear'
          AND conrelid = 'fps.tlkpprogram'::regclass
    ) THEN
        ALTER TABLE fps.tlkpprogram
            ADD CONSTRAINT fk_tlkpprogram_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
