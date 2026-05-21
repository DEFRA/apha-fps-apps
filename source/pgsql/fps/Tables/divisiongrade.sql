CREATE TABLE IF NOT EXISTS fps.divisiongrade (
    divisiongrade character varying(10) NOT NULL,
    gradecode character varying(10) NOT NULL,
    division character varying(10) NOT NULL,
    chargerate money DEFAULT 0,
    directrate money DEFAULT 0,
    payrate money DEFAULT 0,
    npr money DEFAULT 0,
    ohr money DEFAULT 0,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_divisiongrade PRIMARY KEY (divisiongrade, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.divisiongrade_default PARTITION OF fps.divisiongrade
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.divisiongrade_y2016 PARTITION OF fps.divisiongrade
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.divisiongrade_y2017 PARTITION OF fps.divisiongrade
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.divisiongrade_y2018 PARTITION OF fps.divisiongrade
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.divisiongrade_y2019 PARTITION OF fps.divisiongrade
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.divisiongrade_y2020 PARTITION OF fps.divisiongrade
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.divisiongrade_y2021 PARTITION OF fps.divisiongrade
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.divisiongrade_y2022 PARTITION OF fps.divisiongrade
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.divisiongrade_y2023 PARTITION OF fps.divisiongrade
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.divisiongrade_y2024 PARTITION OF fps.divisiongrade
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.divisiongrade_y2025 PARTITION OF fps.divisiongrade
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.divisiongrade_y2026 PARTITION OF fps.divisiongrade
    FOR VALUES IN (2026);

-- Foreign keys for fps.divisiongrade
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_divisiongrade_division'
          AND conrelid = 'fps.divisiongrade'::regclass
    ) THEN
        ALTER TABLE fps.divisiongrade
            ADD CONSTRAINT fk_divisiongrade_division FOREIGN KEY (division) REFERENCES fps.tlkpdivision(divname);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_divisiongrade_fpsyear'
          AND conrelid = 'fps.divisiongrade'::regclass
    ) THEN
        ALTER TABLE fps.divisiongrade
            ADD CONSTRAINT fk_divisiongrade_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_divisiongrade_gradecode'
          AND conrelid = 'fps.divisiongrade'::regclass
    ) THEN
        ALTER TABLE fps.divisiongrade
            ADD CONSTRAINT fk_divisiongrade_gradecode FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_divisiongrade_gradecode_1'
          AND conrelid = 'fps.divisiongrade'::regclass
    ) THEN
        ALTER TABLE fps.divisiongrade
            ADD CONSTRAINT fk_divisiongrade_gradecode_1 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2016(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_divisiongrade_gradecode_10'
          AND conrelid = 'fps.divisiongrade'::regclass
    ) THEN
        ALTER TABLE fps.divisiongrade
            ADD CONSTRAINT fk_divisiongrade_gradecode_10 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2025(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_divisiongrade_gradecode_11'
          AND conrelid = 'fps.divisiongrade'::regclass
    ) THEN
        ALTER TABLE fps.divisiongrade
            ADD CONSTRAINT fk_divisiongrade_gradecode_11 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2026(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_divisiongrade_gradecode_12'
          AND conrelid = 'fps.divisiongrade'::regclass
    ) THEN
        ALTER TABLE fps.divisiongrade
            ADD CONSTRAINT fk_divisiongrade_gradecode_12 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_default(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_divisiongrade_gradecode_2'
          AND conrelid = 'fps.divisiongrade'::regclass
    ) THEN
        ALTER TABLE fps.divisiongrade
            ADD CONSTRAINT fk_divisiongrade_gradecode_2 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2017(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_divisiongrade_gradecode_3'
          AND conrelid = 'fps.divisiongrade'::regclass
    ) THEN
        ALTER TABLE fps.divisiongrade
            ADD CONSTRAINT fk_divisiongrade_gradecode_3 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2018(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_divisiongrade_gradecode_4'
          AND conrelid = 'fps.divisiongrade'::regclass
    ) THEN
        ALTER TABLE fps.divisiongrade
            ADD CONSTRAINT fk_divisiongrade_gradecode_4 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2019(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_divisiongrade_gradecode_5'
          AND conrelid = 'fps.divisiongrade'::regclass
    ) THEN
        ALTER TABLE fps.divisiongrade
            ADD CONSTRAINT fk_divisiongrade_gradecode_5 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2020(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_divisiongrade_gradecode_6'
          AND conrelid = 'fps.divisiongrade'::regclass
    ) THEN
        ALTER TABLE fps.divisiongrade
            ADD CONSTRAINT fk_divisiongrade_gradecode_6 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2021(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_divisiongrade_gradecode_7'
          AND conrelid = 'fps.divisiongrade'::regclass
    ) THEN
        ALTER TABLE fps.divisiongrade
            ADD CONSTRAINT fk_divisiongrade_gradecode_7 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2022(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_divisiongrade_gradecode_8'
          AND conrelid = 'fps.divisiongrade'::regclass
    ) THEN
        ALTER TABLE fps.divisiongrade
            ADD CONSTRAINT fk_divisiongrade_gradecode_8 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2023(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_divisiongrade_gradecode_9'
          AND conrelid = 'fps.divisiongrade'::regclass
    ) THEN
        ALTER TABLE fps.divisiongrade
            ADD CONSTRAINT fk_divisiongrade_gradecode_9 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2024(gradecode, fpsyear);
    END IF;
END $$;
