CREATE TABLE IF NOT EXISTS fps.monthlytime (
    pactstaffid character varying(50) NOT NULL,
    timecode character varying(50) NOT NULL,
    month double precision NOT NULL,
    parentproject character varying(20) NOT NULL,
    workgroup character varying(50),
    hours double precision,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_monthlytime PRIMARY KEY (pactstaffid, timecode, month, parentproject, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.monthlytime_default PARTITION OF fps.monthlytime
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.monthlytime_y2016 PARTITION OF fps.monthlytime
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.monthlytime_y2017 PARTITION OF fps.monthlytime
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.monthlytime_y2018 PARTITION OF fps.monthlytime
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.monthlytime_y2019 PARTITION OF fps.monthlytime
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.monthlytime_y2020 PARTITION OF fps.monthlytime
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.monthlytime_y2021 PARTITION OF fps.monthlytime
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.monthlytime_y2022 PARTITION OF fps.monthlytime
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.monthlytime_y2023 PARTITION OF fps.monthlytime
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.monthlytime_y2024 PARTITION OF fps.monthlytime
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.monthlytime_y2025 PARTITION OF fps.monthlytime
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.monthlytime_y2026 PARTITION OF fps.monthlytime
    FOR VALUES IN (2026);

-- Foreign keys for fps.monthlytime
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlytime_fpsyear'
          AND conrelid = 'fps.monthlytime'::regclass
    ) THEN
        ALTER TABLE fps.monthlytime
            ADD CONSTRAINT fk_monthlytime_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlytime_pactstaffid'
          AND conrelid = 'fps.monthlytime'::regclass
    ) THEN
        ALTER TABLE fps.monthlytime
            ADD CONSTRAINT fk_monthlytime_pactstaffid FOREIGN KEY (pactstaffid, fpsyear) REFERENCES fps.tblwgemployee(pactid, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlytime_pactstaffid_1'
          AND conrelid = 'fps.monthlytime'::regclass
    ) THEN
        ALTER TABLE fps.monthlytime
            ADD CONSTRAINT fk_monthlytime_pactstaffid_1 FOREIGN KEY (pactstaffid, fpsyear) REFERENCES fps.tblwgemployee_y2016(pactid, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlytime_pactstaffid_10'
          AND conrelid = 'fps.monthlytime'::regclass
    ) THEN
        ALTER TABLE fps.monthlytime
            ADD CONSTRAINT fk_monthlytime_pactstaffid_10 FOREIGN KEY (pactstaffid, fpsyear) REFERENCES fps.tblwgemployee_y2025(pactid, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlytime_pactstaffid_11'
          AND conrelid = 'fps.monthlytime'::regclass
    ) THEN
        ALTER TABLE fps.monthlytime
            ADD CONSTRAINT fk_monthlytime_pactstaffid_11 FOREIGN KEY (pactstaffid, fpsyear) REFERENCES fps.tblwgemployee_y2026(pactid, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlytime_pactstaffid_12'
          AND conrelid = 'fps.monthlytime'::regclass
    ) THEN
        ALTER TABLE fps.monthlytime
            ADD CONSTRAINT fk_monthlytime_pactstaffid_12 FOREIGN KEY (pactstaffid, fpsyear) REFERENCES fps.tblwgemployee_default(pactid, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlytime_pactstaffid_2'
          AND conrelid = 'fps.monthlytime'::regclass
    ) THEN
        ALTER TABLE fps.monthlytime
            ADD CONSTRAINT fk_monthlytime_pactstaffid_2 FOREIGN KEY (pactstaffid, fpsyear) REFERENCES fps.tblwgemployee_y2017(pactid, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlytime_pactstaffid_3'
          AND conrelid = 'fps.monthlytime'::regclass
    ) THEN
        ALTER TABLE fps.monthlytime
            ADD CONSTRAINT fk_monthlytime_pactstaffid_3 FOREIGN KEY (pactstaffid, fpsyear) REFERENCES fps.tblwgemployee_y2018(pactid, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlytime_pactstaffid_4'
          AND conrelid = 'fps.monthlytime'::regclass
    ) THEN
        ALTER TABLE fps.monthlytime
            ADD CONSTRAINT fk_monthlytime_pactstaffid_4 FOREIGN KEY (pactstaffid, fpsyear) REFERENCES fps.tblwgemployee_y2019(pactid, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlytime_pactstaffid_5'
          AND conrelid = 'fps.monthlytime'::regclass
    ) THEN
        ALTER TABLE fps.monthlytime
            ADD CONSTRAINT fk_monthlytime_pactstaffid_5 FOREIGN KEY (pactstaffid, fpsyear) REFERENCES fps.tblwgemployee_y2020(pactid, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlytime_pactstaffid_6'
          AND conrelid = 'fps.monthlytime'::regclass
    ) THEN
        ALTER TABLE fps.monthlytime
            ADD CONSTRAINT fk_monthlytime_pactstaffid_6 FOREIGN KEY (pactstaffid, fpsyear) REFERENCES fps.tblwgemployee_y2021(pactid, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlytime_pactstaffid_7'
          AND conrelid = 'fps.monthlytime'::regclass
    ) THEN
        ALTER TABLE fps.monthlytime
            ADD CONSTRAINT fk_monthlytime_pactstaffid_7 FOREIGN KEY (pactstaffid, fpsyear) REFERENCES fps.tblwgemployee_y2022(pactid, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlytime_pactstaffid_8'
          AND conrelid = 'fps.monthlytime'::regclass
    ) THEN
        ALTER TABLE fps.monthlytime
            ADD CONSTRAINT fk_monthlytime_pactstaffid_8 FOREIGN KEY (pactstaffid, fpsyear) REFERENCES fps.tblwgemployee_y2023(pactid, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlytime_pactstaffid_9'
          AND conrelid = 'fps.monthlytime'::regclass
    ) THEN
        ALTER TABLE fps.monthlytime
            ADD CONSTRAINT fk_monthlytime_pactstaffid_9 FOREIGN KEY (pactstaffid, fpsyear) REFERENCES fps.tblwgemployee_y2024(pactid, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlytime_timecodevalid'
          AND conrelid = 'fps.monthlytime'::regclass
    ) THEN
        ALTER TABLE fps.monthlytime
            ADD CONSTRAINT fk_monthlytime_timecodevalid FOREIGN KEY (workgroup, timecode, parentproject, fpsyear) REFERENCES fps.timecodevalid(workgroup, timecode, parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlytime_timecodevalid_1'
          AND conrelid = 'fps.monthlytime'::regclass
    ) THEN
        ALTER TABLE fps.monthlytime
            ADD CONSTRAINT fk_monthlytime_timecodevalid_1 FOREIGN KEY (workgroup, timecode, parentproject, fpsyear) REFERENCES fps.timecodevalid_y2016(workgroup, timecode, parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlytime_timecodevalid_10'
          AND conrelid = 'fps.monthlytime'::regclass
    ) THEN
        ALTER TABLE fps.monthlytime
            ADD CONSTRAINT fk_monthlytime_timecodevalid_10 FOREIGN KEY (workgroup, timecode, parentproject, fpsyear) REFERENCES fps.timecodevalid_y2025(workgroup, timecode, parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlytime_timecodevalid_11'
          AND conrelid = 'fps.monthlytime'::regclass
    ) THEN
        ALTER TABLE fps.monthlytime
            ADD CONSTRAINT fk_monthlytime_timecodevalid_11 FOREIGN KEY (workgroup, timecode, parentproject, fpsyear) REFERENCES fps.timecodevalid_y2026(workgroup, timecode, parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlytime_timecodevalid_12'
          AND conrelid = 'fps.monthlytime'::regclass
    ) THEN
        ALTER TABLE fps.monthlytime
            ADD CONSTRAINT fk_monthlytime_timecodevalid_12 FOREIGN KEY (workgroup, timecode, parentproject, fpsyear) REFERENCES fps.timecodevalid_default(workgroup, timecode, parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlytime_timecodevalid_2'
          AND conrelid = 'fps.monthlytime'::regclass
    ) THEN
        ALTER TABLE fps.monthlytime
            ADD CONSTRAINT fk_monthlytime_timecodevalid_2 FOREIGN KEY (workgroup, timecode, parentproject, fpsyear) REFERENCES fps.timecodevalid_y2017(workgroup, timecode, parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlytime_timecodevalid_3'
          AND conrelid = 'fps.monthlytime'::regclass
    ) THEN
        ALTER TABLE fps.monthlytime
            ADD CONSTRAINT fk_monthlytime_timecodevalid_3 FOREIGN KEY (workgroup, timecode, parentproject, fpsyear) REFERENCES fps.timecodevalid_y2018(workgroup, timecode, parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlytime_timecodevalid_4'
          AND conrelid = 'fps.monthlytime'::regclass
    ) THEN
        ALTER TABLE fps.monthlytime
            ADD CONSTRAINT fk_monthlytime_timecodevalid_4 FOREIGN KEY (workgroup, timecode, parentproject, fpsyear) REFERENCES fps.timecodevalid_y2019(workgroup, timecode, parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlytime_timecodevalid_5'
          AND conrelid = 'fps.monthlytime'::regclass
    ) THEN
        ALTER TABLE fps.monthlytime
            ADD CONSTRAINT fk_monthlytime_timecodevalid_5 FOREIGN KEY (workgroup, timecode, parentproject, fpsyear) REFERENCES fps.timecodevalid_y2020(workgroup, timecode, parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlytime_timecodevalid_6'
          AND conrelid = 'fps.monthlytime'::regclass
    ) THEN
        ALTER TABLE fps.monthlytime
            ADD CONSTRAINT fk_monthlytime_timecodevalid_6 FOREIGN KEY (workgroup, timecode, parentproject, fpsyear) REFERENCES fps.timecodevalid_y2021(workgroup, timecode, parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlytime_timecodevalid_7'
          AND conrelid = 'fps.monthlytime'::regclass
    ) THEN
        ALTER TABLE fps.monthlytime
            ADD CONSTRAINT fk_monthlytime_timecodevalid_7 FOREIGN KEY (workgroup, timecode, parentproject, fpsyear) REFERENCES fps.timecodevalid_y2022(workgroup, timecode, parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlytime_timecodevalid_8'
          AND conrelid = 'fps.monthlytime'::regclass
    ) THEN
        ALTER TABLE fps.monthlytime
            ADD CONSTRAINT fk_monthlytime_timecodevalid_8 FOREIGN KEY (workgroup, timecode, parentproject, fpsyear) REFERENCES fps.timecodevalid_y2023(workgroup, timecode, parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_monthlytime_timecodevalid_9'
          AND conrelid = 'fps.monthlytime'::regclass
    ) THEN
        ALTER TABLE fps.monthlytime
            ADD CONSTRAINT fk_monthlytime_timecodevalid_9 FOREIGN KEY (workgroup, timecode, parentproject, fpsyear) REFERENCES fps.timecodevalid_y2024(workgroup, timecode, parentproject, fpsyear);
    END IF;
END $$;
