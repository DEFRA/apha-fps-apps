CREATE TABLE IF NOT EXISTS fps.timecodevalid (
    timecode character varying(50) NOT NULL,
    workgroup character varying(50) NOT NULL,
    parentproject character varying(20) NOT NULL,
    testcode character varying(50),
    jobcode character varying(50),
    portfolio character varying(20),
    active boolean NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_timecodevalid PRIMARY KEY (workgroup, timecode, parentproject, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.timecodevalid_default PARTITION OF fps.timecodevalid
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.timecodevalid_y2016 PARTITION OF fps.timecodevalid
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.timecodevalid_y2017 PARTITION OF fps.timecodevalid
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.timecodevalid_y2018 PARTITION OF fps.timecodevalid
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.timecodevalid_y2019 PARTITION OF fps.timecodevalid
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.timecodevalid_y2020 PARTITION OF fps.timecodevalid
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.timecodevalid_y2021 PARTITION OF fps.timecodevalid
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.timecodevalid_y2022 PARTITION OF fps.timecodevalid
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.timecodevalid_y2023 PARTITION OF fps.timecodevalid
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.timecodevalid_y2024 PARTITION OF fps.timecodevalid
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.timecodevalid_y2025 PARTITION OF fps.timecodevalid
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.timecodevalid_y2026 PARTITION OF fps.timecodevalid
    FOR VALUES IN (2026);

-- Foreign keys for fps.timecodevalid
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_timecodevalid_fpsyear'
          AND conrelid = 'fps.timecodevalid'::regclass
    ) THEN
        ALTER TABLE fps.timecodevalid
            ADD CONSTRAINT fk_timecodevalid_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_timecodevalid_parentproject'
          AND conrelid = 'fps.timecodevalid'::regclass
    ) THEN
        ALTER TABLE fps.timecodevalid
            ADD CONSTRAINT fk_timecodevalid_parentproject FOREIGN KEY (parentproject, fpsyear) REFERENCES fps.tlkpproject(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_timecodevalid_parentproject_1'
          AND conrelid = 'fps.timecodevalid'::regclass
    ) THEN
        ALTER TABLE fps.timecodevalid
            ADD CONSTRAINT fk_timecodevalid_parentproject_1 FOREIGN KEY (parentproject, fpsyear) REFERENCES fps.tlkpproject_y2016(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_timecodevalid_parentproject_10'
          AND conrelid = 'fps.timecodevalid'::regclass
    ) THEN
        ALTER TABLE fps.timecodevalid
            ADD CONSTRAINT fk_timecodevalid_parentproject_10 FOREIGN KEY (parentproject, fpsyear) REFERENCES fps.tlkpproject_y2025(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_timecodevalid_parentproject_11'
          AND conrelid = 'fps.timecodevalid'::regclass
    ) THEN
        ALTER TABLE fps.timecodevalid
            ADD CONSTRAINT fk_timecodevalid_parentproject_11 FOREIGN KEY (parentproject, fpsyear) REFERENCES fps.tlkpproject_y2026(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_timecodevalid_parentproject_12'
          AND conrelid = 'fps.timecodevalid'::regclass
    ) THEN
        ALTER TABLE fps.timecodevalid
            ADD CONSTRAINT fk_timecodevalid_parentproject_12 FOREIGN KEY (parentproject, fpsyear) REFERENCES fps.tlkpproject_default(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_timecodevalid_parentproject_2'
          AND conrelid = 'fps.timecodevalid'::regclass
    ) THEN
        ALTER TABLE fps.timecodevalid
            ADD CONSTRAINT fk_timecodevalid_parentproject_2 FOREIGN KEY (parentproject, fpsyear) REFERENCES fps.tlkpproject_y2017(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_timecodevalid_parentproject_3'
          AND conrelid = 'fps.timecodevalid'::regclass
    ) THEN
        ALTER TABLE fps.timecodevalid
            ADD CONSTRAINT fk_timecodevalid_parentproject_3 FOREIGN KEY (parentproject, fpsyear) REFERENCES fps.tlkpproject_y2018(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_timecodevalid_parentproject_4'
          AND conrelid = 'fps.timecodevalid'::regclass
    ) THEN
        ALTER TABLE fps.timecodevalid
            ADD CONSTRAINT fk_timecodevalid_parentproject_4 FOREIGN KEY (parentproject, fpsyear) REFERENCES fps.tlkpproject_y2019(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_timecodevalid_parentproject_5'
          AND conrelid = 'fps.timecodevalid'::regclass
    ) THEN
        ALTER TABLE fps.timecodevalid
            ADD CONSTRAINT fk_timecodevalid_parentproject_5 FOREIGN KEY (parentproject, fpsyear) REFERENCES fps.tlkpproject_y2020(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_timecodevalid_parentproject_6'
          AND conrelid = 'fps.timecodevalid'::regclass
    ) THEN
        ALTER TABLE fps.timecodevalid
            ADD CONSTRAINT fk_timecodevalid_parentproject_6 FOREIGN KEY (parentproject, fpsyear) REFERENCES fps.tlkpproject_y2021(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_timecodevalid_parentproject_7'
          AND conrelid = 'fps.timecodevalid'::regclass
    ) THEN
        ALTER TABLE fps.timecodevalid
            ADD CONSTRAINT fk_timecodevalid_parentproject_7 FOREIGN KEY (parentproject, fpsyear) REFERENCES fps.tlkpproject_y2022(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_timecodevalid_parentproject_8'
          AND conrelid = 'fps.timecodevalid'::regclass
    ) THEN
        ALTER TABLE fps.timecodevalid
            ADD CONSTRAINT fk_timecodevalid_parentproject_8 FOREIGN KEY (parentproject, fpsyear) REFERENCES fps.tlkpproject_y2023(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_timecodevalid_parentproject_9'
          AND conrelid = 'fps.timecodevalid'::regclass
    ) THEN
        ALTER TABLE fps.timecodevalid
            ADD CONSTRAINT fk_timecodevalid_parentproject_9 FOREIGN KEY (parentproject, fpsyear) REFERENCES fps.tlkpproject_y2024(parentproject, fpsyear);
    END IF;
END $$;
