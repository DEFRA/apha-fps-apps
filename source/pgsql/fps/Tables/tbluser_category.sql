CREATE TABLE IF NOT EXISTS fps.tbluser_category (
    user_id integer NOT NULL,
    category character varying(20) NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tbluser_category PRIMARY KEY (user_id, category, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.tbluser_category_default PARTITION OF fps.tbluser_category
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.tbluser_category_y2016 PARTITION OF fps.tbluser_category
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.tbluser_category_y2017 PARTITION OF fps.tbluser_category
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.tbluser_category_y2018 PARTITION OF fps.tbluser_category
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.tbluser_category_y2019 PARTITION OF fps.tbluser_category
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.tbluser_category_y2020 PARTITION OF fps.tbluser_category
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.tbluser_category_y2021 PARTITION OF fps.tbluser_category
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.tbluser_category_y2022 PARTITION OF fps.tbluser_category
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.tbluser_category_y2023 PARTITION OF fps.tbluser_category
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.tbluser_category_y2024 PARTITION OF fps.tbluser_category
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.tbluser_category_y2025 PARTITION OF fps.tbluser_category
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.tbluser_category_y2026 PARTITION OF fps.tbluser_category
    FOR VALUES IN (2026);

-- Foreign keys for fps.tbluser_category
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbluser_category_fpsyear'
          AND conrelid = 'fps.tbluser_category'::regclass
    ) THEN
        ALTER TABLE fps.tbluser_category
            ADD CONSTRAINT fk_tbluser_category_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
