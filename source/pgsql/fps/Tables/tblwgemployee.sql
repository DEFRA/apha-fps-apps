CREATE TABLE IF NOT EXISTS fps.tblwgemployee (
    pactid character varying(50) NOT NULL,
    spnumber character varying(10) NOT NULL,
    workgroupgrade character varying(50) NOT NULL,
    personstatus character varying(10) DEFAULT 'A'::character varying NOT NULL,
    personclass character varying(10),
    hrspaid double precision NOT NULL,
    leave double precision NOT NULL,
    sickspecial double precision NOT NULL,
    hrsavail double precision NOT NULL,
    makeavailable integer DEFAULT '-1'::integer NOT NULL,
    timerecorder integer DEFAULT 0 NOT NULL,
    startdate timestamp without time zone,
    enddate timestamp without time zone,
    hoursperweek double precision,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tblwgemployee PRIMARY KEY (pactid, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.tblwgemployee_default PARTITION OF fps.tblwgemployee
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.tblwgemployee_y2016 PARTITION OF fps.tblwgemployee
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.tblwgemployee_y2017 PARTITION OF fps.tblwgemployee
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.tblwgemployee_y2018 PARTITION OF fps.tblwgemployee
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.tblwgemployee_y2019 PARTITION OF fps.tblwgemployee
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.tblwgemployee_y2020 PARTITION OF fps.tblwgemployee
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.tblwgemployee_y2021 PARTITION OF fps.tblwgemployee
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.tblwgemployee_y2022 PARTITION OF fps.tblwgemployee
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.tblwgemployee_y2023 PARTITION OF fps.tblwgemployee
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.tblwgemployee_y2024 PARTITION OF fps.tblwgemployee
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.tblwgemployee_y2025 PARTITION OF fps.tblwgemployee
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.tblwgemployee_y2026 PARTITION OF fps.tblwgemployee
    FOR VALUES IN (2026);

-- Foreign keys for fps.tblwgemployee
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblwgemployee_fpsyear'
          AND conrelid = 'fps.tblwgemployee'::regclass
    ) THEN
        ALTER TABLE fps.tblwgemployee
            ADD CONSTRAINT fk_tblwgemployee_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblwgemployee_spnumber'
          AND conrelid = 'fps.tblwgemployee'::regclass
    ) THEN
        ALTER TABLE fps.tblwgemployee
            ADD CONSTRAINT fk_tblwgemployee_spnumber FOREIGN KEY (spnumber, fpsyear) REFERENCES fps.tblemployee(spnumber, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblwgemployee_spnumber_1'
          AND conrelid = 'fps.tblwgemployee'::regclass
    ) THEN
        ALTER TABLE fps.tblwgemployee
            ADD CONSTRAINT fk_tblwgemployee_spnumber_1 FOREIGN KEY (spnumber, fpsyear) REFERENCES fps.tblemployee_y2016(spnumber, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblwgemployee_spnumber_10'
          AND conrelid = 'fps.tblwgemployee'::regclass
    ) THEN
        ALTER TABLE fps.tblwgemployee
            ADD CONSTRAINT fk_tblwgemployee_spnumber_10 FOREIGN KEY (spnumber, fpsyear) REFERENCES fps.tblemployee_y2025(spnumber, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblwgemployee_spnumber_11'
          AND conrelid = 'fps.tblwgemployee'::regclass
    ) THEN
        ALTER TABLE fps.tblwgemployee
            ADD CONSTRAINT fk_tblwgemployee_spnumber_11 FOREIGN KEY (spnumber, fpsyear) REFERENCES fps.tblemployee_y2026(spnumber, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblwgemployee_spnumber_12'
          AND conrelid = 'fps.tblwgemployee'::regclass
    ) THEN
        ALTER TABLE fps.tblwgemployee
            ADD CONSTRAINT fk_tblwgemployee_spnumber_12 FOREIGN KEY (spnumber, fpsyear) REFERENCES fps.tblemployee_default(spnumber, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblwgemployee_spnumber_2'
          AND conrelid = 'fps.tblwgemployee'::regclass
    ) THEN
        ALTER TABLE fps.tblwgemployee
            ADD CONSTRAINT fk_tblwgemployee_spnumber_2 FOREIGN KEY (spnumber, fpsyear) REFERENCES fps.tblemployee_y2017(spnumber, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblwgemployee_spnumber_3'
          AND conrelid = 'fps.tblwgemployee'::regclass
    ) THEN
        ALTER TABLE fps.tblwgemployee
            ADD CONSTRAINT fk_tblwgemployee_spnumber_3 FOREIGN KEY (spnumber, fpsyear) REFERENCES fps.tblemployee_y2018(spnumber, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblwgemployee_spnumber_4'
          AND conrelid = 'fps.tblwgemployee'::regclass
    ) THEN
        ALTER TABLE fps.tblwgemployee
            ADD CONSTRAINT fk_tblwgemployee_spnumber_4 FOREIGN KEY (spnumber, fpsyear) REFERENCES fps.tblemployee_y2019(spnumber, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblwgemployee_spnumber_5'
          AND conrelid = 'fps.tblwgemployee'::regclass
    ) THEN
        ALTER TABLE fps.tblwgemployee
            ADD CONSTRAINT fk_tblwgemployee_spnumber_5 FOREIGN KEY (spnumber, fpsyear) REFERENCES fps.tblemployee_y2020(spnumber, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblwgemployee_spnumber_6'
          AND conrelid = 'fps.tblwgemployee'::regclass
    ) THEN
        ALTER TABLE fps.tblwgemployee
            ADD CONSTRAINT fk_tblwgemployee_spnumber_6 FOREIGN KEY (spnumber, fpsyear) REFERENCES fps.tblemployee_y2021(spnumber, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblwgemployee_spnumber_7'
          AND conrelid = 'fps.tblwgemployee'::regclass
    ) THEN
        ALTER TABLE fps.tblwgemployee
            ADD CONSTRAINT fk_tblwgemployee_spnumber_7 FOREIGN KEY (spnumber, fpsyear) REFERENCES fps.tblemployee_y2022(spnumber, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblwgemployee_spnumber_8'
          AND conrelid = 'fps.tblwgemployee'::regclass
    ) THEN
        ALTER TABLE fps.tblwgemployee
            ADD CONSTRAINT fk_tblwgemployee_spnumber_8 FOREIGN KEY (spnumber, fpsyear) REFERENCES fps.tblemployee_y2023(spnumber, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblwgemployee_spnumber_9'
          AND conrelid = 'fps.tblwgemployee'::regclass
    ) THEN
        ALTER TABLE fps.tblwgemployee
            ADD CONSTRAINT fk_tblwgemployee_spnumber_9 FOREIGN KEY (spnumber, fpsyear) REFERENCES fps.tblemployee_y2024(spnumber, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblwgemployee_workgroupgrade'
          AND conrelid = 'fps.tblwgemployee'::regclass
    ) THEN
        ALTER TABLE fps.tblwgemployee
            ADD CONSTRAINT fk_tblwgemployee_workgroupgrade FOREIGN KEY (workgroupgrade, fpsyear) REFERENCES fps.workgroupgrade(wggrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblwgemployee_workgroupgrade_1'
          AND conrelid = 'fps.tblwgemployee'::regclass
    ) THEN
        ALTER TABLE fps.tblwgemployee
            ADD CONSTRAINT fk_tblwgemployee_workgroupgrade_1 FOREIGN KEY (workgroupgrade, fpsyear) REFERENCES fps.workgroupgrade_y2016(wggrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblwgemployee_workgroupgrade_10'
          AND conrelid = 'fps.tblwgemployee'::regclass
    ) THEN
        ALTER TABLE fps.tblwgemployee
            ADD CONSTRAINT fk_tblwgemployee_workgroupgrade_10 FOREIGN KEY (workgroupgrade, fpsyear) REFERENCES fps.workgroupgrade_y2025(wggrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblwgemployee_workgroupgrade_11'
          AND conrelid = 'fps.tblwgemployee'::regclass
    ) THEN
        ALTER TABLE fps.tblwgemployee
            ADD CONSTRAINT fk_tblwgemployee_workgroupgrade_11 FOREIGN KEY (workgroupgrade, fpsyear) REFERENCES fps.workgroupgrade_y2026(wggrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblwgemployee_workgroupgrade_12'
          AND conrelid = 'fps.tblwgemployee'::regclass
    ) THEN
        ALTER TABLE fps.tblwgemployee
            ADD CONSTRAINT fk_tblwgemployee_workgroupgrade_12 FOREIGN KEY (workgroupgrade, fpsyear) REFERENCES fps.workgroupgrade_default(wggrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblwgemployee_workgroupgrade_2'
          AND conrelid = 'fps.tblwgemployee'::regclass
    ) THEN
        ALTER TABLE fps.tblwgemployee
            ADD CONSTRAINT fk_tblwgemployee_workgroupgrade_2 FOREIGN KEY (workgroupgrade, fpsyear) REFERENCES fps.workgroupgrade_y2017(wggrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblwgemployee_workgroupgrade_3'
          AND conrelid = 'fps.tblwgemployee'::regclass
    ) THEN
        ALTER TABLE fps.tblwgemployee
            ADD CONSTRAINT fk_tblwgemployee_workgroupgrade_3 FOREIGN KEY (workgroupgrade, fpsyear) REFERENCES fps.workgroupgrade_y2018(wggrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblwgemployee_workgroupgrade_4'
          AND conrelid = 'fps.tblwgemployee'::regclass
    ) THEN
        ALTER TABLE fps.tblwgemployee
            ADD CONSTRAINT fk_tblwgemployee_workgroupgrade_4 FOREIGN KEY (workgroupgrade, fpsyear) REFERENCES fps.workgroupgrade_y2019(wggrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblwgemployee_workgroupgrade_5'
          AND conrelid = 'fps.tblwgemployee'::regclass
    ) THEN
        ALTER TABLE fps.tblwgemployee
            ADD CONSTRAINT fk_tblwgemployee_workgroupgrade_5 FOREIGN KEY (workgroupgrade, fpsyear) REFERENCES fps.workgroupgrade_y2020(wggrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblwgemployee_workgroupgrade_6'
          AND conrelid = 'fps.tblwgemployee'::regclass
    ) THEN
        ALTER TABLE fps.tblwgemployee
            ADD CONSTRAINT fk_tblwgemployee_workgroupgrade_6 FOREIGN KEY (workgroupgrade, fpsyear) REFERENCES fps.workgroupgrade_y2021(wggrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblwgemployee_workgroupgrade_7'
          AND conrelid = 'fps.tblwgemployee'::regclass
    ) THEN
        ALTER TABLE fps.tblwgemployee
            ADD CONSTRAINT fk_tblwgemployee_workgroupgrade_7 FOREIGN KEY (workgroupgrade, fpsyear) REFERENCES fps.workgroupgrade_y2022(wggrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblwgemployee_workgroupgrade_8'
          AND conrelid = 'fps.tblwgemployee'::regclass
    ) THEN
        ALTER TABLE fps.tblwgemployee
            ADD CONSTRAINT fk_tblwgemployee_workgroupgrade_8 FOREIGN KEY (workgroupgrade, fpsyear) REFERENCES fps.workgroupgrade_y2023(wggrade, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblwgemployee_workgroupgrade_9'
          AND conrelid = 'fps.tblwgemployee'::regclass
    ) THEN
        ALTER TABLE fps.tblwgemployee
            ADD CONSTRAINT fk_tblwgemployee_workgroupgrade_9 FOREIGN KEY (workgroupgrade, fpsyear) REFERENCES fps.workgroupgrade_y2024(wggrade, fpsyear);
    END IF;
END $$;
