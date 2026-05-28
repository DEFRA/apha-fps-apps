CREATE TABLE IF NOT EXISTS fps.tlkptestcapability (
    testcode character varying(20) NOT NULL,
    workgroup character varying(50) NOT NULL,
    planportfolio character varying(20) NOT NULL,
    unitcost money DEFAULT 0,
    predoutturn double precision DEFAULT 0,
    sop character varying(50),
    smscode character varying(50),
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tlkptestcapability PRIMARY KEY (testcode, workgroup, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.tlkptestcapability_default PARTITION OF fps.tlkptestcapability
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.tlkptestcapability_y2016 PARTITION OF fps.tlkptestcapability
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.tlkptestcapability_y2017 PARTITION OF fps.tlkptestcapability
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.tlkptestcapability_y2018 PARTITION OF fps.tlkptestcapability
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.tlkptestcapability_y2019 PARTITION OF fps.tlkptestcapability
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.tlkptestcapability_y2020 PARTITION OF fps.tlkptestcapability
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.tlkptestcapability_y2021 PARTITION OF fps.tlkptestcapability
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.tlkptestcapability_y2022 PARTITION OF fps.tlkptestcapability
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.tlkptestcapability_y2023 PARTITION OF fps.tlkptestcapability
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.tlkptestcapability_y2024 PARTITION OF fps.tlkptestcapability
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.tlkptestcapability_y2025 PARTITION OF fps.tlkptestcapability
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.tlkptestcapability_y2026 PARTITION OF fps.tlkptestcapability
    FOR VALUES IN (2026);

-- Foreign keys for fps.tlkptestcapability
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_fpsyear'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_planportfolio'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_planportfolio FOREIGN KEY (planportfolio, fpsyear) REFERENCES fps.tlkpproject(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_planportfolio_1'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_planportfolio_1 FOREIGN KEY (planportfolio, fpsyear) REFERENCES fps.tlkpproject_y2016(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_planportfolio_10'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_planportfolio_10 FOREIGN KEY (planportfolio, fpsyear) REFERENCES fps.tlkpproject_y2025(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_planportfolio_11'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_planportfolio_11 FOREIGN KEY (planportfolio, fpsyear) REFERENCES fps.tlkpproject_y2026(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_planportfolio_12'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_planportfolio_12 FOREIGN KEY (planportfolio, fpsyear) REFERENCES fps.tlkpproject_default(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_planportfolio_2'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_planportfolio_2 FOREIGN KEY (planportfolio, fpsyear) REFERENCES fps.tlkpproject_y2017(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_planportfolio_3'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_planportfolio_3 FOREIGN KEY (planportfolio, fpsyear) REFERENCES fps.tlkpproject_y2018(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_planportfolio_4'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_planportfolio_4 FOREIGN KEY (planportfolio, fpsyear) REFERENCES fps.tlkpproject_y2019(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_planportfolio_5'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_planportfolio_5 FOREIGN KEY (planportfolio, fpsyear) REFERENCES fps.tlkpproject_y2020(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_planportfolio_6'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_planportfolio_6 FOREIGN KEY (planportfolio, fpsyear) REFERENCES fps.tlkpproject_y2021(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_planportfolio_7'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_planportfolio_7 FOREIGN KEY (planportfolio, fpsyear) REFERENCES fps.tlkpproject_y2022(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_planportfolio_8'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_planportfolio_8 FOREIGN KEY (planportfolio, fpsyear) REFERENCES fps.tlkpproject_y2023(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_planportfolio_9'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_planportfolio_9 FOREIGN KEY (planportfolio, fpsyear) REFERENCES fps.tlkpproject_y2024(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_testcode'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_testcode FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_testcode_1'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_testcode_1 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2016(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_testcode_10'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_testcode_10 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2025(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_testcode_11'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_testcode_11 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2026(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_testcode_12'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_testcode_12 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_default(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_testcode_2'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_testcode_2 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2017(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_testcode_3'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_testcode_3 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2018(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_testcode_4'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_testcode_4 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2019(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_testcode_5'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_testcode_5 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2020(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_testcode_6'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_testcode_6 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2021(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_testcode_7'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_testcode_7 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2022(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_testcode_8'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_testcode_8 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2023(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_testcode_9'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_testcode_9 FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct_y2024(itemcode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_workgroup'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_workgroup FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_workgroup_1'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_workgroup_1 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2016(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_workgroup_10'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_workgroup_10 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2025(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_workgroup_11'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_workgroup_11 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2026(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_workgroup_12'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_workgroup_12 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_default(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_workgroup_2'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_workgroup_2 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2017(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_workgroup_3'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_workgroup_3 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2018(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_workgroup_4'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_workgroup_4 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2019(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_workgroup_5'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_workgroup_5 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2020(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_workgroup_6'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_workgroup_6 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2021(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_workgroup_7'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_workgroup_7 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2022(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_workgroup_8'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_workgroup_8 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2023(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkptestcapability_workgroup_9'
          AND conrelid = 'fps.tlkptestcapability'::regclass
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_workgroup_9 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2024(workgroup, fpsyear);
    END IF;
END $$;
