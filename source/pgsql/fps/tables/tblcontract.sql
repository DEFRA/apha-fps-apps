CREATE TABLE IF NOT EXISTS fps.tblcontract (
    contractno character varying(10) NOT NULL,
    category character varying(20) NOT NULL,
    manager character varying(50),
    customer character varying(50),
    title character varying(100),
    registereddate timestamp without time zone,
    startdate timestamp without time zone,
    enddate timestamp without time zone,
    contractdoc bytea,
    duration integer,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tblcontract PRIMARY KEY (contractno, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.tblcontract_default PARTITION OF fps.tblcontract
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.tblcontract_y2016 PARTITION OF fps.tblcontract
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.tblcontract_y2017 PARTITION OF fps.tblcontract
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.tblcontract_y2018 PARTITION OF fps.tblcontract
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.tblcontract_y2019 PARTITION OF fps.tblcontract
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.tblcontract_y2020 PARTITION OF fps.tblcontract
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.tblcontract_y2021 PARTITION OF fps.tblcontract
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.tblcontract_y2022 PARTITION OF fps.tblcontract
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.tblcontract_y2023 PARTITION OF fps.tblcontract
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.tblcontract_y2024 PARTITION OF fps.tblcontract
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.tblcontract_y2025 PARTITION OF fps.tblcontract
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.tblcontract_y2026 PARTITION OF fps.tblcontract
    FOR VALUES IN (2026);

-- Foreign keys for fps.tblcontract
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblcontract_3__10'
          AND conrelid = 'fps.tblcontract'::regclass
    ) THEN
        ALTER TABLE fps.tblcontract
            ADD CONSTRAINT fk_tblcontract_3__10 FOREIGN KEY (category) REFERENCES fps.tblcategory(category);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblcontract_customer'
          AND conrelid = 'fps.tblcontract'::regclass
    ) THEN
        ALTER TABLE fps.tblcontract
            ADD CONSTRAINT fk_tblcontract_customer FOREIGN KEY (customer) REFERENCES fps.tlkpcustomer(customer);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblcontract_fpsyear'
          AND conrelid = 'fps.tblcontract'::regclass
    ) THEN
        ALTER TABLE fps.tblcontract
            ADD CONSTRAINT fk_tblcontract_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
