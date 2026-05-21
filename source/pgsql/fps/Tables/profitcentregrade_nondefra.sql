CREATE TABLE IF NOT EXISTS fps.profitcentregrade_nondefra (
    pcgrade character varying(20) NOT NULL,
    divisiongrade character varying(10) NOT NULL,
    gradecode character varying(10) NOT NULL,
    profitcentre character varying(50) NOT NULL,
    chargerate money DEFAULT 0,
    directrate money DEFAULT 0,
    payrate money DEFAULT 0,
    npr money DEFAULT 0,
    ohr money DEFAULT 0,
    hrsavailable double precision DEFAULT 0,
    oldchargerate money DEFAULT 0,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_profitcentregrade_nondefra PRIMARY KEY (pcgrade, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.profitcentregrade_nondefra_default PARTITION OF fps.profitcentregrade_nondefra
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.profitcentregrade_nondefra_y2016 PARTITION OF fps.profitcentregrade_nondefra
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.profitcentregrade_nondefra_y2017 PARTITION OF fps.profitcentregrade_nondefra
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.profitcentregrade_nondefra_y2018 PARTITION OF fps.profitcentregrade_nondefra
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.profitcentregrade_nondefra_y2019 PARTITION OF fps.profitcentregrade_nondefra
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.profitcentregrade_nondefra_y2020 PARTITION OF fps.profitcentregrade_nondefra
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.profitcentregrade_nondefra_y2021 PARTITION OF fps.profitcentregrade_nondefra
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.profitcentregrade_nondefra_y2022 PARTITION OF fps.profitcentregrade_nondefra
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.profitcentregrade_nondefra_y2023 PARTITION OF fps.profitcentregrade_nondefra
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.profitcentregrade_nondefra_y2024 PARTITION OF fps.profitcentregrade_nondefra
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.profitcentregrade_nondefra_y2025 PARTITION OF fps.profitcentregrade_nondefra
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.profitcentregrade_nondefra_y2026 PARTITION OF fps.profitcentregrade_nondefra
    FOR VALUES IN (2026);

-- Foreign keys for fps.profitcentregrade_nondefra
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_nondefra_divisiongrade'
          AND conrelid = 'fps.profitcentregrade_nondefra'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade_nondefra
            ADD CONSTRAINT fk_profitcentregrade_nondefra_divisiongrade FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_nondefra_divisiongrade_1'
          AND conrelid = 'fps.profitcentregrade_nondefra'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade_nondefra
            ADD CONSTRAINT fk_profitcentregrade_nondefra_divisiongrade_1 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2016(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_nondefra_divisiongrade_10'
          AND conrelid = 'fps.profitcentregrade_nondefra'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade_nondefra
            ADD CONSTRAINT fk_profitcentregrade_nondefra_divisiongrade_10 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2025(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_nondefra_divisiongrade_11'
          AND conrelid = 'fps.profitcentregrade_nondefra'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade_nondefra
            ADD CONSTRAINT fk_profitcentregrade_nondefra_divisiongrade_11 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2026(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_nondefra_divisiongrade_12'
          AND conrelid = 'fps.profitcentregrade_nondefra'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade_nondefra
            ADD CONSTRAINT fk_profitcentregrade_nondefra_divisiongrade_12 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_default(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_nondefra_divisiongrade_2'
          AND conrelid = 'fps.profitcentregrade_nondefra'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade_nondefra
            ADD CONSTRAINT fk_profitcentregrade_nondefra_divisiongrade_2 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2017(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_nondefra_divisiongrade_3'
          AND conrelid = 'fps.profitcentregrade_nondefra'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade_nondefra
            ADD CONSTRAINT fk_profitcentregrade_nondefra_divisiongrade_3 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2018(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_nondefra_divisiongrade_4'
          AND conrelid = 'fps.profitcentregrade_nondefra'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade_nondefra
            ADD CONSTRAINT fk_profitcentregrade_nondefra_divisiongrade_4 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2019(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_nondefra_divisiongrade_5'
          AND conrelid = 'fps.profitcentregrade_nondefra'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade_nondefra
            ADD CONSTRAINT fk_profitcentregrade_nondefra_divisiongrade_5 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2020(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_nondefra_divisiongrade_6'
          AND conrelid = 'fps.profitcentregrade_nondefra'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade_nondefra
            ADD CONSTRAINT fk_profitcentregrade_nondefra_divisiongrade_6 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2021(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_nondefra_divisiongrade_7'
          AND conrelid = 'fps.profitcentregrade_nondefra'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade_nondefra
            ADD CONSTRAINT fk_profitcentregrade_nondefra_divisiongrade_7 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2022(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_nondefra_divisiongrade_8'
          AND conrelid = 'fps.profitcentregrade_nondefra'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade_nondefra
            ADD CONSTRAINT fk_profitcentregrade_nondefra_divisiongrade_8 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2023(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_nondefra_divisiongrade_9'
          AND conrelid = 'fps.profitcentregrade_nondefra'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade_nondefra
            ADD CONSTRAINT fk_profitcentregrade_nondefra_divisiongrade_9 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2024(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_nondefra_fpsyear'
          AND conrelid = 'fps.profitcentregrade_nondefra'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade_nondefra
            ADD CONSTRAINT fk_profitcentregrade_nondefra_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_nondefra_gradecode'
          AND conrelid = 'fps.profitcentregrade_nondefra'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade_nondefra
            ADD CONSTRAINT fk_profitcentregrade_nondefra_gradecode FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_nondefra_gradecode_1'
          AND conrelid = 'fps.profitcentregrade_nondefra'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade_nondefra
            ADD CONSTRAINT fk_profitcentregrade_nondefra_gradecode_1 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2016(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_nondefra_gradecode_10'
          AND conrelid = 'fps.profitcentregrade_nondefra'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade_nondefra
            ADD CONSTRAINT fk_profitcentregrade_nondefra_gradecode_10 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2025(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_nondefra_gradecode_11'
          AND conrelid = 'fps.profitcentregrade_nondefra'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade_nondefra
            ADD CONSTRAINT fk_profitcentregrade_nondefra_gradecode_11 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2026(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_nondefra_gradecode_12'
          AND conrelid = 'fps.profitcentregrade_nondefra'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade_nondefra
            ADD CONSTRAINT fk_profitcentregrade_nondefra_gradecode_12 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_default(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_nondefra_gradecode_2'
          AND conrelid = 'fps.profitcentregrade_nondefra'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade_nondefra
            ADD CONSTRAINT fk_profitcentregrade_nondefra_gradecode_2 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2017(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_nondefra_gradecode_3'
          AND conrelid = 'fps.profitcentregrade_nondefra'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade_nondefra
            ADD CONSTRAINT fk_profitcentregrade_nondefra_gradecode_3 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2018(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_nondefra_gradecode_4'
          AND conrelid = 'fps.profitcentregrade_nondefra'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade_nondefra
            ADD CONSTRAINT fk_profitcentregrade_nondefra_gradecode_4 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2019(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_nondefra_gradecode_5'
          AND conrelid = 'fps.profitcentregrade_nondefra'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade_nondefra
            ADD CONSTRAINT fk_profitcentregrade_nondefra_gradecode_5 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2020(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_nondefra_gradecode_6'
          AND conrelid = 'fps.profitcentregrade_nondefra'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade_nondefra
            ADD CONSTRAINT fk_profitcentregrade_nondefra_gradecode_6 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2021(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_nondefra_gradecode_7'
          AND conrelid = 'fps.profitcentregrade_nondefra'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade_nondefra
            ADD CONSTRAINT fk_profitcentregrade_nondefra_gradecode_7 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2022(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_nondefra_gradecode_8'
          AND conrelid = 'fps.profitcentregrade_nondefra'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade_nondefra
            ADD CONSTRAINT fk_profitcentregrade_nondefra_gradecode_8 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2023(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_nondefra_gradecode_9'
          AND conrelid = 'fps.profitcentregrade_nondefra'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade_nondefra
            ADD CONSTRAINT fk_profitcentregrade_nondefra_gradecode_9 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2024(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_nondefra_profitcentre'
          AND conrelid = 'fps.profitcentregrade_nondefra'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade_nondefra
            ADD CONSTRAINT fk_profitcentregrade_nondefra_profitcentre FOREIGN KEY (profitcentre) REFERENCES fps.tblkpprofitcentre(profitcentre);
    END IF;
END $$;
