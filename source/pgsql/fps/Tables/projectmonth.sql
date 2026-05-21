CREATE TABLE IF NOT EXISTS fps.projectmonth (
    project character varying(20) NOT NULL,
    monthno integer NOT NULL,
    costprofile money,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_projectmonth PRIMARY KEY (project, monthno, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.projectmonth_default PARTITION OF fps.projectmonth
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.projectmonth_y2016 PARTITION OF fps.projectmonth
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.projectmonth_y2017 PARTITION OF fps.projectmonth
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.projectmonth_y2018 PARTITION OF fps.projectmonth
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.projectmonth_y2019 PARTITION OF fps.projectmonth
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.projectmonth_y2020 PARTITION OF fps.projectmonth
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.projectmonth_y2021 PARTITION OF fps.projectmonth
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.projectmonth_y2022 PARTITION OF fps.projectmonth
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.projectmonth_y2023 PARTITION OF fps.projectmonth
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.projectmonth_y2024 PARTITION OF fps.projectmonth
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.projectmonth_y2025 PARTITION OF fps.projectmonth
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.projectmonth_y2026 PARTITION OF fps.projectmonth
    FOR VALUES IN (2026);

-- Foreign keys for fps.projectmonth
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_projectmonth_fpsyear'
          AND conrelid = 'fps.projectmonth'::regclass
    ) THEN
        ALTER TABLE fps.projectmonth
            ADD CONSTRAINT fk_projectmonth_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
