CREATE TABLE IF NOT EXISTS fps.tlkpjobcode (
    jobcode character varying(50) NOT NULL,
    parentproject character varying(20),
    jobcodeworkgroup character varying(50),
    newprog character varying(20),
    type character varying(15),
    jobcodename character varying(255),
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tlkpjobcode PRIMARY KEY (jobcode, fpsyear),
    CONSTRAINT tlkpjobcode_ck_tlkpjobcode_1__11 CHECK (type IS NOT NULL)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.tlkpjobcode_default PARTITION OF fps.tlkpjobcode
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.tlkpjobcode_y2016 PARTITION OF fps.tlkpjobcode
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.tlkpjobcode_y2017 PARTITION OF fps.tlkpjobcode
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.tlkpjobcode_y2018 PARTITION OF fps.tlkpjobcode
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.tlkpjobcode_y2019 PARTITION OF fps.tlkpjobcode
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.tlkpjobcode_y2020 PARTITION OF fps.tlkpjobcode
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.tlkpjobcode_y2021 PARTITION OF fps.tlkpjobcode
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.tlkpjobcode_y2022 PARTITION OF fps.tlkpjobcode
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.tlkpjobcode_y2023 PARTITION OF fps.tlkpjobcode
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.tlkpjobcode_y2024 PARTITION OF fps.tlkpjobcode
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.tlkpjobcode_y2025 PARTITION OF fps.tlkpjobcode
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.tlkpjobcode_y2026 PARTITION OF fps.tlkpjobcode
    FOR VALUES IN (2026);

-- Foreign keys for fps.tlkpjobcode
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpjobcode_fpsyear'
          AND conrelid = 'fps.tlkpjobcode'::regclass
    ) THEN
        ALTER TABLE fps.tlkpjobcode
            ADD CONSTRAINT fk_tlkpjobcode_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpjobcode_parentproject'
          AND conrelid = 'fps.tlkpjobcode'::regclass
    ) THEN
        ALTER TABLE fps.tlkpjobcode
            ADD CONSTRAINT fk_tlkpjobcode_parentproject FOREIGN KEY (parentproject, fpsyear) REFERENCES fps.tlkpproject(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpjobcode_parentproject_1'
          AND conrelid = 'fps.tlkpjobcode'::regclass
    ) THEN
        ALTER TABLE fps.tlkpjobcode
            ADD CONSTRAINT fk_tlkpjobcode_parentproject_1 FOREIGN KEY (parentproject, fpsyear) REFERENCES fps.tlkpproject_y2016(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpjobcode_parentproject_10'
          AND conrelid = 'fps.tlkpjobcode'::regclass
    ) THEN
        ALTER TABLE fps.tlkpjobcode
            ADD CONSTRAINT fk_tlkpjobcode_parentproject_10 FOREIGN KEY (parentproject, fpsyear) REFERENCES fps.tlkpproject_y2025(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpjobcode_parentproject_11'
          AND conrelid = 'fps.tlkpjobcode'::regclass
    ) THEN
        ALTER TABLE fps.tlkpjobcode
            ADD CONSTRAINT fk_tlkpjobcode_parentproject_11 FOREIGN KEY (parentproject, fpsyear) REFERENCES fps.tlkpproject_y2026(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpjobcode_parentproject_12'
          AND conrelid = 'fps.tlkpjobcode'::regclass
    ) THEN
        ALTER TABLE fps.tlkpjobcode
            ADD CONSTRAINT fk_tlkpjobcode_parentproject_12 FOREIGN KEY (parentproject, fpsyear) REFERENCES fps.tlkpproject_default(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpjobcode_parentproject_2'
          AND conrelid = 'fps.tlkpjobcode'::regclass
    ) THEN
        ALTER TABLE fps.tlkpjobcode
            ADD CONSTRAINT fk_tlkpjobcode_parentproject_2 FOREIGN KEY (parentproject, fpsyear) REFERENCES fps.tlkpproject_y2017(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpjobcode_parentproject_3'
          AND conrelid = 'fps.tlkpjobcode'::regclass
    ) THEN
        ALTER TABLE fps.tlkpjobcode
            ADD CONSTRAINT fk_tlkpjobcode_parentproject_3 FOREIGN KEY (parentproject, fpsyear) REFERENCES fps.tlkpproject_y2018(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpjobcode_parentproject_4'
          AND conrelid = 'fps.tlkpjobcode'::regclass
    ) THEN
        ALTER TABLE fps.tlkpjobcode
            ADD CONSTRAINT fk_tlkpjobcode_parentproject_4 FOREIGN KEY (parentproject, fpsyear) REFERENCES fps.tlkpproject_y2019(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpjobcode_parentproject_5'
          AND conrelid = 'fps.tlkpjobcode'::regclass
    ) THEN
        ALTER TABLE fps.tlkpjobcode
            ADD CONSTRAINT fk_tlkpjobcode_parentproject_5 FOREIGN KEY (parentproject, fpsyear) REFERENCES fps.tlkpproject_y2020(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpjobcode_parentproject_6'
          AND conrelid = 'fps.tlkpjobcode'::regclass
    ) THEN
        ALTER TABLE fps.tlkpjobcode
            ADD CONSTRAINT fk_tlkpjobcode_parentproject_6 FOREIGN KEY (parentproject, fpsyear) REFERENCES fps.tlkpproject_y2021(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpjobcode_parentproject_7'
          AND conrelid = 'fps.tlkpjobcode'::regclass
    ) THEN
        ALTER TABLE fps.tlkpjobcode
            ADD CONSTRAINT fk_tlkpjobcode_parentproject_7 FOREIGN KEY (parentproject, fpsyear) REFERENCES fps.tlkpproject_y2022(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpjobcode_parentproject_8'
          AND conrelid = 'fps.tlkpjobcode'::regclass
    ) THEN
        ALTER TABLE fps.tlkpjobcode
            ADD CONSTRAINT fk_tlkpjobcode_parentproject_8 FOREIGN KEY (parentproject, fpsyear) REFERENCES fps.tlkpproject_y2023(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpjobcode_parentproject_9'
          AND conrelid = 'fps.tlkpjobcode'::regclass
    ) THEN
        ALTER TABLE fps.tlkpjobcode
            ADD CONSTRAINT fk_tlkpjobcode_parentproject_9 FOREIGN KEY (parentproject, fpsyear) REFERENCES fps.tlkpproject_y2024(parentproject, fpsyear);
    END IF;
END $$;
