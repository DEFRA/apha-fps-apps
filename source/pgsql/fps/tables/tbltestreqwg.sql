CREATE TABLE IF NOT EXISTS fps.tbltestreqwg (
    testcode character varying(20) NOT NULL,
    buyer character varying(20) NOT NULL,
    workgroup character varying(50) NOT NULL,
    amount integer DEFAULT 0,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tbltestreqwg PRIMARY KEY (testcode, buyer, workgroup, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.tbltestreqwg_default PARTITION OF fps.tbltestreqwg
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.tbltestreqwg_y2016 PARTITION OF fps.tbltestreqwg
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.tbltestreqwg_y2017 PARTITION OF fps.tbltestreqwg
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.tbltestreqwg_y2018 PARTITION OF fps.tbltestreqwg
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.tbltestreqwg_y2019 PARTITION OF fps.tbltestreqwg
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.tbltestreqwg_y2020 PARTITION OF fps.tbltestreqwg
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.tbltestreqwg_y2021 PARTITION OF fps.tbltestreqwg
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.tbltestreqwg_y2022 PARTITION OF fps.tbltestreqwg
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.tbltestreqwg_y2023 PARTITION OF fps.tbltestreqwg
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.tbltestreqwg_y2024 PARTITION OF fps.tbltestreqwg
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.tbltestreqwg_y2025 PARTITION OF fps.tbltestreqwg
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.tbltestreqwg_y2026 PARTITION OF fps.tbltestreqwg
    FOR VALUES IN (2026);

-- Foreign keys for fps.tbltestreqwg
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestreqwg_fpsyear'
          AND conrelid = 'fps.tbltestreqwg'::regclass
    ) THEN
        ALTER TABLE fps.tbltestreqwg
            ADD CONSTRAINT fk_tbltestreqwg_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
