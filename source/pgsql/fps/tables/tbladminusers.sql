CREATE TABLE IF NOT EXISTS fps.tbladminusers (
    mnumber character varying(50) NOT NULL,
    name character varying(50) NOT NULL,
    seedeptincome boolean DEFAULT false NOT NULL,
    seedbwindow boolean DEFAULT false NOT NULL,
    dt2number character varying(50),
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tbladminusers PRIMARY KEY (mnumber, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.tbladminusers_default PARTITION OF fps.tbladminusers
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.tbladminusers_y2016 PARTITION OF fps.tbladminusers
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.tbladminusers_y2017 PARTITION OF fps.tbladminusers
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.tbladminusers_y2018 PARTITION OF fps.tbladminusers
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.tbladminusers_y2019 PARTITION OF fps.tbladminusers
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.tbladminusers_y2020 PARTITION OF fps.tbladminusers
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.tbladminusers_y2021 PARTITION OF fps.tbladminusers
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.tbladminusers_y2022 PARTITION OF fps.tbladminusers
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.tbladminusers_y2023 PARTITION OF fps.tbladminusers
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.tbladminusers_y2024 PARTITION OF fps.tbladminusers
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.tbladminusers_y2025 PARTITION OF fps.tbladminusers
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.tbladminusers_y2026 PARTITION OF fps.tbladminusers
    FOR VALUES IN (2026);

-- Foreign keys for fps.tbladminusers
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbladminusers_fpsyear'
          AND conrelid = 'fps.tbladminusers'::regclass
    ) THEN
        ALTER TABLE fps.tbladminusers
            ADD CONSTRAINT fk_tbladminusers_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
