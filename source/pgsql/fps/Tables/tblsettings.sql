CREATE TABLE IF NOT EXISTS fps.tblsettings (
    id character varying(50) NOT NULL,
    setting character varying(255),
    notes character varying(255),
    fpsyear integer NOT NULL,
    updated_by character varying(100),
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    CONSTRAINT pk_tblsettings PRIMARY KEY (id, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.tblsettings_default PARTITION OF fps.tblsettings
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.tblsettings_y2016 PARTITION OF fps.tblsettings
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.tblsettings_y2017 PARTITION OF fps.tblsettings
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.tblsettings_y2018 PARTITION OF fps.tblsettings
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.tblsettings_y2019 PARTITION OF fps.tblsettings
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.tblsettings_y2020 PARTITION OF fps.tblsettings
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.tblsettings_y2021 PARTITION OF fps.tblsettings
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.tblsettings_y2022 PARTITION OF fps.tblsettings
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.tblsettings_y2023 PARTITION OF fps.tblsettings
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.tblsettings_y2024 PARTITION OF fps.tblsettings
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.tblsettings_y2025 PARTITION OF fps.tblsettings
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.tblsettings_y2026 PARTITION OF fps.tblsettings
    FOR VALUES IN (2026);

COMMENT ON TABLE fps.tblsettings IS 'Application-level configuration settings. Only business-logic constants belong here; infrastructure config moves to appsettings.json.';
COMMENT ON COLUMN fps.tblsettings.id IS 'Unique setting key referenced by application code.';
COMMENT ON COLUMN fps.tblsettings.setting IS 'The setting value as text.';
COMMENT ON COLUMN fps.tblsettings.notes IS 'Free-text description of purpose, origin, and usage.';
COMMENT ON COLUMN fps.tblsettings.fpsyear IS 'Fiscal year scope. NULL = not year-specific.';
COMMENT ON COLUMN fps.tblsettings.updated_by IS 'User or service account that last modified the row.';
COMMENT ON COLUMN fps.tblsettings.updated_at IS 'Timestamp of last modification (auto-set on insert).';

-- Foreign keys for fps.tblsettings
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblsettings_fpsyear'
          AND conrelid = 'fps.tblsettings'::regclass
    ) THEN
        ALTER TABLE fps.tblsettings
            ADD CONSTRAINT fk_tblsettings_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
