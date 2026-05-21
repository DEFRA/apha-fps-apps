CREATE TABLE IF NOT EXISTS fps.tbluser_profitcentre (
    profitcentre character varying(50) NOT NULL,
    user_id integer NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tbluser_profitcentre PRIMARY KEY (profitcentre, user_id, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.tbluser_profitcentre_default PARTITION OF fps.tbluser_profitcentre
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.tbluser_profitcentre_y2016 PARTITION OF fps.tbluser_profitcentre
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.tbluser_profitcentre_y2017 PARTITION OF fps.tbluser_profitcentre
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.tbluser_profitcentre_y2018 PARTITION OF fps.tbluser_profitcentre
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.tbluser_profitcentre_y2019 PARTITION OF fps.tbluser_profitcentre
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.tbluser_profitcentre_y2020 PARTITION OF fps.tbluser_profitcentre
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.tbluser_profitcentre_y2021 PARTITION OF fps.tbluser_profitcentre
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.tbluser_profitcentre_y2022 PARTITION OF fps.tbluser_profitcentre
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.tbluser_profitcentre_y2023 PARTITION OF fps.tbluser_profitcentre
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.tbluser_profitcentre_y2024 PARTITION OF fps.tbluser_profitcentre
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.tbluser_profitcentre_y2025 PARTITION OF fps.tbluser_profitcentre
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.tbluser_profitcentre_y2026 PARTITION OF fps.tbluser_profitcentre
    FOR VALUES IN (2026);

-- Foreign keys for fps.tbluser_profitcentre
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbluser_profitcentre_fpsyear'
          AND conrelid = 'fps.tbluser_profitcentre'::regclass
    ) THEN
        ALTER TABLE fps.tbluser_profitcentre
            ADD CONSTRAINT fk_tbluser_profitcentre_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
