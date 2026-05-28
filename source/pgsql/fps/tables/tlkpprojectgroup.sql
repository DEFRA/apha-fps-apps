CREATE TABLE IF NOT EXISTS fps.tlkpprojectgroup (
    projectgroup character varying(50) NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tlkpprojectgroup PRIMARY KEY (projectgroup, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.tlkpprojectgroup_default PARTITION OF fps.tlkpprojectgroup
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.tlkpprojectgroup_y2016 PARTITION OF fps.tlkpprojectgroup
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.tlkpprojectgroup_y2017 PARTITION OF fps.tlkpprojectgroup
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.tlkpprojectgroup_y2018 PARTITION OF fps.tlkpprojectgroup
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.tlkpprojectgroup_y2019 PARTITION OF fps.tlkpprojectgroup
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.tlkpprojectgroup_y2020 PARTITION OF fps.tlkpprojectgroup
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.tlkpprojectgroup_y2021 PARTITION OF fps.tlkpprojectgroup
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.tlkpprojectgroup_y2022 PARTITION OF fps.tlkpprojectgroup
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.tlkpprojectgroup_y2023 PARTITION OF fps.tlkpprojectgroup
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.tlkpprojectgroup_y2024 PARTITION OF fps.tlkpprojectgroup
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.tlkpprojectgroup_y2025 PARTITION OF fps.tlkpprojectgroup
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.tlkpprojectgroup_y2026 PARTITION OF fps.tlkpprojectgroup
    FOR VALUES IN (2026);

-- Foreign keys for fps.tlkpprojectgroup
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpprojectgroup_fpsyear'
          AND conrelid = 'fps.tlkpprojectgroup'::regclass
    ) THEN
        ALTER TABLE fps.tlkpprojectgroup
            ADD CONSTRAINT fk_tlkpprojectgroup_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
