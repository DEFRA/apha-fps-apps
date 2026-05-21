CREATE TABLE IF NOT EXISTS fps.proj_invoice (
    projectparent character varying(20) NOT NULL,
    month integer,
    amount money,
    costofwork money,
    wip money,
    profitloss money,
    detail character varying(100),
    invoicecounter integer GENERATED ALWAYS AS IDENTITY,
    x character varying(5),
    type character varying(10),
    fpsyear integer NOT NULL,
    CONSTRAINT pk_proj_invoice PRIMARY KEY (invoicecounter, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.proj_invoice_default PARTITION OF fps.proj_invoice
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.proj_invoice_y2016 PARTITION OF fps.proj_invoice
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.proj_invoice_y2017 PARTITION OF fps.proj_invoice
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.proj_invoice_y2018 PARTITION OF fps.proj_invoice
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.proj_invoice_y2019 PARTITION OF fps.proj_invoice
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.proj_invoice_y2020 PARTITION OF fps.proj_invoice
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.proj_invoice_y2021 PARTITION OF fps.proj_invoice
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.proj_invoice_y2022 PARTITION OF fps.proj_invoice
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.proj_invoice_y2023 PARTITION OF fps.proj_invoice
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.proj_invoice_y2024 PARTITION OF fps.proj_invoice
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.proj_invoice_y2025 PARTITION OF fps.proj_invoice
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.proj_invoice_y2026 PARTITION OF fps.proj_invoice
    FOR VALUES IN (2026);

-- Foreign keys for fps.proj_invoice
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_proj_invoice_fpsyear'
          AND conrelid = 'fps.proj_invoice'::regclass
    ) THEN
        ALTER TABLE fps.proj_invoice
            ADD CONSTRAINT fk_proj_invoice_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_proj_invoice_projectparent'
          AND conrelid = 'fps.proj_invoice'::regclass
    ) THEN
        ALTER TABLE fps.proj_invoice
            ADD CONSTRAINT fk_proj_invoice_projectparent FOREIGN KEY (projectparent, fpsyear) REFERENCES fps.tlkpproject(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_proj_invoice_projectparent1'
          AND conrelid = 'fps.proj_invoice'::regclass
    ) THEN
        ALTER TABLE fps.proj_invoice
            ADD CONSTRAINT fk_proj_invoice_projectparent1 FOREIGN KEY (projectparent, fpsyear) REFERENCES fps.tlkpproject_y2016(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_proj_invoice_projectparent10'
          AND conrelid = 'fps.proj_invoice'::regclass
    ) THEN
        ALTER TABLE fps.proj_invoice
            ADD CONSTRAINT fk_proj_invoice_projectparent10 FOREIGN KEY (projectparent, fpsyear) REFERENCES fps.tlkpproject_y2025(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_proj_invoice_projectparent11'
          AND conrelid = 'fps.proj_invoice'::regclass
    ) THEN
        ALTER TABLE fps.proj_invoice
            ADD CONSTRAINT fk_proj_invoice_projectparent11 FOREIGN KEY (projectparent, fpsyear) REFERENCES fps.tlkpproject_y2026(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_proj_invoice_projectparent12'
          AND conrelid = 'fps.proj_invoice'::regclass
    ) THEN
        ALTER TABLE fps.proj_invoice
            ADD CONSTRAINT fk_proj_invoice_projectparent12 FOREIGN KEY (projectparent, fpsyear) REFERENCES fps.tlkpproject_default(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_proj_invoice_projectparent2'
          AND conrelid = 'fps.proj_invoice'::regclass
    ) THEN
        ALTER TABLE fps.proj_invoice
            ADD CONSTRAINT fk_proj_invoice_projectparent2 FOREIGN KEY (projectparent, fpsyear) REFERENCES fps.tlkpproject_y2017(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_proj_invoice_projectparent3'
          AND conrelid = 'fps.proj_invoice'::regclass
    ) THEN
        ALTER TABLE fps.proj_invoice
            ADD CONSTRAINT fk_proj_invoice_projectparent3 FOREIGN KEY (projectparent, fpsyear) REFERENCES fps.tlkpproject_y2018(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_proj_invoice_projectparent4'
          AND conrelid = 'fps.proj_invoice'::regclass
    ) THEN
        ALTER TABLE fps.proj_invoice
            ADD CONSTRAINT fk_proj_invoice_projectparent4 FOREIGN KEY (projectparent, fpsyear) REFERENCES fps.tlkpproject_y2019(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_proj_invoice_projectparent5'
          AND conrelid = 'fps.proj_invoice'::regclass
    ) THEN
        ALTER TABLE fps.proj_invoice
            ADD CONSTRAINT fk_proj_invoice_projectparent5 FOREIGN KEY (projectparent, fpsyear) REFERENCES fps.tlkpproject_y2020(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_proj_invoice_projectparent6'
          AND conrelid = 'fps.proj_invoice'::regclass
    ) THEN
        ALTER TABLE fps.proj_invoice
            ADD CONSTRAINT fk_proj_invoice_projectparent6 FOREIGN KEY (projectparent, fpsyear) REFERENCES fps.tlkpproject_y2021(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_proj_invoice_projectparent7'
          AND conrelid = 'fps.proj_invoice'::regclass
    ) THEN
        ALTER TABLE fps.proj_invoice
            ADD CONSTRAINT fk_proj_invoice_projectparent7 FOREIGN KEY (projectparent, fpsyear) REFERENCES fps.tlkpproject_y2022(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_proj_invoice_projectparent8'
          AND conrelid = 'fps.proj_invoice'::regclass
    ) THEN
        ALTER TABLE fps.proj_invoice
            ADD CONSTRAINT fk_proj_invoice_projectparent8 FOREIGN KEY (projectparent, fpsyear) REFERENCES fps.tlkpproject_y2023(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_proj_invoice_projectparent9'
          AND conrelid = 'fps.proj_invoice'::regclass
    ) THEN
        ALTER TABLE fps.proj_invoice
            ADD CONSTRAINT fk_proj_invoice_projectparent9 FOREIGN KEY (projectparent, fpsyear) REFERENCES fps.tlkpproject_y2024(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_proj_invoice_projectparent_1'
          AND conrelid = 'fps.proj_invoice'::regclass
    ) THEN
        ALTER TABLE fps.proj_invoice
            ADD CONSTRAINT fk_proj_invoice_projectparent_1 FOREIGN KEY (projectparent, fpsyear) REFERENCES fps.tlkpproject_y2016(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_proj_invoice_projectparent_10'
          AND conrelid = 'fps.proj_invoice'::regclass
    ) THEN
        ALTER TABLE fps.proj_invoice
            ADD CONSTRAINT fk_proj_invoice_projectparent_10 FOREIGN KEY (projectparent, fpsyear) REFERENCES fps.tlkpproject_y2025(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_proj_invoice_projectparent_11'
          AND conrelid = 'fps.proj_invoice'::regclass
    ) THEN
        ALTER TABLE fps.proj_invoice
            ADD CONSTRAINT fk_proj_invoice_projectparent_11 FOREIGN KEY (projectparent, fpsyear) REFERENCES fps.tlkpproject_y2026(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_proj_invoice_projectparent_12'
          AND conrelid = 'fps.proj_invoice'::regclass
    ) THEN
        ALTER TABLE fps.proj_invoice
            ADD CONSTRAINT fk_proj_invoice_projectparent_12 FOREIGN KEY (projectparent, fpsyear) REFERENCES fps.tlkpproject_default(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_proj_invoice_projectparent_2'
          AND conrelid = 'fps.proj_invoice'::regclass
    ) THEN
        ALTER TABLE fps.proj_invoice
            ADD CONSTRAINT fk_proj_invoice_projectparent_2 FOREIGN KEY (projectparent, fpsyear) REFERENCES fps.tlkpproject_y2017(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_proj_invoice_projectparent_3'
          AND conrelid = 'fps.proj_invoice'::regclass
    ) THEN
        ALTER TABLE fps.proj_invoice
            ADD CONSTRAINT fk_proj_invoice_projectparent_3 FOREIGN KEY (projectparent, fpsyear) REFERENCES fps.tlkpproject_y2018(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_proj_invoice_projectparent_4'
          AND conrelid = 'fps.proj_invoice'::regclass
    ) THEN
        ALTER TABLE fps.proj_invoice
            ADD CONSTRAINT fk_proj_invoice_projectparent_4 FOREIGN KEY (projectparent, fpsyear) REFERENCES fps.tlkpproject_y2019(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_proj_invoice_projectparent_5'
          AND conrelid = 'fps.proj_invoice'::regclass
    ) THEN
        ALTER TABLE fps.proj_invoice
            ADD CONSTRAINT fk_proj_invoice_projectparent_5 FOREIGN KEY (projectparent, fpsyear) REFERENCES fps.tlkpproject_y2020(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_proj_invoice_projectparent_6'
          AND conrelid = 'fps.proj_invoice'::regclass
    ) THEN
        ALTER TABLE fps.proj_invoice
            ADD CONSTRAINT fk_proj_invoice_projectparent_6 FOREIGN KEY (projectparent, fpsyear) REFERENCES fps.tlkpproject_y2021(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_proj_invoice_projectparent_7'
          AND conrelid = 'fps.proj_invoice'::regclass
    ) THEN
        ALTER TABLE fps.proj_invoice
            ADD CONSTRAINT fk_proj_invoice_projectparent_7 FOREIGN KEY (projectparent, fpsyear) REFERENCES fps.tlkpproject_y2022(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_proj_invoice_projectparent_8'
          AND conrelid = 'fps.proj_invoice'::regclass
    ) THEN
        ALTER TABLE fps.proj_invoice
            ADD CONSTRAINT fk_proj_invoice_projectparent_8 FOREIGN KEY (projectparent, fpsyear) REFERENCES fps.tlkpproject_y2023(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_proj_invoice_projectparent_9'
          AND conrelid = 'fps.proj_invoice'::regclass
    ) THEN
        ALTER TABLE fps.proj_invoice
            ADD CONSTRAINT fk_proj_invoice_projectparent_9 FOREIGN KEY (projectparent, fpsyear) REFERENCES fps.tlkpproject_y2024(parentproject, fpsyear);
    END IF;
END $$;
