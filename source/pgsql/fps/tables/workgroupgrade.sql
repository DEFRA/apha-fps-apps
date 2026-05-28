CREATE TABLE IF NOT EXISTS fps.workgroupgrade (
    wggrade character varying(50) NOT NULL,
    profitcentregrade character varying(20) NOT NULL,
    gradecode character varying(10) NOT NULL,
    workgroup character varying(50) NOT NULL,
    chargeratewg money,
    directratewg money DEFAULT 0,
    payratewg money DEFAULT 0,
    nprwg money DEFAULT 0,
    ohrwg money DEFAULT 0,
    avsalary money DEFAULT 0,
    hrschangedby character varying(50),
    fpsyear integer NOT NULL,
    CONSTRAINT pk_workgroupgrade PRIMARY KEY (wggrade, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.workgroupgrade_default PARTITION OF fps.workgroupgrade
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.workgroupgrade_y2016 PARTITION OF fps.workgroupgrade
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.workgroupgrade_y2017 PARTITION OF fps.workgroupgrade
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.workgroupgrade_y2018 PARTITION OF fps.workgroupgrade
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.workgroupgrade_y2019 PARTITION OF fps.workgroupgrade
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.workgroupgrade_y2020 PARTITION OF fps.workgroupgrade
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.workgroupgrade_y2021 PARTITION OF fps.workgroupgrade
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.workgroupgrade_y2022 PARTITION OF fps.workgroupgrade
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.workgroupgrade_y2023 PARTITION OF fps.workgroupgrade
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.workgroupgrade_y2024 PARTITION OF fps.workgroupgrade
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.workgroupgrade_y2025 PARTITION OF fps.workgroupgrade
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.workgroupgrade_y2026 PARTITION OF fps.workgroupgrade
    FOR VALUES IN (2026);

-- Foreign keys for fps.workgroupgrade
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroupgrade_fpsyear'
          AND conrelid = 'fps.workgroupgrade'::regclass
    ) THEN
        ALTER TABLE fps.workgroupgrade
            ADD CONSTRAINT fk_workgroupgrade_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroupgrade_gradecode'
          AND conrelid = 'fps.workgroupgrade'::regclass
    ) THEN
        ALTER TABLE fps.workgroupgrade
            ADD CONSTRAINT fk_workgroupgrade_gradecode FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroupgrade_gradecode_1'
          AND conrelid = 'fps.workgroupgrade'::regclass
    ) THEN
        ALTER TABLE fps.workgroupgrade
            ADD CONSTRAINT fk_workgroupgrade_gradecode_1 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2016(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroupgrade_gradecode_10'
          AND conrelid = 'fps.workgroupgrade'::regclass
    ) THEN
        ALTER TABLE fps.workgroupgrade
            ADD CONSTRAINT fk_workgroupgrade_gradecode_10 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2025(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroupgrade_gradecode_11'
          AND conrelid = 'fps.workgroupgrade'::regclass
    ) THEN
        ALTER TABLE fps.workgroupgrade
            ADD CONSTRAINT fk_workgroupgrade_gradecode_11 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2026(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroupgrade_gradecode_12'
          AND conrelid = 'fps.workgroupgrade'::regclass
    ) THEN
        ALTER TABLE fps.workgroupgrade
            ADD CONSTRAINT fk_workgroupgrade_gradecode_12 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_default(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroupgrade_gradecode_2'
          AND conrelid = 'fps.workgroupgrade'::regclass
    ) THEN
        ALTER TABLE fps.workgroupgrade
            ADD CONSTRAINT fk_workgroupgrade_gradecode_2 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2017(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroupgrade_gradecode_3'
          AND conrelid = 'fps.workgroupgrade'::regclass
    ) THEN
        ALTER TABLE fps.workgroupgrade
            ADD CONSTRAINT fk_workgroupgrade_gradecode_3 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2018(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroupgrade_gradecode_4'
          AND conrelid = 'fps.workgroupgrade'::regclass
    ) THEN
        ALTER TABLE fps.workgroupgrade
            ADD CONSTRAINT fk_workgroupgrade_gradecode_4 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2019(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroupgrade_gradecode_5'
          AND conrelid = 'fps.workgroupgrade'::regclass
    ) THEN
        ALTER TABLE fps.workgroupgrade
            ADD CONSTRAINT fk_workgroupgrade_gradecode_5 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2020(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroupgrade_gradecode_6'
          AND conrelid = 'fps.workgroupgrade'::regclass
    ) THEN
        ALTER TABLE fps.workgroupgrade
            ADD CONSTRAINT fk_workgroupgrade_gradecode_6 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2021(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroupgrade_gradecode_7'
          AND conrelid = 'fps.workgroupgrade'::regclass
    ) THEN
        ALTER TABLE fps.workgroupgrade
            ADD CONSTRAINT fk_workgroupgrade_gradecode_7 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2022(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroupgrade_gradecode_8'
          AND conrelid = 'fps.workgroupgrade'::regclass
    ) THEN
        ALTER TABLE fps.workgroupgrade
            ADD CONSTRAINT fk_workgroupgrade_gradecode_8 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2023(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroupgrade_gradecode_9'
          AND conrelid = 'fps.workgroupgrade'::regclass
    ) THEN
        ALTER TABLE fps.workgroupgrade
            ADD CONSTRAINT fk_workgroupgrade_gradecode_9 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2024(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroupgrade_workgroup'
          AND conrelid = 'fps.workgroupgrade'::regclass
    ) THEN
        ALTER TABLE fps.workgroupgrade
            ADD CONSTRAINT fk_workgroupgrade_workgroup FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroupgrade_workgroup_1'
          AND conrelid = 'fps.workgroupgrade'::regclass
    ) THEN
        ALTER TABLE fps.workgroupgrade
            ADD CONSTRAINT fk_workgroupgrade_workgroup_1 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2016(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroupgrade_workgroup_10'
          AND conrelid = 'fps.workgroupgrade'::regclass
    ) THEN
        ALTER TABLE fps.workgroupgrade
            ADD CONSTRAINT fk_workgroupgrade_workgroup_10 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2025(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroupgrade_workgroup_11'
          AND conrelid = 'fps.workgroupgrade'::regclass
    ) THEN
        ALTER TABLE fps.workgroupgrade
            ADD CONSTRAINT fk_workgroupgrade_workgroup_11 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2026(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroupgrade_workgroup_12'
          AND conrelid = 'fps.workgroupgrade'::regclass
    ) THEN
        ALTER TABLE fps.workgroupgrade
            ADD CONSTRAINT fk_workgroupgrade_workgroup_12 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_default(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroupgrade_workgroup_2'
          AND conrelid = 'fps.workgroupgrade'::regclass
    ) THEN
        ALTER TABLE fps.workgroupgrade
            ADD CONSTRAINT fk_workgroupgrade_workgroup_2 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2017(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroupgrade_workgroup_3'
          AND conrelid = 'fps.workgroupgrade'::regclass
    ) THEN
        ALTER TABLE fps.workgroupgrade
            ADD CONSTRAINT fk_workgroupgrade_workgroup_3 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2018(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroupgrade_workgroup_4'
          AND conrelid = 'fps.workgroupgrade'::regclass
    ) THEN
        ALTER TABLE fps.workgroupgrade
            ADD CONSTRAINT fk_workgroupgrade_workgroup_4 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2019(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroupgrade_workgroup_5'
          AND conrelid = 'fps.workgroupgrade'::regclass
    ) THEN
        ALTER TABLE fps.workgroupgrade
            ADD CONSTRAINT fk_workgroupgrade_workgroup_5 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2020(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroupgrade_workgroup_6'
          AND conrelid = 'fps.workgroupgrade'::regclass
    ) THEN
        ALTER TABLE fps.workgroupgrade
            ADD CONSTRAINT fk_workgroupgrade_workgroup_6 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2021(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroupgrade_workgroup_7'
          AND conrelid = 'fps.workgroupgrade'::regclass
    ) THEN
        ALTER TABLE fps.workgroupgrade
            ADD CONSTRAINT fk_workgroupgrade_workgroup_7 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2022(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroupgrade_workgroup_8'
          AND conrelid = 'fps.workgroupgrade'::regclass
    ) THEN
        ALTER TABLE fps.workgroupgrade
            ADD CONSTRAINT fk_workgroupgrade_workgroup_8 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2023(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroupgrade_workgroup_9'
          AND conrelid = 'fps.workgroupgrade'::regclass
    ) THEN
        ALTER TABLE fps.workgroupgrade
            ADD CONSTRAINT fk_workgroupgrade_workgroup_9 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2024(workgroup, fpsyear);
    END IF;
END $$;
