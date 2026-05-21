CREATE TABLE IF NOT EXISTS fps.tblemployee (
    spnumber character varying(10) NOT NULL,
    firstname character varying(20),
    lastname character varying(20),
    title character varying(4),
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tblemployee PRIMARY KEY (spnumber, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.tblemployee_default PARTITION OF fps.tblemployee
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.tblemployee_y2016 PARTITION OF fps.tblemployee
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.tblemployee_y2017 PARTITION OF fps.tblemployee
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.tblemployee_y2018 PARTITION OF fps.tblemployee
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.tblemployee_y2019 PARTITION OF fps.tblemployee
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.tblemployee_y2020 PARTITION OF fps.tblemployee
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.tblemployee_y2021 PARTITION OF fps.tblemployee
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.tblemployee_y2022 PARTITION OF fps.tblemployee
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.tblemployee_y2023 PARTITION OF fps.tblemployee
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.tblemployee_y2024 PARTITION OF fps.tblemployee
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.tblemployee_y2025 PARTITION OF fps.tblemployee
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.tblemployee_y2026 PARTITION OF fps.tblemployee
    FOR VALUES IN (2026);

-- Foreign keys for fps.tblemployee
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblemployee_fpsyear'
          AND conrelid = 'fps.tblemployee'::regclass
    ) THEN
        ALTER TABLE fps.tblemployee
            ADD CONSTRAINT fk_tblemployee_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
