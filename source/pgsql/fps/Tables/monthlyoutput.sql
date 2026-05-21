CREATE TABLE IF NOT EXISTS fps.monthlyoutput (
    testcode character varying(20) NOT NULL,
    buyer character varying(20) NOT NULL,
    month double precision NOT NULL,
    workgroup character varying(50) NOT NULL,
    volume double precision,
    wgbuyer character varying(50),
    fpsyear integer NOT NULL,
    CONSTRAINT pk_monthlyoutput PRIMARY KEY (testcode, buyer, month, workgroup, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.monthlyoutput_default PARTITION OF fps.monthlyoutput
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.monthlyoutput_y2016 PARTITION OF fps.monthlyoutput
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.monthlyoutput_y2017 PARTITION OF fps.monthlyoutput
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.monthlyoutput_y2018 PARTITION OF fps.monthlyoutput
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.monthlyoutput_y2019 PARTITION OF fps.monthlyoutput
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.monthlyoutput_y2020 PARTITION OF fps.monthlyoutput
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.monthlyoutput_y2021 PARTITION OF fps.monthlyoutput
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.monthlyoutput_y2022 PARTITION OF fps.monthlyoutput
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.monthlyoutput_y2023 PARTITION OF fps.monthlyoutput
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.monthlyoutput_y2024 PARTITION OF fps.monthlyoutput
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.monthlyoutput_y2025 PARTITION OF fps.monthlyoutput
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.monthlyoutput_y2026 PARTITION OF fps.monthlyoutput
    FOR VALUES IN (2026);

-- Foreign keys for fps.monthlyoutput
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlyoutput_fpsyear'
          AND conrelid = 'fps.monthlyoutput'::regclass
    ) THEN
        ALTER TABLE fps.monthlyoutput
            ADD CONSTRAINT fk_monthlyoutput_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlyoutput_testcode_buyer'
          AND conrelid = 'fps.monthlyoutput'::regclass
    ) THEN
        ALTER TABLE fps.monthlyoutput
            ADD CONSTRAINT fk_monthlyoutput_testcode_buyer FOREIGN KEY (testcode, buyer, fpsyear) REFERENCES fps.tlkptestreqmt(testcode, buyer, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlyoutput_testcode_buyer_1'
          AND conrelid = 'fps.monthlyoutput'::regclass
    ) THEN
        ALTER TABLE fps.monthlyoutput
            ADD CONSTRAINT fk_monthlyoutput_testcode_buyer_1 FOREIGN KEY (testcode, buyer, fpsyear) REFERENCES fps.tlkptestreqmt_y2016(testcode, buyer, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlyoutput_testcode_buyer_10'
          AND conrelid = 'fps.monthlyoutput'::regclass
    ) THEN
        ALTER TABLE fps.monthlyoutput
            ADD CONSTRAINT fk_monthlyoutput_testcode_buyer_10 FOREIGN KEY (testcode, buyer, fpsyear) REFERENCES fps.tlkptestreqmt_y2025(testcode, buyer, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlyoutput_testcode_buyer_11'
          AND conrelid = 'fps.monthlyoutput'::regclass
    ) THEN
        ALTER TABLE fps.monthlyoutput
            ADD CONSTRAINT fk_monthlyoutput_testcode_buyer_11 FOREIGN KEY (testcode, buyer, fpsyear) REFERENCES fps.tlkptestreqmt_y2026(testcode, buyer, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlyoutput_testcode_buyer_12'
          AND conrelid = 'fps.monthlyoutput'::regclass
    ) THEN
        ALTER TABLE fps.monthlyoutput
            ADD CONSTRAINT fk_monthlyoutput_testcode_buyer_12 FOREIGN KEY (testcode, buyer, fpsyear) REFERENCES fps.tlkptestreqmt_default(testcode, buyer, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlyoutput_testcode_buyer_2'
          AND conrelid = 'fps.monthlyoutput'::regclass
    ) THEN
        ALTER TABLE fps.monthlyoutput
            ADD CONSTRAINT fk_monthlyoutput_testcode_buyer_2 FOREIGN KEY (testcode, buyer, fpsyear) REFERENCES fps.tlkptestreqmt_y2017(testcode, buyer, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlyoutput_testcode_buyer_3'
          AND conrelid = 'fps.monthlyoutput'::regclass
    ) THEN
        ALTER TABLE fps.monthlyoutput
            ADD CONSTRAINT fk_monthlyoutput_testcode_buyer_3 FOREIGN KEY (testcode, buyer, fpsyear) REFERENCES fps.tlkptestreqmt_y2018(testcode, buyer, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlyoutput_testcode_buyer_4'
          AND conrelid = 'fps.monthlyoutput'::regclass
    ) THEN
        ALTER TABLE fps.monthlyoutput
            ADD CONSTRAINT fk_monthlyoutput_testcode_buyer_4 FOREIGN KEY (testcode, buyer, fpsyear) REFERENCES fps.tlkptestreqmt_y2019(testcode, buyer, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlyoutput_testcode_buyer_5'
          AND conrelid = 'fps.monthlyoutput'::regclass
    ) THEN
        ALTER TABLE fps.monthlyoutput
            ADD CONSTRAINT fk_monthlyoutput_testcode_buyer_5 FOREIGN KEY (testcode, buyer, fpsyear) REFERENCES fps.tlkptestreqmt_y2020(testcode, buyer, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlyoutput_testcode_buyer_6'
          AND conrelid = 'fps.monthlyoutput'::regclass
    ) THEN
        ALTER TABLE fps.monthlyoutput
            ADD CONSTRAINT fk_monthlyoutput_testcode_buyer_6 FOREIGN KEY (testcode, buyer, fpsyear) REFERENCES fps.tlkptestreqmt_y2021(testcode, buyer, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlyoutput_testcode_buyer_7'
          AND conrelid = 'fps.monthlyoutput'::regclass
    ) THEN
        ALTER TABLE fps.monthlyoutput
            ADD CONSTRAINT fk_monthlyoutput_testcode_buyer_7 FOREIGN KEY (testcode, buyer, fpsyear) REFERENCES fps.tlkptestreqmt_y2022(testcode, buyer, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlyoutput_testcode_buyer_8'
          AND conrelid = 'fps.monthlyoutput'::regclass
    ) THEN
        ALTER TABLE fps.monthlyoutput
            ADD CONSTRAINT fk_monthlyoutput_testcode_buyer_8 FOREIGN KEY (testcode, buyer, fpsyear) REFERENCES fps.tlkptestreqmt_y2023(testcode, buyer, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlyoutput_testcode_buyer_9'
          AND conrelid = 'fps.monthlyoutput'::regclass
    ) THEN
        ALTER TABLE fps.monthlyoutput
            ADD CONSTRAINT fk_monthlyoutput_testcode_buyer_9 FOREIGN KEY (testcode, buyer, fpsyear) REFERENCES fps.tlkptestreqmt_y2024(testcode, buyer, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlyoutput_testcode_workgroup'
          AND conrelid = 'fps.monthlyoutput'::regclass
    ) THEN
        ALTER TABLE fps.monthlyoutput
            ADD CONSTRAINT fk_monthlyoutput_testcode_workgroup FOREIGN KEY (testcode, workgroup, fpsyear) REFERENCES fps.tlkptestcapability(testcode, workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlyoutput_testcode_workgroup_1'
          AND conrelid = 'fps.monthlyoutput'::regclass
    ) THEN
        ALTER TABLE fps.monthlyoutput
            ADD CONSTRAINT fk_monthlyoutput_testcode_workgroup_1 FOREIGN KEY (testcode, workgroup, fpsyear) REFERENCES fps.tlkptestcapability_y2016(testcode, workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlyoutput_testcode_workgroup_10'
          AND conrelid = 'fps.monthlyoutput'::regclass
    ) THEN
        ALTER TABLE fps.monthlyoutput
            ADD CONSTRAINT fk_monthlyoutput_testcode_workgroup_10 FOREIGN KEY (testcode, workgroup, fpsyear) REFERENCES fps.tlkptestcapability_y2025(testcode, workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlyoutput_testcode_workgroup_11'
          AND conrelid = 'fps.monthlyoutput'::regclass
    ) THEN
        ALTER TABLE fps.monthlyoutput
            ADD CONSTRAINT fk_monthlyoutput_testcode_workgroup_11 FOREIGN KEY (testcode, workgroup, fpsyear) REFERENCES fps.tlkptestcapability_y2026(testcode, workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlyoutput_testcode_workgroup_12'
          AND conrelid = 'fps.monthlyoutput'::regclass
    ) THEN
        ALTER TABLE fps.monthlyoutput
            ADD CONSTRAINT fk_monthlyoutput_testcode_workgroup_12 FOREIGN KEY (testcode, workgroup, fpsyear) REFERENCES fps.tlkptestcapability_default(testcode, workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlyoutput_testcode_workgroup_2'
          AND conrelid = 'fps.monthlyoutput'::regclass
    ) THEN
        ALTER TABLE fps.monthlyoutput
            ADD CONSTRAINT fk_monthlyoutput_testcode_workgroup_2 FOREIGN KEY (testcode, workgroup, fpsyear) REFERENCES fps.tlkptestcapability_y2017(testcode, workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlyoutput_testcode_workgroup_3'
          AND conrelid = 'fps.monthlyoutput'::regclass
    ) THEN
        ALTER TABLE fps.monthlyoutput
            ADD CONSTRAINT fk_monthlyoutput_testcode_workgroup_3 FOREIGN KEY (testcode, workgroup, fpsyear) REFERENCES fps.tlkptestcapability_y2018(testcode, workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlyoutput_testcode_workgroup_4'
          AND conrelid = 'fps.monthlyoutput'::regclass
    ) THEN
        ALTER TABLE fps.monthlyoutput
            ADD CONSTRAINT fk_monthlyoutput_testcode_workgroup_4 FOREIGN KEY (testcode, workgroup, fpsyear) REFERENCES fps.tlkptestcapability_y2019(testcode, workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlyoutput_testcode_workgroup_5'
          AND conrelid = 'fps.monthlyoutput'::regclass
    ) THEN
        ALTER TABLE fps.monthlyoutput
            ADD CONSTRAINT fk_monthlyoutput_testcode_workgroup_5 FOREIGN KEY (testcode, workgroup, fpsyear) REFERENCES fps.tlkptestcapability_y2020(testcode, workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlyoutput_testcode_workgroup_6'
          AND conrelid = 'fps.monthlyoutput'::regclass
    ) THEN
        ALTER TABLE fps.monthlyoutput
            ADD CONSTRAINT fk_monthlyoutput_testcode_workgroup_6 FOREIGN KEY (testcode, workgroup, fpsyear) REFERENCES fps.tlkptestcapability_y2021(testcode, workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlyoutput_testcode_workgroup_7'
          AND conrelid = 'fps.monthlyoutput'::regclass
    ) THEN
        ALTER TABLE fps.monthlyoutput
            ADD CONSTRAINT fk_monthlyoutput_testcode_workgroup_7 FOREIGN KEY (testcode, workgroup, fpsyear) REFERENCES fps.tlkptestcapability_y2022(testcode, workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlyoutput_testcode_workgroup_8'
          AND conrelid = 'fps.monthlyoutput'::regclass
    ) THEN
        ALTER TABLE fps.monthlyoutput
            ADD CONSTRAINT fk_monthlyoutput_testcode_workgroup_8 FOREIGN KEY (testcode, workgroup, fpsyear) REFERENCES fps.tlkptestcapability_y2023(testcode, workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlyoutput_testcode_workgroup_9'
          AND conrelid = 'fps.monthlyoutput'::regclass
    ) THEN
        ALTER TABLE fps.monthlyoutput
            ADD CONSTRAINT fk_monthlyoutput_testcode_workgroup_9 FOREIGN KEY (testcode, workgroup, fpsyear) REFERENCES fps.tlkptestcapability_y2024(testcode, workgroup, fpsyear);
    END IF;
END $$;
