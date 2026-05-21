CREATE TABLE IF NOT EXISTS fps.tlkptestreqmt (
    testcode character varying(20) NOT NULL,
    buyer character varying(20) NOT NULL,
    unitprice money,
    norequired double precision,
    projectbuyercode character varying(50),
    testbuyercode character varying(50),
    datecreated timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    active smallint DEFAULT 1,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tlkptestreqmt PRIMARY KEY (testcode, buyer, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.tlkptestreqmt_default PARTITION OF fps.tlkptestreqmt
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.tlkptestreqmt_y2016 PARTITION OF fps.tlkptestreqmt
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.tlkptestreqmt_y2017 PARTITION OF fps.tlkptestreqmt
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.tlkptestreqmt_y2018 PARTITION OF fps.tlkptestreqmt
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.tlkptestreqmt_y2019 PARTITION OF fps.tlkptestreqmt
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.tlkptestreqmt_y2020 PARTITION OF fps.tlkptestreqmt
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.tlkptestreqmt_y2021 PARTITION OF fps.tlkptestreqmt
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.tlkptestreqmt_y2022 PARTITION OF fps.tlkptestreqmt
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.tlkptestreqmt_y2023 PARTITION OF fps.tlkptestreqmt
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.tlkptestreqmt_y2024 PARTITION OF fps.tlkptestreqmt
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.tlkptestreqmt_y2025 PARTITION OF fps.tlkptestreqmt
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.tlkptestreqmt_y2026 PARTITION OF fps.tlkptestreqmt
    FOR VALUES IN (2026);

-- Foreign keys for fps.tlkptestreqmt
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestreqmt_fpsyear'
          AND conrelid = 'fps.tlkptestreqmt'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestreqmt
            ADD CONSTRAINT fk_tlkptestreqmt_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestreqmt_testcode'
          AND conrelid = 'fps.tlkptestreqmt'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestreqmt
            ADD CONSTRAINT fk_tlkptestreqmt_testcode FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestreqmt_testcode_1'
          AND conrelid = 'fps.tlkptestreqmt'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestreqmt
            ADD CONSTRAINT fk_tlkptestreqmt_testcode_1 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2016(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestreqmt_testcode_10'
          AND conrelid = 'fps.tlkptestreqmt'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestreqmt
            ADD CONSTRAINT fk_tlkptestreqmt_testcode_10 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2025(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestreqmt_testcode_11'
          AND conrelid = 'fps.tlkptestreqmt'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestreqmt
            ADD CONSTRAINT fk_tlkptestreqmt_testcode_11 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2026(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestreqmt_testcode_12'
          AND conrelid = 'fps.tlkptestreqmt'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestreqmt
            ADD CONSTRAINT fk_tlkptestreqmt_testcode_12 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_default(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestreqmt_testcode_2'
          AND conrelid = 'fps.tlkptestreqmt'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestreqmt
            ADD CONSTRAINT fk_tlkptestreqmt_testcode_2 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2017(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestreqmt_testcode_3'
          AND conrelid = 'fps.tlkptestreqmt'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestreqmt
            ADD CONSTRAINT fk_tlkptestreqmt_testcode_3 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2018(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestreqmt_testcode_4'
          AND conrelid = 'fps.tlkptestreqmt'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestreqmt
            ADD CONSTRAINT fk_tlkptestreqmt_testcode_4 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2019(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestreqmt_testcode_5'
          AND conrelid = 'fps.tlkptestreqmt'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestreqmt
            ADD CONSTRAINT fk_tlkptestreqmt_testcode_5 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2020(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestreqmt_testcode_6'
          AND conrelid = 'fps.tlkptestreqmt'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestreqmt
            ADD CONSTRAINT fk_tlkptestreqmt_testcode_6 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2021(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestreqmt_testcode_7'
          AND conrelid = 'fps.tlkptestreqmt'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestreqmt
            ADD CONSTRAINT fk_tlkptestreqmt_testcode_7 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2022(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestreqmt_testcode_8'
          AND conrelid = 'fps.tlkptestreqmt'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestreqmt
            ADD CONSTRAINT fk_tlkptestreqmt_testcode_8 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2023(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestreqmt_testcode_9'
          AND conrelid = 'fps.tlkptestreqmt'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestreqmt
            ADD CONSTRAINT fk_tlkptestreqmt_testcode_9 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2024(itemcode, fpsyear);
    END IF;
END $$;
