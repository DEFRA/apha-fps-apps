CREATE TABLE IF NOT EXISTS fps.tbltestrccost (
    testcode character varying(20) NOT NULL,
    profitcentre character varying(50) NOT NULL,
    price money DEFAULT 0 NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tbltestrccost PRIMARY KEY (testcode, profitcentre, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.tbltestrccost_default PARTITION OF fps.tbltestrccost
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.tbltestrccost_y2016 PARTITION OF fps.tbltestrccost
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.tbltestrccost_y2017 PARTITION OF fps.tbltestrccost
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.tbltestrccost_y2018 PARTITION OF fps.tbltestrccost
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.tbltestrccost_y2019 PARTITION OF fps.tbltestrccost
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.tbltestrccost_y2020 PARTITION OF fps.tbltestrccost
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.tbltestrccost_y2021 PARTITION OF fps.tbltestrccost
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.tbltestrccost_y2022 PARTITION OF fps.tbltestrccost
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.tbltestrccost_y2023 PARTITION OF fps.tbltestrccost
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.tbltestrccost_y2024 PARTITION OF fps.tbltestrccost
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.tbltestrccost_y2025 PARTITION OF fps.tbltestrccost
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.tbltestrccost_y2026 PARTITION OF fps.tbltestrccost
    FOR VALUES IN (2026);

-- Foreign keys for fps.tbltestrccost
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrccost_fpsyear'
          AND conrelid = 'fps.tbltestrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrccost
            ADD CONSTRAINT fk_tbltestrccost_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrccost_profitcentre'
          AND conrelid = 'fps.tbltestrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrccost
            ADD CONSTRAINT fk_tbltestrccost_profitcentre FOREIGN KEY (profitcentre) REFERENCES fps.tblkpprofitcentre(profitcentre);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrccost_testcode'
          AND conrelid = 'fps.tbltestrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrccost
            ADD CONSTRAINT fk_tbltestrccost_testcode FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrccost_testcode_1'
          AND conrelid = 'fps.tbltestrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrccost
            ADD CONSTRAINT fk_tbltestrccost_testcode_1 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2016(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrccost_testcode_10'
          AND conrelid = 'fps.tbltestrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrccost
            ADD CONSTRAINT fk_tbltestrccost_testcode_10 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2025(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrccost_testcode_11'
          AND conrelid = 'fps.tbltestrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrccost
            ADD CONSTRAINT fk_tbltestrccost_testcode_11 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2026(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrccost_testcode_12'
          AND conrelid = 'fps.tbltestrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrccost
            ADD CONSTRAINT fk_tbltestrccost_testcode_12 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_default(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrccost_testcode_2'
          AND conrelid = 'fps.tbltestrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrccost
            ADD CONSTRAINT fk_tbltestrccost_testcode_2 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2017(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrccost_testcode_3'
          AND conrelid = 'fps.tbltestrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrccost
            ADD CONSTRAINT fk_tbltestrccost_testcode_3 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2018(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrccost_testcode_4'
          AND conrelid = 'fps.tbltestrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrccost
            ADD CONSTRAINT fk_tbltestrccost_testcode_4 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2019(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrccost_testcode_5'
          AND conrelid = 'fps.tbltestrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrccost
            ADD CONSTRAINT fk_tbltestrccost_testcode_5 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2020(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrccost_testcode_6'
          AND conrelid = 'fps.tbltestrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrccost
            ADD CONSTRAINT fk_tbltestrccost_testcode_6 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2021(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrccost_testcode_7'
          AND conrelid = 'fps.tbltestrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrccost
            ADD CONSTRAINT fk_tbltestrccost_testcode_7 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2022(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrccost_testcode_8'
          AND conrelid = 'fps.tbltestrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrccost
            ADD CONSTRAINT fk_tbltestrccost_testcode_8 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2023(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrccost_testcode_9'
          AND conrelid = 'fps.tbltestrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrccost
            ADD CONSTRAINT fk_tbltestrccost_testcode_9 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2024(itemcode, fpsyear);
    END IF;
END $$;
