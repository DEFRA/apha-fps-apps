CREATE TABLE IF NOT EXISTS fps.tblanimals (
    animaltype character varying(50) NOT NULL,
    species character varying(50),
    security_level character varying(50),
    dailyrate money,
    planbyweek boolean DEFAULT false NOT NULL,
    defradailyrate money,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tblanimals PRIMARY KEY (animaltype, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.tblanimals_default PARTITION OF fps.tblanimals
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.tblanimals_y2016 PARTITION OF fps.tblanimals
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.tblanimals_y2017 PARTITION OF fps.tblanimals
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.tblanimals_y2018 PARTITION OF fps.tblanimals
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.tblanimals_y2019 PARTITION OF fps.tblanimals
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.tblanimals_y2020 PARTITION OF fps.tblanimals
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.tblanimals_y2021 PARTITION OF fps.tblanimals
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.tblanimals_y2022 PARTITION OF fps.tblanimals
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.tblanimals_y2023 PARTITION OF fps.tblanimals
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.tblanimals_y2024 PARTITION OF fps.tblanimals
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.tblanimals_y2025 PARTITION OF fps.tblanimals
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.tblanimals_y2026 PARTITION OF fps.tblanimals
    FOR VALUES IN (2026);

-- Foreign keys for fps.tblanimals
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblanimals_fpsyear'
          AND conrelid = 'fps.tblanimals'::regclass
    ) THEN
        ALTER TABLE fps.tblanimals
            ADD CONSTRAINT fk_tblanimals_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
