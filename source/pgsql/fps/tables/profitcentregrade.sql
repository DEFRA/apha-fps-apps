CREATE TABLE IF NOT EXISTS fps.profitcentregrade (
    pcgrade character varying(20) NOT NULL,
    divisiongrade character varying(10) NOT NULL,
    gradecode character varying(10) NOT NULL,
    profitcentre character varying(50) NOT NULL,
    directrate money DEFAULT 0,
    payrate money DEFAULT 0,
    npr money DEFAULT 0,
    ohr money DEFAULT 0,
    chargerate money,
    hrsavailable double precision DEFAULT 0,
    oldchargerate money DEFAULT 0,
    defrachargerate money,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_profitcentregrade PRIMARY KEY (pcgrade, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.profitcentregrade_default PARTITION OF fps.profitcentregrade
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.profitcentregrade_y2016 PARTITION OF fps.profitcentregrade
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.profitcentregrade_y2017 PARTITION OF fps.profitcentregrade
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.profitcentregrade_y2018 PARTITION OF fps.profitcentregrade
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.profitcentregrade_y2019 PARTITION OF fps.profitcentregrade
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.profitcentregrade_y2020 PARTITION OF fps.profitcentregrade
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.profitcentregrade_y2021 PARTITION OF fps.profitcentregrade
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.profitcentregrade_y2022 PARTITION OF fps.profitcentregrade
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.profitcentregrade_y2023 PARTITION OF fps.profitcentregrade
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.profitcentregrade_y2024 PARTITION OF fps.profitcentregrade
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.profitcentregrade_y2025 PARTITION OF fps.profitcentregrade
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.profitcentregrade_y2026 PARTITION OF fps.profitcentregrade
    FOR VALUES IN (2026);

-- Foreign keys for fps.profitcentregrade
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_divisiongrade'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_divisiongrade FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_divisiongrade1'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_divisiongrade1 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2016(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_divisiongrade10'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_divisiongrade10 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2025(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_divisiongrade11'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_divisiongrade11 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2026(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_divisiongrade12'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_divisiongrade12 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_default(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_divisiongrade2'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_divisiongrade2 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2017(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_divisiongrade3'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_divisiongrade3 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2018(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_divisiongrade4'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_divisiongrade4 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2019(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_divisiongrade5'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_divisiongrade5 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2020(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_divisiongrade6'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_divisiongrade6 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2021(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_divisiongrade7'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_divisiongrade7 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2022(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_divisiongrade8'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_divisiongrade8 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2023(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_divisiongrade9'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_divisiongrade9 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2024(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_divisiongrade_1'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_divisiongrade_1 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2016(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_divisiongrade_10'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_divisiongrade_10 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2025(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_divisiongrade_11'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_divisiongrade_11 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2026(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_divisiongrade_12'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_divisiongrade_12 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_default(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_divisiongrade_2'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_divisiongrade_2 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2017(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_divisiongrade_3'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_divisiongrade_3 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2018(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_divisiongrade_4'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_divisiongrade_4 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2019(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_divisiongrade_5'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_divisiongrade_5 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2020(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_divisiongrade_6'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_divisiongrade_6 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2021(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_divisiongrade_7'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_divisiongrade_7 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2022(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_divisiongrade_8'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_divisiongrade_8 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2023(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_divisiongrade_9'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_divisiongrade_9 FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade_y2024(divisiongrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_fpsyear'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_gradecode'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_gradecode FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_gradecode1'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_gradecode1 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2016(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_gradecode10'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_gradecode10 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2025(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_gradecode11'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_gradecode11 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2026(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_gradecode12'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_gradecode12 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_default(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_gradecode2'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_gradecode2 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2017(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_gradecode3'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_gradecode3 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2018(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_gradecode4'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_gradecode4 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2019(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_gradecode5'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_gradecode5 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2020(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_gradecode6'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_gradecode6 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2021(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_gradecode7'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_gradecode7 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2022(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_gradecode8'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_gradecode8 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2023(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_gradecode9'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_gradecode9 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2024(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_gradecode_1'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_gradecode_1 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2016(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_gradecode_10'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_gradecode_10 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2025(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_gradecode_11'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_gradecode_11 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2026(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_gradecode_12'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_gradecode_12 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_default(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_gradecode_2'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_gradecode_2 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2017(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_gradecode_3'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_gradecode_3 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2018(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_gradecode_4'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_gradecode_4 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2019(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_gradecode_5'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_gradecode_5 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2020(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_gradecode_6'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_gradecode_6 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2021(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_gradecode_7'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_gradecode_7 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2022(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_gradecode_8'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_gradecode_8 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2023(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_gradecode_9'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_gradecode_9 FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade_y2024(gradecode, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_profitcentregrade_profitcentre'
          AND conrelid = 'fps.profitcentregrade'::regclass
    ) THEN
        ALTER TABLE fps.profitcentregrade
            ADD CONSTRAINT fk_profitcentregrade_profitcentre FOREIGN KEY (profitcentre) REFERENCES fps.tblkpprofitcentre(profitcentre);
    END IF;
END $$;
