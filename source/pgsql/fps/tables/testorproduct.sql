CREATE TABLE IF NOT EXISTS fps.testorproduct (
    itemcode character varying(20) NOT NULL,
    itemdescription character varying(200),
    testmanager character varying(50),
    jobstatus character varying(2),
    unitpricevla money DEFAULT 0,
    priceahvg money,
    owner character varying(2),
    chargemethod character varying(5),
    shortdescription character(18),
    defraunitprice money DEFAULT 0 NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_testorproduct PRIMARY KEY (itemcode, fpsyear),
    CONSTRAINT testorproduct_owner_cannot_be_null CHECK (owner IS NOT NULL AND (owner::text = 'PT'::text OR owner::text = 'PA'::text OR owner::text = 'SD'::text OR owner::text = 'LT'::text))
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.testorproduct_default PARTITION OF fps.testorproduct
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.testorproduct_y2016 PARTITION OF fps.testorproduct
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.testorproduct_y2017 PARTITION OF fps.testorproduct
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.testorproduct_y2018 PARTITION OF fps.testorproduct
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.testorproduct_y2019 PARTITION OF fps.testorproduct
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.testorproduct_y2020 PARTITION OF fps.testorproduct
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.testorproduct_y2021 PARTITION OF fps.testorproduct
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.testorproduct_y2022 PARTITION OF fps.testorproduct
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.testorproduct_y2023 PARTITION OF fps.testorproduct
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.testorproduct_y2024 PARTITION OF fps.testorproduct
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.testorproduct_y2025 PARTITION OF fps.testorproduct
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.testorproduct_y2026 PARTITION OF fps.testorproduct
    FOR VALUES IN (2026);

-- Foreign keys for fps.testorproduct
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_testorproduct_fpsyear'
          AND conrelid = 'fps.testorproduct'::regclass
    ) THEN
        ALTER TABLE fps.testorproduct
            ADD CONSTRAINT fk_testorproduct_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
