CREATE TABLE IF NOT EXISTS fps.tbltestrequirementrccost (
    testcode character varying(20) NOT NULL,
    buyer character varying(20) NOT NULL,
    profitcentre character varying(50) NOT NULL,
    price money NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tbltestrequirementrccost PRIMARY KEY (testcode, buyer, profitcentre, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.tbltestrequirementrccost_default PARTITION OF fps.tbltestrequirementrccost
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.tbltestrequirementrccost_y2016 PARTITION OF fps.tbltestrequirementrccost
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.tbltestrequirementrccost_y2017 PARTITION OF fps.tbltestrequirementrccost
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.tbltestrequirementrccost_y2018 PARTITION OF fps.tbltestrequirementrccost
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.tbltestrequirementrccost_y2019 PARTITION OF fps.tbltestrequirementrccost
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.tbltestrequirementrccost_y2020 PARTITION OF fps.tbltestrequirementrccost
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.tbltestrequirementrccost_y2021 PARTITION OF fps.tbltestrequirementrccost
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.tbltestrequirementrccost_y2022 PARTITION OF fps.tbltestrequirementrccost
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.tbltestrequirementrccost_y2023 PARTITION OF fps.tbltestrequirementrccost
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.tbltestrequirementrccost_y2024 PARTITION OF fps.tbltestrequirementrccost
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.tbltestrequirementrccost_y2025 PARTITION OF fps.tbltestrequirementrccost
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.tbltestrequirementrccost_y2026 PARTITION OF fps.tbltestrequirementrccost
    FOR VALUES IN (2026);

-- Foreign keys for fps.tbltestrequirementrccost
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrequirementrccost_fpsyear'
          AND conrelid = 'fps.tbltestrequirementrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrequirementrccost
            ADD CONSTRAINT fk_tbltestrequirementrccost_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrequirementrccost_testcode_buyer'
          AND conrelid = 'fps.tbltestrequirementrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrequirementrccost
            ADD CONSTRAINT fk_tbltestrequirementrccost_testcode_buyer FOREIGN KEY (testcode, buyer, fpsyear) REFERENCES fps.tlkptestreqmt(testcode, buyer, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrequirementrccost_testcode_buyer_1'
          AND conrelid = 'fps.tbltestrequirementrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrequirementrccost
            ADD CONSTRAINT fk_tbltestrequirementrccost_testcode_buyer_1 FOREIGN KEY (testcode, buyer, fpsyear) REFERENCES fps.tlkptestreqmt_y2016(testcode, buyer, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrequirementrccost_testcode_buyer_10'
          AND conrelid = 'fps.tbltestrequirementrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrequirementrccost
            ADD CONSTRAINT fk_tbltestrequirementrccost_testcode_buyer_10 FOREIGN KEY (testcode, buyer, fpsyear) REFERENCES fps.tlkptestreqmt_y2025(testcode, buyer, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrequirementrccost_testcode_buyer_11'
          AND conrelid = 'fps.tbltestrequirementrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrequirementrccost
            ADD CONSTRAINT fk_tbltestrequirementrccost_testcode_buyer_11 FOREIGN KEY (testcode, buyer, fpsyear) REFERENCES fps.tlkptestreqmt_y2026(testcode, buyer, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrequirementrccost_testcode_buyer_12'
          AND conrelid = 'fps.tbltestrequirementrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrequirementrccost
            ADD CONSTRAINT fk_tbltestrequirementrccost_testcode_buyer_12 FOREIGN KEY (testcode, buyer, fpsyear) REFERENCES fps.tlkptestreqmt_default(testcode, buyer, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrequirementrccost_testcode_buyer_2'
          AND conrelid = 'fps.tbltestrequirementrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrequirementrccost
            ADD CONSTRAINT fk_tbltestrequirementrccost_testcode_buyer_2 FOREIGN KEY (testcode, buyer, fpsyear) REFERENCES fps.tlkptestreqmt_y2017(testcode, buyer, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrequirementrccost_testcode_buyer_3'
          AND conrelid = 'fps.tbltestrequirementrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrequirementrccost
            ADD CONSTRAINT fk_tbltestrequirementrccost_testcode_buyer_3 FOREIGN KEY (testcode, buyer, fpsyear) REFERENCES fps.tlkptestreqmt_y2018(testcode, buyer, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrequirementrccost_testcode_buyer_4'
          AND conrelid = 'fps.tbltestrequirementrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrequirementrccost
            ADD CONSTRAINT fk_tbltestrequirementrccost_testcode_buyer_4 FOREIGN KEY (testcode, buyer, fpsyear) REFERENCES fps.tlkptestreqmt_y2019(testcode, buyer, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrequirementrccost_testcode_buyer_5'
          AND conrelid = 'fps.tbltestrequirementrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrequirementrccost
            ADD CONSTRAINT fk_tbltestrequirementrccost_testcode_buyer_5 FOREIGN KEY (testcode, buyer, fpsyear) REFERENCES fps.tlkptestreqmt_y2020(testcode, buyer, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrequirementrccost_testcode_buyer_6'
          AND conrelid = 'fps.tbltestrequirementrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrequirementrccost
            ADD CONSTRAINT fk_tbltestrequirementrccost_testcode_buyer_6 FOREIGN KEY (testcode, buyer, fpsyear) REFERENCES fps.tlkptestreqmt_y2021(testcode, buyer, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrequirementrccost_testcode_buyer_7'
          AND conrelid = 'fps.tbltestrequirementrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrequirementrccost
            ADD CONSTRAINT fk_tbltestrequirementrccost_testcode_buyer_7 FOREIGN KEY (testcode, buyer, fpsyear) REFERENCES fps.tlkptestreqmt_y2022(testcode, buyer, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrequirementrccost_testcode_buyer_8'
          AND conrelid = 'fps.tbltestrequirementrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrequirementrccost
            ADD CONSTRAINT fk_tbltestrequirementrccost_testcode_buyer_8 FOREIGN KEY (testcode, buyer, fpsyear) REFERENCES fps.tlkptestreqmt_y2023(testcode, buyer, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrequirementrccost_testcode_buyer_9'
          AND conrelid = 'fps.tbltestrequirementrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrequirementrccost
            ADD CONSTRAINT fk_tbltestrequirementrccost_testcode_buyer_9 FOREIGN KEY (testcode, buyer, fpsyear) REFERENCES fps.tlkptestreqmt_y2024(testcode, buyer, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrequirementrccost_testcode_profitcentre'
          AND conrelid = 'fps.tbltestrequirementrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrequirementrccost
            ADD CONSTRAINT fk_tbltestrequirementrccost_testcode_profitcentre FOREIGN KEY (testcode, profitcentre, fpsyear) REFERENCES fps.tbltestrccost(testcode, profitcentre, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrequirementrccost_testcode_profitcentre_1'
          AND conrelid = 'fps.tbltestrequirementrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrequirementrccost
            ADD CONSTRAINT fk_tbltestrequirementrccost_testcode_profitcentre_1 FOREIGN KEY (testcode, profitcentre, fpsyear) REFERENCES fps.tbltestrccost_y2016(testcode, profitcentre, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrequirementrccost_testcode_profitcentre_10'
          AND conrelid = 'fps.tbltestrequirementrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrequirementrccost
            ADD CONSTRAINT fk_tbltestrequirementrccost_testcode_profitcentre_10 FOREIGN KEY (testcode, profitcentre, fpsyear) REFERENCES fps.tbltestrccost_y2025(testcode, profitcentre, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrequirementrccost_testcode_profitcentre_11'
          AND conrelid = 'fps.tbltestrequirementrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrequirementrccost
            ADD CONSTRAINT fk_tbltestrequirementrccost_testcode_profitcentre_11 FOREIGN KEY (testcode, profitcentre, fpsyear) REFERENCES fps.tbltestrccost_y2026(testcode, profitcentre, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrequirementrccost_testcode_profitcentre_12'
          AND conrelid = 'fps.tbltestrequirementrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrequirementrccost
            ADD CONSTRAINT fk_tbltestrequirementrccost_testcode_profitcentre_12 FOREIGN KEY (testcode, profitcentre, fpsyear) REFERENCES fps.tbltestrccost_default(testcode, profitcentre, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrequirementrccost_testcode_profitcentre_2'
          AND conrelid = 'fps.tbltestrequirementrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrequirementrccost
            ADD CONSTRAINT fk_tbltestrequirementrccost_testcode_profitcentre_2 FOREIGN KEY (testcode, profitcentre, fpsyear) REFERENCES fps.tbltestrccost_y2017(testcode, profitcentre, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrequirementrccost_testcode_profitcentre_3'
          AND conrelid = 'fps.tbltestrequirementrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrequirementrccost
            ADD CONSTRAINT fk_tbltestrequirementrccost_testcode_profitcentre_3 FOREIGN KEY (testcode, profitcentre, fpsyear) REFERENCES fps.tbltestrccost_y2018(testcode, profitcentre, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrequirementrccost_testcode_profitcentre_4'
          AND conrelid = 'fps.tbltestrequirementrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrequirementrccost
            ADD CONSTRAINT fk_tbltestrequirementrccost_testcode_profitcentre_4 FOREIGN KEY (testcode, profitcentre, fpsyear) REFERENCES fps.tbltestrccost_y2019(testcode, profitcentre, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrequirementrccost_testcode_profitcentre_5'
          AND conrelid = 'fps.tbltestrequirementrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrequirementrccost
            ADD CONSTRAINT fk_tbltestrequirementrccost_testcode_profitcentre_5 FOREIGN KEY (testcode, profitcentre, fpsyear) REFERENCES fps.tbltestrccost_y2020(testcode, profitcentre, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrequirementrccost_testcode_profitcentre_6'
          AND conrelid = 'fps.tbltestrequirementrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrequirementrccost
            ADD CONSTRAINT fk_tbltestrequirementrccost_testcode_profitcentre_6 FOREIGN KEY (testcode, profitcentre, fpsyear) REFERENCES fps.tbltestrccost_y2021(testcode, profitcentre, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrequirementrccost_testcode_profitcentre_7'
          AND conrelid = 'fps.tbltestrequirementrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrequirementrccost
            ADD CONSTRAINT fk_tbltestrequirementrccost_testcode_profitcentre_7 FOREIGN KEY (testcode, profitcentre, fpsyear) REFERENCES fps.tbltestrccost_y2022(testcode, profitcentre, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrequirementrccost_testcode_profitcentre_8'
          AND conrelid = 'fps.tbltestrequirementrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrequirementrccost
            ADD CONSTRAINT fk_tbltestrequirementrccost_testcode_profitcentre_8 FOREIGN KEY (testcode, profitcentre, fpsyear) REFERENCES fps.tbltestrccost_y2023(testcode, profitcentre, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrequirementrccost_testcode_profitcentre_9'
          AND conrelid = 'fps.tbltestrequirementrccost'::regclass
    ) THEN
        ALTER TABLE fps.tbltestrequirementrccost
            ADD CONSTRAINT fk_tbltestrequirementrccost_testcode_profitcentre_9 FOREIGN KEY (testcode, profitcentre, fpsyear) REFERENCES fps.tbltestrccost_y2024(testcode, profitcentre, fpsyear);
    END IF;
END $$;
