CREATE TABLE IF NOT EXISTS fps.workgroup (
    workgroup character varying(50) NOT NULL,
    profitcentre character varying(50) NOT NULL,
    costcentre double precision,
    owner character varying(50),
    description character varying(45),
    centraloverhead money DEFAULT 0,
    sendemail smallint,
    cos90 smallint,
    costcentreold double precision,
    email_recipient character varying(50),
    fpsyear integer NOT NULL,
    CONSTRAINT pk_workgroup PRIMARY KEY (workgroup, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.workgroup_default PARTITION OF fps.workgroup
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.workgroup_y2016 PARTITION OF fps.workgroup
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.workgroup_y2017 PARTITION OF fps.workgroup
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.workgroup_y2018 PARTITION OF fps.workgroup
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.workgroup_y2019 PARTITION OF fps.workgroup
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.workgroup_y2020 PARTITION OF fps.workgroup
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.workgroup_y2021 PARTITION OF fps.workgroup
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.workgroup_y2022 PARTITION OF fps.workgroup
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.workgroup_y2023 PARTITION OF fps.workgroup
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.workgroup_y2024 PARTITION OF fps.workgroup
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.workgroup_y2025 PARTITION OF fps.workgroup
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.workgroup_y2026 PARTITION OF fps.workgroup
    FOR VALUES IN (2026);

-- Foreign keys for fps.workgroup
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroup_costcentre'
          AND conrelid = 'fps.workgroup'::regclass
    ) THEN
        ALTER TABLE fps.workgroup
            ADD CONSTRAINT fk_workgroup_costcentre FOREIGN KEY (costcentre, fpsyear) REFERENCES fps.costcentre(costcentre, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroup_costcentre_1'
          AND conrelid = 'fps.workgroup'::regclass
    ) THEN
        ALTER TABLE fps.workgroup
            ADD CONSTRAINT fk_workgroup_costcentre_1 FOREIGN KEY (costcentre, fpsyear) REFERENCES fps.costcentre_y2016(costcentre, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroup_costcentre_10'
          AND conrelid = 'fps.workgroup'::regclass
    ) THEN
        ALTER TABLE fps.workgroup
            ADD CONSTRAINT fk_workgroup_costcentre_10 FOREIGN KEY (costcentre, fpsyear) REFERENCES fps.costcentre_y2025(costcentre, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroup_costcentre_11'
          AND conrelid = 'fps.workgroup'::regclass
    ) THEN
        ALTER TABLE fps.workgroup
            ADD CONSTRAINT fk_workgroup_costcentre_11 FOREIGN KEY (costcentre, fpsyear) REFERENCES fps.costcentre_y2026(costcentre, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroup_costcentre_12'
          AND conrelid = 'fps.workgroup'::regclass
    ) THEN
        ALTER TABLE fps.workgroup
            ADD CONSTRAINT fk_workgroup_costcentre_12 FOREIGN KEY (costcentre, fpsyear) REFERENCES fps.costcentre_default(costcentre, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroup_costcentre_2'
          AND conrelid = 'fps.workgroup'::regclass
    ) THEN
        ALTER TABLE fps.workgroup
            ADD CONSTRAINT fk_workgroup_costcentre_2 FOREIGN KEY (costcentre, fpsyear) REFERENCES fps.costcentre_y2017(costcentre, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroup_costcentre_3'
          AND conrelid = 'fps.workgroup'::regclass
    ) THEN
        ALTER TABLE fps.workgroup
            ADD CONSTRAINT fk_workgroup_costcentre_3 FOREIGN KEY (costcentre, fpsyear) REFERENCES fps.costcentre_y2018(costcentre, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroup_costcentre_4'
          AND conrelid = 'fps.workgroup'::regclass
    ) THEN
        ALTER TABLE fps.workgroup
            ADD CONSTRAINT fk_workgroup_costcentre_4 FOREIGN KEY (costcentre, fpsyear) REFERENCES fps.costcentre_y2019(costcentre, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroup_costcentre_5'
          AND conrelid = 'fps.workgroup'::regclass
    ) THEN
        ALTER TABLE fps.workgroup
            ADD CONSTRAINT fk_workgroup_costcentre_5 FOREIGN KEY (costcentre, fpsyear) REFERENCES fps.costcentre_y2020(costcentre, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroup_costcentre_6'
          AND conrelid = 'fps.workgroup'::regclass
    ) THEN
        ALTER TABLE fps.workgroup
            ADD CONSTRAINT fk_workgroup_costcentre_6 FOREIGN KEY (costcentre, fpsyear) REFERENCES fps.costcentre_y2021(costcentre, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroup_costcentre_7'
          AND conrelid = 'fps.workgroup'::regclass
    ) THEN
        ALTER TABLE fps.workgroup
            ADD CONSTRAINT fk_workgroup_costcentre_7 FOREIGN KEY (costcentre, fpsyear) REFERENCES fps.costcentre_y2022(costcentre, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroup_costcentre_8'
          AND conrelid = 'fps.workgroup'::regclass
    ) THEN
        ALTER TABLE fps.workgroup
            ADD CONSTRAINT fk_workgroup_costcentre_8 FOREIGN KEY (costcentre, fpsyear) REFERENCES fps.costcentre_y2023(costcentre, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroup_costcentre_9'
          AND conrelid = 'fps.workgroup'::regclass
    ) THEN
        ALTER TABLE fps.workgroup
            ADD CONSTRAINT fk_workgroup_costcentre_9 FOREIGN KEY (costcentre, fpsyear) REFERENCES fps.costcentre_y2024(costcentre, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroup_fpsyear'
          AND conrelid = 'fps.workgroup'::regclass
    ) THEN
        ALTER TABLE fps.workgroup
            ADD CONSTRAINT fk_workgroup_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_workgroup_profitcentre'
          AND conrelid = 'fps.workgroup'::regclass
    ) THEN
        ALTER TABLE fps.workgroup
            ADD CONSTRAINT fk_workgroup_profitcentre FOREIGN KEY (profitcentre) REFERENCES fps.tblkpprofitcentre(profitcentre);
    END IF;
END $$;
