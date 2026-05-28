CREATE TABLE IF NOT EXISTS fps.plancatwggrade (
    plancategory character varying(50) NOT NULL,
    wggrade character varying(50) NOT NULL,
    hours integer DEFAULT 0,
    createdby character varying(10),
    selleragrees character varying(10),
    buyeragrees character varying(10),
    fpsyear integer NOT NULL,
    CONSTRAINT pk_plancatwggrade PRIMARY KEY (plancategory, wggrade, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.plancatwggrade_default PARTITION OF fps.plancatwggrade
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.plancatwggrade_y2016 PARTITION OF fps.plancatwggrade
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.plancatwggrade_y2017 PARTITION OF fps.plancatwggrade
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.plancatwggrade_y2018 PARTITION OF fps.plancatwggrade
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.plancatwggrade_y2019 PARTITION OF fps.plancatwggrade
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.plancatwggrade_y2020 PARTITION OF fps.plancatwggrade
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.plancatwggrade_y2021 PARTITION OF fps.plancatwggrade
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.plancatwggrade_y2022 PARTITION OF fps.plancatwggrade
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.plancatwggrade_y2023 PARTITION OF fps.plancatwggrade
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.plancatwggrade_y2024 PARTITION OF fps.plancatwggrade
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.plancatwggrade_y2025 PARTITION OF fps.plancatwggrade
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.plancatwggrade_y2026 PARTITION OF fps.plancatwggrade
    FOR VALUES IN (2026);

-- Foreign keys for fps.plancatwggrade
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_plancatwggrade_fpsyear'
          AND conrelid = 'fps.plancatwggrade'::regclass
    ) THEN
        ALTER TABLE fps.plancatwggrade
            ADD CONSTRAINT fk_plancatwggrade_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_plancatwggrade_plancategory'
          AND conrelid = 'fps.plancatwggrade'::regclass
    ) THEN
        ALTER TABLE fps.plancatwggrade
            ADD CONSTRAINT fk_plancatwggrade_plancategory FOREIGN KEY (plancategory) REFERENCES fps.tblkpplanningcategory(planningcategory);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_plancatwggrade_wggrade'
          AND conrelid = 'fps.plancatwggrade'::regclass
    ) THEN
        ALTER TABLE fps.plancatwggrade
            ADD CONSTRAINT fk_plancatwggrade_wggrade FOREIGN KEY (wggrade, fpsyear) REFERENCES fps.workgroupgrade(wggrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_plancatwggrade_wggrade_1'
          AND conrelid = 'fps.plancatwggrade'::regclass
    ) THEN
        ALTER TABLE fps.plancatwggrade
            ADD CONSTRAINT fk_plancatwggrade_wggrade_1 FOREIGN KEY (wggrade, fpsyear) REFERENCES fps.workgroupgrade_y2016(wggrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_plancatwggrade_wggrade_10'
          AND conrelid = 'fps.plancatwggrade'::regclass
    ) THEN
        ALTER TABLE fps.plancatwggrade
            ADD CONSTRAINT fk_plancatwggrade_wggrade_10 FOREIGN KEY (wggrade, fpsyear) REFERENCES fps.workgroupgrade_y2025(wggrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_plancatwggrade_wggrade_11'
          AND conrelid = 'fps.plancatwggrade'::regclass
    ) THEN
        ALTER TABLE fps.plancatwggrade
            ADD CONSTRAINT fk_plancatwggrade_wggrade_11 FOREIGN KEY (wggrade, fpsyear) REFERENCES fps.workgroupgrade_y2026(wggrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_plancatwggrade_wggrade_12'
          AND conrelid = 'fps.plancatwggrade'::regclass
    ) THEN
        ALTER TABLE fps.plancatwggrade
            ADD CONSTRAINT fk_plancatwggrade_wggrade_12 FOREIGN KEY (wggrade, fpsyear) REFERENCES fps.workgroupgrade_default(wggrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_plancatwggrade_wggrade_2'
          AND conrelid = 'fps.plancatwggrade'::regclass
    ) THEN
        ALTER TABLE fps.plancatwggrade
            ADD CONSTRAINT fk_plancatwggrade_wggrade_2 FOREIGN KEY (wggrade, fpsyear) REFERENCES fps.workgroupgrade_y2017(wggrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_plancatwggrade_wggrade_3'
          AND conrelid = 'fps.plancatwggrade'::regclass
    ) THEN
        ALTER TABLE fps.plancatwggrade
            ADD CONSTRAINT fk_plancatwggrade_wggrade_3 FOREIGN KEY (wggrade, fpsyear) REFERENCES fps.workgroupgrade_y2018(wggrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_plancatwggrade_wggrade_4'
          AND conrelid = 'fps.plancatwggrade'::regclass
    ) THEN
        ALTER TABLE fps.plancatwggrade
            ADD CONSTRAINT fk_plancatwggrade_wggrade_4 FOREIGN KEY (wggrade, fpsyear) REFERENCES fps.workgroupgrade_y2019(wggrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_plancatwggrade_wggrade_5'
          AND conrelid = 'fps.plancatwggrade'::regclass
    ) THEN
        ALTER TABLE fps.plancatwggrade
            ADD CONSTRAINT fk_plancatwggrade_wggrade_5 FOREIGN KEY (wggrade, fpsyear) REFERENCES fps.workgroupgrade_y2020(wggrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_plancatwggrade_wggrade_6'
          AND conrelid = 'fps.plancatwggrade'::regclass
    ) THEN
        ALTER TABLE fps.plancatwggrade
            ADD CONSTRAINT fk_plancatwggrade_wggrade_6 FOREIGN KEY (wggrade, fpsyear) REFERENCES fps.workgroupgrade_y2021(wggrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_plancatwggrade_wggrade_7'
          AND conrelid = 'fps.plancatwggrade'::regclass
    ) THEN
        ALTER TABLE fps.plancatwggrade
            ADD CONSTRAINT fk_plancatwggrade_wggrade_7 FOREIGN KEY (wggrade, fpsyear) REFERENCES fps.workgroupgrade_y2022(wggrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_plancatwggrade_wggrade_8'
          AND conrelid = 'fps.plancatwggrade'::regclass
    ) THEN
        ALTER TABLE fps.plancatwggrade
            ADD CONSTRAINT fk_plancatwggrade_wggrade_8 FOREIGN KEY (wggrade, fpsyear) REFERENCES fps.workgroupgrade_y2023(wggrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_plancatwggrade_wggrade_9'
          AND conrelid = 'fps.plancatwggrade'::regclass
    ) THEN
        ALTER TABLE fps.plancatwggrade
            ADD CONSTRAINT fk_plancatwggrade_wggrade_9 FOREIGN KEY (wggrade, fpsyear) REFERENCES fps.workgroupgrade_y2024(wggrade, fpsyear);
    END IF;
END $$;
